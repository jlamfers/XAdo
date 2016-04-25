using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
    public class AdoParameterImpl  : IAdoParameter
    {
        private IDbDataParameter 
            _parameter;
        private object 
            _value;

        public DbType? DbType { get; set; }
        public ParameterDirection? Direction { get; set; }
        public byte? Precision { get; set; }
        public byte? Scale { get; set; }
        public int? Size { get; set; }

        public virtual object Value
        {
            get 
            { 
                return _parameter != null ? (_parameter.Value == DBNull.Value ? null : _parameter.Value) : _value; 
            }
            set
            {
                _value = value;
                if (_parameter != null)
                {
                    _parameter.Value = value ?? DBNull.Value;
                }
            }
        }

        public virtual IDbDataParameter CreateDbParameter(IDbCommand command, string name)
        {

            _parameter = _value as IDbDataParameter;
            if (_parameter != null)
            {
                return _parameter;
            }

            var p = command.CreateParameter();

            p.ParameterName = name;

            if (Precision != null)
            {
                p.Precision = Precision.Value;
            }
            if (Scale != null)
            {
                p.Scale = Scale.Value;
            }
            if (Size != null)
            {
                p.Size = Size.Value;
            }
            if (Direction != null)
            {
                p.Direction = Direction.Value;
            }
            p.Value = _value ?? DBNull.Value;
            if (DbType != null)
            {
                p.DbType = DbType.Value;
            }
            if (Value is DbDataReader || Value is IEnumerable<SqlDataRecord> || Value is DataTable)
            {
                var sp = p as SqlParameter;
                if (sp != null)
                {
                    sp.SqlDbType = SqlDbType.Structured;
                }
            }
            _parameter = p;
            return p;
        }

    }
}