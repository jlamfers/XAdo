using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
   public class SqlTemplateFormatterImpl : ISqlTemplateFormatter
   {

      private static readonly Regex

         // finds anything between curly braces {...}
         PlaceholderRegex = new Regex(@"\{[^\}]*\}", RegexOptions.Compiled),

         // finds anything that starts with --$ until end of line
         TemplateRegex = new Regex(@"\-\-\s?\$.*(\r\n?|\n)", RegexOptions.Compiled | RegexOptions.Multiline);

      private static readonly LRUCache<Tuple<string, Type>, object>
         Cache = new LRUCache<Tuple<string, Type>, object>("LRUCache.SqlTemplates.Size",1000);

      public string Format(string template, object args)
      {
         args = args ?? new object();
         var formatter = (Action<TextWriter, object>)Cache.GetOrAdd(Tuple.Create(template, args.GetType()), t => BuildTemplateFormatter(t.Item1,args.GetType()));
         using (var sw = new StringWriter())
         {
            formatter(sw, args);
            return sw.GetStringBuilder().ToString();
         }

      }
      private static Action<TextWriter, object> BuildTemplateFormatter(string source, Type argType)
      {
         if (source == null) throw new ArgumentNullException("source");

         // the compiled formatter has an internal array of seperate writers which all are invoked 
         // each time the compiled formatter is invoked
         var writers = new List<Action<TextWriter, object>>();

         var templateMatches = TemplateRegex.Matches(source);
         var index = 0;

         foreach (Match match in templateMatches)
         {
            var literal = source.Substring(index, match.Index - index);
            if (literal.Length > 0)
            {
               writers.Add((w, a) => w.Write(literal));
            }
            index = match.Index + match.Value.Length;
            var formatString = match.Value.Substring(match.Value.IndexOf('$') + 1); // strip "--$" or "-- $"
            List<Func<object, object>> arguments;
            var normalizedFormatString = TryTransformFormatString(formatString, argType, out arguments);
            if (!string.IsNullOrEmpty(normalizedFormatString))
            {
               writers.Add(
                  (w, e) =>
                  {
                     var args = new object[arguments.Count];
                     for (var i = 0; i < args.Length; i++)
                     {
                        if ( (args[i]=arguments[i](e)) == null) return;
                     }
                     w.WriteLine(normalizedFormatString, args);
                  });
            }
         }
         if (index < source.Length - 1)
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
                  continue;
               }
               return null;
            }
            if (members.Count() > 1)
            {
               throw new XAdoException("Ambigious member for type " + argType.Name + ": " + match.Value);
            }
            var m = members[0];
            if (m.MemberType == MemberTypes.Field)
            {
               var f = (FieldInfo)m;
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
               var p = (PropertyInfo)m;
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
