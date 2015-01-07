using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor {
    public class ElementNode : ElementNodeBase, global::Invert.uFrame.Editor.IRegisteredInstance,IClassTypeNode {
        
        public override System.Collections.Generic.IEnumerable<ElementComputedPropertyNode> ComputedProperties {
            get
            {
                return this.ChildItems.OfType<PropertyChildItem>()
                    .SelectMany(p => p.OutputsTo<ElementComputedPropertyNode>())
                    .Distinct();
            }
        }

        public IEnumerable<ITypedItem> AllProperties
        {
            get
            {
                foreach (var item in ComputedProperties)
                    yield return item;
                foreach (var item in Properties)
                    yield return item;
            }
        }
        
 
        public string ClassName
        {
            get { return this.Name + "ViewModel"; }
        }

        public override bool ValidateInput(IDiagramNodeItem arg1, IDiagramNodeItem arg2)
        {
            if (arg1 is GenericTypedChildItem) return true;
            return base.ValidateInput(arg1, arg2);
        }
        public override bool ValidateOutput(IDiagramNodeItem arg1, IDiagramNodeItem arg2)
        {
            if (arg1 is GenericTypedChildItem) return true;
            return base.ValidateOutput(arg1, arg2);
        }
    }
}
