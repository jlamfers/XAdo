using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace XAdo.Core
{

    public class CollectionDataReader : DbDataReader
    {
        private readonly IEnumerable 
            _collection;

        private readonly IEnumerator
            _enumerator;

        private readonly DataTable
            _schemaDatatable;

        private IList<GetterSetterUtil.IGetterSetter>
            _getters;

        private IList<Type> 
            _fieldTypes;
        private IList<string> 
            _fieldNames;
        private int 
            _fieldCount;

        private bool 
            _isClosed;
        private readonly bool 
            _hasRows;
        readonly Dictionary<string, int>
            _ordinals = new Dictionary<string, int>(); 
        private object 
            _firstRow;

        public CollectionDataReader(IEnumerable<IEnumerable<KeyValuePair<string, object>>> collection, IEnumerable<KeyValuePair<string, Type>> columnNameTypeMap = null)
        {
            if (collection == null) throw new ArgumentNullException("collection");

            _collection = collection;
            _enumerator = collection.GetEnumerator();
            _schemaDatatable = CreateSchemaDataTable();

            if (_enumerator.MoveNext())
            {
                _firstRow = _enumerator.Current;
                _hasRows = true;
            }
            if (_firstRow == null)
            {
                if (columnNameTypeMap == null)
                {
                    throw new AdoException("Cannot determine metadata. No items found in list of dynamic items. You must provide a meta argument (ordered list of column-name/type maps) in order to let it work with empty dynamic collections.");
                }
                BuildMetaFromMap(columnNameTypeMap);
                return;
            }
            BuildGettersAndMeta((IEnumerable<KeyValuePair<string, object>>)_firstRow);
        }

        // this constructor is invoked by subtype with constrainted generic type
        protected CollectionDataReader(IEnumerable source, Type elementType)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (elementType == null) throw new ArgumentNullException("elementType");

            _collection = source;
            _enumerator = source.GetEnumerator();
            _schemaDatatable = CreateSchemaDataTable();

            if (_enumerator.MoveNext())
            {
                _firstRow = _enumerator.Current;
                _hasRows = true;
            }
            BuildGettersAndMeta(elementType);
        }


        public override string GetName(int i)
        {
            return _fieldNames[i];
        }

        public override string GetDataTypeName(int i)
        {
            return _getters[i].Type.FullName;
        }

        public override IEnumerator GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        public override Type GetFieldType(int i)
        {
            return _fieldTypes[i];
        }

        public override object GetValue(int i)
        {
            return _getters[i].Get(_enumerator.Current);
        }

        public override int GetValues(object[] values)
        {
            var count = Math.Min(values.Length, _getters.Count);
            for (var i = 0; i < count; i++)
            {
                values[i] = _getters[i].Get(_enumerator.Current);
            }
            return count;
        }

        public override int GetOrdinal(string name)
        {
            return _ordinals[name];
        }

        public override bool GetBoolean(int i)
        {
            return (bool)_getters[i].Get(_enumerator.Current);
        }

        public override byte GetByte(int i)
        {
            return (byte)_getters[i].Get(_enumerator.Current);
        }

        public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            var bytes = (IList<byte>)_getters[i].Get(_enumerator.Current);
            for (var j = 0; j < length; j++)
            {
                buffer[bufferoffset + j] = bytes[(int) fieldOffset + j];
            }
            return length;
        }

        public override char GetChar(int i)
        {
            return (char)_getters[i].Get(_enumerator.Current);
        }

        public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            var chars = (IList<char>)_getters[i].Get(_enumerator.Current);
            for (var j = 0; j < length; j++)
            {
                buffer[bufferoffset + j] = chars[(int)fieldoffset + j];
            }
            return length;
        }

        public override Guid GetGuid(int i)
        {
            return (Guid)_getters[i].Get(_enumerator.Current);
        }

        public override short GetInt16(int i)
        {
            return (Int16)_getters[i].Get(_enumerator.Current);
        }

        public override int GetInt32(int i)
        {
            return (Int32)_getters[i].Get(_enumerator.Current);
        }

        public override long GetInt64(int i)
        {
            return (Int64)_getters[i].Get(_enumerator.Current);
        }

        public override float GetFloat(int i)
        {
            return (float)_getters[i].Get(_enumerator.Current);
        }

        public override double GetDouble(int i)
        {
            return (double)_getters[i].Get(_enumerator.Current);
        }

        public override string GetString(int i)
        {
            return (string)_getters[i].Get(_enumerator.Current);
        }

        public override decimal GetDecimal(int i)
        {
            return (decimal)_getters[i].Get(_enumerator.Current);
        }

        public override DateTime GetDateTime(int i)
        {
            return (DateTime)_getters[i].Get(_enumerator.Current);
        }

        public new IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public override bool IsDBNull(int i)
        {
            return _getters[i].Get(_enumerator.Current) == null;
        }

        public override int FieldCount { get { return _fieldCount; } }

        public override bool HasRows
        {
            get { return _hasRows; }
        }

        public override object this[int i]
        {
            get { return _getters[i].Get(_enumerator.Current); }
        }

        public override object this[string name]
        {
            get { return _getters[GetOrdinal(name)].Get(_enumerator.Current); }
        }


        public override void Close()
        {
            _isClosed = true;
        }

        public override DataTable GetSchemaTable()
        {
            return _schemaDatatable;
        }

        public override bool NextResult()
        {
            return false;
        }

        public override bool Read()
        {
            if (_firstRow != null)
            {
                _firstRow = null;
                return true;
            }
            return _enumerator.MoveNext();
        }

        public override int Depth
        {
            get { return 0; }
        }

        public override bool IsClosed { get { return _isClosed; } }
        
        public override int RecordsAffected
        {
            get { return 0; }
        }

        private static DataTable CreateSchemaDataTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("ColumnName", typeof(string));
            dt.Columns.Add("ColumnOrdinal", typeof(int));
            dt.Columns.Add("IsKey", typeof(bool));
            dt.Columns.Add("DataType", typeof(Type));
            return dt;
        }

        private void BuildMetaFromMap(IEnumerable<KeyValuePair<string, Type>> columnNameTypeMap)
        {
            var count = 0;
            columnNameTypeMap = columnNameTypeMap.ToArray();
            _fieldCount = columnNameTypeMap.Count();
            _fieldTypes = new List<Type>();
            _fieldNames = new List<string>();
            foreach (var m in columnNameTypeMap)
            {
                _fieldNames.Add(m.Key);
                _fieldTypes.Add(m.Value);
                _ordinals[m.Key] = count;
                _schemaDatatable.Rows.Add(m.Key, count, false, m.Value);
                count++;
            }
        }

        private void BuildGettersAndMeta(Type rowType)
        {
            _getters = rowType
                .GetProperties()
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                .Select(p => p.ToGetterSetter())
                .ToList();
            BuildMetaFromGetters();
        }

        private void BuildGettersAndMeta(IEnumerable<KeyValuePair<string, object>> rowData)
        {
            _getters = rowData.Select(kv => kv.ToGetterSetter()).ToList();
            BuildMetaFromGetters();
        }

        private void BuildMetaFromGetters()
        {
            _fieldCount = _getters.Count;
            _fieldTypes = _getters.Select(g => g.Type.EnsureNotNullable()).ToList();
            _fieldNames = _getters.Select(p => p.Name).ToList();
            for (var i = 0; i < _getters.Count; i++)
            {
                _ordinals[_getters[i].Name] = i;
            }

            for (var i = 0; i < _fieldNames.Count; i++)
            {
                _schemaDatatable.Rows.Add(_fieldNames[i], i, false, _fieldTypes[i]);
            }
        }
    }

    public class CollectionDataReader<T> : CollectionDataReader
        where T: class
    {
        public CollectionDataReader(IEnumerable<T> source) : base(source, typeof(T))
        {
        }
    }
}
