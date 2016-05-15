
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using XAdo.Core.Interface;

namespace XAdo.Core
{
    /// <summary>
    /// This AdoRow basicly is an (ordered) IDictionary implementation that 
    /// can be instantiated quickly from an ADO datareader, for each fetched row.
    /// It inherits DynamicObject, so it provides dynamic behaviour
    /// </summary>
    public class AdoRow : DynamicObject, IAdoRow
    {
        internal class Meta
        {
            public IList<string> ColumnNames;
            public IList<Type> Types;
            public IDictionary<string, int> Index;
        }
        private IList<string> _columnNames;
        private IList<object> _values;
        private IList<Type> _types;

        private IDictionary<string, int> _index;

        public AdoRow()
        {
            _columnNames = new List<string>();
            _values = new List<object>();
            _index = new Dictionary<string, int>();
        }
        public AdoRow(string[] columnNames, object[] values, Type[] types = null, IDictionary<string, int> index = null)
        {
            _columnNames = columnNames;
            _values = values;
            _types = types;
            _index = index;
        }

        internal AdoRow(Meta meta, object[] values)
        {
            _columnNames = meta.ColumnNames;
            _values = values;
            _types = meta.Types;
            _index = meta.Index;
        }

        public AdoRow(IEnumerable<KeyValuePair<string,object>> other )
        {
            _columnNames = new List<string>();
            _values = new List<object>();
            _index = new Dictionary<string, int>();
            foreach (var kv in other)
            {
                Add(kv);
            }
        }

        public object this[int index]
        {
            get
            {
                EnsureDBNullFiltered();
                return _values[index];
            }
            set { _values[index] = value; }
        }

        #region IDictionary<string,object>

        public object this[string name]
        {
            get
            {
                EnsureDBNullFiltered();
                return _values[GetOrdinal(name)];
            }
            set
            {
                var ordinal = GetOrdinal(name, false);
                if (ordinal == -1)
                {
                    Add(name, value);
                }
                else
                {
                    _values[ordinal] = value;
                }
            }
        }

        public bool ContainsKey(string key)
        {
            return GetOrdinal(key, false) != -1;
        }

        public void Add(string key, object value)
        {
            if (ContainsKey(key))
            {
                throw new AdoException("key already exists");
            }
            if (_values is List<object>)
            {
                _columnNames.Add(key);
                _values.Add(value);
                _types.Add(value == null ? typeof(object) : value.GetType());
                Index[key] = _values.Count - 1;
            }
            else
            {
                EnsureListTypes();
                _columnNames.Add(key);
                _values.Add(value);
                _types.Add(value == null ? typeof(object) : value.GetType());
            }
        }

        public bool Remove(string key)
        {
            var ordinal = GetOrdinal(key, false);
            if (ordinal == -1)
            {
                return false;
            }
            if (_values is List<object>)
            {
                _columnNames.RemoveAt(ordinal);
                _values.RemoveAt(ordinal);
                _types.RemoveAt(ordinal);
                Index.Remove(key);
            }
            else
            {
                EnsureListTypes();
                _columnNames.RemoveAt(ordinal);
                _values.RemoveAt(ordinal);
                _types.RemoveAt(ordinal);
            }
            return true;
        }

        public bool TryGetValue(string key, out object value)
        {
            var ordinal = GetOrdinal(key, false);
            value = null;
            if (ordinal == -1) return false;
            EnsureDBNullFiltered();
            value = _values[ordinal];
            return true;
        }

        public ICollection<string> Keys
        {
            get { return ColumnNames; }
        }

        public ICollection<string> ColumnNames
        {
            get { return _columnNames.ToArray(); }
        }

        public ICollection<object> Values
        {
            get
            {
                EnsureDBNullFiltered();
                return _values.ToArray();
            }
        }

        public ICollection<Type> ColumnTypes
        {
            get
            {
                if (_types == null)
                {
                    _types = _values.Select(v => v == null ? typeof (object) : v.GetType()).ToList();
                }
                return _types.ToArray();
            }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            EnsureDBNullFiltered();
            for (var i = 0; i < _columnNames.Count; i++)
            {
                yield return new KeyValuePair<string, object>(_columnNames[i], _values[i]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            EnsureListTypes();
            _columnNames.Clear();
            _values.Clear();
            _types.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            object value;
            return TryGetValue(item.Key, out value) && Equals(item.Value, value);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            EnsureDBNullFiltered();
            for (var i = 0; i < _columnNames.Count; i++)
            {
                array[i + arrayIndex] = new KeyValuePair<string, object>(_columnNames[i], _values[i]);
            }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return Contains(item) && Remove(item.Key);
        }

        public int Count { get { return _columnNames.Count; } }

        public bool IsReadOnly { get { return false; } }

        #endregion

        public override string ToString()
        {
            var sb = new StringBuilder("{");
            foreach (var kv in this)
            {
                sb.AppendFormat(kv.Value == null ? "{0}=NULL, " : "{0}='{1}', ", kv.Key, kv.Value);
            }
            if (sb.Length > 1) sb.Length -= 2;
            return sb.Append('}').ToString();
        }

        #region DynamicObject

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _columnNames.ToArray();
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var name = binder.Name;
            var ordinal = GetOrdinal(name, false);
            if (ordinal == -1)
            {
                Add(name, value);
            }
            else
            {
                _values[ordinal] = value;
            }
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            var name = binder.Name;
            var ordinal = GetOrdinal(name, false);
            if (ordinal == -1) return false;
            EnsureDBNullFiltered();
            result = _values[ordinal];
            return true;
        }

        #endregion

        #region Private

        private bool _dbnullFiltered;

        private void EnsureDBNullFiltered()
        {
            if (_dbnullFiltered) return;
            for (var i = 0; i < _values.Count; i++)
            {
                if (_values[i] == DBNull.Value)
                {
                    _values[i] = null;
                }
            }
            _dbnullFiltered = true;
        }

        private int GetOrdinal(string name, bool throwException = true)
        {
            int ordinal;
            if (Index.TryGetValue(name, out ordinal)) return ordinal;
            if (!throwException) return -1;
            throw new AdoException("Column " + name + " does not exist.");
        }

        private IDictionary<string, int> Index
        {
            get
            {
                if (_index != null) return _index;
                _index = new Dictionary<string, int>();
                for (var i = 0; i < _columnNames.Count; i++)
                {
                    _index.Add(_columnNames[i], i);
                }
                return _index;
            }
        }

        private void EnsureListTypes()
        {
            _columnNames = _columnNames as List<string> ?? new List<string>(_columnNames);
            _values = _values as List<object> ?? new List<object>(_values);
            if (_types != null)
            {
                _types = _types as List<Type> ?? new List<Type>(_types);
            }
            else
            {
                _types = _values.Select(v => v == null ? typeof (object) : v.GetType()).ToList();
            }
            _index = null;
        }
        #endregion

    }

}