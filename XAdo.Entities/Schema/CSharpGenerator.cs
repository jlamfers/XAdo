﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CSharp;

namespace XAdo.Quobs.Schema
{
   public class CSharpGenerator
   {
      private static readonly CodeDomProvider _codeDomProvider = new CSharpCodeProvider();

      private string _namespace;
      private string _prefix;
      private Regex _excludedTables;

      public virtual string Generate(string connectionString, string providerInvariantName, string @namespace,string prefix = "Db", Regex excludedTables = null)
      {
         using (var writer = new StringWriter())
         {
            Generate(writer, connectionString, providerInvariantName, @namespace, prefix, excludedTables);
            return writer.GetStringBuilder().ToString();
         }
      }

      public virtual void Generate(TextWriter writer, string connectionString, string providerInvariantName, string @namespace,string prefix = "Db", Regex excludedTables = null)
      {
         if (connectionString == null) throw new ArgumentNullException("connectionString");
         if (providerInvariantName == null) throw new ArgumentNullException("providerInvariantName");
         if (@namespace == null) throw new ArgumentNullException("namespace");
         _namespace = @namespace;
         _prefix = prefix;
         _excludedTables = excludedTables;
         var schema = new DbSchemaReader().Read(connectionString, providerInvariantName);
         Generate(writer,schema);
      }

      public virtual void Generate(TextWriter writer, DbSchema schema)
      {
         var namespaces = schema.Tables.SelectMany(t => t.Columns.Select(c => c.Type.Namespace)).Distinct().ToList();
         namespaces.Add("System.ComponentModel.DataAnnotations");
         namespaces.Add("XAdo.Quobs.Attributes");

         namespaces =
            namespaces.Where(n => n.StartsWith("System.") || n == "System")
               .OrderBy(x => x)
               .Concat(namespaces.Where(n => !(n.StartsWith("System.") || n == "System")).OrderBy(x => x))
               .ToList();

         var w = new IndentedTextWriter(writer, "   ");
         foreach (var ns in namespaces)
         {
            w.WriteLine("using {0};", ns);
         }
         w.WriteLine();
         w.WriteLine("namespace " + _namespace);
         w.WriteLine("{");
         w.Indent++;

         foreach (var t in schema.Tables.Where(t => _excludedTables == null || !_excludedTables.IsMatch(GetTableName(t))))
         {
            WriteTableAttributes(w, t);
            WriteTable(w, t);
         }
         w.WriteLine();
         WriteJoinExtension(w, schema);

         w.Indent--;
         w.WriteLine("}");
      }

      protected virtual void WriteTable(IndentedTextWriter w, TableSchemaItem t)
      {
         w.WriteLine("public partial class {0}{1}", _prefix,  NormalizeName(t.Name));
         w.WriteLine("{");
         w.Indent++;
         foreach (var c in t.Columns)
         {
            WriteColumnAttributes(w,c);
            WriteColumn(w,c);
         }
         w.Indent--;
         w.WriteLine("}");
      }

      protected virtual void WriteTableAttributes(IndentedTextWriter w, TableSchemaItem t)
      {
         var crud = t.IsView ? ", Crud=\"R\"" : "";
         w.WriteLine("[Table(TableName = \"{0}\"{1})]",GetTableName(t),crud);
         if (t.FKeyTables.Any())
         {
            w.WriteLine("[ReferencedBy(new []{{{0}}})]",string.Join(", ",t.FKeyTables.Select(x => string.Format("typeof({0}{1})",_prefix, NormalizeName(x.Name))).ToArray()));
         }
      }

      protected virtual void WriteColumn(IndentedTextWriter w, ColumnSchemaItem c)
      {
         var type = Nullable.GetUnderlyingType(c.Type) ?? c.Type;
         var typeName =  type.Name + (type.IsValueType ? "?" : "");
         w.WriteLine("public virtual {0} {1} {{get; set;}}",typeName, NormalizePropertyName(c));
      }

      protected virtual void WriteColumnAttributes(IndentedTextWriter w, ColumnSchemaItem c)
      {
         using (var sw = new StringWriter())
         {
            if (c.IsPkey)
            {
               sw.Write(", Key");
            }
            if (c.IsAutoIncrement)
            {
               sw.Write(", AutoIncrement");
            }
            if (c.IsUnique)
            {
               sw.Write(", Unique");
            }
            if (!c.IsNullable)
            {
               sw.Write(", Required");
            }
            if (c.MaxLength > 0)
            {
               sw.Write(", MaxLength({0})", c.MaxLength);
            }
            var normalizedName = NormalizePropertyName(c);
            if (normalizedName != c.Name)
            {
               sw.Write(", Column(ColumnName = \"{0}\")", GetColumnName(c));
            }
            var s = sw.ToString().TrimStart(',').TrimStart();
            if (s.Length > 0)
            {
               w.WriteLine("["+s+"]");
            }
         }
         if (c.References != null)
         {
            w.WriteLine("[References( Type=typeof({0}{1}), Member=\"{2}\", Column=\"{3}\", Constraint=\"{4}\")]",_prefix, NormalizeName(c.References.Table.Name),NormalizePropertyName(c.References),c.References.Name.Replace(".","\\\\."),c.FKey.FKeyConstrantName);
         }

      }

