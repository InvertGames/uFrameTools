using UnityEngine;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddNewViewCommand : AddItemCommand<ViewData>
    {
        public override string Title
        {
            get { return "Add New View"; }
        }
        public override void Perform(DiagramViewModel node)
        {
            var data = new ViewData()
            {
                //Data = node.Data,
                Name = node.Data.GetUniqueName(node.Data.CurrentFilter.Name + "View"),
                Location = new Vector2(15, 15)
            };
            node.CurrentRepository.AddNode(data);

            if (node.Data.CurrentFilter is ElementData)
            {
                var element = node.Data.CurrentFilter as ElementData;
                data.SetElement(element);
            }
            
            //data.Location = node.LastMouseDownPosition;
         
        }
    }
}