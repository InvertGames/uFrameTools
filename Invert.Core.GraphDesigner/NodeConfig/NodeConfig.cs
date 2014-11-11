using System;
using System.Collections.Generic;
using Invert.Common;
using Invert.Core.GraphDesigner;
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

        public NodeColor Color { get; set; }

        public GUIStyle NodeStyle
        {
            get
            {
                switch (Color)
                {
                    case NodeColor.DarkGray:
                        return ElementDesignerStyles.NodeHeader1;
                    case NodeColor.Blue:
                        return ElementDesignerStyles.NodeHeader2;
                    case NodeColor.Gray:
                        return ElementDesignerStyles.NodeHeader3;
                    case NodeColor.LightGray:
                        return ElementDesignerStyles.NodeHeader4;
                    case NodeColor.Black:
                        return ElementDesignerStyles.NodeHeader5;
                    case NodeColor.DarkDarkGray:
                        return ElementDesignerStyles.NodeHeader6;
                    case NodeColor.Orange:
                        return ElementDesignerStyles.NodeHeader7;
                    case NodeColor.Red:
                        return ElementDesignerStyles.NodeHeader8;
                    case NodeColor.YellowGreen:
                        return ElementDesignerStyles.NodeHeader9;
                    case NodeColor.Green:
                        return ElementDesignerStyles.NodeHeader10;
                    case NodeColor.Purple:
                        return ElementDesignerStyles.NodeHeader11;
                    case NodeColor.Pink:
                        return ElementDesignerStyles.NodeHeader12;
                    case NodeColor.Yellow:
                        return ElementDesignerStyles.NodeHeader13;

                }
                return ElementDesignerStyles.NodeHeader1;
            }
        }

        public IUFrameContainer Container { get; set; }
    }

}