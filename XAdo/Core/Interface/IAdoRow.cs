using System;
using System.Collections.Generic;

namespace XAdo.Core.Interface
{
    public interface IAdoRow : IDictionary<string, object>
    {
        ICollection<string> ColumnNames { get; }
        ICollection<Type> ColumnTypes { get; }
    }
}