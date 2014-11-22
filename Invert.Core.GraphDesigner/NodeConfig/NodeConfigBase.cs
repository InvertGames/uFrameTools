using System;
using System.Collections.Generic;
using Invert.uFrame;

namespace Invert.Core.GraphDesigner
{
    public abstract class NodeConfigBase
    {
        public Func<IDiagramNodeItem, IDiagramNodeItem, bool> InputValidator { get; set; }
        public Func<IDiagramNodeItem, IDiagramNodeItem, bool> OutputValidator { get; set; }

        protected NodeConfigBase(IUFrameContainer container)
        {
            Container = container;
            InputValidator = (a,b) => true;
            OutputValidator = (a, b) => true;
        }

        public string Name
        {
            get { return _name ?? NodeType.Name; }
            set { _name = value; }
        }

        public Type NodeType { get; set; }

        
        private List<NodeConfigSectionBase> _sections = new List<NodeConfigSectionBase>();
        private string _name;

        private List<NodeInputConfig> _inputs;
        private List<NodeOutputConfig> _outputs;
        private List<Func<GenericNode, Refactorer>> _refactorers;
        private List<string> _tags;

        public List<NodeConfigSectionBase> Sections
        {
            get { return _sections; }
            set { _sections = value; }
        }

        public List<NodeInputConfig> Inputs
        {
            get { return _inputs ?? (_inputs = new List<NodeInputConfig>()); }
            set { _inputs = value; }
        }

        public List<NodeOutputConfig> Outputs
        {
            get { return _outputs ??(_outputs = new List<NodeOutputConfig>()); }
            set { _outputs = value; }
        }

        //public NodeColor Color { get; set; }
        public List<Func<GenericNode, Refactorer>> Refactorers
        {
            get { return _refactorers ?? new List<Func<GenericNode, Refactorer>>(); }
            set { _refactorers = value; }
        }


        public IUFrameContainer Container { get; set; }

        public List<string> Tags
        {
            get { return _tags ?? (_tags = new List<string>()); }
            set { _tags = value; }
        }

        public abstract bool IsValid(GenericNode node);

        

    }

}