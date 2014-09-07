using System;
using UnityEngine;

namespace Invert.uFrame.Editor.ViewModels
{
    public class ConnectorViewModel : GraphItemViewModel
    {

        public ConnectorViewModel()
        {
            //Strategy = strategy;
        }

        //public IConnectionStrategy Strategy { get; set; }

        public override Vector2 Position { get; set; }

        public Action<ConnectionViewModel> ApplyConnection { get; set; }

        public ConnectorDirection Direction { get; set; }

        public GraphItemViewModel ConnectorFor { get; set; }


        public ConnectorSide Side { get; set; }

        /// <summary>
        /// A percentage value from 0-1f on which to calculate the position
        /// </summary>
        public float SidePercentage { get; set; }

        public bool HasConnections { get; set; }
    }
}