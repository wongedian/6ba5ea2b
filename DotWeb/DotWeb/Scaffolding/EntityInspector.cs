using DotWeb;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotWeb
{
    public class EntityInspector
    {
        Dictionary<string, EntityMeta> entitiesMeta;

        public EntityInspector(Dictionary<string, EntityMeta> entitiesMeta)
        {
            this.entitiesMeta = entitiesMeta;
        }

        public void Inspect(Type entityType, string entitySetName)
        {
            var entityMeta = new EntityMeta();
            entityMeta.Name = entityType.Name;
            entityMeta.Namespace = entityType.Namespace;
            entityMeta.EntitySetName = entitySetName;
            entityMeta.IsScaffold = !(entityType.GetCustomAttributes(typeof(ScaffoldTableAttribute), false).Where(p => (p as ScaffoldTableAttribute).Scaffold == false).Count() > 0);
            var customAttr = entityType.GetCustomAttributes(typeof(ScaffoldAttribute), false).Where(p => !string.IsNullOrEmpty((p as ScaffoldAttribute).Group)).SingleOrDefault();
            if (customAttr != null)
            {
                var scaffoldAttr = customAttr as ScaffoldAttribute;
                var group = scaffoldAttr.Group.Split(new char[] { '|' });
                entityMeta.ScaffoldGroupTitle = group[0];
                if (group.Length > 1)
                    entityMeta.ScaffoldGroupOrderNo = int.Parse(group[1]);
                entityMeta.ScaffoldModuleOrderNo = scaffoldAttr.OrderNo;
                entityMeta.ScaffoldEditMode = scaffoldAttr.EditMode;
            }
            else
            {
                if (entityMeta.IsScaffold)
                {
                    entityMeta.ScaffoldGroupTitle = "Ungrouped";
                    entityMeta.ScaffoldGroupOrderNo = 999;
                }
            }

            entityMeta.EntityProps = new List<EntityProp>();
            foreach (var propertyInfo in entityType.GetProperties())
                InspectProp(entityMeta, propertyInfo);

            if (string.IsNullOrEmpty(entityMeta.LookUpDisplay))
                entityMeta.LookUpDisplay = entityMeta.EntityProps.Where(p => p.Name == "Name").Select(p => p.Name).SingleOrDefault();

            entitiesMeta[entityMeta.Name] = entityMeta;
        }

        private void InspectProp(EntityMeta entityMeta, PropertyInfo propertyInfo)
        {
            Console.WriteLine("INFO: Inspecting entity {0}, property {1}", entityMeta.Name, propertyInfo.Name);
            var entityProp = new EntityProp();
            entityProp.Name = propertyInfo.Name;
            ConvertPropertyType(entityProp, entityMeta, propertyInfo.PropertyType);
            entityProp.ClrPropType = propertyInfo.PropertyType.Name;
            entityProp.IsPrimaryKey = propertyInfo.Name.ToLower() == "id" || propertyInfo.GetCustomAttributes(typeof(KeyAttribute), false).Length > 0;

            if (entityProp.IsPrimaryKey)
                entityMeta.PrimaryKey = entityProp.Name;
            else if (propertyInfo.GetCustomAttributes(typeof(LookUpDisplayAttribute), false).Length > 0)
                entityMeta.LookUpDisplay = propertyInfo.Name;

            if (propertyInfo.GetCustomAttributes(typeof(ScaffoldColumnAttribute), false).Length > 0)
            {
                var attr = propertyInfo.GetCustomAttributes(typeof(ScaffoldColumnAttribute), false)[0];
                entityProp.IsScaffold = (attr as ScaffoldColumnAttribute).Scaffold;
            }

            entityProp.IsScaffold = propertyInfo.GetCustomAttributes(typeof(ScaffoldColumnAttribute), false).Where(p => (p as ScaffoldColumnAttribute).Scaffold == false).SingleOrDefault() == null;
            entityProp.IsScaffold = propertyInfo.GetCustomAttributes(typeof(NotMappedAttribute), false).SingleOrDefault() == null;

            if (entityProp.IsPrimaryKey)
                entityMeta.EntityProps.Insert(0, entityProp);
            else
                entityMeta.EntityProps.Add(entityProp);
        }

        public void Reinspect(Type entityType)
        {
            if (!entitiesMeta.ContainsKey(entityType.Name))
                throw new ArgumentException(string.Format("ERROR: no {0} in entitiesMeta", entityType.Name));
            var entityMeta = entitiesMeta[entityType.Name];
            foreach (var entityProp in entityMeta.EntityProps)
            {
                Console.WriteLine("INFO: Re-inspecting entity {0}, property {1}", entityMeta.Name, entityProp.Name);
                if (entityProp.PropType == PropType.LookUp)
                {
                    var propertyInfo = entityType.GetProperties().Where(p => p.Name == entityProp.Name).SingleOrDefault();
                    if (propertyInfo == null)
                        throw new ApplicationException(string.Format("ERROR: no such property {0}", entityProp.Name));

                    if (!entitiesMeta.ContainsKey(propertyInfo.PropertyType.Name))
                        Inspect(propertyInfo.PropertyType, null);
                    var lookUpEntity = entitiesMeta[propertyInfo.PropertyType.Name];
                    if (!string.IsNullOrEmpty(lookUpEntity.PrimaryKey))
                    {
                        entityProp.LookUpEntity = lookUpEntity;
                        string foreignKey = string.Concat(entityProp.Name, lookUpEntity.PrimaryKey);
                        foreach (var foreignKeyProperty in entityMeta.EntityProps.Where(p => p.Name == foreignKey).ToList())
                            foreignKeyProperty.IsForeignKey = true;
                    }
                    else
                        entityProp.PropType = PropType.Composite;
                }
                else if (entityProp.PropType == PropType.Collection)
                {
                    var propertyInfo = entityType.GetProperties().Where(p => p.Name == entityProp.Name).SingleOrDefault();
                    if (propertyInfo == null)
                        throw new ApplicationException(string.Format("ERROR: no such property {0}", entityProp.Name));
                    var arg = propertyInfo.PropertyType.GenericTypeArguments[0];
                    if (!entitiesMeta.ContainsKey(arg.Name))
                        Inspect(arg, null);
                    entityProp.CollectionEntity = entitiesMeta[arg.Name];
                }

            }
        }

        private void ConvertPropertyType(EntityProp entityProp, EntityMeta entityMeta, Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.String:
                case TypeCode.Char:
                    entityProp.PropType = PropType.Text;
                    break;
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    entityProp.PropType = PropType.Number;
                    break;
                case TypeCode.DateTime:
                    entityProp.PropType = PropType.Date;
                    break;
                case TypeCode.Boolean:
                    entityProp.PropType = PropType.Boolean;
                    break;
                default:
                    if (type.IsEnum)
                        entityProp.PropType = PropType.Enum;
                    else if (typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType)
                        entityProp.PropType =  PropType.Collection;
                    else if (type.IsClass && type.Namespace == entityMeta.Namespace)
                        entityProp.PropType = PropType.LookUp;
                    else if (type.BaseType == typeof(ValueType))
                        entityProp.PropType = PropType.Text;
                    else
                        entityProp.PropType = PropType.Unsupported;
                    break;
            }
        }
    }
}
