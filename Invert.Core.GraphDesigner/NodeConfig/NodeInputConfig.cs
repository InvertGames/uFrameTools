using System;

namespace Invert.uFrame.Editor
{
    public class NodeInputConfig
    {

        public ConfigProperty<IDiagramNodeItem, string> Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private ConfigProperty<IDiagramNodeItem, string> _name;

        public NodeInputConfig NameConfig(ConfigProperty<IDiagramNodeItem, string> name)
        {
            Name = name;
            return this;
        }

        public NodeInputConfig NameConfig(string literal)
        {
            Name = new ConfigProperty<IDiagramNodeItem, string>(literal);
            return this;
        }

        public NodeInputConfig NameConfig(Func<IDiagramNodeItem, string> selector)
        {
            Name = new ConfigProperty<IDiagramNodeItem, string>(selector);
            return this;
        }
        //public string Name { get; set; }
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