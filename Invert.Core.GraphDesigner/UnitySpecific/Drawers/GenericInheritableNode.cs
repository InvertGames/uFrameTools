using System.Collections.Generic;
using System.Linq;

namespace Invert.Core.GraphDesigner
{
    public class GenericInheritableNode : GenericNode, IInhertable
    {
        private BaseClassReference _baseReference;

        public GenericInheritableNode BaseNode
        {
            get
            {
                return BaseReference.InputFrom<GenericInheritableNode>();
            }
            set
            {
                Diagram.ClearInput(BaseReference);
                Diagram.AddConnection(value,BaseReference);
            }
        }

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

        [InputSlot("Base Class")]
        public BaseClassReference BaseReference
        {
            get { return _baseReference ?? (_baseReference = new BaseClassReference() { Node = this }); }
            set { _baseReference = value; }
        }
    }
}