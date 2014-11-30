using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

namespace Invert.Core.GraphDesigner
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
        public NodeConfigBase NodeConfig { get; set; }
        public NodeConfigSectionBase SectionConfig { get; set; }
    }
}