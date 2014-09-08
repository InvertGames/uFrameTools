using System;
using UnityEngine;

namespace Invert.uFrame.Editor.ViewModels
{
    public class ConnectionViewModel : GraphItemViewModel
    {
        public ConnectorViewModel ConnectorA { get; set; }
        public ConnectorViewModel ConnectorB { get; set; }



        public Action<ConnectionViewModel> Apply { get; set; }
        public Action<ConnectionViewModel> Remove { get; set; }

        public override Vector2 Position { get; set; }
        public Color Color { get; set; }

        public override string Name
        {
            get { return ConnectorA.Name + " -> " + ConnectorB.Name; }
            set { }
        }
    }
}