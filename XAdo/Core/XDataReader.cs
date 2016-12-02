using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace XAdo.Core
{
   internal class XDataReader : IDataReader
   {
      private int _row;
      private IDataReader _dr;

      public IDataReader InnerDataReader
      {
         get { return _dr; }
         set { _dr = value; }
      }
      public bool Eof { get;private set; }

      public int Row
      {
         get { return _row; }
      }

      public void Dispose()
      {
         _dr.Dispose();
      }

      public string GetName(int i)
      {
         return _dr.GetName(i);
      }

      public string GetDataTypeName(int i)
      {
         return _dr.GetDataTypeName(i);
      }

      public Type GetFieldType(int i)
      {
         return _dr.GetFieldType(i);
      }

      public object GetValue(int i)
      {
         return _dr.GetValue(i);
      }

      public int GetValues(object[] values)
      {
         return _dr.GetValues(values);
      }

      public int GetOrdinal(string name)
      {
         return _dr.GetOrdinal(name);
      }

      public bool GetBoolean(int i)
      {
         return _dr.GetBoolean(i);
      }

      public byte GetByte(int i)
      {
         return _dr.GetByte(i);
      }

      public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
      {
         return _dr.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
      }

      public char GetChar(int i)
      {
         return _dr.GetChar(i);
      }

      public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
      {
         return _dr.GetChars(i, fieldoffset, buffer, bufferoffset, length);
      }

      public Guid GetGuid(int i)
      {
         return _dr.GetGuid(i);
      }

      public short GetInt16(int i)
      {
         return _dr.GetInt16(i);
      }

      public int GetInt32(int i)
      {
         return _dr.GetInt32(i);
      }

      public long GetInt64(int i)
      {
         return _dr.GetInt64(i);
      }

      public float GetFloat(int i)
      {
         return _dr.GetFloat(i);
      }

      public double GetDouble(int i)
      {
         return _dr.GetDouble(i);
      }

      public string GetString(int i)
      {
         return _dr.GetString(i);
      }

      public decimal GetDecimal(int i)
      {
         return _dr.GetDecimal(i);
      }

      public DateTime GetDateTime(int i)
      {
         return _dr.GetDateTime(i);
      }

      public IDataReader GetData(int i)
      {
         return _dr.GetData(i);
      }

      public bool IsDBNull(int i)
      {
         return _dr.IsDBNull(i);
      }

      public int FieldCount
      {
         get { return _dr.FieldCount; }
      }

      object IDataRecord.this[int i]
      {
         get { return _dr[i]; }
      }

      object IDataRecord.this[string name]
      {
         get { return _dr[name]; }
      }

      public void Close()
      {
         _dr.Close();
      }

      public DataTable GetSchemaTable()
      {
         return _dr.GetSchemaTable();
      }

      public bool NextResult()
      {
         return _dr.NextResult();
      }

      public bool Read()
      {
         if (_dr.Read())
         {
            _row++;
            return true;
         }
         Eof = true;
         return false;
      }

      public int Depth
      {
         get { return _dr.Depth; }
      }

      public bool IsClosed
      {
         get { return _dr.IsClosed; }
      }

      public int RecordsAffected
      {
         get { return _dr.RecordsAffected; }
      }

      public async Task<bool> NextResultAsync()
      {
         return await ((DbDataReader) _dr).NextResultAsync();
      }
   }
}
