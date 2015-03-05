using System;
using Invert.Common;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class HeaderDrawer : Drawer
    {
        private object _textStyle;
        private object _backgroundStyle;
        private float _padding = 12;

        public virtual float Padding
        {
            get { return _padding; }
            set { _padding = value; }
        }

        public object BackgroundStyle
        {
            get { return _backgroundStyle ?? (_backgroundStyle = CachedStyles.ItemStyle); }
            set { _backgroundStyle = value; }
        }

        public object TextStyle
        {
            get { return _textStyle ?? (_textStyle = CachedStyles.ViewModelHeaderStyle); }
            set { _textStyle = value; }
        }

        public DiagramNodeViewModel NodeViewModel
        {
            get { return ViewModelObject as DiagramNodeViewModel; }
        }

        public override void Refresh(IPlatformDrawer platform, Vector2 position, bool hardRefresh = true)
        {
            base.Refresh(platform, position);
            TextSize = platform.CalculateSize(NodeViewModel.Label, TextStyle); //.CalcSize(new GUIContent(NodeViewModel.Label)));
            var width = TextSize.x + (Padding * 2);

            if (NodeViewModel.IsCollapsed)
            {
                this.Bounds = new Rect(position.x, position.y, width + 12, 25 + (Padding * 2));

            }
            else
            {
                this.Bounds = new Rect(position.x, position.y, width + 12, 32);
            }
            var cb = new Rect(this.Bounds);
            cb.width += 4;
            ViewModelObject.ConnectorBounds = cb;
        }

        public Vector2 TextSize { get; set; }




        public override void Draw(IPlatformDrawer platform, float scale)
        {
            base.Draw(platform, scale);
            platform.DrawNodeHeader(Bounds, BackgroundStyle, NodeViewModel.IsCollapsed, scale);

            // The bounds for the main text
#if UNITY_DLL
            var textBounds = new Rect(Bounds.x, Bounds.y + ((Bounds.height / 2f) - (TextSize.y / 2f)), Bounds.width,
                Bounds.height);
#else 
            var textBounds = Bounds;
#endif

            if (NodeViewModel.IsEditing && NodeViewModel.IsEditable)
            {
                // TODO Platform specific
#if UNITY_DLL
                //UnityEngine.GUI.SetNextControlName("EditingField");
                //DiagramDrawer.IsEditingField = true;
                //UnityEditor.EditorGUI.BeginChangeCheck();

                //var newText = GUI.TextField(textBounds.Scale(scale), NodeViewModel.Name,
                //    ElementDesignerStyles.ViewModelHeaderEditingStyle);

                //if (UnityEditor.EditorGUI.EndChangeCheck())
                //{
                //    NodeViewModel.Rename(newText);
                //    Dirty = true;
                //}
#endif
                //textBounds.y += TextSize.y / 2f;
                platform.DrawTextbox(NodeViewModel.GraphItemObject.Identifier, textBounds.Scale(scale), NodeViewModel.Name, CachedStyles.ViewModelHeaderStyle, (v, finished) =>
                {
                    NodeViewModel.Rename(v);
                    if (finished)
                    {
                        NodeViewModel.EndEditing();
                    }
                });

            }
            else
            {
                //var titleStyle = new GUIStyle(TextStyle);
                //titleStyle.normal.textColor = BackgroundStyle.normal.textColor;
                //titleStyle.alignment = TextAnchor.MiddleCenter;
                //titleStyle.fontSize = Mathf.RoundToInt(12*scale);
                platform.DrawLabel(textBounds.Scale(scale), NodeViewModel.Label ?? string.Empty, TextStyle, DrawingAlignment.MiddleCenter);
                //GUI.Label(textBounds.Scale(scale), NodeViewModel.Label ?? string.Empty, titleStyle);
                //if (NodeViewModel.IsCollapsed)
                //{
                //    textBounds.y += TextSize.y/2f;
                //    //titleStyle.fontSize = Mathf.RoundToInt(10*scale);
                //    //titleStyle.fontStyle = FontStyle.Italic;

                //    GUI.Label(textBounds.Scale(scale), NodeViewModel.SubTitle, titleStyle);
                //}

            }
        }
    }
}