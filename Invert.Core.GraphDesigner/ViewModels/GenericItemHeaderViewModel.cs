using Invert.uFrame.Editor.ElementDesigner;
using UnityEngine;

namespace Invert.uFrame.Editor.ViewModels
{
    public class GenericItemHeaderViewModel : GraphItemViewModel
    {
        public override ConnectorViewModel InputConnector
        {
            get { return null; }
        }
        public override ConnectorViewModel OutputConnector
        {
            get { return null; }
        }
        public override Vector2 Position { get; set; }
        public override string Name { get; set; }
        public ViewModel NodeViewModel { get; set; }
        public IEditorCommand AddCommand { get; set; }
        public NodeConfig NodeConfig { get; set; }
        public NodeConfigSection SectionConfig { get; set; }
    }
}