using Invert.Common;
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

        public override void Refresh(IPlatformDrawer platform, Vector2 position)
        {
            base.Refresh(platform, position);
            var size = platform.CalculateSize(ViewModel.Name, CachedStyles.HeaderStyle);

            Bounds = new Rect(position.x + 25, position.y, size.x + 25, 28);
        
            //if (ViewModel.OutputConnector != null)
            //{
            //    guiStyle.alignment = TextAnchor.MiddleRight;
            //   // Bounds = new Rect(position.x, position.y, size.x -25, 28);
            //}

        }

        public override void Draw(IPlatformDrawer platform, float scale)
        {
            base.Draw(platform, scale);
            ViewModel.ConnectorBounds = new Rect(Bounds.x, Bounds.y, Bounds.width - 50, 28);
            var adjusted = new Rect(Bounds);
            adjusted.width -= 50;
            platform.DrawLabel(adjusted.Scale(scale), ViewModel.Name,CachedStyles.HeaderStyle, ViewModel.OutputConnector != null ? DrawingAlignment.MiddleRight : DrawingAlignment.MiddleLeft);

            //GUI.Label(adjusted.Scale(scale), ViewModel.Name, guiStyle);
        }
    }
}