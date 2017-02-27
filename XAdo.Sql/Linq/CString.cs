using System;

namespace XAdo.Quobs.Linq
{

   /// <summary>
   /// CString is a comparable string, and behaves like the native string type.
   /// The difference with System.String is that with the CString type you can compare strings using all compare operators 
   /// <![CDATA[ such as <, <=, >, >= ]]>
   /// Note that this type is mainly meant to be compiled into SQL. If it is evaluated in C# code, 
   /// the C# comparison is forwarded to string.CompareOrdinal(...)
   /// </summary>
   public class CString
   {
      public CString()
      {
      }

      public CString(string value)
      {
         Value = value;
      }

      public string Value { get; private set; }

      public static implicit operator string(CString value)
      {
         return value == null ? null : value.Value;
      }
      public static implicit operator CString(string value)
      {
         return value == null ? null : new CString(value);
      }

      public static bool operator <(CString left, CString right)
      {
         if (left == null) return right != null;
         if (right == null) return false;
         return Compare(left.Value, right.Value) < 0;
      }
      public static bool operator <=(CString left, CString right)
      {
         if (left == null) return true;
         if (right == null) return false;
         return Compare(left.Value, right.Value) <= 0;
      }

      public static bool operator >(CString left, CString right)
      {
         if (right == null) return left != null;
         if (left == null) return false;
         return Compare(left.Value, right.Value) > 0;
      }

      public static bool operator >=(CString left, CString right)
      {
         if (right == null) return true;
         if (left == null) return false;
         return Compare(left.Value, right.Value) >= 0;
      }

      public static bool operator ==(CString left, CString right)
      {
         if (right == null || left == null) return left == null && right == null;
         return Compare(left.Value, right.Value) == 0;
      }

      public static bool operator !=(CString left, CString right)
      {
         return !(left == right);
      }

      public override bool Equals(object obj)
      {
         var otherAsString = obj as string;
         if (otherAsString != null) return Compare(Value, otherAsString) == 0;
         var other = obj as CString;
         if (other != null) return Compare(Value, other.Value) == 0;
         return false;
      }

      public override int GetHashCode()
      {
         return Value == null ? 0 : Value.GetHashCode();
      }

      public override string ToString()
      {
         return Value ?? string.Empty;
      }

      protected static int Compare(string left, string right)
      {
         return string.Compare(left, right,StringComparison.OrdinalIgnoreCase);
      }
   }

   public static class CStringExtensions
   {
      [SqlFormat("{0}")]
      public static CString AsComparable(this string self)
      {
         return new CString(self);
      }
   }

}
