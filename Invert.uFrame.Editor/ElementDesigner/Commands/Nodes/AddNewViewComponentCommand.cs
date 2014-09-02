using UnityEngine;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddNewViewComponentCommand : AddItemCommand<ViewComponentData>
    {
        public override string Title
        {
            get { return "Add New View Component"; }
        }
        public override void Perform(DiagramViewModel node)
        {
            var data = new ViewComponentData()
            {
                Data = node.Data,
                Name = node.Data.GetUniqueName("NewViewComponent"),
                Location = new Vector2(15, 15)
            };
            node.Data.AddNode(data);
            //data.Location = node.LastMouseDownPosition;
        }
    }
}