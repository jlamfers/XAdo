using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using XAdo.Quobs.Core.DbSchema;
using XAdo.Quobs.Dialects;
using ISqlFormatter = XAdo.Quobs.Dialects.Core.ISqlFormatter;

namespace XAdo.Quobs.Core.SqlExpression
{
   public class SqlBuilderContext
   {
      public SqlBuilderContext(ISqlFormatter formatter)
      {
         if (formatter == null) throw new ArgumentNullException("formatter");
         Formatter = formatter;
         Writer = new StringWriter();
         Arguments = new Dictionary<string, object>();
         Items = new Dictionary<object, object>();
      }
      public ISqlFormatter Formatter { get; private set; }
      public TextWriter Writer { get; set; }
      public IDictionary<string, object> Arguments { get; set; }
      public IDictionary<object, object> Items { get; set; }
      public bool ArgumentsAsLiterals { get; set; }

      public virtual void WriteFormattedColumn(MemberExpression exp)
      {
         exp.Member.GetColumnDescriptor().Format(Writer,Formatter);
      }

      public Func<ExpressionVisitor, SqlBuilderContext, Expression, Expression> VisitorHook { get; set; }

      public override string ToString()
      {
         var w = Writer as StringWriter;
         return w == null ? base.ToString() : w.GetStringBuilder().ToString();
      }
   }
}