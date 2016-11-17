using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using XAdo.Quobs.Core.DbSchema.Attributes;
using XAdo.Quobs.Core.SqlExpression;
using XAdo.Quobs.Core.SqlExpression.Core;
using XAdo.Quobs.Core.SqlExpression.Sql;

namespace XAdo.Quobs.Core.DbSchema
{
   public static class DbSchemaDescriptor
   {
      private static int _idInc;

      private static readonly ConcurrentDictionary<MemberInfo,object> Cache = 
         new ConcurrentDictionary<MemberInfo, object>();
      private static readonly ConcurrentDictionary<string, JoinInfo> JoinLookup =
         new ConcurrentDictionary<string, JoinInfo>();

      public class TableDescriptor
      {
         public TableDescriptor(Type type)
         {
            if (type == null) throw new ArgumentNullException("type");
            Type = type;
            var tableAtt = type.GetAnnotation<TableAttribute>();
            if (tableAtt != null)
            {
               Name = tableAtt.Name;
               Schema = tableAtt.Schema;
            }
            else
            {
               Name = type.Name;
            }
            var viewAtt = type.GetAnnotation<DbViewAttribute>();
            IsView = viewAtt != null;
            IsReadOnly = viewAtt != null && viewAtt.IsReadOnly;
            Id = Interlocked.Increment(ref _idInc);

            Columns =
               type.GetProperties()
                  .Cast<MemberInfo>()
                  .Concat(type.GetFields())
                  .Select(m => new ColumnDescriptor(this,m))
                  .ToList()
                  .AsReadOnly();

            foreach (var c in Columns)
            {
               Cache[c.Member] = c;
            }
         }

         public Type Type { get; private set; }
         public int Id { get; private set; }
         public string Schema { get; private set; }
         public string Name { get; private set; }
         public bool IsView { get; private set; }
         public bool IsReadOnly { get; private set; }

         public IList<ColumnDescriptor> Columns { get; private set; }

         public override string ToString()
         {
            return Schema != null ? string.Format("[{0}].[{1}]",Schema, Name) : "["+ Name+"]";
         }

         public string Format(string leftDelimiter, string rightDelimiter, string alias = null)
         {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(Schema))
            {
               sb.Append(Schema.Delimit(leftDelimiter,rightDelimiter));
               sb.Append(".");
            }
            sb.Append(Name.Delimit(leftDelimiter, rightDelimiter));
            if (alias != null)
            {
               sb.Append(" AS ");
               sb.Append(alias.Delimit(leftDelimiter, rightDelimiter));
            }
            return sb.ToString();
         }

      }
      public class ColumnDescriptor
      {
         public ColumnDescriptor(TableDescriptor parent, MemberInfo m)
         {
            Parent = parent;
            Member = m;
            Id = Interlocked.Increment(ref _idInc);
            var columnAtt = m.GetAnnotation<ColumnAttribute>();
            Name = columnAtt != null ? columnAtt.Name : m.Name;
            IsPKey = m.GetAnnotation<KeyAttribute>() != null;
            Required = m.GetAnnotation<RequiredAttribute>() != null;
            IsUnique = m.GetAnnotation<DbUniqueAttribute>() != null;
            IsAutoIncrement = m.GetAnnotation<DbAutoIncrementAttribute>() != null;
            IsReadOnly = m.GetAnnotation<ReadOnlyAttribute>() != null;
            var maxLengthAtt = m.GetAnnotation<MaxLengthAttribute>();
            MaxLength = maxLengthAtt != null ? (int?)maxLengthAtt.Length : null;
            var refAtt = m.GetAnnotation<ReferencesAttribute>();
            if (refAtt != null)
            {
               References = new ReferenceDescriptor(this,refAtt.Type,refAtt.MemberName,refAtt.FKeyName);
            }

         }
         public TableDescriptor Parent { get; private set; }
         public int Id { get; private set; }
         public string Name { get; private set; }
         public bool IsPKey { get; private set; }
         public bool IsUnique { get; private set; }
         public bool IsAutoIncrement { get; private set; }
         public int? MaxLength { get; private set; }
         public bool Required { get; private set; }
         public bool IsReadOnly { get; private set; }
         public MemberInfo Member { get; private set; }
         public ReferenceDescriptor References { get; private set; }
         public override string ToString()
         {
            return Parent + ".[" + Name + "]";
         }
         public string Format(string leftDelimiter, string rightDelimiter, string tableAlias = null)
         {
            var sb = new StringBuilder(tableAlias.Delimit(leftDelimiter, rightDelimiter) ?? Parent.Format(leftDelimiter, rightDelimiter));
            sb.Append(".");
            sb.Append(Name.Delimit(leftDelimiter, rightDelimiter));
            return sb.ToString();
         }

      }
      public class ReferenceDescriptor
      {
         private readonly string _memberName;
         private readonly Type _type;
         private ColumnDescriptor _referencedColumn;

         public ReferenceDescriptor(ColumnDescriptor fkey, Type type, string memberName, string name)
         {
            ForeignKeyColumn = fkey;
            Name = name;
            _type = type;
            _memberName = memberName;
         }

