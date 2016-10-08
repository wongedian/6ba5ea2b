﻿using DotWeb.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotWeb
{
    public class Worker
    {
        DbContext dbApp = null;
        DotWebDb dbConfig;
        string outputDirectory = "";
        string templateDirectory = "";
        int appId = 0;
        App app = null;
        Dictionary<string, EntityMeta> entitiesMeta = new Dictionary<string, EntityMeta>();

        public void Generate(string assemblyName)
        {
            try
            {
                dbApp = GetDbContextFromAssembly(assemblyName);
                if (dbApp == null)
                    throw new ArgumentException("ERROR: There is no dbContext descendant class in the assembly.");

                dbConfig = new DotWebDb();

                if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["appId"]))
                    throw new ArgumentException("ERROR: appId must be specified in config.");
                appId = int.Parse(ConfigurationManager.AppSettings["appId"]);
                app = dbConfig.Apps.Find(appId);
                if (app == null)
                    throw new ArgumentException(string.Format("ERROR: appId {0} not found in configdb.", appId));

                EnsureOutputDirectory();
                EnsureTemplateDirectory();
                EmptyOutputDirectory();
                DeleteExistingNavigation();

                var properties = dbApp.GetDbSetProperties();
                var entitySets = properties.ToDictionary(x => x.Name, x => x.GetValue(dbApp, null));

                if (entitySets.Count() == 0)
                    Console.WriteLine("WARNING: No entities in dbContext!");
                else
                {
                    foreach (var entitySetName in entitySets.Keys)
                    {
                        var entityType = GetEntityTypeFromEntitySet(entitySets[entitySetName]);
                        InspectEntity(entitySetName, entityType);
                    }

                    foreach (var entitySetName in entitySets.Keys)
                    {
                        var entityType = GetEntityTypeFromEntitySet(entitySets[entitySetName]);
                        if (!entitiesMeta[entityType.Name].IsScaffold)
                            continue;
                        ReinspectEntity(entityType);
                    }

                    foreach (var entitySetName in entitySets.Keys)
                    {
                        var entityType = GetEntityTypeFromEntitySet(entitySets[entitySetName]);
                        if (!entitiesMeta[entityType.Name].IsScaffold)
                            continue;

                        ProcessEntity(entitySetName, entitiesMeta[entityType.Name]);
                    }
                }
            }
            finally
            {
                if (dbConfig != null)
                    dbConfig.Dispose();
                if (dbApp != null)
                    dbApp.Dispose();
            }
        }

        private void EnsureOutputDirectory()
        {
            outputDirectory = ConfigurationManager.AppSettings["outputDirectory"];
            if (string.IsNullOrEmpty(outputDirectory))
            {
                Console.WriteLine("WARNING: No outputDirectory entry specified, using 'output'.");
                outputDirectory = "output";
            }

            outputDirectory = string.Concat(Helper.GetAssemblyDirectory(), Path.DirectorySeparatorChar, outputDirectory);

            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);
        }

        private void EnsureTemplateDirectory()
        {
            templateDirectory = string.Concat(Helper.GetAssemblyDirectory(), Path.DirectorySeparatorChar, "templates");

            if (!Directory.Exists(templateDirectory))
                throw new DirectoryNotFoundException(templateDirectory);
        }

        private void EmptyOutputDirectory()
        {
            if (ConfigurationManager.AppSettings["emptyOutputDirectoryBeforeGeneration"] == "1" || ConfigurationManager.AppSettings["emptyOutputDirectoryBeforeGeneration"] == "true")
            {
                foreach (var dir in Directory.EnumerateDirectories(outputDirectory))
                    Directory.Delete(dir, true);

                foreach (var file in Directory.EnumerateFiles(outputDirectory))
                    File.Delete(file);
            }
        }

        private void DeleteExistingNavigation()
        {
            var deletedModules = dbConfig.Modules.Include(m => m.Group).Include(m => m.Group.App).Where(m => m.Group.App.Id == appId && m.ModuleType == ModuleType.AutoGenerated).ToList();
            dbConfig.Modules.RemoveRange(deletedModules);
            dbConfig.SaveChanges();

            var emptyGroups = dbConfig.Groups.Include(g => g.Modules).Where(g => g.Modules.Count == 0).ToList();
            dbConfig.Groups.RemoveRange(emptyGroups);
            dbConfig.SaveChanges();
        }

        private DbContext GetDbContextFromAssembly(string assemblyName)
        {
            var assembly = Assembly.LoadFrom(string.Concat(Helper.GetAssemblyDirectory(), Path.DirectorySeparatorChar, assemblyName));
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (type.BaseType == typeof(DbContext))
                {
                    object connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["default"];
                    if (connectionString == null)
                        throw new ArgumentException("ERROR: You must provide 'default' connection string in app.config");

                    return (DbContext)Activator.CreateInstance(type, connectionString.ToString());
                }
            }
            return null;
        }

        private Type GetEntityTypeFromEntitySet(object entitySet)
        {
            try
            {
                var types = entitySet.GetType().GetGenericArguments();
                if (types.Length != 1)
                    throw new ArgumentException("ERROR: entitySet does not meet criteria");

                return types[0];
            }
            catch
            {
                return null;
            }

        }

        private void InspectEntity(string entitySetName, Type entityType)
        {
            string entityName = entityType.Name;
            Console.WriteLine(string.Format("INFO: Inspecting entity {0}", entityName));

            var entityInspector = new EntityInspector(entitiesMeta);
            entityInspector.Inspect(entityType, entitySetName);
        }

        private void ReinspectEntity(Type entityType)
        {
            string entityName = entityType.Name;
            Console.WriteLine(string.Format("INFO: Re-inspecting entity {0}", entityName));

            var entityInspector = new EntityInspector(entitiesMeta);
            entityInspector.Reinspect(entityType);
        }

        private void ProcessEntity(string entitySetName, EntityMeta entityMeta)
        {
            string entityDirectory = string.Concat(outputDirectory, Path.DirectorySeparatorChar, entityMeta.Name);
            if (!Directory.Exists(entityDirectory))
                Directory.CreateDirectory(entityDirectory);

            GenerateListPage(entityMeta, entityDirectory);
            GenerateEditPage(entityMeta, entityDirectory);
            GenerateNavigation(entityMeta);
        }

        private void GenerateListPage(EntityMeta entityMeta, string entityDirectory)
        {
            Console.WriteLine("INFO: Generating List page for {0}", entityMeta.Name);

            string templateName = "List";
            if (entityMeta.ChildProps.Count == 1)
                templateName = "MasterDetail";
            else if (entityMeta.ChildProps.Count > 1)
                templateName = "MultipleDetails";

            var aspx = new StringBuilder(File.ReadAllText(string.Concat(templateDirectory, Path.DirectorySeparatorChar, string.Concat(templateName, ".aspx.txt"))));
            var cs = new StringBuilder(File.ReadAllText(string.Concat(templateDirectory, Path.DirectorySeparatorChar, string.Concat(templateName, ".aspx.cs.txt"))));
            var designer = new StringBuilder(File.ReadAllText(string.Concat(templateDirectory, Path.DirectorySeparatorChar, string.Concat(templateName, ".aspx.designer.cs.txt"))));

            // replace variables from app.config
            ReplaceVariablesFromConfig(new StringBuilder[] { aspx, cs, designer });

            aspx = aspx.Replace("{entity}", entityMeta.Name);
            cs = cs.Replace("{entity}", entityMeta.Name);
            designer = designer.Replace("{entity}", entityMeta.Name);

            aspx = aspx.Replace("{columnList}", GenerateGridColumns(entityMeta).ToString());
            if (entityMeta.ChildProps.Count == 1)
            {
                aspx = aspx.Replace("{detailGrid}", GenerateDetailGridView(entityMeta, entityMeta.ChildProps[0]).ToString());
                aspx = aspx.Replace("{detailDataSource}", GenerateDetailDataSource(entityMeta, entityMeta.ChildProps[0]).ToString());
                cs = cs.Replace("{detailDataSource}", GenerateDetailDataSourceCodeBehind(entityMeta, entityMeta.ChildProps[0]).ToString());
                designer = designer.Replace("{detailDataSource}", GenerateDetailDataSourceDesigner(entityMeta.ChildProps[0]).ToString());
            }
            else if (entityMeta.ChildProps.Count > 1)
            {
                var childPages = new StringBuilder();
                foreach (var childProp in entityMeta.ChildProps)
                    childPages = childPages.Append(GenerateDetailTabPage(entityMeta, childProp));
                aspx = aspx.Replace("{tabPages}", childPages.ToString());

                var detailDataSources = new StringBuilder();
                var detailDataSourcesCodeBehind = new StringBuilder();
                var detailDataSourcesDesigner = new StringBuilder();
                foreach (var childProp in entityMeta.ChildProps)
                {
                    detailDataSources = detailDataSources.Append(GenerateDetailDataSource(entityMeta, childProp));
                    detailDataSourcesCodeBehind = detailDataSourcesCodeBehind.Append(GenerateDetailDataSourceCodeBehind(entityMeta, childProp));
                    detailDataSourcesDesigner = detailDataSourcesDesigner.Append(GenerateDetailDataSourceDesigner(childProp));
                }
                aspx = aspx.Replace("{detailDataSources}", detailDataSources.ToString());
                cs = cs.Replace("{detailDataSources}", detailDataSourcesCodeBehind.ToString());
                designer = designer.Replace("{detailDataSources}", detailDataSourcesDesigner.ToString());
            }

            aspx = aspx.Replace("{lookUpDataSources}", GenerateLookUpDataSources(entityMeta).ToString());
            designer = designer.Replace("{lookUpDataSources}", GenerateLookUpDataSourcesCodeBehind(entityMeta).ToString());

            aspx = aspx.Replace("{keyFieldName}", entityMeta.PrimaryKey);
            aspx = aspx.Replace("{contextTypeName}", dbApp.GetType().FullName);
            aspx = aspx.Replace("{entitySetName}", entityMeta.EntitySetName);

            // write result
            File.WriteAllText(string.Concat(entityDirectory, Path.DirectorySeparatorChar, "List.aspx"), aspx.ToString());
            File.WriteAllText(string.Concat(entityDirectory, Path.DirectorySeparatorChar, "List.aspx.cs"), cs.ToString());
            File.WriteAllText(string.Concat(entityDirectory, Path.DirectorySeparatorChar, "List.aspx.designer.cs"), designer.ToString());
        }

        public void ReplaceVariablesFromConfig(params StringBuilder[] inputBuilders)
        {
            foreach (var builder in inputBuilders)
            {
                var outputBuilder = inputBuilders.Where(ib => ib == builder).Single();
                foreach (string key in ConfigurationManager.AppSettings.Keys)
                    if (key.StartsWith("{") && key.EndsWith("}"))
                        outputBuilder = builder.Replace(key, ConfigurationManager.AppSettings[key]);
            }
        }

        private StringBuilder GenerateGridColumns(EntityMeta entityMeta, EntityMeta parentEntity = null)
        {
            Console.WriteLine("INFO: Generating grid columns for entity {0}", entityMeta.Name);
            var cl = new StringBuilder();

            for (int i = 0; i < entityMeta.EntityProps.Count; i++)
            {
                var entityProp = entityMeta.EntityProps[i];
                if (entityProp.PropType != PropType.Unsupported && entityProp.IsScaffold)
                {
                    if (entityProp.IsForeignKey)
                        continue;
                    else if (entityProp.PropType == PropType.Text || entityProp.PropType == PropType.Number)
                    {
                        bool visible = !(parentEntity != null && entityProp.Name == string.Concat(parentEntity.Name, parentEntity.PrimaryKey));
                        cl = cl.AppendFormat("<dx:GridViewDataTextColumn FieldName=\"{0}\" VisibleIndex=\"{1}\" Visible=\"{2}\"></dx:GridViewDataTextColumn>{3}",
                            entityProp.Name, i, visible, Environment.NewLine);
                    }
                    else if (entityProp.PropType == PropType.LookUp)
                    {
                        if (entityProp.LookUpEntity == null || (parentEntity != null && entityProp.LookUpEntity == parentEntity))
                            continue;

                        var textField = string.IsNullOrEmpty(entityProp.LookUpEntity.LookUpDisplay) ? entityProp.LookUpEntity.PrimaryKey : entityProp.LookUpEntity.LookUpDisplay;
                        cl = cl.AppendFormat("<dx:GridViewDataComboBoxColumn FieldName=\"{0}{1}\" VisibleIndex=\"{2}\">" + Environment.NewLine +
                                                "  <PropertiesComboBox DataSourceID=\"{3}DataSource\" TextField=\"{4}\" ValueField=\"{5}\"></PropertiesComboBox>" + Environment.NewLine +
                                                "</dx:GridViewDataComboBoxColumn>" + Environment.NewLine, entityProp.Name, entityProp.LookUpEntity.PrimaryKey, i,
                                                entityProp.LookUpEntity.EntitySetName, textField, entityProp.LookUpEntity.PrimaryKey);
                    }
                }
            }
            if (cl.Length > 0)
                cl = cl.Replace(Environment.NewLine, "", cl.Length - Environment.NewLine.Length, Environment.NewLine.Length);

            return cl;
        }

        private StringBuilder GenerateDetailTabPage(EntityMeta entityMeta, EntityProp entityProp)
        {
            var result = new StringBuilder();
            result = result.AppendFormat("<dx:TabPage Text=\"{0}\" Visible=\"true\">{1}", entityProp.Name, Environment.NewLine);
            result = result.AppendFormat("    <ContentCollection>{0}", Environment.NewLine);
            result = result.AppendFormat("        <dx:ContentControl runat=\"server\">{0}", Environment.NewLine);
            result = result.Append(GenerateDetailGridView(entityMeta, entityProp));
            result = result.AppendFormat("        </dx:ContentControl>{0}", Environment.NewLine);
            result = result.AppendFormat("    </ContentCollection>{0}", Environment.NewLine);
            result = result.AppendFormat("</dx:TabPage>{0}", Environment.NewLine);

            return result;
        }

        private StringBuilder GenerateDetailGridView(EntityMeta entityMeta, EntityProp entityProp)
        {
            Console.WriteLine("INFO: Generating child grid view for entity {0}", entityProp.CollectionEntity.Name);
            var result = new StringBuilder();
            if (entityProp.PropType != PropType.Collection)
            {
                Console.WriteLine("WARNING: {0} is not collection, skip it.", entityProp.Name);
                return result;
            }
            result = result.Append(File.ReadAllText(string.Concat(templateDirectory, Path.DirectorySeparatorChar, "_detailGridView.aspx.txt")));
            var detailEntity = entityProp.CollectionEntity.EntitySetName.ToCamelCase();
            result = result.Replace("{detailGrid}", string.Concat(detailEntity, "Grid"));
            result = result.Replace("{detailGridDataSource}", string.Concat(detailEntity, "DataSource"));
            result = result.Replace("{detailKeyFieldName}", entityProp.CollectionEntity.PrimaryKey);
            result = result.Replace("{detailColumnList}", GenerateGridColumns(entityProp.CollectionEntity, entityMeta).ToString());

            ReplaceVariablesFromConfig(new StringBuilder[] { result });

            return result;
        }

        private StringBuilder GenerateLookUpDataSources(EntityMeta entityMeta)
        {
            Console.WriteLine("INFO: Generating lookup data sources for entity {0}", entityMeta.Name);
            var ds = new StringBuilder();

            var lookUps = entityMeta.EntityProps.Where(p => p.PropType == PropType.LookUp).Select(x => x.LookUpEntity).Distinct().ToList();
            var childEntities = entityMeta.ChildProps.Where(p => p.PropType == PropType.Collection).Select(x => x.CollectionEntity).ToList();
            foreach (var childEntity in childEntities)
                lookUps.AddRange(childEntity.EntityProps.Where(p => p.PropType == PropType.LookUp).Select(x => x.LookUpEntity).Where(l => l != entityMeta).Distinct().ToList());
            if (lookUps.Count == 0) return ds;
            var result = lookUps.Distinct();

            foreach (var lookUp in result)
            {
                ds = ds.AppendFormat("  <ef:EntityDataSource ID=\"{0}DataSource\" runat=\"server\" ContextTypeName=\"{1}\" EntitySetName=\"{2}\" />{3}",
                        lookUp.EntitySetName.ToCamelCase(), dbApp.GetType().FullName, lookUp.EntitySetName, Environment.NewLine);
            }
            return ds;
        }

        private StringBuilder GenerateDetailDataSource(EntityMeta entityMeta, EntityProp entityProp)
        {
            Console.WriteLine("INFO: Generating detail data source for entity {0}", entityProp.CollectionEntity.Name);
            StringBuilder result = new StringBuilder();
            result = result.AppendFormat("<ef:EntityDataSource ID=\"{0}DataSource\" runat=\"server\" ContextTypeName=\"{1}\" EntitySetName=\"{2}\" EnableInsert=\"true\" EnableUpdate=\"true\" EnableDelete=\"true\"{3}",
                entityProp.CollectionEntity.EntitySetName.ToCamelCase(), dbApp.GetType().FullName, entityProp.CollectionEntity.EntitySetName, Environment.NewLine);
            result = result.AppendFormat("    AutoGenerateWhereClause=\"True\">{0}", Environment.NewLine);
            result = result.AppendFormat("    <WhereParameters>{0}", Environment.NewLine);
            EntityProp primaryKeyProp = entityMeta.EntityProps.Where(p => p.IsPrimaryKey == true).SingleOrDefault();
            if (primaryKeyProp == null)
                throw new ApplicationException(string.Format("{0} does not have primary key", entityMeta.Name));
            result = result.AppendFormat("        <asp:SessionParameter SessionField=\"{0}{1}\" Name=\"{0}{1}\" DbType=\"Int64\"/>{2}",
                entityMeta.Name, entityMeta.PrimaryKey, Environment.NewLine);
            result = result.AppendFormat("    </WhereParameters>{0}", Environment.NewLine);
            result = result.AppendFormat("</ef:EntityDataSource>{0}", Environment.NewLine);

            return result;
        }

        private StringBuilder GenerateDetailDataSourceDesigner(EntityProp entityProp)
        {
            Console.WriteLine("INFO: Generating detail data sources designer-code for property {0}", entityProp.Name);
            var ds = new StringBuilder();
            ds = ds.Append(Environment.NewLine);
            ds = ds.AppendFormat("        /// <summary>{0}", Environment.NewLine);
            ds = ds.AppendFormat("        /// {0}DataSource control.{1}", entityProp.CollectionEntity.EntitySetName, Environment.NewLine);
            ds = ds.AppendFormat("        /// </summary>{0}", Environment.NewLine);
            ds = ds.AppendFormat("        /// <remarks>{0}", Environment.NewLine);
            ds = ds.AppendFormat("        /// Auto-generated field.{0}", Environment.NewLine);
            ds = ds.AppendFormat("        /// To modify move field declaration from designer file to code-behind file.{0}", Environment.NewLine);
            ds = ds.AppendFormat("        /// </remarks>{0}", Environment.NewLine);
            ds = ds.AppendFormat("        protected global::Microsoft.AspNet.EntityDataSource.EntityDataSource {0}DataSource;{1}", entityProp.CollectionEntity.EntitySetName.ToCamelCase(), Environment.NewLine);

            return ds;
        }

        private StringBuilder GenerateDetailDataSourceCodeBehind(EntityMeta entityMeta, EntityProp entityProp)
        {
            Console.WriteLine("INFO: Generating detail data sources code-behind for property {0}", entityProp.Name);
            var ds = new StringBuilder();
            ds = ds.Append(Environment.NewLine);
            ds = ds.AppendFormat("        protected void {0}Grid_DataSelect(object sender, EventArgs e){1}", entityProp.CollectionEntity.EntitySetName.ToCamelCase(), Environment.NewLine);
            ds = ds.Append("        {" + Environment.NewLine);
            ds = ds.AppendFormat("            Session[\"{0}{1}\"] = (sender as ASPxGridView).GetMasterRowKeyValue();{2}", entityMeta.Name, entityMeta.PrimaryKey, Environment.NewLine);
            ds = ds.Append("        }" + Environment.NewLine);

            ds = ds.Append(Environment.NewLine);
            ds = ds.AppendFormat("        protected void {0}Grid_RowInserting(object sender, DevExpress.Web.Data.ASPxDataInsertingEventArgs e){1}", entityProp.CollectionEntity.EntitySetName.ToCamelCase(), Environment.NewLine);
            ds = ds.Append("        {" + Environment.NewLine);
            ds = ds.AppendFormat("            e.NewValues[\"{0}{1}\"] = (sender as ASPxGridView).GetMasterRowKeyValue();{2}", entityMeta.Name, entityMeta.PrimaryKey, Environment.NewLine);
            ds = ds.Append("        }" + Environment.NewLine);

            return ds;
        }

        private StringBuilder GenerateLookUpDataSourcesCodeBehind(EntityMeta entityMeta)
        {
            Console.WriteLine("INFO: Generating lookup data sources code-behind for entity {0}", entityMeta.Name);

            var ds = new StringBuilder();

            IList<EntityMeta> lookUps = entityMeta.EntityProps.Where(p => p.PropType == PropType.LookUp).Select(x => x.LookUpEntity).Distinct().ToList();
            foreach (var lookUp in lookUps)
            {
                ds = ds.AppendFormat("        /// <summary>" + Environment.NewLine +
                                     "        /// {0}DataSource control." + Environment.NewLine +
                                     "        /// </summary>" + Environment.NewLine +
                                     "        /// <remarks>" + Environment.NewLine +
                                     "        /// Auto-generated field." + Environment.NewLine +
                                     "        /// To modify move field declaration from designer file to code-behind file." + Environment.NewLine +
                                     "        /// </remarks>" + Environment.NewLine +
                                     "        protected global::Microsoft.AspNet.EntityDataSource.EntityDataSource {1}DataSource;" + Environment.NewLine +
                                     Environment.NewLine,
                                     lookUp.EntitySetName.ToCamelCase(), lookUp.EntitySetName.ToCamelCase());
            }

            return ds;
        }

        private void GenerateEditPage(EntityMeta entityMeta, string entityDirectory)
        {

        }

        private void GenerateNavigation(EntityMeta entityMeta)
        {
            Console.WriteLine(string.Format("Generate navigation for entity {0}.", entityMeta.Name));
            if (string.IsNullOrEmpty(entityMeta.ScaffoldGroupTitle))
                return;

            var group = dbConfig.Groups.Include(g => g.App).Where(g => g.App.Id == appId && g.Title.Equals(entityMeta.ScaffoldGroupTitle)).SingleOrDefault();
            if (group == null)
            {
                group = new Group() { App = app, Title = entityMeta.ScaffoldGroupTitle, OrderNo = entityMeta.ScaffoldGroupOrderNo };
                dbConfig.Groups.Add(group);
                dbConfig.SaveChanges();
            }

            var module = dbConfig.Modules.Include(m => m.Group).Where(m => m.Title == entityMeta.Name && m.ModuleType == ModuleType.AutoGenerated).SingleOrDefault();
            if (module == null)
            {
                module = new Module() { Group = group, ModuleType = ModuleType.AutoGenerated, Title = entityMeta.Name, OrderNo = entityMeta.ScaffoldModuleOrderNo, ScaffoldEntity = entityMeta.Name };
                dbConfig.Modules.Add(module);
                dbConfig.SaveChanges();
            }
        }
    }
}