using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
    public class XAdoCommandFactoryImpl : IXAdoCommandFactory
    {
        private readonly IXAdoParameterFactory _parameterFactory;

        public XAdoCommandFactoryImpl(IXAdoParameterFactory parameterFactory)
        {
            if (parameterFactory == null) throw new ArgumentNullException("parameterFactory");
            _parameterFactory = parameterFactory;
        }

        public virtual IDbCommand CreateCommand(IDbConnection cn, string sql, object param, IDbTransaction tr, int? commandTimeout, CommandType? commandType)
        {
            if (cn == null) throw new ArgumentNullException("cn");
            if (sql == null) throw new ArgumentNullException("sql");

            var cmd = cn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Connection = cn;

            if (tr != null)
            {
                cmd.Transaction = tr;
            }

            cmd.CommandType = commandType ?? GetDefaultCommandType(sql);
            if (commandTimeout != null)
            {
                cmd.CommandTimeout = commandTimeout.Value;
            }
            return param == null ? cmd : FillParams(cmd, param);
        }

        public virtual IDbCommand FillParams(IDbCommand command, object param)
        {
            if (command == null) throw new ArgumentNullException("command");

            command.Parameters.Clear();

            if (param == null) return command;

            foreach (var kv in AsAdoParameterDictionary(param))
            {
                var e = kv.Value.Value as IEnumerable;
                if (e != null && !(e is string))
                {
                    if (!(e is IDictionary) && TryHandleEnumParams(command, kv.Key, e))
                    {
                        continue;
                    }
                }
                command.Parameters.Add(kv.Value.CreateDbParameter(command, kv.Key));
            }
            return command;
        }

        public virtual CommandType GetDefaultCommandType(string sql)
        {
            if (sql == null) throw new ArgumentNullException("sql");
            return sql.Contains(" ") ? CommandType.Text : CommandType.StoredProcedure;
        }

        protected virtual IDictionary<string, IXAdoParameter> AsAdoParameterDictionary(object target)
        {
            if (target == null) return new Dictionary<string, IXAdoParameter>();

            var paramDict = target as IDictionary<string, IXAdoParameter>;
            if (paramDict != null) return paramDict;

            var objectDict = target as IDictionary<string, object>;
            if (objectDict != null)
            {
                return objectDict.ToDictionary(kv => kv.Key, kv => kv.Value as IXAdoParameter ?? _parameterFactory.Create(kv.Value));
            }
            return target.GetType().GetProperties().ToDictionary(p => p.Name, p =>
            {
                var v = p.GetValue(target,null);
                return v as IXAdoParameter ?? _parameterFactory.Create(v);
            });
        }

        /// <returns>true if param is handled, else false</returns>
        protected virtual bool TryHandleEnumParams(IDbCommand command, string paramName, IEnumerable param)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (paramName == null) throw new ArgumentNullException("paramName");
            if (param == null) throw new ArgumentNullException("param");

            var sql = command.CommandText;

            paramName = paramName.TrimStart('@', ':', '?');

            var anyReplaced = false;

            command.CommandText = Regex.Replace(sql,
                @"(?<context>[\s\)]IN\s*\(?)\s*(?<parname>[\?\:\@]" + Regex.Escape(paramName) + ")", m =>
                {
                    anyReplaced = true;
                    var parName = m.Groups["parname"].Value;
                    var context = m.Groups["context"].Value;
                    var sb = new StringBuilder();
                    if (!context.EndsWith("("))
                    {
                        sb.Append("(");
                    }
                    string comma = null;
                    var index = 0;
                    foreach (var v in param)
                    {
                        sb.Append(comma);
                        sb.Append(parName);
                        sb.Append("_");
                        sb.Append(index);
                        command.Parameters.Add(_parameterFactory.Create(v).CreateDbParameter(command, parName + "_" + index));
                        comma = comma ?? ", ";
                        index++;
                    }
                    if (!context.EndsWith("("))
                    {
                        sb.Append(")");
                    }
                    sb.Insert(0, context);
                    return sb.ToString();
                },
                RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            return anyReplaced;

        }
    }
}
