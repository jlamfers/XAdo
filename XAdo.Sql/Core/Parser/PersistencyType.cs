using System;
using System.Text;

namespace XAdo.Quobs.Core.Parser
{
   [Flags]
   public enum PersistencyType
   {
      None = 0,
      Default = 15, // all
      Create = 1,
      Read = 2,
      Update = 4,
      Delete = 8
   }
   
   public static class PersistencyTypeExtension
   {
      public static bool CanCreate(this PersistencyType self)
      {
         return self.HasFlag(PersistencyType.Create);
      }
      public static bool CanRead(this PersistencyType self)
      {
         return self.HasFlag(PersistencyType.Read);
      }
      public static bool CanUpdate(this PersistencyType self)
      {
         return self.HasFlag(PersistencyType.Update);
      }
      public static bool CanDelete(this PersistencyType self)
      {
         return self.HasFlag(PersistencyType.Delete);
      }

      public static bool CanCreate(this PersistencyType? self)
      {
         return self.GetValueOrDefault().CanCreate();
      }
      public static bool CanRead(this PersistencyType? self)
      {
         return self.GetValueOrDefault().CanRead();
      }
      public static bool CanUpdate(this PersistencyType? self)
      {
         return self.GetValueOrDefault().CanUpdate();
      }
      public static bool CanDelete(this PersistencyType? self)
      {
         return self.GetValueOrDefault().CanDelete();
      }

      public static PersistencyType ToPersistencyType(this string crud, PersistencyType? other = null)
      {
         var result = other.GetValueOrDefault();
         if (crud == null) return result;
         foreach (var ch in crud)
         {
            switch (char.ToUpper(ch))
            {
               case 'C':
                  result = result | PersistencyType.Create;
                  break;
               case 'U':
                  result = result | PersistencyType.Update;
                  break;
               case 'R':
                  result = result | PersistencyType.Read;
                  break;
               case 'D':
                  result = result | PersistencyType.Delete;
                  break;
            }
         }
         return result;
      }

      public static string ToStringEx(this PersistencyType self)
      {
         return new StringBuilder()
            .Append(self.CanCreate() ? "C" : "-")
            .Append(self.CanRead() ? "R" : "-")
            .Append(self.CanUpdate() ? "U" : "-")
            .Append(self.CanDelete() ? "D" : "-")
            .ToString();
         
      }

      
   }
}