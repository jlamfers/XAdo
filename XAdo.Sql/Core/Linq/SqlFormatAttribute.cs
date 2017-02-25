﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using XAdo.Sql.Core.Common;
using XAdo.Sql.Dialects;

namespace XAdo.Sql.Core.Linq
{
   //todo: add providername??

   [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property,AllowMultiple = true)]
   public class SqlFormatAttribute : Attribute
   {

      private readonly PropertyInfo 
         _formatProperty;
      private readonly string 
         _formatValue;

      public SqlFormatAttribute(string format)
         : this(format, null)
      {
      }

      public SqlFormatAttribute(string formatSpec, string providerName)
      {
         if (formatSpec == null) throw new ArgumentNullException("formatSpec");

         if (formatSpec.Contains("=>"))
         {
            // syntactic sugar
            _formatProperty = typeof(ISqlDialect).GetProperty(formatSpec.Split('.').Last());

            if (_formatProperty == null)
            {
               throw new Exception("property could not be resolved on type " + typeof(ISqlDialect).Name + ": " + formatSpec);
            }
         }
         else
         {
            _formatValue = formatSpec;
         }

         ProviderName = providerName;
      }

      public string ProviderName { get; private set; }

      public bool IncludeGenericParameters { get; set; }

      public string GetFormat()
      {
         if (_formatValue == null)
         {
            throw new InvalidOperationException("This attribute instance needs a ISqlDialect instance for being able to resolve the property " + _formatProperty);
         }
         return _formatValue;
      }
      public string GetFormat(ISqlDialect provider)
      {
         if (ProviderName != null && ProviderName != provider.ProviderName)
         {
            throw new Exception("Invalid provider: this attribute is bound to " + ProviderName + " and cannot be requested for provider " + provider.ProviderName);
         }
         return _formatValue ?? (string) _formatProperty.GetValue(provider);
      }
      public int Order { get; set; }

   }

   public static class SqlFormatAttributeExtensions
   {
      public static SqlFormatAttribute[] GetSqlFormatAttributes(this MemberInfo self, string providerName)
      {
         return self.GetAnnotations<SqlFormatAttribute>().Where(a => a.ProviderName == null || a.ProviderName == providerName).ToArray();
      }

      public static SqlFormatAttribute[] GetSqlFormatAttributes(this MemberInfo self, ISqlDialect dialect)
      {
         return self.GetSqlFormatAttributes(dialect.ProviderName);
      }
   }
}
