using System;
using System.Data;

namespace XAdo.Sql.Core
{
   public static class DataRecordGetters
   {
      public static object GetNValue(this IDataRecord reader, int index)
      {
         return reader.IsDBNull(index) ? null : reader.GetValue(index);
      }
      public static string GetNString(this IDataRecord reader, int index)
      {
         return reader.IsDBNull(index) ? null : reader.GetString(index);
      }
      public static Byte? GetNByte(this IDataRecord reader, int index)
      {
         return reader.IsDBNull(index) ? (Byte?)null : reader.GetByte(index);
      }
      public static Boolean? GetNBoolean(this IDataRecord reader, int index)
      {
         return reader.IsDBNull(index) ? (Boolean?)null : reader.GetBoolean(index);
      }
      public static Char? GetNChar(this IDataRecord reader, int index)
      {
         return reader.IsDBNull(index) ? (Char?)null : reader.GetChar(index);
      }
      public static Decimal? GetNDecimal(this IDataRecord reader, int index)
      {
         return reader.IsDBNull(index) ? (Decimal?)null : reader.GetDecimal(index);
      }
      public static Double? GetNDouble(this IDataRecord reader, int index)
      {
         return reader.IsDBNull(index) ? (Double?)null : reader.GetDouble(index);
      }
      public static Single? GetNFloat(this IDataRecord reader, int index)
      {
         return reader.IsDBNull(index) ? (Single?)null : reader.GetFloat(index);
      }
      public static Guid? GetNGuid(this IDataRecord reader, int index)
      {
         return reader.IsDBNull(index) ? (Guid?)null : reader.GetGuid(index);
      }
      public static Int16? GetNInt16(this IDataRecord reader, int index)
      {
         return reader.IsDBNull(index) ? (Int16?)null : reader.GetInt16(index);
      }
      public static Int32? GetNInt32(this IDataRecord reader, int index)
      {
         return reader.IsDBNull(index) ? (Int32?)null : reader.GetInt32(index);
      }
      public static Int64? GetNInt64(this IDataRecord reader, int index)
      {
         return reader.IsDBNull(index) ? (Int64?)null : reader.GetInt64(index);
      }
      public static DateTime? GetNDateTime(this IDataRecord reader, int index)
      {
         return reader.IsDBNull(index) ? (DateTime?)null : reader.GetDateTime(index);
      }

      public static object GetValue(this IDataRecord reader, int index)
      {
         try
         {
            return reader.GetValue(index);

         }
         catch (Exception ex)
         {
            throw new DbReaderException(index,ex);
         }
      }
      public static string GetString(this IDataRecord reader, int index)
      {
         try
         {
            return reader.GetString(index);
         }
         catch (Exception ex)
         {
            throw new DbReaderException(index, ex);
         }
      }
      public static Byte GetByte(this IDataRecord reader, int index)
      {
         try
         {
            return reader.GetByte(index);
         }
         catch (Exception ex)
         {
            throw new DbReaderException(index, ex);
         }
      }
      public static Boolean GetBoolean(this IDataRecord reader, int index)
      {
         try
         {
            return reader.GetBoolean(index);
         }
         catch (Exception ex)
         {
            throw new DbReaderException(index, ex);
         }
      }
      public static Char GetChar(this IDataRecord reader, int index)
      {
         try
         {
            return reader.GetChar(index);
         }
         catch (Exception ex)
         {
            throw new DbReaderException(index, ex);
         }
      }
      public static Decimal GetDecimal(this IDataRecord reader, int index)
      {
         try
         {
            return reader.GetDecimal(index);
         }
         catch (Exception ex)
         {
            throw new DbReaderException(index, ex);
         }
      }
      public static Double GetDouble(this IDataRecord reader, int index)
      {
         try
         {
            return reader.GetDouble(index);
         }
         catch (Exception ex)
         {
            throw new DbReaderException(index, ex);
         }
      }
      public static Single GetFloat(this IDataRecord reader, int index)
      {
         try
         {
            return reader.GetFloat(index);
         }
         catch (Exception ex)
         {
            throw new DbReaderException(index, ex);
         }
      }
      public static Guid GetGuid(this IDataRecord reader, int index)
      {
         try
         {
            return reader.GetGuid(index);
         }
         catch (Exception ex)
         {
            throw new DbReaderException(index, ex);
         }
      }
      public static Int16 GetInt16(this IDataRecord reader, int index)
      {
         try
         {
            return reader.GetInt16(index);
         }
         catch (Exception ex)
         {
            throw new DbReaderException(index, ex);
         }
      }
      public static Int32 GetInt32(this IDataRecord reader, int index)
      {
         try
         {
            return reader.GetInt32(index);
         }
         catch (Exception ex)
         {
            throw new DbReaderException(index, ex);
         }
      }
      public static Int64 GetInt64(this IDataRecord reader, int index)
      {
         try
         {
            return reader.GetInt64(index);
         }
         catch (Exception ex)
         {
            throw new DbReaderException(index, ex);
         }
      }
      public static DateTime GetDateTime(this IDataRecord reader, int index)
      {
         try
         {
            return reader.GetDateTime(index);
         }
         catch (Exception ex)
         {
            throw new DbReaderException(index, ex);
         }
      }

   }
}