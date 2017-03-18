namespace XAdo.Core.Interface
{
   public interface ISqlTemplateFormatter
   {
      string Format(string template, object argumentsObject);
   }
}