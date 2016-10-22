﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using XAdo.Quobs.Expressions;

namespace XAdo.Quobs.Attributes
{
   public static class Descriptor
   {
      private static int _aliasInc;

      private static readonly ConcurrentDictionary<MemberInfo,object> Cache = 
         new ConcurrentDictionary<MemberInfo, object>(); 

      public class TableDescriptor
      {
         public TableDescriptor(Type type)
         {
            if (type == null) throw new ArgumentNullException("type");
            EntityType = type;
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
            Alias = "__t_" + Interlocked.Increment(ref _aliasInc);

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

         public Type EntityType { get; private set; }
         public string Alias { get; private set; }
         public string Schema { get; private set; }
         public string Name { get; private set; }
         public bool IsView { get; private set; }
         public bool IsReadOnly { get; private set; }

         public IList<ColumnDescriptor> Columns { get; private set; }

         public override string ToString()
         {
            return Schema != null ? "[{0}].[{1}]".FormatWith(Schema, Name) : "["+ Name+"]";
         }
      }
      public class ColumnDescriptor
      {
         public ColumnDescriptor(TableDescriptor parent, MemberInfo m)
         {
            Parent = parent;
            Member = m;
            Alias = "__c_"+Interlocked.Increment(ref _aliasInc);
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
         public string Alias { get; private set; }
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
      }
      public class ReferenceDescriptor
      {
         private readonly string _memberName;
         private readonly Type _type;
         private ColumnDescriptor _referencedColumn;

         public ReferenceDescriptor(ColumnDescriptor fkey, Type type, string memberName, string name)
         {
            ForeignKey = fkey;
            Name = name;
            _type = type;
            _memberName = memberName;
         }

         public string Name { get; private set; }
         public ColumnDescriptor ForeignKey { get; private set; }
         public ColumnDescriptor Referenced
         {
            get {
               return _referencedColumn ?? (_referencedColumn = (ColumnDescriptor) Cache[_type.GetMember(_memberName).Single()]);
            }
         }

         public override string ToString()
         {
            return "FK " + ForeignKey + " => " + Referenced;
         }
      }
      public class JoinDescriptor
      {
         public JoinDescriptor(string expression)
         {
            Expression = expression;
         }
         public string Expression { get; private set; }
         public JoinType JoinType { get; set; }
      }

      public static TableDescriptor GetDescriptor(this Type self)
      {
         return Cache.GetOrAdd(self, t => new TableDescriptor(self)).CastTo<TableDescriptor>();
      }
      public static ColumnDescriptor GetDescriptor(this FieldInfo self)
      {
         return Cache.GetOrAdd(self, t => self.ReflectedType.GetDescriptor().Columns.Single(c => c.Member == self)).CastTo<ColumnDescriptor>();
      }
      public static ColumnDescriptor GetDescriptor(this PropertyInfo self)
      {
         return Cache.GetOrAdd(self, t => self.ReflectedType.GetDescriptor().Columns.Single(c => c.Member == self)).CastTo<ColumnDescriptor>();
      }
      public static ColumnDescriptor GetDescriptor(this MemberInfo self)
      {
         switch (self.MemberType)
         {
            case MemberTypes.Property:
               return self.CastTo<PropertyInfo>().GetDescriptor();
            case MemberTypes.Field:
               return self.CastTo<FieldInfo>().GetDescriptor();
            default:
               throw new NotSupportedException("type " + self.MemberType + " is not supported.");
         }
      }
      public static JoinDescriptor GetJoinDescriptor(this MethodCallExpression self)
      {
         var att = self.Method.GetAnnotation<JoinMethodAttribute>();
         if (att == null) return null;
         var result =  new JoinDescriptor(att.Expression);
         if (self.Method.GetParameters().Count() == 2)
         {
            result.JoinType = (JoinType)self.Arguments[1].GetExpressionValue();
         }
         return result;
      }

   }
}
