using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XAdo.Core;
using XAdo.Core.Interface;

namespace XAdo.Quobs.Core
{
   public class CodeBuilder
   {
      private readonly IXAdoDbSession _session;

      public CodeBuilder(IXAdoDbSession session)
      {
         _session = session;
      }

      public string Generate(string sql, string @namespace)
      {
         using (var sw = new StringWriter())
         {
            Generate(sw, sql, @namespace);
            return sw.GetStringBuilder().ToString();
         }
      }

      public void Generate(TextWriter writer, string sql, string @namespace)
      {
         var sqlResource = _session.GetSqlResource(sql);
         var type = sqlResource.GetEntityType(_session);
         using (var w = new IndentedTextWriter(writer))
         {
            w.WriteLine("using System;");
            if (@namespace != null) { 
               w.WriteLine("namespace " + @namespace);
               w.WriteLine("{");
               w.Indent += 1;
            }

            Generate(w,type,sqlResource.Table.TableName);

            if (@namespace != null)
            {
               w.Indent -= 1;
               w.WriteLine("}");
            }
         }
      }

      public void Generate(IndentedTextWriter w, Type type, string name)
      {
         if (type.Namespace != null && (type.Namespace == "System" || type.Namespace.StartsWith("System.")))
         {
            return;
         }

         w.WriteLine("public partial class " + name);
         w.WriteLine("{");
         w.Indent += 1;
         foreach (var props in type.GetProperties().Where(p => p.PropertyType.IsScalarType()))
         {
            w.WriteLine("public {0} {1} {{ get; set; }}", GetTypeName(props.PropertyType), props.Name);
         }
         foreach (var props in type.GetProperties().Where(p => !p.PropertyType.IsScalarType()))
         {
            w.WriteLine("public {0} {1} {{ get; set; }}", props.Name, props.Name);
         }
         w.Indent -= 1;
         w.WriteLine("}");

         var handled = new HashSet<Type>();
         foreach (var p in type.GetProperties().Where(p => !p.PropertyType.IsScalarType()))
         {
            if (handled.Contains(p.PropertyType)) continue;
            Generate(w,p.PropertyType,p.Name);
            handled.Add(p.PropertyType);
         }
      }

      private static string GetTypeName(Type type)
      {
         if (type.IsArray)
         {
            return GetTypeName(type.GetElementType()) + "[]";
         }
         if (type.IsNullable())
         {
            return GetTypeName(type.EnsureNotNullable()) + "?";
         }
         if (type.IsGenericType)
         {
            return type.Name.Split('`')[0] + "<" + string.Join(",", type.GetGenericArguments().Select(GetTypeName).ToArray()) + ">";
         }
         return type.Name;
      }
   }
}
