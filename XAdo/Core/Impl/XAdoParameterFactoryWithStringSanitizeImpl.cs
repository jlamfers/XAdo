using System.Data;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
   public class XAdoParameterFactoryWithStringSanitizeImpl : XAdoParameterFactoryImpl
   {
      public XAdoParameterFactoryWithStringSanitizeImpl(IXAdoClassBinder classBinder) : base(classBinder)
      {
      }

      protected override void OnParameterCreated(IXAdoParameter parameter)
      {
         if (!IsString(parameter))
            return;

         SanitizeStringParameter(parameter);
      }

      protected virtual bool IsString(IXAdoParameter p)
      {
         return p.Value is string || (p.DbType != null && p.DbType.ToString().Contains("String"));

      }

      protected virtual void SanitizeStringParameter(IXAdoParameter p)
      {
         const int defaultLength = 4000;
         var length = p.Size;

         var isAnsi = p.DbType.HasValue && (p.DbType == DbType.AnsiString || p.DbType == DbType.AnsiStringFixedLength);
         if (!length.HasValue && p.Value != null && ((string)p.Value).Length <= defaultLength)
         {
            p.Size = defaultLength;
         }
         p.DbType = isAnsi
            ? (length.HasValue ? DbType.AnsiStringFixedLength : DbType.AnsiString)
            : (length.HasValue ? DbType.StringFixedLength : DbType.String);
      }

   }

   public partial class Extensions
   {
      public static IXAdoContextInitializer EnableAutoStringSanitize(this IXAdoContextInitializer self)
      {
         self.BindSingleton<IXAdoParameterFactory, XAdoParameterFactoryWithStringSanitizeImpl>();
         return self;
      }
   }
}