using System.Collections.Generic;
using System.Linq;
using XAdo.Core.Interface;

namespace XAdo.Sql.Core
{
   public class QueryContext
   {
      private readonly ISqlDialect _dialect;

      private const string
         SkipParameterName = "__skip",
         TakeParameterName = "__take";

      private List<string>
         _orderColumns;
      private int?
         _skipValue;
      private int?
         _takeValue;

      public QueryContext(ISqlDialect dialect)
      {
         _dialect = dialect;
      }

      public string Where { get; set; }
      public string Having { get; set; }
      public string Order
      {
         get { return _orderColumns != null && _orderColumns.Any() ? string.Join(", ", _orderColumns.ToArray()) : null; }
      }

      public int? SkipValue
      {
         get { return _skipValue; }
         set
         {
            _skipValue = value;
            if (value == null)
            {
               _takeValue = null;
               return;
            }
            if (_takeValue == null)
            {
               _takeValue = int.MaxValue - 100;
            }
         }
      }
      public int? TakeValue
      {
         get { return _takeValue; }
         set
         {
            _takeValue = value;
            if (value == null)
            {
               _skipValue = null;
               return;
            }
            if (_skipValue == null)
            {
               _skipValue = 0;
            }
         }
      }

      public string Skip
      {
         get { return _skipValue != null ? _dialect.ParameterFormat.FormatWith(SkipParameterName) : null; }
      }
      public string Take
      {
         get { return _takeValue != null ? _dialect.ParameterFormat.FormatWith(TakeParameterName) : null; }
      }
      public bool? Inner { get; private set; }


      public IAdoSession Session { get; set; }
      public IDictionary<string, object> WhereArguments { get; set; }
      public IDictionary<string, object> HavingArguments { get; set; }
      public List<string> OrderColumns
      {
         get { return _orderColumns ?? (_orderColumns = new List<string>()); }
      }
      public IDictionary<string, object> GetArguments()
      {
         var args = WhereArguments == null || HavingArguments == null
            ? (WhereArguments ?? HavingArguments)
            : new Dictionary<string, object>()
               .AddRange(WhereArguments)
               .AddRange(HavingArguments);
         if (_skipValue != null)
         {
            args[SkipParameterName] = _skipValue;
         }
         if (_takeValue != null)
         {
            args[TakeParameterName] = _takeValue;
         }
         return args;
      }

      public QueryContext Clone(bool forInnerQuery=false)
      {
         var clone = new QueryContext(_dialect)
         {
            Where = Where,
            Having = Having,
            Session = Session,
            WhereArguments = WhereArguments,
            HavingArguments = HavingArguments,
            Inner = forInnerQuery ? (bool?)true : null
         };
         if (!forInnerQuery)
         {
            clone._skipValue = _skipValue;
            clone._takeValue = _takeValue;
            clone._orderColumns = _orderColumns != null ? _orderColumns.ToList() : null;
         }
         return clone;
      }

   }
}