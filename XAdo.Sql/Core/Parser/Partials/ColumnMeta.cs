using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using XAdo.Core;
using XAdo.Core.SimpleJson;
using XAdo.DbSchema;

namespace XAdo.Quobs.Core.Parser.Partials
{
   public class JsonAnnotation
   {
      public bool? outputOnUpdate { get; set; }
      public bool? outputOnCreate { get; set; }
      public string type { get; set; }
      public int? maxLength { get; set; }
      public string map { get; set; }
      public bool? @readonly { get; set; }
      public bool? notnull { get; set; }
      public bool? pkey { get; set; }
      public bool? autoIncrement { get; set; }
      public bool? unique { get; set; }
      public bool? outerJoin { get; set; }
      public string crud { get; set; }
   }
   
   public sealed class ColumnMeta : ICloneable
   {
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private static IDictionary<string, Type> _typeMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
      {
         {"byte",typeof(byte)},
         {"byte[]",typeof(byte[])},
         {"short",typeof(short)},
         {"int16",typeof(short)},
         {"int",typeof(int)},
         {"int32",typeof(int)},
         {"long",typeof(long)},
         {"int64",typeof(long)},
         {"string",typeof(string)},
         {"decimal",typeof(decimal)},
         {"money",typeof(decimal)},
         {"float",typeof(float)},
         {"single",typeof(float)},
         {"double",typeof(double)},
         {"bool",typeof(bool)},
         {"datetime",typeof(DateTime)},
         {"guid",typeof(Guid)},
         {"datetimeoffset",typeof(DateTimeOffset)},
         {"timespan",typeof(TimeSpan)},
      }.AsReadOnly();
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private PersistencyType? _persistencyType;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private bool? _outputOnUpdate;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private bool? _outputOnCreate;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private bool? _isPKey2;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private bool? _isAutoIncrement2;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private bool? _isNotNull;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private bool? _isUnique;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private bool? _isReadOnly;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private int? _maxLength;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private Type _type;

      private ColumnMeta() { }

      internal ColumnMeta(bool @readonly = false)
      {
         if (@readonly)
         {
            _persistencyType = PersistencyType.Read;
         }
      }


      public bool IsPKey
      {
         get { return _isPKey2.GetValueOrDefault(); }
         private set
         {
            _isPKey2 = value;
            if (value)
            {
               _persistencyType = Persistency & ~PersistencyType.Update;
            }
         }
      }
      public bool IsAutoIncrement
      {
         get { return _isAutoIncrement2.GetValueOrDefault(); }
         private set
         {
            _isAutoIncrement2 = value;
            if (value)
            {
               _persistencyType = Persistency & ~PersistencyType.Create;
               _persistencyType = Persistency & ~PersistencyType.Update;
               _outputOnCreate = true;
            }
         }
      }
      public bool IsNotNull
      {
         get { return _isNotNull.GetValueOrDefault(); }
         //private set { _isNotNull = value; }
      }
      public bool IsUnique
      {
         get { return _isUnique.GetValueOrDefault(); }
         //private set { _isUnique = value; }
      }
      public bool IsReadOnly
      {
         get { return _isReadOnly.GetValueOrDefault(_persistencyType != null ? _persistencyType==PersistencyType.Read : IsCalculated); }
         //private set { _isReadOnly = value; }
      }
      public int MaxLength
      {
         get { return _maxLength.GetValueOrDefault(-1); }
         //private set { _maxLength = value; }
      }
      public string JsonData { get; private set; }

      public bool IsCalculated { get; private set; }
      public bool IsOuterJoinColumn { get; private set; }

      public PersistencyType Persistency
      {
         get { return _persistencyType.GetValueOrDefault(PersistencyType.Default); }
         //private set { _persistencyType = value; }
      }

      public bool OutputOnUpdate
      {
         get { return _outputOnUpdate.GetValueOrDefault(false); }
         //set { _onUpdateIO = value; }
      }
      public bool OutputOnCreate
      {
         get { return _outputOnCreate.GetValueOrDefault(false); }
         //set { _onCreateIO = value; }
      }

