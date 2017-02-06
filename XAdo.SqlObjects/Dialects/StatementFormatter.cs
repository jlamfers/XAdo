using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XAdo.SqlObjects.Dialects
{
   public class StatementFormatter
   {
      private readonly string _expression;
      private Action<TextWriter, Action<TextWriter>[]> _writer;

      public StatementFormatter(string expression)
      {
         _expression = expression;
      }

      public string Format(params object[] args)
      {
         return Format(args.Select(a => (a as Action<TextWriter>) ?? (x => x.Write(a))).ToArray());
      }
      public void Format(TextWriter w, params object[] args)
      {
         Format(w, args.Select(a => (a as Action<TextWriter>) ?? (x => x.Write(a))).ToArray());
      }
      public void Format(TextWriter w, params Action<TextWriter>[] args)
      {
         if (string.IsNullOrEmpty(_expression))
         {
            return;
         }
         _writer = _writer ?? Compile();
         _writer(w, args);
      }
      public string Format(params Action<TextWriter>[] args)
      {
         using (var sw = new StringWriter())
         {
            Format(sw, args);
            return sw.GetStringBuilder().ToString();
         }
      }

      Action<TextWriter, Action<TextWriter>[]> Compile()
      {
         var writers = new List<Action<TextWriter, Action<TextWriter>[]>>();
         var sb = new StringBuilder();
         var pch = '\0';
         for(var i = 0; i < _expression.Length;i++)
         {
            var ch = _expression[i];
            if (ch=='{')
            {
               if (pch == '{')
               {
                  pch = '\0';
                  sb.Append('{');
                  continue;
               }
               if (sb.Length > 0)
               {
                  var s = sb.ToString();
                  writers.Add((w,a) => w.Write(s));
               }
               sb.Length = 0;
            }
            else if (ch == '}')
            {
               if (pch == '}')
               {
                  pch = '\0';
                  sb.Append('}');
                  continue;
               }
               try
               {
                  var value = sb.ToString().Trim();
                  sb.Length = 0;
                  if (value.EndsWith("..."))
                  {
                     // e.g., {0,...}
                     var sep = value.Substring(value.Length - 4,1);
                     var startIndex = int.Parse(value.Substring(0, value.Length - 4));
                     writers.Add((w, a) =>
                     {
                        var s = "";
                        for (var j = startIndex; j < a.Length; j++)
                        {
                           w.Write(s);
                           a[j](w);
                           s = sep;
                        }
                     });
                     continue;
                  }
                  var index = int.Parse(value);
                  writers.Add((w, a) =>
                  {
                     try
                     {
                        a[index](w);
                     }
                     catch (IndexOutOfRangeException)
                     {
                        throw new FormatException("index " + index + " is out of arguments range in format string \"" + _expression+"\". params length: " + a.Length);
                     }
                  });
               }
               catch
               {
                  throw new FormatException("Expression has an invalid placeholder at position " + i+": \"" + _expression+"\". Note that format specifiers are not allowed.");
               }
               
            }
            else
            {
               sb.Append(ch);
            }
            pch = ch;
         }
         if (sb.Length > 0)
         {
            var s = sb.ToString();
            writers.Add((w, a) => w.Write(s));
         }
         var wa = writers.ToArray();
         return (w, a) =>
         {
            for (var i = 0; i < wa.Length; i++)
            {
               wa[i](w, a);
            }
         };
      }
   }

   public static class StringExtension
   {
      private static readonly ConcurrentDictionary<string, StatementFormatter>
         Cache = new ConcurrentDictionary<string, StatementFormatter>();

      public static void Format(this string format, TextWriter writer, params Action<TextWriter>[] args)
      {
         Cache.GetOrAdd(format, f => new StatementFormatter(f)).Format(writer, args);
      }
      public static string Format(this string format, params Action<TextWriter>[] args)
      {
         return Cache.GetOrAdd(format, f => new StatementFormatter(f)).Format(args);
      }
   }
}
