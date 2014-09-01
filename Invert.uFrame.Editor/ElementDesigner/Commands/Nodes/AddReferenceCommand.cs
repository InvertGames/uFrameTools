using System.Collections.Generic;
using UnityEngine;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddReferenceCommand : EditorCommand<IElementDesignerData>,
        IToolbarCommand, IDiagramContextCommand, IDynamicOptionsCommand
    {
        public override void Perform(IElementDesignerData node)
        {
            var diagram = SelectedOption.Value as IElementDesignerData;
            if (diagram != null)
            {
                if (node.ExternalReferences.Contains(diagram.Identifier))
                {
                    node.ExternalReferences.Remove(diagram.Identifier);
                }
                else
                {
                    node.ExternalReferences.Add(diagram.Identifier);
                }
            }
               
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
            if (designerData == null) yield break;
            foreach (var importable in UFrameAssetManager.Diagrams)
            {
                yield return new UFContextMenuItem()
                {
                    Name = "Add Reference/" + importable.Name,
                    Value = importable,
                    Checked = designerData.ExternalReferences.Contains(importable.Identifier)
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