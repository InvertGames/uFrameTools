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
