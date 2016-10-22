using System;
using System.Data;

namespace XAdo.Core.Impl
{
    public static class NullableGetters
    {
       public static object GetValue(IDataRecord reader, int index)
       {
          return reader.IsDBNull(index) ? null : reader.GetValue(index);
       }
       public static string GetString(IDataRecord reader, int index)
        {
            return reader.IsDBNull(index) ? null : reader.GetString(index);
        }
        public static Byte? GetByte(IDataRecord reader, int index)
        {
            return reader.IsDBNull(index) ? (Byte?)null : reader.GetByte(index);
        }

        public static Boolean? GetBoolean(IDataRecord reader, int index)
        {
            return reader.IsDBNull(index) ? (Boolean?)null : reader.GetBoolean(index);
        }

        public static Char? GetChar(IDataRecord reader, int index)
        {
            return reader.IsDBNull(index) ? (Char?)null : reader.GetChar(index);
        }

        public static Decimal? GetDecimal(IDataRecord reader, int index)
        {
            return reader.IsDBNull(index) ? (Decimal?)null : reader.GetDecimal(index);
        }

        public static Double? GetDouble(IDataRecord reader, int index)
        {
            return reader.IsDBNull(index) ? (Double?)null : reader.GetDouble(index);
        }

        public static Single? GetFloat(IDataRecord reader, int index)
        {
            return reader.IsDBNull(index) ? (Single?)null : reader.GetFloat(index);
        }

        public static Guid? GetGuid(IDataRecord reader, int index)
        {
            return reader.IsDBNull(index) ? (Guid?)null : reader.GetGuid(index);
        }

        public static Int16? GetInt16(IDataRecord reader, int index)
        {
            return reader.IsDBNull(index) ? (Int16?)null : reader.GetInt16(index);
        }

        public static Int32? GetInt32(IDataRecord reader, int index)
        {
            return reader.IsDBNull(index) ? (Int32?)null : reader.GetInt32(index);
        }

        public static Int64? GetInt64(IDataRecord reader, int index)
        {
            return reader.IsDBNull(index) ? (Int64?)null : reader.GetInt64(index);
        }

        public static DateTime? GetDateTime(IDataRecord reader, int index)
        {
            return reader.IsDBNull(index) ? (DateTime?)null : reader.GetDateTime(index);
        }
    }
}