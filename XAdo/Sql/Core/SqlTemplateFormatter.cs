using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace XAdo.Sql.Core
{
   public static class SqlTemplateFormatter
   {

      private static readonly Regex 

         // finds anything between curly braces {...}
         PlaceholderRegex = new Regex(@"\{[^\}]*\}", RegexOptions.Compiled),

         // finds anything that starts with --$ until end of line
         TemplateRegex = new Regex(@"\-\-\s?\$.*(\r\n?|\n)", RegexOptions.Compiled | RegexOptions.Multiline);

      private static readonly LRUCache<Tuple<string,Type>,object>
         _cache = new LRUCache<Tuple<string, Type>, object>();

      public static string FormatSqlTemplate<T>(this string template, T args)
      {
         var formatter = (Action<TextWriter, T>)_cache.GetOrAdd(Tuple.Create(template, typeof (T)), t => BuildTemplateFormatter<T>(t.Item1));
         using (var sw = new StringWriter())
         {
            formatter(sw, args);
            return sw.GetStringBuilder().ToString();
         }

      }
      private static Action<TextWriter, TArgs> BuildTemplateFormatter<TArgs>(string source)
      {
         if (source == null) throw new ArgumentNullException("source");

         // the compiled formatter has an internal array of seperate writers which all are invoked 
         // each time the compiled formatter is invoked
         var writers = new List<Action<TextWriter, TArgs>>();

         var templateMatches = TemplateRegex.Matches(source);
         var index = 0;

         foreach (Match match in templateMatches)
         {
            var literal = source.Substring(index, match.Index - index);
            if (literal.Length > 0)
            {
               writers.Add((w,a) => w.Write(literal));
            }
            index = match.Index + match.Value.Length;
            var formatString = match.Value.Substring(match.Value.IndexOf('$')+1); // strip --$
            List<Func<TArgs, object>> arguments;
            if (TryTransformFormatString(ref formatString, out arguments))
            {
               writers.Add(
                  (w, e) =>
                  {
                     var args = arguments.Select(x => x(e)).ToArray();
                     if (args.All(x => x != null))
                     {
                        w.WriteLine(formatString, args);
                     }
                  });
            }
         }
         if (index < source.Length-1)
         {
            var literal = source.Substring(index);
            writers.Add((w, a) => w.Write(literal));
         }

         var writerArray = writers.ToArray();
         return (w, a) =>
         {
            foreach (var writer in writerArray)
            {
               writer(w, a);
            }
         };
      }

      private static bool TryTransformFormatString<T>(ref string formatstring, out List<Func<T, object>> arguments)
      {
         arguments = new List<Func<T, object>>();
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

            var members = typeof(T).GetMember(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (!members.Any())
            {
               if (notExists)
               {
                  arguments.Add(obj => new object());
                  return true;
               }
               return false;
            }
            if (members.Count() > 1)
            {
               throw new Exception("Ambigious member for type " + typeof(T).Name + ": " + match.Value);
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
                  arguments.Add(obj => f.GetValue(obj));
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
                  arguments.Add(obj => p.GetValue(obj));
               }
            }
         }
         if (index < formatstring.Length)
         {
            sb.Append(formatstring.Substring(index));
         }
         formatstring = sb.ToString();
         return true;
      }

   }
}
