using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using XAdo.Quobs.Core.DbSchema;
using XAdo.Quobs.Core.SqlExpression.Core;
using XAdo.Quobs.Dialect;

namespace XAdo.Quobs.Core
{
   public class UpdateExpressionCompiler : ExpressionVisitor
   {
      private static ConcurrentDictionary<Type,int>
         _keys = new ConcurrentDictionary<Type, int>();

      private readonly ISqlFormatter _formatter;
      private bool _argumentsAsLiterals;

      private IDictionary<string, object> _arguments;
      private List<Tuple<DbSchemaDescriptor.ColumnDescriptor, string>> _assignments;
      private List<Tuple<DbSchemaDescriptor.ColumnDescriptor, string>> _keyConstraint;
      private string _tableName;
      private Type _tableType;
      private int _key;

      public class CompileResult
      {
         public CompileResult(IDictionary<string, object> arguments, IList<Tuple<DbSchemaDescriptor.ColumnDescriptor, string>> assignments, IList<Tuple<DbSchemaDescriptor.ColumnDescriptor, string>> keyConstraint, string tableName)
         {
            TableName = tableName;
            KeyConstraint = keyConstraint;
            Assignments = assignments;
            Arguments = arguments;
         }

         public IDictionary<string, object> Arguments { get;  private set; }
         public IList<Tuple<DbSchemaDescriptor.ColumnDescriptor, string>> Assignments { get; private set; }
         public IList<Tuple<DbSchemaDescriptor.ColumnDescriptor, string>> KeyConstraint { get; private set; }
         public string TableName { get; private set; }
      }

      public UpdateExpressionCompiler(ISqlFormatter formatter)
      {
         _formatter = formatter;
         
      }

      public CompileResult Compile(LambdaExpression expression, bool argumentsAsLiterals = false)
      {
         _argumentsAsLiterals = argumentsAsLiterals;
         _arguments = new Dictionary<string, object>();
         _assignments = new List<Tuple<DbSchemaDescriptor.ColumnDescriptor, string>>();
         _keyConstraint = new List<Tuple<DbSchemaDescriptor.ColumnDescriptor, string>>();
         _tableType = expression.Body.Type;
         _tableName = _tableType.GetTableDescriptor().Format(_formatter);  
         _key = _keys.GetOrAdd(_tableType, t => _keys.Count + 1);

         Visit(expression);

         return new CompileResult(_arguments,_assignments,_keyConstraint,_tableName);
      }

      protected override Expression VisitNew(NewExpression node)
      {
         if (node.Members != null)
         {
            for (var i = 0; i < node.Members.Count; i++)
            {
               var d = node.Members[i].GetColumnDescriptor();
               var t = Tuple.Create(d, AddArgument(node.Members[i],node.Arguments[i]));
               if (d.IsPKey)
               {
                  _keyConstraint.Add(t);
               }
               _assignments.Add(t);
            }
         }
         return base.VisitNew(node);
      }

      protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
      {
         var d = node.Member.GetColumnDescriptor();
         var t = Tuple.Create(d, AddArgument(node.Member,node.Expression));
         if (d.IsPKey)
         {
            _keyConstraint.Add(t);
         }
         _assignments.Add(t);
         return base.VisitMemberAssignment(node);
      }

      private string AddArgument(MemberInfo member, Expression expression)
      {
         var value = expression.GetExpressionValue();
         if (_argumentsAsLiterals)
         {
            return _formatter.FormatValue(value);
         }
         var name = member.Name + "_" + _key + "_";
         _arguments.Add(name,value);
         return _formatter.FormatParameter(name);
      }
   }
}
