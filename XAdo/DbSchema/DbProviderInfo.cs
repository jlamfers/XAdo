using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace XAdo.DbSchema
{

   /// <summary>
   /// inspired by: https://www.codeproject.com/Articles/52076/Using-Information-from-the-NET-DataProvider
   /// </summary>
   public class DbProviderInfo
   {
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] 
      private static readonly Type 
         IdentifierCaseType = Enum.GetUnderlyingType(typeof(IdentifierCase));

      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private static readonly Type 
         GroupByBehaviorType = Enum.GetUnderlyingType(typeof(GroupByBehavior));

      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private static readonly Type 
         SupportedJoinOperatorsType = Enum.GetUnderlyingType(typeof(SupportedJoinOperators));

      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string _compositeIdentifierSeparatorPattern;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string _dataSourceProductName;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string _dataSourceProductVersion;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string _dataSourceProductVersionNormalized;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string _parameterMarkerFormat;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string _parameterMarkerPattern;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string _identifierPattern;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string _parameterNamePattern;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string _quotedIdentifierPattern;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string _statementSeparatorPattern;

      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private Regex _quotedIdentifierCase;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private Regex _stringLiteralPattern;

      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private GroupByBehavior _groupByBehavior;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private IdentifierCase _identifierCase;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private bool _orderByColumnsInSelect;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int _parameterNameMaxLength;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private SupportedJoinOperators _supportedJoinOperators;

      public virtual DbProviderInfo Initialize(DbConnection cn)
      {
         if (cn == null) throw new ArgumentNullException("cn");
         var opened = false;
         if (cn.State != ConnectionState.Open)
         {
            cn.Close();
            cn.Open();
            opened = true;
         }
         var dataTable = cn.GetSchema(DbMetaDataCollectionNames.DataSourceInformation);
         var row = dataTable.Rows[0];
         foreach (DataColumn column in dataTable.Columns)
         {
            var columnName = column.ColumnName;
            var value = row[column.Ordinal];
            if (value == DBNull.Value)
            {
               value = null;
            }

            if (string.IsNullOrEmpty(columnName) || value == null) 
               continue;

            switch (columnName)
            {
               case "QuotedIdentifierCase":
                  _quotedIdentifierCase = new Regex(value.ToString());
                  break;
               case "StringLiteralPattern":
                  _stringLiteralPattern = new Regex(value.ToString());
                  break;
               case "GroupByBehavior":
                  value = Convert.ChangeType(value, GroupByBehaviorType);
                  _groupByBehavior = (GroupByBehavior) value;
                  break;
               case "IdentifierCase":
                  value = Convert.ChangeType(value, IdentifierCaseType);
                  _identifierCase = (IdentifierCase) value;
                  break;
               case "SupportedJoinOperators":
                  value = Convert.ChangeType(value, SupportedJoinOperatorsType);
                  _supportedJoinOperators = (SupportedJoinOperators) value;
                  break;
               default:
                  var field = typeof (DbProviderInfo).GetField("_" + columnName, BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);
                  if (field != null)
                  {
                     field.SetValue(this, value);
                  }
                  break;
            }
         }
         if (opened)
         {
            cn.Close();
         }
         return this;
      }

      public string CompositeIdentifierSeparatorPattern
      {
         get { return _compositeIdentifierSeparatorPattern; }
      }

      public string DataSourceProductName
      {
         get { return _dataSourceProductName; }
      }

      public string DataSourceProductVersion
      {
         get { return _dataSourceProductVersion; }
      }

      public string DataSourceProductVersionNormalized
      {
         get { return _dataSourceProductVersionNormalized; }
      }

      public GroupByBehavior GroupByBehavior
      {
         get { return _groupByBehavior; }
      }

      public string IdentifierPattern
      {
         get { return _identifierPattern; }
      }

      public IdentifierCase IdentifierCase
      {
         get { return _identifierCase; }
      }

      public bool OrderByColumnsInSelect
      {
         get { return _orderByColumnsInSelect; }
      }

      public string ParameterMarkerFormat
      {
         get { return _parameterMarkerFormat; }
      }

      public string ParameterMarkerPattern
      {
         get { return _parameterMarkerPattern; }
      }

      public int ParameterNameMaxLength
      {
         get { return _parameterNameMaxLength; }
      }

      public string ParameterNamePattern
      {
         get { return _parameterNamePattern; }
      }

      public string QuotedIdentifierPattern
      {
         get { return _quotedIdentifierPattern; }
      }

      public Regex QuotedIdentifierCase
      {
         get { return _quotedIdentifierCase; }
      }

      public string StatementSeparatorPattern
      {
         get { return _statementSeparatorPattern; }
      }

      public Regex StringLiteralPattern
      {
         get { return _stringLiteralPattern; }
      }

      public SupportedJoinOperators SupportedJoinOperators
      {
         get { return _supportedJoinOperators; }
      }
   }
}
