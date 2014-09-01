using UnityEngine;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddNewSubSystemCommand : AddItemCommand<SubSystemData>
    {
        public override string Title
        {
            get { return "Add New Subsystem"; }
        }
        public override void Perform(ElementsDiagram node)
        {
            var data = new SubSystemData()
            {
                Data = node.Data,
                Name = node.Data.GetUniqueName("New Sub System"),
                Location = new Vector2(15, 15)
            };
            node.Data.AddNode(data);
            data.Location = node.LastMouseDownPosition;
        }
    }
}