      protected virtual string NormalizeName(string dbname)
      {
         var normalized = dbname.Replace(" ", "_").Replace(".", "_").Replace("-", "_");
         if (!_codeDomProvider.IsValidIdentifier(normalized))
         {
            normalized = _codeDomProvider.CreateEscapedIdentifier("_" + normalized);
         }
         return normalized;
      }

      protected virtual string NormalizePropertyName(ColumnSchemaItem c)
      {
         var normalized = NormalizeName(c.Name);
         if (normalized == NormalizeName(c.TableName))
         {
            normalized = "_" + normalized;
         }
         return normalized;
      }

      protected virtual string GetTableName(TableSchemaItem t)
      {
         return t.Owner != null
            ? t.Owner.Replace(".", "\\\\.") + "." + t.Name.Replace(".", "\\\\.")
            : t.Name.Replace(".", "\\\\.");
      }

      protected virtual string GetColumnName(ColumnSchemaItem c)
      {
         return c.Name.Replace(".", "\\\\.");
      }

      protected virtual void WriteJoinExtension(IndentedTextWriter w, DbSchema schema)
      {
         w.WriteLine("public static partial class JoinExtension");
         w.WriteLine("{");
         w.Indent++;
         foreach (var t in schema.Tables)
         {
            var fkeys = t.FKeyColumns.OrderBy(k => k.FKey.FKeyConstrantName).ToArray();
            for(var i = 0; i < fkeys.Length; i++)
            {
               var cols = new List<ColumnSchemaItem>(new[] {fkeys[i]});
               while (i < fkeys.Length - 1 && fkeys[i].FKey.FKeyConstrantName == fkeys[i + 1].FKey.FKeyConstrantName)
               {
                  cols.Add(fkeys[++i]);
               }

               var name = cols[0].FKey.FKeyConstrantName;
               if (cols.Count == 1)
               {
                  name = ColumnToReferenceName(cols[0].Name) ?? cols[0].References.TableName;
               }
               else
               {
                  var names = cols.Select(c => ColumnToReferenceName(c.Name)).ToArray();
                  if (names.All(s => s != null))
                  {
                     name = string.Join("", names);
                  }
               }
               name = NormalizeName(name);
               //w.WriteLine("// " + cols[0].FKey);

               var expression = new StringBuilder("JOIN ");
               expression.Append(FormatFullTableName(cols[0].References.Table));
               expression.Append(" ON ");
               var and = "";
               foreach (var col in cols)
               {
                  expression.Append(and);
                  expression.Append(FormatColumnJoin(col));
                  and = " AND ";
               }
               w.WriteLine("[JoinMethod( Expression = @\"{0}\")]", expression);
               w.WriteLine("public static {0}{1} {2}(this {3}{4} self, JoinType join){{return null;}}",_prefix,NormalizeName(cols[0].References.TableName),name,_prefix,NormalizeName(cols[0].TableName));
               w.WriteLine("[JoinMethod( Expression = @\"{0}\")]", expression);
               w.WriteLine("public static {0}{1} {2}(this {3}{4} self){{return null;}}", _prefix, NormalizeName(cols[0].References.TableName), name, _prefix, NormalizeName(cols[0].TableName));

            }


         }
         w.Indent--;
         w.WriteLine("}");

      }

      protected virtual string FormatFullTableName(TableSchemaItem t)
      {
         return string.Format("[{0}].[{1}]", t.Owner, t.Name);
      }
      protected virtual string FormatColumnJoin(ColumnSchemaItem c)
      {
         return string.Format("[{0}].[{1}].[{2}] = [{3}].[{4}].[{5}]", 
            c.Table.Owner,
            c.TableName,
            c.Name,
            c.References.Table.Owner,
            c.References.TableName,
            c.References.Name
            );
      }

      protected virtual string ColumnToReferenceName(string name)
      {
         return name.EndsWith("Id", StringComparison.InvariantCultureIgnoreCase)
            ? name.Substring(0, name.Length - 2)
            : (name.EndsWith("Code", StringComparison.InvariantCultureIgnoreCase)
               ? name.Substring(0, name.Length - 4)
               : null);
      }
   }
}
