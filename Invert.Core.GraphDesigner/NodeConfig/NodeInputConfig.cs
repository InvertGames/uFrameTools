using System;

namespace Invert.uFrame.Editor
{
    public class NodeInputConfig
    {
        public string Name { get; set; }
        public bool IsInput { get; set; }
        public bool IsOutput { get; set; }
        public string OutputName { get; set; }
        public Type ReferenceType { get; set; }
        public Type SourceType { get; set; }
        public bool IsAlias { get; set; }
        public bool AllowMultiple { get; set; }
        public Func<IDiagramNodeItem, IDiagramNodeItem, bool> Validator { get; set; }
    }
}