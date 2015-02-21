using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Invert.Core.GraphDesigner
{
    public class GenericInheritableNode : GenericNode, IInhertable
    {
        public sealed override bool AllowMultipleOutputs
        {
            get { return true; }
        }


        private BaseClassReference _baseReference;
        [Browsable(false)]
        public virtual GenericInheritableNode BaseNode
        {
            get
            {
                return this.InputFrom<GenericInheritableNode>();
            }
            set { throw new System.NotImplementedException(); }
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

        public IEnumerable<IDiagramNodeItem> ChildItemsWithInherited
        {
            get { return BaseNodesWithThis.SelectMany(p => p.ChildItems); }
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
        //[Browsable(false)]
        ////[InputSlot("Base Class", OrderIndex = -1)]
        //public BaseClassReference BaseReference
        //{
        //    get { return _baseReference ?? (_baseReference = new BaseClassReference() { Node = this }); }
        //    set { _baseReference = value; }
        //}

        public override bool ValidateInput(IDiagramNodeItem a, IDiagramNodeItem b)
        {
            if (b is GenericInheritableNode)
            {
                if (a.GetType() != b.GetType()) return false;
            }

            return base.ValidateInput(a, b);
        }

        public override bool ValidateOutput(IDiagramNodeItem a, IDiagramNodeItem b)
        {
            if (b is GenericInheritableNode)
            {
                if (BaseNodes.Any(p => p == b)) return false;

                if (a == b) return false; // Can't inherit from the same item
                if (a.GetType() != b.GetType()) return false; // Can't inherit from another type    
            }

            return base.ValidateOutput(a, b);
        }
    }
}