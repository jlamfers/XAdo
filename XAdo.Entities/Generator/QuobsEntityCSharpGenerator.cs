using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CSharp;

namespace XAdo.Quobs.Generator
{
    public class QuobsEntityCSharpGenerator
    {
        private const string DefaultTableClassNamePrefix = "Db";

        public class DbColumn
        {

            private static readonly CodeDomProvider _codeDomProvider = new CSharpCodeProvider();

            private string _propertyName;
            public DbTable Table { get; set; }
            public string Name { get; set; }
            public bool IsPKey { get; set; }
            public bool IsIdentity { get; set; }
            public Type Type { get; set; }
            public string PropertyName
            {
                get
                {
                    if (_propertyName != null)
                    {
                        return _propertyName;
                    }
                    // normalize the property name, ensure it is valid
                    _propertyName = Name.Replace(" ", "_").Replace(".", "_").Replace("-", "_");
                    if (_propertyName == Table.ClassName)
                    {
                        _propertyName = _propertyName + "_";
                    }
                    if (!_codeDomProvider.IsValidIdentifier(_propertyName))
                    {
                        var prevName = _propertyName;
                        _propertyName = _codeDomProvider.CreateEscapedIdentifier(_propertyName);
                        if (prevName == _propertyName)
                        {
                            _propertyName = _codeDomProvider.CreateEscapedIdentifier("_" + _propertyName);
                        }
                    }
                    return _propertyName;
                }
                set { _propertyName = value; }
            }

            public string RenderAnnotations()
            {
                var sb = new StringBuilder();
                var comma = "";
                if (PropertyName != Name || IsPKey)
                {
                    sb.AppendFormat("Column(");
                }
                if (PropertyName != Name)
                {
                   sb.AppendFormat("ColumnName = \"{0}\"", Name.Replace(".", "\\\\."));
                    comma = ", ";
                }
                if (IsPKey)
                {
                    sb.AppendFormat("{0}IsPKey = true", comma);
                    comma = ", ";
                }
                if (IsIdentity)
                {
                    sb.AppendFormat("{0}IsOutputOnInsert = true", comma);
                }
                if (sb.Length > 0)
                {
                    sb.Insert(0, "[");
                    sb.Append(")]");
                    return sb.ToString();
                }
                return null;
            }
            public string RenderProperty()
            {
                return "public virtual " + RenderType() + " " + PropertyName + "{get; set;}";
            }
            public string RenderType()
            {
                var type = Nullable.GetUnderlyingType(Type) ?? Type;
                return type.Name + (type.IsValueType ? "?" : "");
            }
            public string TypeNameSpace()
            {
                var type = Nullable.GetUnderlyingType(Type) ?? Type;
                return type.Namespace;
            }

        }

        public class DbTable
        {
            private const string NormalizedPKeyName = "Id";

            private static readonly CodeDomProvider _codeDomProvider = new CSharpCodeProvider();

            private string _className;

            public DbTable(DataRow tableRow, string classNamePrefix, bool normalizePrimaryKeyName)
            {
                ClassNamePrefix = classNamePrefix;
                NormalizePrimaryKeyName = normalizePrimaryKeyName;
                Schema = tableRow.ItemArray[1].ToString();
                Name = tableRow.ItemArray[2].ToString();
                TableType = (string)tableRow["table_type"];
                Columns = new List<DbColumn>();
            }

            public void Add(DataColumn dataColumn)
            {
                string propertyName = null;
                var ispkey = dataColumn.AutoIncrement || dataColumn.Table.PrimaryKey.Contains(dataColumn);
                if (NormalizePrimaryKeyName)
                {
                    if (ispkey && dataColumn.Table.PrimaryKey.Length == 1 && !dataColumn.Table.Columns.Contains(NormalizedPKeyName))
                    {
                        propertyName = NormalizedPKeyName;
                    }
                }
                var column = new DbColumn
                {
                    IsIdentity = dataColumn.AutoIncrement,
                    IsPKey = ispkey,
                    Name = dataColumn.ColumnName,
                    Table = this,
                    Type = dataColumn.DataType,
                    PropertyName = propertyName
                };
                Columns.Add(column);
            }

            public string ClassNamePrefix { get; set; }
            public bool NormalizePrimaryKeyName { get; set; }
            public string Schema { get; set; }
            public string Name { get; set; }
            public string TableType { get; set; }
            public IList<DbColumn> Columns { get; set; }
            public string ClassName
            {
                get
                {
                    if (_className != null)
                    {
                        return _className;
                    }
                    // normalize the class name, ensure it is valid
                    _className = ClassNamePrefix + Name.Replace(" ", "_").Replace(".", "_").Replace("-", "_");
                    if (!_codeDomProvider.IsValidIdentifier(_className))
                    {
                        var prevName = _className;
                        _className = _codeDomProvider.CreateEscapedIdentifier(_className);
                        if (prevName == _className)
                        {
                            _className = _codeDomProvider.CreateEscapedIdentifier("_" + _className);
                        }
                    }
                    return _className;
                }
            }
            public string RenderAnnotations()
            {
                var sb = new StringBuilder();

               var crud = TableType == "VIEW" ? ", Crud=\"R\"" : "";

                if (Schema != null)
                   sb.AppendFormat("Table(TableName = \"{0}.{1}\"{2})", Schema.Replace(".", "\\\\."), Name.Replace(".", "\\\\."), crud);
                else
                   sb.AppendFormat("Table(TableName = \"{0}\"{1})", Name.Replace(".", "\\\\."), crud);
                sb.Insert(0, "[");
                sb.Append("]");
                return sb.ToString();
            }

