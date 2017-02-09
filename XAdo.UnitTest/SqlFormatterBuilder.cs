using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace XAdo.UnitTest
{
   public static class SqlFormatterBuilder
   {
      private static readonly ConcurrentDictionary<Tuple<string,Type>,object>
         _cache = new ConcurrentDictionary<Tuple<string, Type>, object>();

      public static string FormatSql<T>(this string template, T args)
      {
         var formatter = (Action<TextWriter, T>)_cache.GetOrAdd(Tuple.Create(template, typeof (T)), t => BuildFormatter<T>(t.Item1));
         using (var sw = new StringWriter())
         {
            formatter(sw, args);
            return sw.GetStringBuilder().ToString();
         }

      }
      public static Action<TextWriter, T> BuildFormatter<T>(string source)
      {
         var writers = new List<Action<TextWriter, T>>();

         var lines = source.Split('\n').Select(s => s.TrimEnd('\r'));
         var sb = new StringBuilder();
         foreach (var item in lines)
         {
            var prefix = "";
            var line = item;
            var idx = line.IndexOf("--$");
            if (idx == -1)
            {
               sb.AppendLine(line);
               continue;
            }
            if (idx > 0)
            {
               prefix = line.Substring(0, idx);
               if (!string.IsNullOrWhiteSpace(prefix))
               {
                  sb.Append(prefix);
                  prefix = Environment.NewLine;
               }
               line = line.Substring(idx);
            }
            var s = sb.ToString();
            sb.Length = 0;
            writers.Add((w, e) => w.Write(s));

            var formatstring = line.Substring(3);
            List<Func<T, object>> args;
            if (TryTransformFormatString(ref formatstring, out args))
            {
               var fs = formatstring;
               var a = args.ToArray();
               var pf = prefix;
               if (pf == Environment.NewLine)
               {
                  pf = "";
               }
               writers.Add(
                  (w, e) =>
                  {
                     var @params = a.Select(x => x(e)).ToArray();
                     if (@params.All(x => x != null))
                     {
                        w.Write(pf);
                        w.WriteLine(fs, @params);
                     }
                  });
            }
            else if (prefix == Environment.NewLine)
            {
               writers.Add((w, e) => w.WriteLine());
            }
         }
         if (sb.Length > 0)
         {
            var s = sb.ToString();
            writers.Add((w, e) => w.Write(s));
         }
         var wa = writers.ToArray();
         return (w, target) =>
         {
            foreach (var e in wa)
            {
               e(w, target);
            }
         };
      }

      private static readonly Regex _regex = new Regex(@"\{[^\}]*\}",RegexOptions.Compiled);

      private static bool TryTransformFormatString<T>(ref string format, out List<Func<T, object>> arguments)
      {
         arguments = new List<Func<T, object>>();
         var matches = _regex.Matches(format);
         var workerIndex = 0;
         var placeholderindex = 0;
         var sb = new StringBuilder();
         foreach (Match arg in matches)
         {
            var name = arg.Value.Substring(1, arg.Value.Length - 2);
            var fragment = format.Substring(workerIndex, arg.Index - workerIndex);
            workerIndex = (arg.Index + arg.Length);
            sb.Append(fragment);
            if (!name.StartsWith("?"))
            {
               sb
                  .Append("{")
                  .Append(placeholderindex++)
                  .Append("}");
            }
            else
            {
               placeholderindex++;
               name = name.Substring(1);
            }

            var members = typeof(T).GetMember(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            if (!members.Any())
            {
               return false;
            }
            if (members.Count() > 1)
            {
               throw new Exception("Ambigious member for type " + typeof(T).Name + ": " + arg.Value);
            }
            var m = members[0];
            if (m.MemberType == MemberTypes.Field)
            {
               var f = (FieldInfo)m;
               arguments.Add(obj => f.GetValue(obj));
            }
            else
            {
               var p = (PropertyInfo)m;
               arguments.Add(obj => p.GetValue(obj));
            }
         }
         if (workerIndex < format.Length)
         {
            sb.Append(format.Substring(workerIndex));
         }
         format = sb.ToString();
         return true;
      }

   }
}
