using System.Collections.Generic;
using System.Linq;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class SelectDependantPropertiesCommand : EditorCommand<ViewModelPropertyData>, IDynamicOptionsCommand, IDiagramNodeItemCommand
    {
        public override string Group
        {
            get { return "Dependant Properties"; }
        }

        public override void Perform(ViewModelPropertyData node)
        {
            if (node == null) return;
            var property = SelectedOption.Value as string;
            if (property == null)
            {
                return;
            }
            if (node.DependantPropertyIdentifiers.Contains(property))
            {
                node.DependantPropertyIdentifiers.Remove(property);
            }
            else
            {
                node.DependantPropertyIdentifiers.Add(property);
            }
            

        }

        public override string CanPerform(ViewModelPropertyData node)
        {
            if (node == null) 
                return "This operation can only be performed on a view.";
            return null;
        }

        public IEnumerable<UFContextMenuItem> GetOptions(object item)
        {
            var propertyData = item as ViewModelPropertyData;
            if (propertyData == null) yield break;
            var element = propertyData.Node as ElementData;
            if (element == null) yield break;

            var properties = element.AllProperties;
            
            foreach (var property in properties)
            {
                if (property == propertyData) continue;
                if (property.DependantPropertyIdentifiers.Contains(propertyData.Identifier)) continue;
                yield return new UFContextMenuItem()
                {
                    Name = "Dependent On/" + property.Name,
                    Value = property.Identifier,
                    Checked = propertyData.DependantPropertyIdentifiers.Contains(property.Identifier)
                };
            }

        }

        public UFContextMenuItem SelectedOption { get; set; }
        public MultiOptionType OptionsType { get; private set; }
    }
}