using System.Collections.Generic;

namespace Invert.uFrame.Editor.ElementDesigner
{
    public class PopToFilterCommand : ElementsDiagramToolbarCommand, IDynamicOptionsCommand
    {

        public override void Perform(DiagramViewModel node)
        {
            node.Data.PopToFilter(SelectedOption.Name);
        }

        public IEnumerable<UFContextMenuItem> GetOptions(object arg)
        {
            var item = arg as DiagramViewModel;
            if (item == null)
            {
                yield break;
            }
            
            yield return new UFContextMenuItem()
            {
                Name = item.Data.RootFilter.Name, 
                Checked = item.Data.CurrentFilter == item.Data.RootFilter
            };
            foreach (var filter in item.Data.GetFilterPath())
            {
                yield return new UFContextMenuItem()
                {
                    Name = filter.Name,
                    Checked = item.Data.CurrentFilter == filter
                };
            }
        }

        public override ToolbarPosition Position
        {
            get { return ToolbarPosition.Left; }
        }

        public UFContextMenuItem SelectedOption { get; set; }
        public MultiOptionType OptionsType { get{ return MultiOptionType.Buttons; } }
    }
}