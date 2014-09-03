using System.Collections.Generic;
using UnityEngine;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class ShowItemCommand : EditorCommand<DiagramViewModel>,
        IToolbarCommand, IDiagramContextCommand, IDynamicOptionsCommand
    {
        public override void Perform(DiagramViewModel node)
        {
            var diagramItem = SelectedOption.Value as IDiagramNode;

            node.Data.CurrentFilter.Locations[diagramItem] = new Vector2(0f, 0f);
        }

        public override string CanPerform(DiagramViewModel node)
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
            var designerData = item as DiagramViewModel;
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