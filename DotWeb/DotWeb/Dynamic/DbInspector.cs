using DotWeb.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;

namespace DotWeb
{
    /// <summary>
    /// <para>This class is the inspector engine; its main task is to inspect metadata of a database: tables, columns,
    /// primary keys, and foreign keys. The output of this inspection process is metadata which is stored in
    /// dotwebdb database.</para>
    /// <para>The engine can be run from a dedicated console app, or included in the web application itself
    /// to perform inspection on-demand when application start. The first method is preferable, but the second method is the quickest
    /// way to see the end result.</para>
    /// </summary>
    public class DbInspector
    {
        private DotWebDb dbConfig;
        private string dbName = "";
        private SchemaInfo schemaInfo;
        private int appId = 0;
        private List<FKInfo> fkInfo;

        /// <summary>
        /// Default constructor, initiating schemaInfo object.
        /// </summary>
        public DbInspector()
        {
             schemaInfo = new SchemaInfo();
        }

        /// <summary>
        /// Inspection results are stored transiently to this property before calling next method to save it to the database.
        /// </summary>
        public SchemaInfo SchemaInfo
        {
            get { return schemaInfo; }
        }

        /// <summary>
        /// Ensure that appId key presents in the configuration file. Every web application must has corresponding appId.
        /// </summary>
        /// <param name="context">An instance of <see cref="DotWebDb"/>.</param>
        private void EnsureApp(DotWebDb context)
        {
            var app = context.Apps.SingleOrDefault(a => a.Id == appId);
            if (app == null)
            {
                app = new App();
                // App is initiated with a default value
                var appName = "Sample App " + DateTime.Today.ToShortDateString();
                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["appName"]))
                    // If appName presents in configuration file, use it
                    appName = ConfigurationManager.AppSettings["appName"];

