using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor {
    public class ElementComputedPropertyNode : ElementComputedPropertyNodeBase, ITypedItem {
        private string _type;

        [JsonProperty, NodeProperty(InspectorType.TypeSelection)]
        public string Type
        {
            get { return string.IsNullOrEmpty(_type) ? typeof(bool).Name : _type; }
            set { _type = value; }
        }

        public override IEnumerable<IComputedSubProperties> PossibleProperties
        {
            get
            {
                return InputProperties.Select(p => p.RelatedTypeNode).OfType<ElementNode>().SelectMany(p => p.AllProperties).Cast<IComputedSubProperties>();
            }
        }

        public IEnumerable<PropertyChildItem> InputProperties
        {
            get { return this.InputsFrom<PropertyChildItem>(); }
        }



        public override string RelatedTypeName
        {
            get
            {
                var type = this.OutputTo<IClassTypeNode>();
                if (type != null)
                {
                    return type.ClassName;
                }
                return string.IsNullOrEmpty(Type) ?  "bool" : Type;
            }
        }
    }
}
