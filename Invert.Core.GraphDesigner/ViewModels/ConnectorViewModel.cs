using System;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class ConnectorViewModel : GraphItemViewModel
    {

        public ConnectorViewModel()
        {
            //Strategy = strategy;
        }

        //public IConnectionStrategy Strategy { get; set; }

        public override Vector2 Position { get; set; }

        public override string Name
        {
            get
            {
                var item = ConnectorFor.DataObject as IDiagramNodeItem;
                if (item != null && item.Node != item)
                {
                    return string.Format("{0}:{1}", item.Node.Name, item.Name);
                }
                return  ConnectorFor.Name;
            }
            set { }
        }

        public Action<ConnectionViewModel> ApplyConnection { get; set; }

        public ConnectorDirection Direction { get; set; }

        public GraphItemViewModel ConnectorFor { get; set; }

//        public Func<IDiagramNodeItem, IDiagramNodeItem, bool> Validator { get; set; }

        public NodeInputConfig Configuration { get; set; }

        public ConnectorSide Side { get; set; }

        /// <summary>
        /// A percentage value from 0-1f on which to calculate the position
        /// </summary>
        public float SidePercentage { get; set; }

        public bool HasConnections { get; set; }
        public Type ConnectorForType { get; set; }

        public bool AlwaysVisible { get; set; }
        public bool Disabled { get; set; }
    }
}