using Invert.Core.GraphDesigner;
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
                Name = node.DiagramData.GetUniqueName(node.DiagramData.CurrentFilter.Name + "View"),
                Location = new Vector2(15, 15)
            };
            node.CurrentRepository.AddNode(data);

            if (node.DiagramData.CurrentFilter is ElementData)
            {
                var element = node.DiagramData.CurrentFilter as ElementData;
                data.SetElement(element);
            }
            
            //data.Location = node.LastMouseDownPosition;
         
        }
    }
}