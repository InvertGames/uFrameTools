using System;
using System.Collections.Generic;

namespace Invert.uFrame.Editor
{
    public class NodeConfigSection<TNode> : NodeConfigSection where TNode : GenericNode
    {
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
    public class NodeConfigSection
    {
        public bool IsProxy { get; set; }
        private bool _allowAdding = true;

        public string Name { get; set; }

        public bool CopyLocal { get; set; }

        public Type ChildType { get; set; }

        public bool AllowAdding
        {
            get { return _allowAdding; }
            set { _allowAdding = value; }
        }

        public Func<GenericNode, IEnumerable<IGraphItem>> GenericSelector { get; set; }
        public Type ReferenceType { get; set; }
        public bool AllowDuplicates { get; set; }
        
    }
}