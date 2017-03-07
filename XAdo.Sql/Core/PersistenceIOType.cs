using System;
using System.Runtime.Serialization;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Converters;

namespace XAdo.Quobs.Core
{
   [Flags]
   //[JsonConverter(typeof(StringEnumConverter))]
   public enum PersistenceIOType
   {
      [EnumMember(Value = "default")]
      Default = 3,
      [EnumMember(Value = "input")]
      Input = 1,
      [EnumMember(Value = "output")]
      Output = 2
   }
}