      public Type Type {
         get { return _type ?? typeof (object); }
      }

      internal ColumnMeta InitializeByTag(string tag, bool isCalculated, out string map)
      {
         map = null;
         if (isCalculated)
         {
            _persistencyType = PersistencyType.Read;
            IsCalculated = true;
         }

         if (string.IsNullOrWhiteSpace(tag))
         {
            return this;
         }
         var x = SimpleJson.DeserializeObject<JsonAnnotation>(tag);
         _isReadOnly = x.@readonly;
         map = x.map;
         if (x.autoIncrement.HasValue)
         {
            IsAutoIncrement = x.autoIncrement.Value;
         }
         if (x.pkey.HasValue)
         {
            IsPKey = x.pkey.Value;
         }
         if (x.outerJoin.HasValue)
         {
            IsOuterJoinColumn = x.outerJoin.Value;
         }
         _isNotNull = x.notnull;
         _isUnique = x.unique;
         _maxLength = x.maxLength;
         _outputOnCreate = x.outputOnCreate;
         _outputOnUpdate = x.outputOnUpdate;
         if (x.type != null)
         {
            _type = _typeMap[x.type];
         }
         if (x.crud != null)
         {
            _persistencyType = x.crud.ToPersistencyType(_persistencyType);
         }
         return this;
      }

      internal void InitializeByAdoMeta(XAdoColumnMeta meta)
      {
         if (meta == null) return;
         _isNotNull = _isNotNull ?? !meta.AllowDBNull;
         _isPKey2 = _isPKey2 ?? meta.PKey;
         _isReadOnly = _isReadOnly ?? meta.ReadOnly;
         _type = _type ?? (meta.AllowDBNull ? meta.DataType.EnsureNullable() : meta.DataType);
         _isAutoIncrement2 = _isAutoIncrement2 ?? meta.AutoIncrement;
         _maxLength = _maxLength ?? meta.MaxLength;
         _isUnique = _isUnique ?? meta.Unique;
      }

      internal void InitializeByDbColumn(DbColumnItem column)
      {
         DbColumn = column;
         if (column == null) return;
         _isNotNull = _isNotNull ?? !column.IsNullable;
         _isPKey2 = _isPKey2 ?? column.IsPkey;
         _isReadOnly = _isReadOnly ?? column.IsReadOnly;
         _type = _type ?? (column.IsNullable ? column.Type.EnsureNullable() : column.Type);
         _isAutoIncrement2 = _isAutoIncrement2 ?? column.IsAutoIncrement;
         _maxLength = _maxLength ?? column.MaxLength;
         _isUnique = _isUnique ?? column.IsUnique;
      }
      internal void SetReadOnly(bool value)
      {
         _isReadOnly = value;
      }

      public DbColumnItem DbColumn { get; private set; }
      public override string ToString()
      {
         var sb = new StringBuilder();
         if (IsPKey) sb.Append("pk ");
         if (IsAutoIncrement) sb.Append("auto ");
         if (IsCalculated) sb.Append("calc ");
         if (IsNotNull) sb.Append("not-null ");
         if (IsOuterJoinColumn) sb.Append("outer-join ");
         sb.Append(Persistency.ToStringEx());
         sb.Append(JsonData);
         return sb.ToString();
      }

      object ICloneable.Clone()
      {
         return Clone();
      }

      public ColumnMeta Clone()
      {
         return new ColumnMeta
         {
            _persistencyType = _persistencyType,
            _outputOnCreate = _outputOnCreate,
            _isReadOnly = _isReadOnly,
            _isPKey2 = _isPKey2,
            _isUnique = _isUnique,
            _maxLength = _maxLength,
            _isAutoIncrement2 = _isAutoIncrement2,
            _isNotNull = _isNotNull,
            _outputOnUpdate = _outputOnUpdate,
            DbColumn = DbColumn,

            IsCalculated = IsCalculated,
            _type = Type,
            IsOuterJoinColumn = IsOuterJoinColumn,
            JsonData = JsonData
            
        };
      }

   }
}
