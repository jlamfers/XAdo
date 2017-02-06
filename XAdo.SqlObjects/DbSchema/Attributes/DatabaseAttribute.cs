using System;

namespace XAdo.SqlObjects.DbSchema.Attributes
{
   [AttributeUsage(AttributeTargets.Class)]
   public class DatabaseAttribute : Attribute
   {
      private string _name;

      public DatabaseAttribute(string name)
      {
         _name = name;
      }

      public virtual string Name
      {
         get { return _name; }
         protected set { _name = value; }
      }
   }

}
