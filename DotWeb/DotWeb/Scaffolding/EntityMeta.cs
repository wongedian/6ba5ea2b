using DotWeb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotWeb
{
    /// <summary>
    /// This class holds information about meta data of entity, used for code generation / scaffolding.
    /// </summary>
    public class EntityMeta
    {
        public EntityMeta()
        {
            IsScaffold = true;
        }

        /// <summary>
        /// Name of the entity
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Namespace of the entity
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Entityset name, used in DbContext
        /// </summary>
        public string EntitySetName { get; set; }

        /// <summary>
        /// Key property, usually Id or any property anotated with [Key] attribute
        /// </summary>
        public string PrimaryKey { get; set; }

        /// <summary>
        /// For lookup purpose. Lookup value uses PrimaryKey, this is for the lookup display.
        /// </summary>
        public string LookUpDisplay { get; set; }

        public bool IsScaffold { get; set; }

        public string ScaffoldGroupTitle { get; set; }

        public int ScaffoldGroupOrderNo { get; set; }

        public int ScaffoldModuleOrderNo { get; set; }

        public EditMode ScaffoldEditMode { get; set; }

        /// <summary>
        /// List of property info
        /// </summary>
        public IList<EntityProp> EntityProps { get; set; }


        public IList<EntityProp> ChildProps
        {
            get
            {
                return EntityProps.Where(p => p.PropType == PropType.Collection).ToList();
            }
        }
    }
}
