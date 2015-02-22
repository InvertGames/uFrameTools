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

        public override void Refresh(IPlatformDrawer platform, Vector2 position, bool hardRefresh = true)
        {
            base.Refresh(platform, position);
            var size = platform.CalculateSize(ViewModel.Name, CachedStyles.HeaderStyle);
            if (ViewModel.InputConnector != null)
            ViewModel.InputConnector.AlwaysVisible = true;
            if (ViewModel.OutputConnector != null)
                ViewModel.OutputConnector.AlwaysVisible = true;
            Bounds = new Rect(position.x , position.y, size.x + 38, 25);
        
            //if (ViewModel.OutputConnector != null)
            //{
            //    guiStyle.alignment = TextAnchor.MiddleRight;
            //   // Bounds = new Rect(position.x, position.y, size.x -25, 28);
            //}

        }

        public override void OnLayout()
        {
            base.OnLayout();
            ViewModel.ConnectorBounds = new Rect(Bounds.x + 15, Bounds.y, Bounds.width - 28, 28);
        }

        public override void Draw(IPlatformDrawer platform, float scale)
        {
            base.Draw(platform, scale);
         
            var adjusted = new Rect(Bounds);
            adjusted.width -= 40;
            adjusted.x += 15;
            platform.DrawLabel(adjusted.Scale(scale), ViewModel.Name,CachedStyles.HeaderStyle, ViewModel.OutputConnector != null ? DrawingAlignment.MiddleRight : DrawingAlignment.MiddleLeft);

            //GUI.Label(adjusted.Scale(scale), ViewModel.Name, guiStyle);
        }
    }
}