         public string Name { get; private set; }
         public ColumnDescriptor ForeignKeyColumn { get; private set; }
         public ColumnDescriptor ReferencedColumn
         {
            get {
               return _referencedColumn ?? (_referencedColumn = (ColumnDescriptor) Cache[_type.GetMember(_memberName).Single()]);
            }
         }

         public override string ToString()
         {
            return "FKey " + ForeignKeyColumn + " => " + ReferencedColumn;
         }
      }

      public class JoinInfo
      {
         private class JoinBuilderContext : SqlBuilderContext
         {
            private readonly Type _leftTableType;

            public JoinBuilderContext(ISqlFormatter formatter, Type leftTableType)
               : base(formatter)
            {
               _leftTableType = leftTableType;
               ArgumentsAsLiterals = true;
            }

            public override void WriteFormattedColumn(MemberExpression exp)
            {
               Writer.Write(exp.Member.ReflectedType == _leftTableType ? "{0}" : "{1}");
               Writer.Write(".");
               Writer.Write(Formatter.FormatIdentifier(exp.Member.GetColumnDescriptor().Name));
            }
         }

         private ConcurrentDictionary<Type, string>
            _sqlExpressionCache;

         private TableDescriptor 
            _leftTable,
            _rightTable;

         private IList<ColumnDescriptor> 
            _leftColumns,
            _rightColumns;

         private JoinInfo() { }
         public JoinInfo(string constraintName, Type leftTable, Type rightTable)
         {
            if (constraintName == null) throw new ArgumentNullException("constraintName");
            if (leftTable == null) throw new ArgumentNullException("leftTable");
            if (rightTable == null) throw new ArgumentNullException("rightTable");
            ConstraintName = constraintName;
            _leftTable = leftTable.GetTableDescriptor();
            _rightTable = rightTable.GetTableDescriptor();
            var references = _leftTable.Columns.Where(c => c.References != null && c.References.Name == constraintName)
               .Select(c => c.References)
               .ToArray();
            _leftColumns = references.Select(r => r.ForeignKeyColumn).ToList().AsReadOnly();
            _rightColumns = references.Select(r => r.ReferencedColumn).ToList().AsReadOnly();
         }

         public JoinInfo(string constraintName, Expression expression, Type leftTable, Type rightTable)
         {
            if (constraintName == null) throw new ArgumentNullException("constraintName");
            if (expression == null) throw new ArgumentNullException("expression");
            if (leftTable == null) throw new ArgumentNullException("leftTable");
            if (rightTable == null) throw new ArgumentNullException("rightTable");
            ConstraintName = constraintName;
            Expression = expression;
            _leftTable = leftTable.GetTableDescriptor();
            _rightTable = rightTable.GetTableDescriptor();
            _sqlExpressionCache = new ConcurrentDictionary<Type, string>();

         }

         public string ConstraintName { get; private set; }
         public Expression Expression { get; private set; }
         public bool Reversed { get; private set; }

         public JoinInfo Reverse(bool? reversed = null)
         {
            if (Expression != null)
            {
               throw new NotImplementedException("JoinInfo cannot be reversed when it has been defined by a literal expression");
            }
            reversed = reversed ?? !Reversed;
            return reversed.Value == Reversed
               ? this
               : new JoinInfo
               {
                  ConstraintName = ConstraintName,
                  _leftTable = _rightTable,
                  _rightTable = _leftTable,
                  _leftColumns = _rightColumns,
                  _rightColumns = _leftColumns,
                  Reversed = reversed.Value
               };
         }

         public string Format(ISqlFormatter formatter, string leftAlias=null, string rightAlias = null)
         {
            string delimiterLeft = formatter.IdentifierDelimiterLeft;
            string delimiterRight = formatter.IdentifierDelimiterRight;
            using (var sw = new StringWriter())
            {
               sw.Write("JOIN {0} ",_rightTable.Format(delimiterLeft,delimiterRight));
               if (rightAlias != null)
               {
                  sw.Write(" AS ");
                  sw.Write(rightAlias.Delimit(delimiterLeft,delimiterRight));
                  sw.Write(" ");
               }
               sw.Write(" ON ");
               if (Expression != null)
               {
                  var sql = _sqlExpressionCache.GetOrAdd(formatter.GetType(), t => CompileExpression(formatter));
                  sw.Write(sql, leftAlias ?? _leftTable.Format(delimiterLeft, delimiterRight), rightAlias ?? _rightTable.Format(delimiterLeft, delimiterRight));
               }
               else
               {
                  var and = "";
                  for (var i = 0; i < _leftColumns.Count; i++)
                  {
                     sw.Write(and);
                     sw.Write(_leftColumns[i].Format(delimiterLeft, delimiterRight, leftAlias));
                     sw.Write(" = ");
                     sw.Write(_rightColumns[i].Format(delimiterLeft, delimiterRight, rightAlias));
                     and = " AND ";
                  }
               }
               return sw.GetStringBuilder().ToString();
            }
         }

