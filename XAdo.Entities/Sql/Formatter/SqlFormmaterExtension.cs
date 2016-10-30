using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using XAdo.Quobs.Descriptor;
using XAdo.Quobs.Expressions;

namespace XAdo.Quobs.Sql.Formatter
{
   public static class SqlFormmaterExtension
   {

      public static ISqlFormatter ConcatenateSqlStatements(this ISqlFormatter self, TextWriter w, params string[] statements)
      {
         if (self == null) throw new ArgumentNullException("self");
         if (w == null) throw new ArgumentNullException("w");
         if (statements == null) throw new ArgumentNullException("statements");
         foreach (var s in statements)
         {
            w.Write(s);
            w.Write(self.StatementSeperator);
            w.Write(Environment.NewLine);
         }
         return self;
      }

      public static string FormatIdentifier(this ISqlFormatter self, params string[] identifiers)
      {
         using (var sw = new StringWriter())
         {
            self.FormatIdentifier(sw, identifiers);
            return sw.GetStringBuilder().ToString();
         }
      }
      public static ISqlFormatter FormatIdentifier(this ISqlFormatter self, TextWriter w, params string[] identifiers)
      {
         string sep = null;
         foreach (var p in identifiers)
         {
            if (p == null) continue;
            w.Write(sep);
            var delimited = p.StartsWith(self.IdentifierDelimiterLeft);
            if (!delimited)
               w.Write(self.IdentifierDelimiterLeft);
            w.Write(p);
            if (!delimited)
               w.Write(self.IdentifierDelimiterRight);
            sep = sep ?? ".";
         }
         return self;
      }

      public static ISqlFormatter FormatParameterName(this ISqlFormatter self, TextWriter w, string parameterName)
      {
         if (self == null) throw new ArgumentNullException("self");
         if (w == null) throw new ArgumentNullException("w");
         if (parameterName == null) throw new ArgumentNullException("parameterName");
         w.Write(parameterName.StartsWith(self.ParameterPrefix) ? parameterName  : self.ParameterPrefix + parameterName);
         return self;
      }
      public static string FormatParameterName(this ISqlFormatter self, string parameterName)
      {
         if (self == null) throw new ArgumentNullException("self");
         if (parameterName == null) throw new ArgumentNullException("parameterName");
         return parameterName.StartsWith(self.ParameterPrefix) ? parameterName : self.ParameterPrefix + parameterName;
      }

      public static ISqlFormatter FormatTable(this ISqlFormatter self, TextWriter w, SchemaDescriptor.TableDescriptor table, string alias = null)
      {
         if (self == null) throw new ArgumentNullException("self");
         if (w == null) throw new ArgumentNullException("w");
         if (table == null) throw new ArgumentNullException("table");
         self.FormatIdentifier(w, table.Schema, table.Name);
         if (alias == null) return self;
         w.Write(" AS ");
         self.FormatIdentifier(w,alias);
         return self;
      }
      public static ISqlFormatter FormatTable(this ISqlFormatter self, TextWriter w, Type type, string alias = null)
      {
         if (self == null) throw new ArgumentNullException("self");
         if (w == null) throw new ArgumentNullException("w");
         if (type == null) throw new ArgumentNullException("type");
         return self.FormatTable(w, type.GetTableDescriptor(), alias);
      }
      public static string FormatTable(this ISqlFormatter self, Type type, string alias = null)
      {
         using (var sw = new StringWriter())
         {
            self.FormatTable(sw, type,alias);
            return sw.GetStringBuilder().ToString();
         }
      }

      public static ISqlFormatter FormatColumn(this ISqlFormatter self, TextWriter w, SchemaDescriptor.ColumnDescriptor column, bool aliased = false, string columnAlias = null, string tableAlias = null)
      {
         if (self == null) throw new ArgumentNullException("self");
         if (w == null) throw new ArgumentNullException("w");
         if (column == null) throw new ArgumentNullException("column");
         if (tableAlias != null)
         {
            self.FormatIdentifier(w,tableAlias, column.Name);
         }
         else
         {
            self.FormatIdentifier(w,column.Parent.Schema,column.Parent.Name, column.Name);
         }
         if (!aliased) return self;
         w.Write(" AS ");
         self.FormatIdentifier(w, columnAlias ?? column.Member.Name);
         return self;
      }
      public static string FormatColumn(this ISqlFormatter self, SchemaDescriptor.ColumnDescriptor column, bool aliased = false, string columnAlias = null, string tableAlias = null)
      {
         using (var sw = new StringWriter())
         {
            self.FormatColumn(sw, column, aliased, columnAlias, tableAlias);
            return sw.GetStringBuilder().ToString();
         }

      }
      public static ISqlFormatter FormatColumn(this ISqlFormatter self, TextWriter w, MemberInfo column, bool aliased = false, string columnAlias = null, string tableAlias = null)
      {
         if (self == null) throw new ArgumentNullException("self");
         if (w == null) throw new ArgumentNullException("w");
         if (column == null) throw new ArgumentNullException("column");
         return self.FormatColumn(w, column.GetColumnDescriptor(),aliased,columnAlias,tableAlias);
      }

      public static ISqlFormatter FormatColumn(this ISqlFormatter self, TextWriter w, Expression column, bool aliased = false, string columnAlias = null, string tableAlias = null)
      {
         if (self == null) throw new ArgumentNullException("self");
         if (w == null) throw new ArgumentNullException("w");
         if (column == null) throw new ArgumentNullException("column");
         return self.FormatColumn(w, column.GetMemberInfo(), aliased, columnAlias, tableAlias);
      }
   }
}