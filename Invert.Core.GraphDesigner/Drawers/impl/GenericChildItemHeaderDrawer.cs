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

        public override void Refresh(IPlatformDrawer platform, Vector2 position)
        {
            base.Refresh(platform, position);
            var width = platform.CalculateSize(ViewModel.Name, CachedStyles.HeaderStyle).x + 12;
            //ElementDesignerStyles.HeaderStyle.CalcSize(new GUIContent(ViewModel.Name)).x + 20);

            Bounds = new Rect(position.x + 12, position.y, width + 20, 25);
        }

        public Rect _AddButtonRect;

        public override void Draw(IPlatformDrawer platform, float scale)
        {
            base.Draw(platform, scale);
      
            _AddButtonRect = new Rect
            {
                y = Bounds.y + ((Bounds.height/2) - 8),
                x = (Bounds.x + Bounds.width) - 34,
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