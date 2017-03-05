using System;
using System.Linq;
using System.Reflection;
using XAdo.Quobs.Core.Interface;

namespace XAdo.Quobs.Core
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
            // syntactic sugar with dialect reference
            _formatProperty = typeof(ISqlDialect).GetProperty(formatSpec.Split('.').Last().Trim());

            if (_formatProperty == null)
            {
               throw new QuobException("property could not be resolved on type " + typeof(ISqlDialect).Name + ": " + formatSpec);
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
         if (!IsDialectReference)
         {
            throw new QuobException("This attribute instance needs a ISqlDialect instance for being able to resolve the property " + _formatProperty);
         }
         return _formatValue;
      }
      public string GetFormat(ISqlDialect provider)
      {
         if (ProviderName != null && ProviderName != provider.ProviderName)
         {
            throw new QuobException("Invalid provider: this attribute is bound to " + ProviderName + " and cannot be requested for provider " + provider.ProviderName);
         }
         return _formatValue ?? (string) _formatProperty.GetValue(provider);
      }
      public int Order { get; set; }

      public bool IsDialectReference
      {
         get { return _formatProperty != null; }
      }

   }

   public static class SqlFormatAttributeExtensions
   {
      public static SqlFormatAttribute[] GetSqlFormatAttributes(this MemberInfo self, string providerName)
      {
         var annotations = self.GetAnnotations<SqlFormatAttribute>().ToArray();
         var probe = annotations.Where(a => a.ProviderName.EqualsOrdinalIgnoreCase(providerName)).ToArray();
         if (probe.Any())
         {
            return
               probe.Concat(annotations.Where(a => a.ProviderName == null && a.IsDialectReference))
                  .OrderBy(a => a.Order)
                  .ToArray();
         }
         return annotations.Where(a => a.IsDialectReference || a.ProviderName == null).OrderBy(a => a.Order).ToArray();

      }

      public static SqlFormatAttribute[] GetSqlFormatAttributes(this MemberInfo self, ISqlDialect dialect)
      {
         return self.GetSqlFormatAttributes(dialect.ProviderName);
      }
   }
}
