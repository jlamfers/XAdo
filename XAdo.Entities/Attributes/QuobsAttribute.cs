using System;

namespace XAdo.Quobs.Attributes
{
   public class QuobsAttribute : Attribute
   {
      private string _crud;

      public QuobsAttribute(string sqlExpression)
      {
         SqlExpression = sqlExpression;
         Crud = "CRUD";
      }

      public string SqlExpression { get; set; }

      public string Crud
      {
         get { return _crud; }
         set { _crud = (value ?? "").ToUpper(); }
      }

      public bool IsReadOnly
      {
         get { return Crud == "R"; }
      }
      public bool CanCreate
      {
         get { return Crud.Contains("C"); }
      }
      public bool CanRead
      {
         get { return Crud.Contains("R"); }
      }
      public bool CanUpdate
      {
         get { return Crud.Contains("U"); }
      }
      public bool CanDelete
      {
         get { return Crud.Contains("D"); }
      }

   }

   public class PKeyAttribute : Attribute { }
}
