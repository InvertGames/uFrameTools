using System;
using System.Collections.Generic;
using Invert.Common;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class SectionHeaderDrawer : Drawer<SectionHeaderViewModel>
    {

        public SectionHeaderDrawer(SectionHeaderViewModel viewModelObject)
            : base(viewModelObject)
        {
        }

        public override void Refresh(Vector2 position)
        {
            base.Refresh(position);


            var width = ElementDesignerStyles.HeaderStyle.CalcSize(new GUIContent(ViewModel.Name)).x + 20;

            Bounds = new Rect(position.x, position.y, width, 25);
        }

        public Rect _AddButtonRect;

        public override void Draw(float scale)
        {
            base.Draw(scale);
            var style = ElementDesignerStyles.HeaderStyle;
            _AddButtonRect = new Rect
            {
                y = Bounds.y + ((Bounds.height/2) - 8),
                x = Bounds.x + Bounds.width - 18,
                width = 16,
                height = 16
            };

            //.Scale(scale);
            //style.normal.textColor = textColorStyle.normal.textColor;
            style.fontStyle = FontStyle.Bold;

            GUI.Box(Bounds.Scale(scale), ViewModel.Name, style);

            if (ViewModel.AddCommand != null)
            {
                if (GUI.Button(_AddButtonRect.Scale(scale), string.Empty, ElementDesignerStyles.AddButtonStyle))
                {

                    this.ViewModel.Select();
                    InvertGraphEditor.ExecuteCommand(ViewModel.AddCommand);
                }
            }

        }


        public Type HeaderType { get; set; }


    }
}