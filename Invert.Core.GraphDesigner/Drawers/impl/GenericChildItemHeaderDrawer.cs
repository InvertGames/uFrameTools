using System;
using Invert.Common;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class GenericChildItemHeaderDrawer : Drawer<GenericItemHeaderViewModel>
    {

        public GenericChildItemHeaderDrawer(GraphItemViewModel viewModelObject)
            : base(viewModelObject)
        {

        }

        public GenericChildItemHeaderDrawer(DiagramNodeViewModel viewModelObject)
            : base(viewModelObject)
        {
        }


        public delegate void AddItemClickedEventHandler();

        public event AddItemClickedEventHandler OnAddItem;

        protected virtual void OnOnAddItem()
        {
            AddItemClickedEventHandler handler = OnAddItem;
            if (handler != null) handler();
        }

        public override void Refresh(IPlatformDrawer platform, Vector2 position, bool hardRefresh = true)
        {
            base.Refresh(platform, position);
            var width = platform.CalculateSize(ViewModel.Name, CachedStyles.HeaderStyle).x + 12;
            //ElementDesignerStyles.HeaderStyle.CalcSize(new GUIContent(ViewModel.Name)).x + 20);
            HeaderBounds = new Rect(position.x - 2, position.y, width + 6, 25);
            Bounds = new Rect(position.x + 5, position.y, width + 20, 25);
            
        }

        public override void OnLayout()
        {
            base.OnLayout();
            ViewModel.ConnectorBounds = Bounds;
        }

        public Rect HeaderBounds { get; set; }

        public Rect _AddButtonRect;

        public override void Draw(IPlatformDrawer platform, float scale)
        {
            base.Draw(platform, scale);
            var b = new Rect(HeaderBounds);
            b.width = Bounds.width + 4;
            platform.DrawStretchBox(b.Scale(scale), CachedStyles.Item6, 0f);
            //platform.DrawStretchBox(Bounds,CachedStyles.Item1, 0);
            _AddButtonRect = new Rect
            {
                y = Bounds.y + ((Bounds.height/2) - 8),
                x = (Bounds.x + Bounds.width) - 25,
                width = 16,
                height = 16
            };

            platform.DrawLabel(Bounds.Scale(scale), ViewModel.Name, CachedStyles.HeaderStyle);
            
            if (ViewModel.AddCommand != null)
            {
                platform.DoButton(_AddButtonRect.Scale(scale), string.Empty, CachedStyles.AddButtonStyle, () =>
                {
                    this.ViewModel.Select();
                    InvertGraphEditor.ExecuteCommand(ViewModel.AddCommand);
                });
               
            }

        }


        public Type HeaderType { get; set; }


    }
}