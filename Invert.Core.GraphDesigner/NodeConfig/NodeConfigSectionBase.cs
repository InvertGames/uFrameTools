using System;
using System.Collections.Generic;

namespace Invert.Core.GraphDesigner
{
    public class NodeConfigSectionBase
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

        public NodeConfigSectionBase()
        {
            InputValidator = (a,b) => true;
            OutputValidator = (a, b) => true;
        }

        public Func<IDiagramNodeItem, IDiagramNodeItem, bool> InputValidator { get; set; }

        public Func<IDiagramNodeItem, IDiagramNodeItem, bool> OutputValidator { get; set; }

        
    }
}