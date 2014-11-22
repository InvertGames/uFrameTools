using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddNewElementCommand : AddItemCommand<ElementData>
    {
        public override string Title
        {
            get { return "Add New Element"; }
        }

        public override void Perform(DiagramViewModel node)
        {
            var data = new ElementData
            {
                //Data = node.Data,
                Name = node.DiagramData.GetUniqueName("NewElement"),
                //BaseTypeName = typeof(ViewModel).FullName,
                Dirty = true
            };
            //data.Location = node.LastMouseDownPosition;
            data.Filter.Locations[data] = data.Location;
            node.CurrentRepository.AddNode(data);
        }
    }

}