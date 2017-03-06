using System;
using System.IO;
using XAdo.Quobs.Core.Interface;
using XAdo.Quobs.Core.Parser.Partials;

namespace XAdo.Quobs.Core.Impl
{
   public class SqlBuilderImpl : ISqlBuilder
   {
      public string BuildSelect(ISqlResource sqlResource, bool throwException = true)
      {
         using (var w = new StringWriter())
         {
            foreach (var partial in sqlResource.Partials)
            {
               partial.WriteAsTemplate(w);
            }
            return w.GetStringBuilder().ToString();
         }
      }

      public string BuildUpdate(ISqlResource sqlResource, bool throwException = true)
      {
         throw new NotImplementedException();
      }

      public string BuildDelete(ISqlResource sqlResource, bool throwException = true)
      {
         throw new NotImplementedException();
      }

      public string BuildInsert(ISqlResource sqlResource, bool throwException = true)
      {
         throw new NotImplementedException();
      }
   }

   internal static class HelperExtensions
   {
      public static void WriteAsTemplate(this SqlPartial self, TextWriter w)
      {
         if (self != null)
         {
            self.Write(w);
            w.WriteLine();
         }
      }
   }
}
