using UnityEngine;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddNewViewCommand : AddItemCommand<ViewData>
    {
        public override string Title
        {
            get { return "Add New View"; }
        }
        public override void Perform(ElementsDiagram node)
        {
            var data = new ViewData()
            {
                Data = node.Data,
                Name = node.Data.GetUniqueName(node.Data.CurrentFilter.Name + "View"),
                Location = new Vector2(15, 15)
            };
            node.Data.AddNode(data);

            if (node.Data.CurrentFilter is ElementData)
            {
                var element = node.Data.CurrentFilter as ElementData;
                element.CreateLink(null, data);
            }
            
            data.Location = node.LastMouseDownPosition;
         
        }
    }
}