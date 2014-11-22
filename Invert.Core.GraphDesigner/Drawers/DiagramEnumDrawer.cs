using Invert.Common;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class DiagramEnumDrawer : DiagramNodeDrawer<EnumNodeViewModel>
    {
        private SectionHeaderDrawer _itemsHeader;

        protected override GUIStyle HeaderStyle
        {
            get { return ElementDesignerStyles.NodeHeader8; }
        }

        public DiagramEnumDrawer()
        {
        }

        public DiagramEnumDrawer(EnumNodeViewModel viewModel)

        {
            ViewModel = viewModel;
        }

    }
}