                app.Id = appId;
                app.Name = appName;
                app.Description = "This app was automatically generated from DbInspector, please change the name and description appropriately.";
                context.Apps.Add(app);
                context.SaveChanges();
            }
            schemaInfo.App = app;
        }

        /// <summary>
        /// The main process of this class => inspecting database metadata.
        /// </summary>
        /// <param name="connectionStringName">Connection string name stored in configuration file.</param>
        public void Inspect(string connectionStringName)
        {
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["appId"]))
                throw new ArgumentException("appId must be specified in config.");
            appId = int.Parse(ConfigurationManager.AppSettings["appId"].ToString());
            fkInfo = SqlHelper.GetForeignKeySchemaInfo(connectionStringName);

            using (var connection = SqlHelper.OpenConnection(connectionStringName))
            {
                dbName = connection.Database;
                InspectTables(connection);
                foreach (var tableMeta in schemaInfo.Tables)
                {
                    InspectColumns(connection, tableMeta);
                    InspectIndexColumns(connection, tableMeta);
                    InspectForeignKeys(connection, tableMeta);
                }
            }
        }

        /// <summary>
        /// The first step of the inspection: collect metadata about tables.
        /// </summary>
        /// <param name="connection">Existing database connection.</param>
        private void InspectTables(DbConnection connection)
        {
            var tables = connection.GetSchema("Tables", new string[] { dbName, null, null, "BASE TABLE" });
            Helper.WriteData("Tables", tables);
            foreach (DataRow row in tables.Rows)
            {
                // Exclude entity framework generated__MigrationHistory table
                if (row["TABLE_NAME"].ToString() != "__MigrationHistory")
                {
                    schemaInfo.Tables.Add(new TableMeta()
                    {
                        Name = row["TABLE_NAME"].ToString(),
                        Caption = row["TABLE_NAME"].ToString().ToTitleCase(),
                        SchemaName = row["TABLE_SCHEMA"].ToString(),
                        AppId = appId
                    });
                }
            }
        }

        /// <summary>
        /// Second step of the inspection: collect metadata about columns.
        /// </summary>
        /// <param name="connection">Existing database connection.</param>
        /// <param name="tableMeta">The <see cref="TableMeta"/> in which this column belongs to.</param>
        private void InspectColumns(DbConnection connection, TableMeta tableMeta)
        {
            var columns = connection.GetSchema("Columns", new[] { dbName, null, tableMeta.Name });
            foreach (DataRow row in columns.Rows)
            {
                var columnMeta = new ColumnMeta() { Table = tableMeta };
                columnMeta.Name = row["COLUMN_NAME"].ToString();
                columnMeta.Caption = columnMeta.Name.ToTitleCase();
                columnMeta.DataType = MapDbTypeToClrType(row["DATA_TYPE"].ToString());

                columnMeta.IsRequired = row["IS_NULLABLE"].ToString() == "NO";
                int maxlength = 0;
                int.TryParse(row["CHARACTER_MAXIMUM_LENGTH"].ToString(), out maxlength);
                if (maxlength > 0)
                    columnMeta.MaxLength = maxlength;
                columnMeta.OrderNo = int.Parse(row["ORDINAL_POSITION"].ToString());

                tableMeta.Columns.Add(columnMeta);
            }
        }

        /// <summary>
        /// Inspect primary keys.
        /// </summary>
        /// <param name="connection">Existing database connection.</param>
        /// <param name="tableMeta">The <see cref="TableMeta"/> in which this column belongs to.</param>
        private void InspectIndexColumns(DbConnection connection, TableMeta tableMeta)
        {
            var indexColumns = connection.GetSchema("IndexColumns", new[] { dbName, null, tableMeta.Name });
            foreach (DataRow row in indexColumns.Rows)
            {
                if (row["constraint_name"].ToString().Substring(0, 3) == "PK_")
                {
                    var primaryKey = row[indexColumns.Columns["column_name"]].ToString();
                    var pkColumn = tableMeta.Columns.SingleOrDefault(c => c.Name == primaryKey);
                    if (pkColumn != null)
                        pkColumn.IsPrimaryKey = true;
                }
            }
        }

        /// <summary>
        /// Inspect foreign keys.
        /// </summary>
        /// <param name="connection">Existing database connection.</param>
        /// <param name="tableMeta">The <see cref="TableMeta"/> in which this column belongs to.</param>
        private void InspectForeignKeys(DbConnection connection, TableMeta tableMeta)
        {
            var foreignKeys = connection.GetSchema("ForeignKeys", new[] { dbName, null, tableMeta.Name });
            foreach (DataRow row in foreignKeys.Rows)
            {
                var constraintName = row[foreignKeys.Columns["CONSTRAINT_NAME"]].ToString();
                if (constraintName.Substring(0, 3) == "FK_")
                {
                    var fki = fkInfo.Single(fk => fk.ConstraintName == constraintName);
                    var foreignKey = fki.ColumnName;
                    var tableName = fki.TableName;
                    var refTableName = fki.RefTableName;
                    var refColName = fki.RefColumnName;

                    var foreignKeyColumn = tableMeta.Columns.SingleOrDefault(c => c.Name == foreignKey);
                    if (foreignKeyColumn != null)
                    {
                        foreignKeyColumn.IsForeignKey = true;
                        foreignKeyColumn.ReferenceTable = schemaInfo.Tables.SingleOrDefault(t => t.Name == refTableName);
                        string fkName = foreignKey.ToTitleCase().Replace(refColName, "").TrimEnd();
                        var foreignKeyCaption = string.Concat(tableName.ToTitleCase(), " - ", fkName);
                        foreignKeyColumn.ReferenceTable.Children.Add(
                            new TableMetaRelation() 
                            { 
                                Parent = foreignKeyColumn.ReferenceTable, 
                                Child = tableMeta,
                                ForeignKeyName = foreignKeyColumn.Name,
                                Caption = foreignKeyCaption.TrimEnd() 
                            }
                        );
                    }
                }
            }
        }

        /// <summary>
        /// This is to generate menu items and groups for navigation menu purpose.
        /// </summary>
        private void GenerateNavigationModules()
        {
            foreach (var tableMeta in schemaInfo.Tables)
            {
                string groupName = tableMeta.SchemaName == "dbo" ? schemaInfo.App.DefaultGroupName : tableMeta.SchemaName;
                var group = schemaInfo.App.Groups.SingleOrDefault(
                    g => g.AppId == appId && g.Name.Equals(groupName, StringComparison.InvariantCultureIgnoreCase));
                if (group == null)
                {
                    group = new Group()
                    {
                        AppId = appId,
                        Name = groupName,
                        Title = groupName,
                        OrderNo = schemaInfo.App.Groups.Count == 0 ? 1 : schemaInfo.App.Groups.Count + 1,
                    };
                    schemaInfo.App.Groups.Add(group);
                }

                var module = group.Modules.SingleOrDefault(m => m.TableName.Equals(tableMeta.Name, StringComparison.InvariantCultureIgnoreCase));
                if (module == null)
                    group.Modules.Add(new Module()
                    {
                        TableName = tableMeta.Name,
                        Title = tableMeta.Name.ToTitleCase(),
                        OrderNo = group.Modules.Count == 0 ? 1 : group.Modules.Count + 1,
                        ModuleType = ModuleType.AutoGenerated
                    });
            }
        }

        /// <summary>
        /// Look up display column can be determined manually in DotWeb admin interface. But, upon auto-generation
        /// it will be deducted from column which name is Name or Title. It thera are no such column names, the 
        /// the first column will be used instead.
        /// </summary>
        private void DetermineColumnForLookUpDisplay()
        {
            foreach (var tableMeta in schemaInfo.Tables)
            {
                // Determines lookup display column
                var lookUpColumns = tableMeta.Columns.Where(c => c.Name.Equals("Name", StringComparison.InvariantCultureIgnoreCase)
                    || c.Name.Equals("Title", StringComparison.InvariantCultureIgnoreCase)).ToList();
                if (lookUpColumns.Count > 0)
                    tableMeta.LookUpDisplayColumn = lookUpColumns[0];
                else
                {
                    lookUpColumns = tableMeta.Columns.Where(c => c.DataType == TypeCode.String).ToList();
                    if (lookUpColumns.Count > 0)
                        tableMeta.LookUpDisplayColumn = lookUpColumns[0];
                    else
                        tableMeta.LookUpDisplayColumn = tableMeta.Columns[0];
                }
            }

        }

        /// <summary>
        /// Map database (SQL Server) type to CLR type.
        /// </summary>
        /// <param name="p">String representing database type.</param>
        /// <returns>TypeCode, as the mapping result.</returns>
        private TypeCode MapDbTypeToClrType(string p)
        {
            switch (p.ToUpper())
            {
                case "NVARCHAR":
                case "VARCHAR":
                case "NCHAR":
                case "CHAR":
                    return TypeCode.String;
                case "BIGINT":
                    return TypeCode.Int64;
                case "INT":
                    return TypeCode.Int32;
                case "FLOAT":
                    return TypeCode.Single;
                case "DECIMAL":
                    return TypeCode.Decimal;
                case "DATETIME":
                    return TypeCode.DateTime;
                case "BIT":
                    return TypeCode.Boolean;
                default:
                    return TypeCode.Object;

            }
        }

        /// <summary>
        /// Save the inspection result to database.
        /// </summary>
        /// <param name="sharedContext">Indicates that dbContext used in the process is the shared one.</param>
        public void SaveToConfig(bool sharedContext = false)
        {
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["appId"]))
                throw new ArgumentException("appId must be specified in config.");
            appId = int.Parse(ConfigurationManager.AppSettings["appId"].ToString());
            if (sharedContext && dbConfig == null)
                throw new ArgumentException("Using shared context requires initialization.");

            var context = dbConfig;
            if (!sharedContext)
                context = new DotWebDb();

            EnsureApp(context);
            foreach (var tableMeta in SchemaInfo.Tables)
            {
                TableMeta dbTable = context.Tables
                    .Include(t => t.Columns)
                    .Include(t => t.Children)
                    .Include(t => t.LookUpDisplayColumn)
                    .Include(t => t.App)
                    .SingleOrDefault(t => t.Name == tableMeta.Name && t.AppId == appId);
                if (dbTable == null)
                {
                    dbTable = tableMeta;
                    context.Tables.Add(dbTable);
                }

                foreach (var columnMeta in tableMeta.Columns)
                {
                    ColumnMeta dbColumn = dbTable.Columns.SingleOrDefault(c => c.Name == columnMeta.Name);
                    if (dbColumn == null)
                    {
                        dbColumn = columnMeta;
                        dbTable.Columns.Add(dbColumn);
                    }
                }
            }

            if (context.ChangeTracker.HasChanges())
                context.SaveChanges();

            if (!sharedContext)
                context.Dispose();
        }

        /// <summary>
        /// Load meta data from dotwebdb.
        /// </summary>
        /// <param name="sharedContext">Indicates that dbContext used in the process is the shared one.</param>
        public void LoadFromConfig(bool sharedContext = false)
        {
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["appId"]))
                throw new ArgumentException("appId must be specified in config.");
            appId = int.Parse(ConfigurationManager.AppSettings["appId"].ToString());
            if (sharedContext && dbConfig == null)
                throw new ArgumentException("using shared context requires initialization.");

            var context = dbConfig;
            if (!sharedContext)
                context = new DotWebDb();

            schemaInfo.App = context.Apps
                .Include(a => a.Groups)
                .Include(a => a.Groups.Select(g => g.Modules))
                .SingleOrDefault(a => a.Id == appId);

            schemaInfo.Tables = context.Tables
                .Include(t => t.Columns)
                .Include(t => t.Children)
                .Include(t => t.LookUpDisplayColumn)
                .Include(t => t.App)
                .Where(t => t.AppId == appId).ToList();

            if (!sharedContext)
                context.Dispose();
        }

        /// <summary>
        /// Main entry for generation purpose.
        /// </summary>
        /// <param name="connectionStringName">Connection string name stored in configuration file.</param>
        public void GenerateFromDb(string connectionStringName)
        {
            dbConfig = new DotWebDb();
            EnsureApp(dbConfig);
            Inspect(connectionStringName);
            GenerateNavigationModules();
            SaveToConfig(true);

            DetermineColumnForLookUpDisplay();
            SaveToConfig(true);

            dbConfig.Dispose();
        }

    }
}
