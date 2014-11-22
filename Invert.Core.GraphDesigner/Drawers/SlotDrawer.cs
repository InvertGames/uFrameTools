using Invert.Common;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class SlotDrawer : SlotDrawer<GraphItemViewModel>
    {
        public SlotDrawer(GraphItemViewModel viewModelObject)
            : base(viewModelObject)
        {
        }


    }

    public class SlotDrawer<TViewModel> : Drawer<TViewModel> where TViewModel : GraphItemViewModel
    {
        public SlotDrawer(TViewModel viewModelObject)
            : base(viewModelObject)
        {

        }

        public override Rect Bounds
        {
            get { return ViewModelObject.Bounds; }
            set { ViewModelObject.Bounds = value; }
        }

        public override void Refresh(Vector2 position)
        {
            base.Refresh(position);
            var size = ElementDesignerStyles.HeaderStyle.CalcSize(new GUIContent(ViewModel.Name));

            Bounds = new Rect(position.x + 25, position.y, size.x + 25, 28);


        }

        public override void Draw(float scale)
        {
            base.Draw(scale);
            ViewModel.ConnectorBounds = new Rect(Bounds.x, Bounds.y, Bounds.width - 50, 28);
            GUI.Label(Bounds.Scale(scale), ViewModel.Name, ElementDesignerStyles.HeaderStyle);
        }
    }
}