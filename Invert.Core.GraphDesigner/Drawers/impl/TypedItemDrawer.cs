using Invert.Common;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class TypedItemDrawer : ItemDrawer
    {
        public TypedItemViewModel TypedItemViewModel
        {
            get { return ViewModelObject as TypedItemViewModel; }
        }

        public TypedItemDrawer(TypedItemViewModel viewModel)
        {
            ViewModelObject = viewModel;
        }

        public override void Refresh(IPlatformDrawer platform, Vector2 position)
        {
            base.Refresh(platform, position);
            var nameSize = platform.CalculateSize(TypedItemViewModel.Name,TextStyle);
            var typeSize = platform.CalculateSize(TypedItemViewModel.TypeLabel, TextStyle);

            Bounds = new Rect(position.x, position.y, 5 + nameSize.x + 5 + typeSize.x + 10, 18);
        }
        
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
            var commandName = ViewModelObject.DataObject.GetType().Name.Replace("Data", "") + "TypeSelection";

            var command = InvertGraphEditor.Container.Resolve<IEditorCommand>(commandName);
            TypedItemViewModel.Select();

            InvertGraphEditor.ExecuteCommand(command);
        }

        public override void Draw(IPlatformDrawer platform, float scale)
        {

            //base.Draw(platform, scale);
            platform.DrawColumns(Bounds.Scale(scale),
                _ => platform.DoButton(_, TypedItemViewModel.RelatedType, CachedStyles.ClearItemStyle, OptionClicked),
                _=>DrawName(_, platform,scale)
                );
        }
    }
}