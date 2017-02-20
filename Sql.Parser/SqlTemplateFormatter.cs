using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Sql.Parser
{
   public static class SqlTemplateFormatter
   {

      private static readonly ConcurrentDictionary<Tuple<string,Type>,Tuple<string,Func<object, object>[]>>
         Cache = new ConcurrentDictionary<Tuple<string, Type>, Tuple<string, Func<object, object>[]>>();

      private static readonly Regex
         // finds anything between curly braces {...}
         PlaceholderRegex = new Regex(@"\{[^\}]*\}", RegexOptions.Compiled);

      public static string FormatTemplate(this string template, object argumentsObject)
      {
         if (string.IsNullOrEmpty(template))
         {
            return null;
         }

         argumentsObject = argumentsObject ?? new object();

         var tuple = Cache.GetOrAdd(Tuple.Create(template, argumentsObject.GetType()), t =>
         {
            List<Func<object, object>> argumentsList;
            var f = TryTransformFormatString(t.Item1, t.Item2, out argumentsList);
            return Tuple.Create(f, argumentsList.ToArray());
         });
         var format = tuple.Item1;
         if (format == null) return null;
         var args = tuple.Item2.Select(x => x(argumentsObject)).ToArray();
         return string.Format(format, args);
      }

      private static string TryTransformFormatString(string formatstring, Type argType, out List<Func<object, object>> arguments)
      {
         arguments = new List<Func<object, object>>();
         var placeholderMatches = PlaceholderRegex.Matches(formatstring);
         var index = 0;
         var placeholderindex = 0;
         var sb = new StringBuilder();
         foreach (Match match in placeholderMatches)
         {
            var name = match.Value.Substring(1, match.Value.Length - 2);
            var literal = formatstring.Substring(index, match.Index - index);
            index = (match.Index + match.Length);
            sb.Append(literal);
            var notExists = false;
            if (name.StartsWith("?"))
            {
               // exists operator, do not create a placeholder
               // trim operator from member name
               placeholderindex++;
               name = name.Substring(1);
            }
            else if (name.StartsWith("!"))
            {
               notExists = true;
               // not exists operator, do not create a placeholder
               // trim operator from member name
               placeholderindex++;
               name = name.Substring(1);
            }
            else
            {
               sb
                  .Append("{")
                  .Append(placeholderindex++)
                  .Append("}");
            }

            var members = argType.GetMember(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (!members.Any())
            {
               if (notExists)
               {
                  arguments.Add(obj => new object());
                  return sb.ToString();
               }
               return null;
            }
            if (members.Count() > 1)
            {
               throw new Exception("Ambigious member for type " + argType.Name + ": " + match.Value);
            }
            var m = members[0];
            if (m.MemberType == MemberTypes.Field)
            {
               var f = (FieldInfo) m;
               if (notExists)
               {
                  arguments.Add(obj => f.GetValue(obj) == null ? new object() : null);
               }
               else
               {
                  arguments.Add(f.GetValue);
               }
            }
            else
            {
               var p = (PropertyInfo) m;
               if (notExists)
               {
                  arguments.Add(obj => p.GetValue(obj) == null ? new object() : null);
               }
               else
               {
                  arguments.Add(p.GetValue);
               }
            }
         }
         if (index < formatstring.Length)
         {
            sb.Append(formatstring.Substring(index));
         }
         formatstring = sb.ToString();
         return formatstring;
      }

   }
}