         public override int GetHashCode()
         {
            unchecked
            {
               return Reversed ? ConstraintName.GetHashCode() * 829 : ConstraintName.GetHashCode();
            }
         }

         public override bool Equals(object obj)
         {
            var other = obj as JoinInfo;
            return other != null && other.ConstraintName == ConstraintName && other.Reversed == Reversed;
         }

         private string CompileExpression(ISqlFormatter formatter)
         {
            var b = new SqlExpressionBuilder();
            var context = new JoinBuilderContext(formatter, _leftTable.Type);
            var result = b.BuildSql(context, Expression);
            return result.ToString();
         }
      }
      public class JoinDescriptor
      {
         public JoinDescriptor(JoinInfo joinInfo, JoinType joinType)
         {
            JoinInfo = joinInfo;
            JoinType = joinType;
         }
         public JoinInfo JoinInfo { get; private set; }
         public JoinType JoinType { get; private set; }

         public string LeftTableAlias { get; set; }
         public string RightTableAlias { get; set; }

         public string Format(ISqlFormatter formatter)
         {
            return JoinType.ToJoinTypeString() + " " +
                   JoinInfo.Format(formatter, LeftTableAlias, RightTableAlias);
         }

         public override int GetHashCode()
         {
            unchecked
            {
               return JoinInfo.GetHashCode() + (JoinType.GetHashCode()*829);
            }
         }

         public override bool Equals(object obj)
         {
            var other = obj as JoinDescriptor;
            return other != null && JoinInfo.Equals(other.JoinInfo) && JoinType == other.JoinType;
         }
      }
      public class JoinPath
      {
         public JoinPath(IEnumerable<JoinDescriptor> joins)
         {
            Joins = joins.ToArray();
         }

         public IList<JoinDescriptor> Joins { get; internal set; }

         public override int GetHashCode()
         {
            unchecked
            {
               var hashcode = 829;
               foreach (var j in Joins)
               {
                  hashcode += j.GetHashCode();
                  hashcode *= 5;
               }
               return hashcode;
            }
         }

         public override bool Equals(object obj)
         {
            var other = obj as JoinPath;
            return other != null &&  Joins.SequenceEqual(other.Joins);
         }

         public bool EqualsOrStartsWith(JoinPath other)
         {
            var i = 0;
            return Joins.Count >= other.Joins.Count && other.Joins.All(j => j.Equals(Joins[i++]));
         }

         public string Format(ISqlFormatter formatter)
         {
            var sb = new StringBuilder();
            foreach (var j in Joins)
            {
               sb.Append("   ");
               sb.AppendLine(j.Format(formatter));
            }
            return sb.ToString();
         }
      }


      public static void DefineJoin<TLeft, TRight>(string name, Expression<Func<TLeft, TRight, bool>> joinExpression)
      {
         if (!JoinLookup.TryAdd(name, new JoinInfo(name, joinExpression, typeof(TLeft), typeof(TRight))))
         {
            throw new QuobException("Join name already exists");
         }
      }

      public static TableDescriptor GetTableDescriptor(this Type self)
      {
         return Cache.GetOrAdd(self, t => new TableDescriptor(self)).CastTo<TableDescriptor>();
      }
      public static ColumnDescriptor GetColumnDescriptor(this FieldInfo self)
      {
         return Cache.GetOrAdd(self, t => self.ReflectedType.GetTableDescriptor().Columns.Single(c => c.Member == self)).CastTo<ColumnDescriptor>();
      }
      public static ColumnDescriptor GetColumnDescriptor(this PropertyInfo self)
      {
         return Cache.GetOrAdd(self, t => self.ReflectedType.GetTableDescriptor().Columns.Single(c => c.Member == self)).CastTo<ColumnDescriptor>();
      }
      public static IList<JoinDescriptor> GetJoinDescriptors(this MethodInfo self, JoinType joinType)
      {
         var atts = self.GetAnnotations<JoinMethodAttribute>();
         if (!atts.Any()) return new JoinDescriptor[0];
         return atts.Select(att =>
         {
            var joinInfo = JoinLookup.GetOrAdd(att.RelationshipName, n =>
               new JoinInfo(att.RelationshipName, att.Reversed ? self.ReturnType : self.GetParameters()[0].ParameterType, att.Reversed ? self.GetParameters()[0].ParameterType : self.ReturnType));

            if (att.Reversed)
            {
               joinInfo = joinInfo.Reverse();
            }

            return new JoinDescriptor(joinInfo, joinType);
         }).ToList();
      }
      public static ColumnDescriptor GetColumnDescriptor(this MemberInfo self)
      {
         switch (self.MemberType)
         {
            case MemberTypes.Property:
               return self.CastTo<PropertyInfo>().GetColumnDescriptor();
            case MemberTypes.Field:
               return self.CastTo<FieldInfo>().GetColumnDescriptor();
            default:
               throw new QuobException("type " + self.MemberType + " is not supported.");
         }
      }

   }

}
