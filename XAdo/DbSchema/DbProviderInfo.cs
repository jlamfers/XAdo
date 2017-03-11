using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace XAdo.DbSchema
{

   /// <summary>
   /// based on: https://www.codeproject.com/Articles/52076/Using-Information-from-the-NET-DataProvider
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
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string _stringLiteralPattern;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string _quotedIdentifierCase;
     

      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private GroupByBehavior _groupByBehavior;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private IdentifierCase _identifierCase;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private bool _orderByColumnsInSelect;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private int _parameterNameMaxLength;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private SupportedJoinOperators _supportedJoinOperators;

      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string _formatIdentifierPattern;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)] private string _compositeIdentifierSeparator;

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
                  _quotedIdentifierCase = value.ToString();
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
                  if (field != null && field.FieldType.IsAssignableFrom(value.GetType()))
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


      public virtual string FormatIdentifier(string identifier)
      {
         if (_formatIdentifierPattern == null)
         {
            var probes = new[] {"[a]", "`a`", "\"a\"", "'a'"};
            var match = probes.FirstOrDefault(p => Regex.IsMatch(p,QuotedIdentifierPattern)) ?? "a";
            _formatIdentifierPattern = match.Replace("a", "{0}");
         }
         return string.Format(_formatIdentifierPattern,identifier);
      }

      public virtual string FormatIdentifier(IEnumerable<string> parts)
      {
         if (_compositeIdentifierSeparator == null)
         {
            _compositeIdentifierSeparator = CompositeIdentifierSeparatorPattern ?? ".";
            if (_compositeIdentifierSeparator.First() == '\\')
            {
               _compositeIdentifierSeparator = _compositeIdentifierSeparator.Substring(1);
            }
         }
         return string.Join(_compositeIdentifierSeparator, parts.Select(FormatIdentifier));

      }

      public virtual string CompositeIdentifierSeparatorPattern
      {
         get { return _compositeIdentifierSeparatorPattern; }
      }

      public virtual string DataSourceProductName
      {
         get { return _dataSourceProductName; }
      }

      public virtual string DataSourceProductVersion
      {
         get { return _dataSourceProductVersion; }
      }

      public virtual string DataSourceProductVersionNormalized
      {
         get { return _dataSourceProductVersionNormalized; }
      }

      public virtual GroupByBehavior GroupByBehavior
      {
         get { return _groupByBehavior; }
      }

      public virtual string IdentifierPattern
      {
         get { return _identifierPattern; }
      }

      public virtual IdentifierCase IdentifierCase
      {
         get { return _identifierCase; }
      }

      public virtual bool OrderByColumnsInSelect
      {
         get { return _orderByColumnsInSelect; }
      }

      public virtual string ParameterMarkerFormat
      {
         get { return _parameterMarkerFormat; }
      }

      public virtual string ParameterMarkerPattern
      {
         get { return _parameterMarkerPattern; }
      }

      public virtual int ParameterNameMaxLength
      {
         get { return _parameterNameMaxLength; }
      }

      public virtual string ParameterNamePattern
      {
         get { return _parameterNamePattern; }
      }

      public virtual string QuotedIdentifierPattern
      {
         get { return _quotedIdentifierPattern; }
      }

      public virtual string QuotedIdentifierCase
      {
         get { return _quotedIdentifierCase; }
      }

      public virtual string StatementSeparatorPattern
      {
         get { return _statementSeparatorPattern; }
      }

      public virtual string StringLiteralPattern
      {
         get { return _stringLiteralPattern; }
      }

      public virtual SupportedJoinOperators SupportedJoinOperators
      {
         get { return _supportedJoinOperators; }
      }
   }
}
