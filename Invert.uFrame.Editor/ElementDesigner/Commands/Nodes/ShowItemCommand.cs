using System.Collections.Generic;
using UnityEngine;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class ShowItemCommand : EditorCommand<IElementDesignerData>,
        IToolbarCommand, IDiagramContextCommand, IDynamicOptionsCommand
    {
        public override void Perform(IElementDesignerData node)
        {
            var diagramItem = SelectedOption.Value as IDiagramNode;
            node.CurrentFilter.Locations[diagramItem] = new Vector2(0f, 0f);
        }

        public override string CanPerform(IElementDesignerData node)
        {
            if (node == null) return "Designer Data must not be null";
            return null;
        }

        public ToolbarPosition Position
        {
            get
            {
                return ToolbarPosition.Right;
            }
        }

        public IEnumerable<UFContextMenuItem> GetOptions(object item)
        {
            var designerData = item as IElementDesignerData;
            foreach (var importable in designerData.GetImportableItems())
            {
                yield return new UFContextMenuItem()
                {
                    Name = "Show Item/" + importable.Name,
                    Value = importable
                };
            }

        }

        public UFContextMenuItem SelectedOption { get; set; }

        public MultiOptionType OptionsType
        {
            get
            {
                return MultiOptionType.DropDown;
            }
        }
    }
}