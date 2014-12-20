using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Invert.Core.GraphDesigner
{
    public class GenericInheritableNode : GenericNode, IInhertable
    {
        private BaseClassReference _baseReference;
        [Browsable(false)]
        public GenericInheritableNode BaseNode
        {
            get
            {
                return BaseReference.InputFrom<GenericInheritableNode>();
            }
            set
            {
                Graph.ClearInput(BaseReference);
                Graph.AddConnection(value,BaseReference);
            }
        }
        [Browsable(false)]
        public IEnumerable<GenericInheritableNode> BaseNodes
        {
            get
            {
                var baseType = BaseNode;
                while (baseType != null)
                {
                    yield return baseType;
                    baseType = baseType.BaseNode;
                }
            }
        }
        [Browsable(false)]
        public IEnumerable<GenericInheritableNode> BaseNodesWithThis
        {
            get
            {
                yield return this;
                var baseType = BaseNode;
                while (baseType != null)
                {
                    yield return baseType;
                    baseType = baseType.BaseNode;
                }
            }
        }
        [Browsable(false)]
        public IEnumerable<GenericInheritableNode> DerivedNodes
        {
            get
            {
                var derived = Project.NodeItems.OfType<GenericInheritableNode>().Where(p => p.BaseNode == this);
                foreach (var derivedItem in derived)
                {
                    yield return derivedItem;
                    foreach (var another in derivedItem.DerivedNodes)
                    {
                        yield return another;
                    }
                }
            }
        }
        [Browsable(false)]
        [InputSlot("Base Class",OrderIndex = -1)]
        public BaseClassReference BaseReference
        {
            get { return _baseReference ?? (_baseReference = new BaseClassReference() { Node = this }); }
            set { _baseReference = value; }
        }

        public override bool ValidateInput(IDiagramNodeItem a, IDiagramNodeItem b)
        {
            if (b is BaseClassReference)
            {
                if (a.GetType() != b.Node.GetType()) return false;
            }
            
            return base.ValidateInput(a, b);
        }

        public override bool ValidateOutput(IDiagramNodeItem a, IDiagramNodeItem b)
        {
            if (b is BaseClassReference)
            {
                if (BaseNodes.Any(p => p == b.Node)) return false;

                if (a.Node == b.Node) return false; // Can't inherit from the same item
                if (a.GetType() != b.Node.GetType()) return false; // Can't inherit from another type    
            }
            
            return base.ValidateOutput(a, b);
        }
    }
}