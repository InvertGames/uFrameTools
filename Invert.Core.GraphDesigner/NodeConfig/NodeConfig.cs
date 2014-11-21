using System;
using System.Collections.Generic;
using Invert.Common;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor.Refactoring;
using UnityEngine;

namespace Invert.uFrame.Editor
{
    public abstract class NodeConfig
    {
        protected NodeConfig(IUFrameContainer container)
        {
            Container = container;
        }

        public string Name
        {
            get { return _name ?? NodeType.Name; }
            set { _name = value; }
        }

        public Type NodeType { get; set; }

        
        private List<NodeConfigSection> _sections = new List<NodeConfigSection>();
        private string _name;

        private List<NodeInputConfig> _inputs;
        private List<NodeOutputConfig> _outputs;
        private List<Func<GenericNode, Refactorer>> _refactorers;
        private List<string> _tags;

        public List<NodeConfigSection> Sections
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