using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using XAdo.Quobs.Core.Mapper;
using XAdo.Quobs.Core.Parser.Partials;
using XAdo.Quobs.Dialects;
using XAdo.Quobs.Linq;

namespace XAdo.Quobs.Core
{
   // immutable object
   public partial class QueryBuilder
   {
      private IUrlExpressionParser _urlParser;

      #region Hidden fields
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private IList<SqlPartial> _partials;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private WithPartial _with;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private bool _withChecked;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private SelectPartial _select;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private TablePartial _table;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private IList<JoinPartial> _joins;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private WherePartial _where;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private bool _whereChecked;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private GroupByPartial _groupBy;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private bool _groupbyChecked;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private HavingPartial _having;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private bool _havingChecked;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private HavingPartial _orderBy;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private bool _orderbyChecked;
      #endregion

      protected QueryBuilder(QueryBuilder other)
      {
         _partials = other._partials;
         Dialect = other.Dialect;
         _countQuery = other._countQuery;
         _binderCache = other._binderCache;
         _mapCache = other._mapCache;
         _urlParser = other._urlParser;
      }

      private QueryBuilder()
      {
         
      }

      public QueryBuilder(IList<SqlPartial> partials, ISqlDialect dialect, IUrlExpressionParser urlParser )
      {
         if (partials == null) throw new ArgumentNullException("partials");
         if (dialect == null) throw new ArgumentNullException("dialect");
         if (urlParser == null) throw new ArgumentNullException("urlParser");

         Dialect = dialect;
         _urlParser = urlParser;
         _partials = partials.MergeTemplate(dialect.SelectTemplate).AsReadOnly();
         BindColumnsToTables();
      }

      public ISqlDialect Dialect { get; private set; }

      public QueryBuilder CreateMap(IList<SqlPartial> partials)
      {
         var mapped = new QueryBuilder
         {
            _partials = partials as ReadOnlyCollection<SqlPartial> ?? partials.ToList().AsReadOnly(),
            Dialect = Dialect,
            _urlParser = _urlParser
         };
         mapped.BindColumnsToTables();
         return mapped;
      }

      private void BindColumnsToTables()
      {
         if (!Joins.Any())
         {
            foreach (MetaColumnPartial c in Select.Columns)
            {
               if (!c.Meta.IsCalculated && c.Meta.Persistency != PersistencyType.Read)
               {
                  c.Table = Table;
               }
            }
            return;
         }
         foreach (var c in Select.Columns)
         {
            c.Table = Tables.SingleOrDefault(t => t.OwnsColumn(c));
         }
      }

      public WithPartial With
      {
         get
         {
            if (_withChecked) return _with;
            _withChecked = true;
            return _with ?? (_with = _partials.OfType<WithPartial>().SingleOrDefault());
         }
      }

      public SelectPartial Select
      {
         get { return _select ?? (_select = _partials.OfType<SelectPartial>().Single()); }
      }

      public TablePartial Table
      {
         get { return _table ?? (_table = _partials.OfType<TablePartial>().Single()); }
      }

      public IList<JoinPartial> Joins
      {
         get { return _joins ?? (_joins = _partials.OfType<JoinPartial>().ToList().AsReadOnly()); }
      }

      public WherePartial Where
      {
         get
         {
            if (_whereChecked) return _where;
            _whereChecked = true;
            return _where ?? (_where = _partials.OfType<WherePartial>().SingleOrDefault());
         }
      }

      public GroupByPartial GroupBy
      {
         get
         {
            if (_groupbyChecked) return _groupBy;
            _groupbyChecked = true;
            return _groupBy ?? (_groupBy = _partials.OfType<GroupByPartial>().SingleOrDefault());
         }
      }

      public HavingPartial Having
      {
         get
         {
            if (_havingChecked) return _having;
            _havingChecked = true;
            return _having ?? (_having = _partials.OfType<HavingPartial>().SingleOrDefault());
         }
      }

      public HavingPartial OrderBy
      {
         get
         {
            if (_orderbyChecked) return _orderBy;
            _orderbyChecked = true;
            return _orderBy ?? (_orderBy = _partials.OfType<HavingPartial>().SingleOrDefault());
         }
      }

   }


}