            public string RenderClassName()
            {
                return "public partial class " + ClassName;
            }

            public string RenderTable()
            {
                var sb = new StringBuilder();
                sb.AppendLine(RenderAnnotations());
                sb.AppendLine(RenderClassName());
                sb.AppendLine("{");
                foreach (var column in Columns)
                {
                    var annotations = column.RenderAnnotations();
                    if (annotations != null)
                    {
                        sb.AppendFormat("   {0}{1}", annotations, Environment.NewLine);
                    }
                    sb.AppendFormat("   {0}{1}", column.RenderProperty(), Environment.NewLine);
                }
                sb.AppendLine("}");
                return sb.ToString();
            }
        }

        public string Generate(string connectionStringName = null, string @namespace = null, string classNamePrefix = DefaultTableClassNamePrefix, bool normalizePrimaryKey = false)
        {
            var w = new StringWriter();
            Generate(w, connectionStringName, @namespace, classNamePrefix, normalizePrimaryKey);
            return w.GetStringBuilder().ToString();
        }
        public string Generate(DbProviderFactory factory, string connectionString, string @namespace = null, string classNamePrefix = DefaultTableClassNamePrefix, bool normalizePrimaryKey = false)
        {
           var w = new StringWriter();
           Generate(w, factory, connectionString, @namespace, classNamePrefix, normalizePrimaryKey);
           return w.GetStringBuilder().ToString();
        }
        public void Generate(TextWriter writer, string connectionStringName = null, string @namespace = null, string classNamePrefix = DefaultTableClassNamePrefix, bool normalizePrimaryKey = false)
        {
            connectionStringName = connectionStringName ?? ConfigurationManager.ConnectionStrings[ConfigurationManager.ConnectionStrings.Count - 1].Name;
            @namespace = @namespace ?? "Model." + connectionStringName;

            var cs = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (cs == null)
            {
               throw new ApplicationException("ConnectionString " + connectionStringName + " could not be resolved.");
            }
            Generate(writer, DbProviderFactories.GetFactory(cs.ProviderName), cs.ConnectionString, @namespace,classNamePrefix, normalizePrimaryKey);
        }
        public void Generate(TextWriter writer, DbProviderFactory factory, string connectionString, string @namespace = null, string classNamePrefix = DefaultTableClassNamePrefix, bool normalizePrimaryKey = false)
        {
           @namespace = @namespace ?? "Quobs.Entities";

           var schema = GetSchema(factory, connectionString, classNamePrefix, normalizePrimaryKey);
           var namespaces = schema.SelectMany(t => t.Columns.Select(c => c.TypeNameSpace())).Distinct().ToArray();
           foreach (var ns in namespaces)
           {
              writer.WriteLine("using {0};", ns);
           }
           writer.WriteLine("using XAdo.Quobs.Attributes;");
           writer.WriteLine();
           writer.Write("namespace ");
           writer.WriteLine(@namespace);
           writer.WriteLine("{");
           foreach (var table in GetSchema(factory, connectionString, classNamePrefix, normalizePrimaryKey))
           {
              writer.WriteLine("   " + table.RenderTable().Replace("\r\n", "\r\n   "));
           }
           writer.Write("}");
        }

        public IList<DbTable> GetSchema(string connectionStringName, string classNamePrefix = DefaultTableClassNamePrefix, bool normalizePrimaryKey = false)
        {
            var cs = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (cs == null)
            {
                throw new ApplicationException("ConnectionString " + connectionStringName + " could not be resolved.");
            }
            return GetSchema(DbProviderFactories.GetFactory(cs.ProviderName), cs.ConnectionString, classNamePrefix, normalizePrimaryKey);

        }
        public IList<DbTable> GetSchema(DbProviderFactory factory, string connectionString, string classNamePrefix = DefaultTableClassNamePrefix, bool normalizePrimaryKeyName = false)
        {
            var schemaTableList = new List<DbTable>();

           using (var connection = factory.CreateConnection())
           {
              connection.ConnectionString = connectionString;
              connection.Open();
              var schemaTable = connection.GetSchema("TABLES");
              var tables = new DataRow[schemaTable.Rows.Count];
              schemaTable.Rows.CopyTo(tables, 0);
              tables = tables.OrderBy(t => t.ItemArray[2]).ToArray();
              foreach (var table in tables)
              {
                 var schemaName = table.ItemArray[1].ToString();
                 var tableName = table.ItemArray[2].ToString();
                 var fullTableName = schemaName + "." + tableName;
                 var sql = "SELECT * FROM " + fullTableName + " WHERE 1 = 2";
                 using (var command = factory.CreateCommand())
                 {
                    command.Connection = connection;
                    command.CommandText = sql;
                    var adapter = factory.CreateDataAdapter();
                    adapter.SelectCommand = command;
                    var ds = new DataSet();
                    adapter.FillSchema(ds, SchemaType.Mapped, fullTableName);

                    var dbTable = new DbTable(table, classNamePrefix, normalizePrimaryKeyName);

                    foreach (DataColumn dataColumn in ds.Tables[0].Columns)
                    {
                       dbTable.Add(dataColumn);
                    }

                    schemaTableList.Add(dbTable);

                 }
              }
              return schemaTableList;
           }
        }

    }

}
