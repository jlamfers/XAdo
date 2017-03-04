using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using XAdo.Core;
using XAdo.Quobs.Core.Common;
using XAdo.Quobs.Core.Mapper;

namespace XAdo.Quobs.Core.Parser.Partials
{
   /*
    * -->../Address/Name*@!+#RD {onUpdate:Input,onCreate=Output,type:int,maxLength:20}
    */
   public sealed class ColumnMeta : ICloneable
   {

      private class JsonAnnotation
      {
         public PersistenceIOType? onUpdate { get; set; }
         public PersistenceIOType? onCreate { get; set; }
         public string type { get; set; }
         public int? maxLength { get; set; }
      }
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
      private PersistenceIOType? _onUpdateIO;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private PersistenceIOType? _onCreateIO;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private bool? _isPKey;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private bool? _isAutoIncrement;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private bool? _isNotNull;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private bool? _isUnique;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private bool? _isReadOnly;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private int? _maxLength;

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
         get { return _isPKey.GetValueOrDefault(); }
         private set { _isPKey = value; }
      }
      public bool IsAutoIncrement
      {
         get { return _isAutoIncrement.GetValueOrDefault(); }
         private set { _isAutoIncrement = value; }
      }
      public bool IsNotNull
      {
         get { return _isNotNull.GetValueOrDefault(); }
         private set { _isNotNull = value; }
      }
      public bool IsUnique
      {
         get { return _isUnique.GetValueOrDefault(); }
         private set { _isUnique = value; }
      }
      public bool IsReadOnly
      {
         get { return _isReadOnly.GetValueOrDefault(_persistencyType != null ? _persistencyType==PersistencyType.Read : IsCalculated); }
         private set { _isReadOnly = value; }
      }
      public int MaxLength
      {
         get { return _maxLength.GetValueOrDefault(-1); }
         private set { _maxLength = value; }
      }
      public string JsonData { get; private set; }

      public bool IsCalculated { get; private set; }
      public bool IsOuterJoinColumn { get; private set; }

      public PersistencyType Persistency
      {
         get { return _persistencyType.GetValueOrDefault(PersistencyType.Default); }
         private set { _persistencyType = value; }
      }

      public PersistenceIOType OnUpdate
      {
         get { return _onUpdateIO.GetValueOrDefault(PersistenceIOType.Default); }
         set { _onUpdateIO = value; }
      }
      public PersistenceIOType OnCreate
      {
         get { return _onCreateIO.GetValueOrDefault(PersistenceIOType.Default); }
         set { _onCreateIO = value; }
      }

      public Type Type { get; private set; }

