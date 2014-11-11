using UnityEngine;

namespace Invert.uFrame.Editor.ViewModels
{
    public class ConnectorHeaderViewModel : GraphItemViewModel
    {
        public override Vector2 Position { get; set; }
        public override string Name { get; set; }
        public bool IsInput { get; set; }
        public bool IsOutput { get; set; }

        public override ConnectorViewModel InputConnector
        {
            get
            {
                if (!IsInput) return null;
                return base.InputConnector;
            }
        }
        public override ConnectorViewModel OutputConnector
        {
            get
            {
                if (!IsOutput) return null;
                return base.OutputConnector;
            }
        }
    }
}