using Invert.Common;
using UnityEditor;
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

        private GUIStyle _textStyle;
        private GUIStyle _backgroundStyle;
        private GUIStyle _selectedItemStyle;

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

        public GUIStyle BackgroundStyle
        {
            get { return _backgroundStyle ?? (_backgroundStyle = ElementDesignerStyles.Item4); }
            set { _backgroundStyle = value; }
        }

        public GUIStyle SelectedItemStyle
        {
            get
            {
                if (ViewModelObject.IsSelected)
                    return ElementDesignerStyles.Item1;
                if (ViewModelObject.IsMouseOver)
                    return ElementDesignerStyles.Item5;


                return ElementDesignerStyles.ClearItemStyle;
            }
            set { _selectedItemStyle = value; }
        }

        public virtual GUIStyle TextStyle
        {
            get { return _textStyle ?? (_textStyle = ElementDesignerStyles.ClearItemStyle); }
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
            Debug.Log("Selected Item");
        }

        public override void Refresh(Vector2 position)
        {
            base.Refresh(position);
            // Calculate the size of the label and add the padding * 2 for left and right
            var textSize = TextStyle.CalcSize(new GUIContent(ItemViewModel.Name));
            var width = textSize.x + (Padding * 2);
            var height = textSize.y + (Padding * 2);

            this.Bounds = new Rect(position.x, position.y, width, height);


        }

        public virtual void DrawOption()
        {

        }


        public override void Draw(float scale)
        {
            base.Draw(scale);

            GUI.Box(Bounds.Scale(scale), string.Empty, SelectedItemStyle);

            GUILayout.BeginArea(Bounds.Scale(scale));
            EditorGUILayout.BeginHorizontal();
            DrawOption();
            if (ItemViewModel.IsSelected && ItemViewModel.IsEditable)
            {
                EditorGUI.BeginChangeCheck();
                GUI.SetNextControlName("EditingField");
                DiagramDrawer.IsEditingField = true;
                var newName = EditorGUILayout.TextField(ItemViewModel.Name, ElementDesignerStyles.ItemTextEditingStyle);
                if (EditorGUI.EndChangeCheck() && !string.IsNullOrEmpty(newName))
                {
                    ItemViewModel.Rename(newName);
                }
            }
            else
            {
                GUILayout.Label(ItemViewModel.Label, ElementDesignerStyles.SelectedItemStyle);
            } 
            if (ItemViewModel.AllowRemoving && ItemViewModel.IsSelected)
                if (GUILayout.Button(string.Empty, ElementDesignerStyles.RemoveButtonStyle.Scale(scale)))
                {
                    InvertGraphEditor.ExecuteCommand(ItemViewModel.RemoveItemCommand);
                }
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
            if (!string.IsNullOrEmpty(ItemViewModel.Highlighter))
            {
                var highlighterPosition = new Rect(Bounds) { width = 4 };
                highlighterPosition.y += 2;
                highlighterPosition.x += 2;
                highlighterPosition.height = Bounds.height - 6;
                GUI.Box(highlighterPosition.Scale(scale), string.Empty,
                    ElementDesignerStyles.GetHighlighter(ItemViewModel.Highlighter));
            }



        }

        protected virtual void DrawItemLabel(IDiagramNodeItem item)
        {
        }

        protected virtual void DrawSelectedItem(IDiagramNodeItem nodeItem)
        {

        }

        protected virtual void DrawSelectedItemLabel(IDiagramNodeItem nodeItem)
        {

        }
    }
}