﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CSharp;
using XAdo.Quobs.Core.DbSchema.Attributes;

namespace XAdo.Quobs.Core.DbSchema.Generator
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
         namespaces.Add("System.ComponentModel.DataAnnotations.Schema");
         namespaces.Add(typeof(DbViewAttribute).Namespace);

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
         w.WriteLine("public abstract partial class DbBaseTable {}");
         w.WriteLine();

         foreach (var t in schema.Tables.Where(t => _excludedTables == null || !_excludedTables.IsMatch(t.Name)))
         {
            WriteTableAttributes(w, t);
            WriteTable(w, t);
         }
         w.WriteLine();
         WriteJoinExtension(w, schema);

         w.Indent--;
         w.WriteLine("}");
      }

      protected virtual void WriteTable(IndentedTextWriter w, DbTableItem t)
      {
         w.WriteLine("public partial class {0}{1} : DbBaseTable", _prefix, NormalizeName(t.Name));
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

      protected virtual void WriteTableAttributes(IndentedTextWriter w, DbTableItem t)
      {
         var schema = t.Owner != null ? string.Format(", Schema=\"{0}\"",t.Owner) : "";
         var view = t.IsView ? ", DbView" : "";
         w.WriteLine("[Table(\"{0}\"{1}){2}]", t.Name, schema, view);
         if (t.FKeyTables.Any())
         {
            w.WriteLine("[ReferencedBy(new []{{{0}}})]",string.Join(", ",t.FKeyTables.Select(x => string.Format("typeof({0}{1})",_prefix, NormalizeName(x.Name))).ToArray()));
         }
      }

      protected virtual void WriteColumn(IndentedTextWriter w, DbColumnItem c)
      {
         var type = Nullable.GetUnderlyingType(c.Type) ?? c.Type;
         var typeName =  type.Name + (type.IsValueType ? "?" : "");
         w.WriteLine("public virtual {0} {1} {{get; set;}}",typeName, NormalizePropertyName(c));
      }

      protected virtual void WriteColumnAttributes(IndentedTextWriter w, DbColumnItem c)
      {
         using (var sw = new StringWriter())
         {
            if (c.IsPkey)
            {
               sw.Write(", Key");
            }
            if (c.IsAutoIncrement)
            {
               sw.Write(", DbAutoIncrement");
            }
            if (c.IsUnique)
            {
               sw.Write(", DbUnique");
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
               sw.Write(", Column(\"{0}\")", c.Name);
            }
            var s = sw.ToString().TrimStart(',').TrimStart();
            if (s.Length > 0)
            {
               w.WriteLine("["+s+"]");
            }
         }
         if (c.References != null)
         {
            w.WriteLine("[References( Type=typeof({0}{1}), MemberName=\"{2}\", ColumnName=\"{3}\", FKeyName=\"{4}\")]",_prefix, NormalizeName(c.References.Table.Name),NormalizePropertyName(c.References),c.References.Name,c.FKey.FKeyConstraintName);
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

      protected virtual string NormalizePropertyName(DbColumnItem c)
      {
         var normalized = NormalizeName(c.Name);
         if (normalized == (_prefix ??"")+ NormalizeName(c.TableName))
         {
            normalized = "_" + normalized;
         }
         return normalized;
      }

      protected virtual void WriteJoinExtension(IndentedTextWriter w, DbSchema schema)
      {
         w.WriteLine("public static partial class JoinExtension");
         w.WriteLine("{");
         w.Indent++;
         foreach (var t in schema.Tables)
         {
            var fkeys = t.FKeyColumns.OrderBy(k => k.FKey.FKeyConstraintName).ToArray();
            for(var i = 0; i < fkeys.Length; i++)
            {
               var cols = new List<DbColumnItem>(new[] {fkeys[i]});
               while (i < fkeys.Length - 1 && fkeys[i].FKey.FKeyConstraintName == fkeys[i + 1].FKey.FKeyConstraintName)
               {
                  cols.Add(fkeys[++i]);
               }

               var name = cols[0].FKey.FKeyConstraintName;
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
               

               var expression = new StringBuilder("JOIN ");
               var tableNamesExpression =
                  string.Format(
                     @"LeftTableType=typeof({0}{1}), RightTableType=typeof({0}{2})",
                       _prefix,
                        NormalizeName(cols[0].References.Table.Name),
                        NormalizeName(cols[0].Table.Name)
                     );

               expression.Append(FormatFullTableName(cols[0].References.Table));
               expression.Append(" ON ");
               var and = "";
               foreach (var col in cols)
               {
                  expression.Append(and);
                  expression.Append(FormatColumnJoin(col));
                  and = " AND ";
               }
               w.WriteLine("[JoinMethod( Expression = @\"{0}\", {1}, FKeyName=\"{2}\")]", expression, tableNamesExpression, fkeys[i].FKey.FKeyConstraintName);
               w.WriteLine("public static {0}{1} {2}(this {3}{4} self, JoinType join){{return null;}}",_prefix,NormalizeName(cols[0].References.TableName),name,_prefix,NormalizeName(cols[0].TableName));
               w.WriteLine("[JoinMethod( Expression = @\"{0}\", {1}, FKeyName=\"{2}\")]", expression, tableNamesExpression, fkeys[i].FKey.FKeyConstraintName);
               w.WriteLine("public static {0}{1} {2}(this {3}{4} self){{return null;}}", _prefix, NormalizeName(cols[0].References.TableName), name, _prefix, NormalizeName(cols[0].TableName));

            }
         }
         w.WriteLine("// joins for child relations");
         WriteJoinExtensionChilds(w,schema);
         w.Indent--;
         w.WriteLine("}");

      }

      protected virtual void WriteJoinExtensionChilds(IndentedTextWriter w, DbSchema schema)
      {
         foreach (var t1 in schema.Tables)
         {
            foreach (var t in t1.FKeyTables)
            {
               var fkeys = t.FKeyColumns.Where(c => c.References.Table==t1).OrderBy(k => k.FKey.FKeyConstraintName).ToArray();
               for (var i = 0; i < fkeys.Length; i++)
               {
                  var cols = new List<DbColumnItem>(new[] {fkeys[i]});
                  while (i < fkeys.Length - 1 && fkeys[i].FKey.FKeyConstraintName == fkeys[i + 1].FKey.FKeyConstraintName)
                  {
                     cols.Add(fkeys[++i]);
                  }
                  var y = t1;
                  var oneRelationToThisTable = t.FKeyColumns.Where(c => c.References.Table == y).Select(c => c.FKey.FKeyConstraintName).Count() == 1;

                  var name = cols[0].FKey.FKeyConstraintName;
                  if (oneRelationToThisTable)
                  {
                     name = cols[0].TableName;
                  }
                  else if (cols.Count == 1)
                  {
                     name = cols[0].TableName + ColumnToReferenceName(cols[0].Name);
                  }
                  else
                  {
                     var names = cols.Select(c => ColumnToReferenceName(c.Name)).ToArray();
                     name = cols[0].TableName + string.Join("", names);
                  }
                  name = NormalizeName(name + "_N");


                  var expression = new StringBuilder("JOIN ");
                  var tableNamesExpression =
                     string.Format(
                        @"LeftTableType=typeof({0}{1}), RightTableType=typeof({0}{2})",
                          _prefix,
                           NormalizeName(cols[0].Table.Name),
                           NormalizeName(cols[0].References.Table.Name)
                        );

                  expression.Append(FormatFullTableName(cols[0].Table));
                  expression.Append(" ON ");
                  var and = "";
                  foreach (var col in cols)
                  {
                     expression.Append(and);
                     expression.Append(FormatColumnJoinChild(col));
                     and = " AND ";
                  }
                  w.WriteLine("[JoinMethod( Expression = @\"{0}\", NChilds=true, {1}, FKeyName=\"{2}\")]", expression, tableNamesExpression, fkeys[i].FKey.FKeyConstraintName);
                  w.WriteLine("public static {0}{1} {2}(this {3}{4} self, JoinType join){{return null;}}", _prefix,
                     NormalizeName(cols[0].TableName), name, _prefix, NormalizeName(cols[0].References.TableName));
                  w.WriteLine("[JoinMethod( Expression = @\"{0}\", NChilds=true, {1}, FKeyName=\"{2}\")]", expression, tableNamesExpression, fkeys[i].FKey.FKeyConstraintName);
                  w.WriteLine("public static {0}{1} {2}(this {3}{4} self){{return null;}}", _prefix,
                     NormalizeName(cols[0].TableName), name, _prefix, NormalizeName(cols[0].References.TableName));

               }
            }


         }
      }

      protected virtual string FormatFullTableName(DbTableItem t)
      {
         return string.Format("[{0}].[{1}]", t.Owner, t.Name);
      }
      protected virtual string FormatColumnJoin(DbColumnItem c)
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
      protected virtual string FormatColumnJoinChild(DbColumnItem c)
      {
         return string.Format("[{0}].[{1}].[{2}] = [{3}].[{4}].[{5}]",
            c.References.Table.Owner,
            c.References.TableName,
            c.References.Name,
            c.Table.Owner,
            c.TableName,
            c.Name
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