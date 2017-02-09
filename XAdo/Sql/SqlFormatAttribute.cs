using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using XAdo.Core;
using XAdo.Sql.Core;

namespace XAdo.Sql
{

   [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property,AllowMultiple = true)]
   public class SqlFormatAttribute : Attribute
   {
      private static readonly ConcurrentDictionary<Type,ISqlDialect>
         Cache = new ConcurrentDictionary<Type, ISqlDialect>();

      private readonly PropertyInfo 
         _formatProperty;
      private readonly string 
         _formatValue;

      public SqlFormatAttribute(string format)
         : this(format, null)
      {
      }

      public SqlFormatAttribute(string formatSpec, Type providerType)
      {
         if (formatSpec == null) throw new ArgumentNullException("formatSpec");

         if (formatSpec.Contains("=>"))
         {
            // syntactic sugar
            _formatProperty = (providerType ?? typeof(ISqlDialect)).GetProperty(formatSpec.Split('.').Last());

            if (_formatProperty == null)
            {
               throw new Exception("property could not be resolved on type " + (providerType ?? typeof(ISqlDialect)).Name + ": " + formatSpec);
            }
         }
         else
         {
            _formatValue = formatSpec;
         }

         if (providerType != null)
         {
            Provider = Cache.GetOrAdd(providerType, Activator.CreateInstance(providerType).CastTo<ISqlDialect>());
         }
      }

      public ISqlDialect Provider { get; private set; }

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
         if (Provider != null && !Provider.GetType().IsAssignableFrom(provider.GetType()))
         {
            throw new Exception("Invalid provider: this attribute is bound to " + Provider.GetType().Name+" and cannot be requested for provider " + provider.GetType().Name);
         }
         return _formatValue ?? (string) _formatProperty.GetValue(provider);
      }

   }
}
