using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotWeb
{
    public enum PropType
    {
        Text,
        Number,
        Date,
        Boolean,
        Enum,
        Collection,
        LookUp,
        Composite,
        Unsupported
    }

    public class EntityProp
    {
        public EntityProp()
        {
            IsScaffold = true;
            IsForeignKey = false;
        }

        public bool IsScaffold { get; set; }

        public string Name { get; set; }

        public PropType PropType { get; set; }

        public string ClrPropType { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool IsForeignKey { get; set; }

        public EntityMeta LookUpEntity { get; set; }

        public EntityMeta CollectionEntity { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