      internal ColumnMeta InitializeByTag(string tag, bool isCalculated)
      {
         if (isCalculated)
         {
            _persistencyType = PersistencyType.Read;
            IsCalculated = true;
         }

         if (string.IsNullOrWhiteSpace(tag))
         {
            return this;
         }

         for (var i = 0; i < tag.Length; i++)
         {
            var ch = tag[i];
            switch (ch)
            {
               case '\t':
               case ' ':
                  continue;
               case Constants.Syntax.Chars.JSON_START:
                  var jsonData = new StringBuilder();
                  while (i < tag.Length)
                  {
                     // read until eoln
                     jsonData.Append(tag[i++]);
                  }
                  JsonData = jsonData.ToString();
                  var jobj = JsonConvert.DeserializeObject<JsonAnnotation>(JsonData);
                  if (jobj.maxLength != null)
                  {
                     MaxLength = jobj.maxLength.Value;
                  }
                  OnCreate = jobj.onCreate.GetValueOrDefault(PersistenceIOType.Default);
                  OnUpdate = jobj.onUpdate.GetValueOrDefault(PersistenceIOType.Default);
                  if (jobj.type != null)
                  {
                     Type type;
                     if (_typeMap.TryGetValue(jobj.type, out type))
                     {
                        Type = type;
                     }
                  }

                  break;
               case Constants.Syntax.Chars.PRIMARY_KEY:
                  _isPKey = true;
                  Persistency &= ~PersistencyType.Update;
                  break;
               case Constants.Syntax.Chars.CALCULATED:
                  IsCalculated = true;
                  Persistency &= ~PersistencyType.Create;
                  Persistency &= ~PersistencyType.Update;
                  break;
               case Constants.Syntax.Chars.AUTO_INCREMENT:
                  _isAutoIncrement = true;
                  _isPKey = true;
                  Persistency &= ~PersistencyType.Create;
                  Persistency &= ~PersistencyType.Update;
                  break;
               case Constants.Syntax.Chars.OUTER_JOIN_COLUMN:
                  IsOuterJoinColumn = true;
                  break;
               case Constants.Syntax.Chars.UNIQUE:
                  IsUnique = true;
                  break;
               case Constants.Syntax.Chars.NOT_NULL:
                  _isNotNull= true;
                  break;
               default:
                  switch (char.ToUpper(ch))
                  {
                     case Constants.Syntax.Chars.SPECIAL_CHARS_STARTER:
                        break;
                     case Constants.Syntax.Chars.CREATE:
                        _persistencyType = _persistencyType.GetValueOrDefault(PersistencyType.None) | PersistencyType.Create;
                        break;
                     case Constants.Syntax.Chars.UPDATE:
                        _persistencyType = _persistencyType.GetValueOrDefault(PersistencyType.None) | PersistencyType.Update;
                        break;
                     case Constants.Syntax.Chars.READ:
                        _persistencyType = _persistencyType.GetValueOrDefault(PersistencyType.None) | PersistencyType.Read;
                        break;
                     case Constants.Syntax.Chars.DELETE:
                        _persistencyType = _persistencyType.GetValueOrDefault(PersistencyType.None) | PersistencyType.Delete;
                        break;
                  }
                  break;
            }
         }
         return this;
      }
      internal void InitializeByAdoMeta(AdoColumnMeta meta)
      {
         if (meta == null) return;
         _isNotNull = _isNotNull ?? !meta.AllowDBNull;
         _isPKey = _isPKey ?? meta.PKey;
         _isReadOnly = _isReadOnly ?? meta.ReadOnly;
         Type = Type ?? (meta.AllowDBNull ? meta.DataType.EnsureNullable() : meta.DataType);
         _isAutoIncrement = _isAutoIncrement ?? meta.AutoIncrement;
         _maxLength = _maxLength ?? meta.MaxLength;
         _isUnique = _isUnique ?? meta.Unique;
      }
      internal void SetReadOnly(bool value)
      {
         IsReadOnly = value;
      }


      public override string ToString()
      {
         var sb = new StringBuilder();
         if (IsPKey) sb.Append(Constants.Syntax.Chars.PRIMARY_KEY);
         if (IsAutoIncrement) sb.Append(Constants.Syntax.Chars.AUTO_INCREMENT);
         if (IsCalculated) sb.Append(Constants.Syntax.Chars.CALCULATED);
         if (IsNotNull) sb.Append(Constants.Syntax.Chars.NOT_NULL);
         if (IsOuterJoinColumn) sb.Append(Constants.Syntax.Chars.OUTER_JOIN_COLUMN);
         sb.Append(Constants.Syntax.Chars.SPECIAL_CHARS_STARTER);
         sb.Append(Persistency.HasFlag(PersistencyType.Create) ? Constants.Syntax.Chars.CREATE : '-');
         sb.Append(Persistency.HasFlag(PersistencyType.Read) ? Constants.Syntax.Chars.READ : '-');
         sb.Append(Persistency.HasFlag(PersistencyType.Update) ? Constants.Syntax.Chars.UPDATE : '-');
         sb.Append(Persistency.HasFlag(PersistencyType.Delete) ? Constants.Syntax.Chars.DELETE : '-');
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
            _onUpdateIO = _onUpdateIO,
            _isReadOnly = _isReadOnly,
            _isPKey = _isPKey,
            _isUnique = _isUnique,
            _maxLength = _maxLength,
            _isAutoIncrement = _isAutoIncrement,
            _isNotNull = _isNotNull,
            _onCreateIO = _onCreateIO,

            IsCalculated = IsCalculated,
            Type = Type,
            IsOuterJoinColumn = IsOuterJoinColumn,
            JsonData = JsonData
            
        };
      }

   }
}
