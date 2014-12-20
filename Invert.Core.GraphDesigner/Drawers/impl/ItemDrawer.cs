using System;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class ItemDrawer<TViewModel> : ItemDrawer where TViewModel : ItemViewModel
    {
        public TViewModel ViewModel
        {
            get { return ViewModelObject as TViewModel; }
        }

        public ItemDrawer(TViewModel viewModelObject)
            : base(viewModelObject)
        {
        }
    }

    public class ItemDrawer : Drawer
    {
        public ItemDrawer(GraphItemViewModel viewModelObject)
            : base(viewModelObject)
        {
        }

        public override Rect Bounds
        {
            get { return ViewModelObject.Bounds; }
            set { ViewModelObject.Bounds = value; }
        }

        private object _textStyle;
        private object _backgroundStyle;
        private string _cachedName;


        public ItemViewModel ItemViewModel
        {
            get { return this.ViewModelObject as ItemViewModel; }
        }

        public ItemDrawer()
        {
        }

        public virtual int Padding
        {
            get { return 1; }
        }

        public object BackgroundStyle
        {
            get { return _backgroundStyle ?? (_backgroundStyle = CachedStyles.Item4); }
            set { _backgroundStyle = value; }
        }

        public object SelectedItemStyle
        {
            get
            {
                if (ViewModelObject.IsSelected)
                    return CachedStyles.Item1;
                if (ViewModelObject.IsMouseOver)
                    return CachedStyles.Item5;


                return CachedStyles.ClearItemStyle;
            }
            set {  }
        }

        public virtual object TextStyle
        {
            get { return _textStyle ?? (_textStyle = CachedStyles.ClearItemStyle); }
            set { _textStyle = value; }
        }

        public override void OnMouseEnter(MouseEvent e)
        {
            base.OnMouseEnter(e);
            ViewModelObject.IsMouseOver = true;
            //Debug.Log("Mouse Enter Item");
        }

        public override void OnMouseExit(MouseEvent e)
        {
            base.OnMouseExit(e);
            ViewModelObject.IsMouseOver = false;
        }

        public override void OnMouseDown(MouseEvent mouseEvent)
        {
            base.OnMouseDown(mouseEvent);
            ViewModelObject.Select();
            InvertApplication.Log("Selected Item");
        }

        public override void Refresh(IPlatformDrawer platform, Vector2 position)
        {
            base.Refresh(platform, position);
            // Calculate the size of the label and add the padding * 2 for left and right
            var textSize = platform.CalculateSize(ItemViewModel.Name, TextStyle);// TextStyle.CalcSize(new GUIContent(ItemViewModel.Name));
            var width = textSize.x + (Padding * 2);
            var height = textSize.y + (Padding * 2);

            this.Bounds = new Rect(position.x, position.y, width, height);
            _cachedName = ItemViewModel.Name;

        }

        public virtual void DrawOption()
        {

        }

        public void DrawBackground(IPlatformDrawer platform, float scale)
        {
            if (IsSelected)
            {
                platform.DrawStretchBox(Bounds.Scale(scale), CachedStyles.Item1, 12f);
            }
        }

        public override void Draw(IPlatformDrawer platform, float scale)
        {
            base.Draw(platform, scale);
         
            
            DrawName(Bounds.Scale(scale), platform, scale);

            // TODO Platform specific
#if UNITY_DLL
            //GUI.Box(Bounds.Scale(scale), string.Empty, SelectedItemStyle);

            //GUILayout.BeginArea(Bounds.Scale(scale));
            //EditorGUILayout.BeginHorizontal();
            //DrawOption();
            //if (ItemViewModel.IsSelected && ItemViewModel.IsEditable)
            //{
            //    EditorGUI.BeginChangeCheck();
            //    GUI.SetNextControlName("EditingField");
            //    DiagramDrawer.IsEditingField = true;
            //    var newName = EditorGUILayout.TextField(ItemViewModel.Name, ElementDesignerStyles.ItemTextEditingStyle);
            //    if (EditorGUI.EndChangeCheck() && !string.IsNullOrEmpty(newName))
            //    {
            //        ItemViewModel.Rename(newName);
            //    }
            //}
            //else
            //{
            //    GUILayout.Label(ItemViewModel.Label, ElementDesignerStyles.SelectedItemStyle);
            //} 
            //if (ItemViewModel.AllowRemoving && ItemViewModel.IsSelected)
            //    if (GUILayout.Button(string.Empty, ElementDesignerStyles.RemoveButtonStyle.Scale(scale)))
            //    {
            //        InvertGraphEditor.ExecuteCommand(ItemViewModel.RemoveItemCommand);
            //    }
            //EditorGUILayout.EndHorizontal();
            //GUILayout.EndArea();
            //if (!string.IsNullOrEmpty(ItemViewModel.Highlighter))
            //{
            //    var highlighterPosition = new Rect(Bounds) { width = 4 };
            //    highlighterPosition.y += 2;
            //    highlighterPosition.x += 2;
            //    highlighterPosition.height = Bounds.height - 6;
            //    GUI.Box(highlighterPosition.Scale(scale), string.Empty,
            //        ElementDesignerStyles.GetHighlighter(ItemViewModel.Highlighter));
            //}
#endif
        }

        protected void DrawName(Rect rect, IPlatformDrawer platform, float scale)
        {
           
            if (ItemViewModel.IsEditing && ItemViewModel.IsEditable)
            {
                platform.DrawTextbox(ItemViewModel.NodeItem.Identifier, rect, ItemViewModel.Name,
                    CachedStyles.ItemTextEditingStyle,
                    (s, finished) =>
                    {
                        ItemViewModel.Rename(s);
                        if (finished)
                        {
                            ItemViewModel.IsEditing = false;
                        }
                    });
            }
            else
            {
                platform.DrawLabel(rect, _cachedName, CachedStyles.ItemTextEditingStyle, DrawingAlignment.MiddleCenter);
            }
        }

    }
}