using System;
using System.Collections.Generic;

namespace Invert.Core.GraphDesigner
{
    public class NodeConfigSection<TNode> : NodeConfigSectionBase where TNode : GenericNode
    {
        public Action<IDiagramNodeItem> OnAdd { get; set; }

        public Func<TNode, IEnumerable<IGraphItem>> Selector
        {
            get
            {
                if (GenericSelector == null) return null;
                return p => GenericSelector(p);
            }
            set { GenericSelector = p => value(p as TNode); }
        }

       
    }
}