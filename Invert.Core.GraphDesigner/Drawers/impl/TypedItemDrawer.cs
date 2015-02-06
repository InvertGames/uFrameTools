using Invert.Common;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class TypedItemDrawer : ItemDrawer
    {
        private Vector2 _nameSize;

        public TypedItemViewModel TypedItemViewModel
        {
            get { return ViewModelObject as TypedItemViewModel; }
        }

        public TypedItemDrawer(TypedItemViewModel viewModel)
        {
            ViewModelObject = viewModel;
        }

        public override void Refresh(IPlatformDrawer platform, Vector2 position, bool hardRefresh = true)
        {
            base.Refresh(platform, position,hardRefresh);
            if (hardRefresh)
            {
                _cachedTypeName = TypedItemViewModel.RelatedType;
                _cachedItemName = TypedItemViewModel.Name;
                _nameSize = platform.CalculateSize(_cachedItemName, CachedStyles.ClearItemStyle);
                _typeSize = platform.CalculateSize(_cachedTypeName, CachedStyles.ItemTextEditingStyle);
            }
            

            Bounds = new Rect(position.x, position.y, _nameSize.x + 5 + _typeSize.x + 40, 18);
        }

        private Vector2 _typeSize;
        private string _cachedTypeName;
        private string _cachedItemName;

        public override void DrawOption()
        {
            base.DrawOption();
            // TODO implement in platform drawer

            //if (GUILayout.Button(
            //    TypedItemViewModel.TypeLabel + (TypedItemViewModel.IsMouseOver ? "..." : string.Empty),
            //    ElementDesignerStyles.ClearItemStyle))
            //{
            //    TypedItemViewModel.NodeViewModel.IsSelected = true;
            //    OptionClicked();
            //}
        }

        public virtual void OptionClicked()
        {
            if (TypedItemViewModel.Data.Precompiled) return;
            var commandName = ViewModelObject.DataObject.GetType().Name.Replace("Data", "") + "TypeSelection";

            var command = InvertGraphEditor.Container.Resolve<IEditorCommand>(commandName);
            TypedItemViewModel.Select();

            InvertGraphEditor.ExecuteCommand(command);
        }

        public override void Draw(IPlatformDrawer platform, float scale)
        {
            DrawBackground(platform,scale);
            var b = new Rect(Bounds);
            b.x += 10;
            b.width -= 20;
            //base.Draw(platform, scale);
            platform.DrawColumns(b.Scale(scale), new float[] { _typeSize.x + 5, _nameSize.x },
                _ => platform.DoButton(_, _cachedTypeName, CachedStyles.ClearItemStyle, OptionClicked),
                _=>DrawName(_, platform,scale,DrawingAlignment.MiddleRight)
                );
        }
    }
}