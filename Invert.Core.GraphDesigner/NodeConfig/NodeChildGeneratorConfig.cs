using System;

namespace Invert.uFrame.Editor
{
    public class NodeChildGeneratorConfig
    {
        public virtual Type ChildType { get; set; }
        public IMemberGenerator Generator { get; set; }

    }
}