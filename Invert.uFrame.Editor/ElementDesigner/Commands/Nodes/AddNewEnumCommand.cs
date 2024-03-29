using UnityEngine;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddNewEnumCommand : AddItemCommand<EnumData>
    {
        public override string Title
        {
            get { return "Add New Enum"; }
        }

        public override void Perform(DiagramViewModel node)
        {
            var data = new EnumData()
            {
                //Data = node.Data,
                Name = node.DiagramData.GetUniqueName("NewEnum"),
                Location = new Vector2(15, 15)
            };
            node.CurrentRepository.AddNode(data);
            //data.Location = node.LastMouseDownPosition;
        }
    }
}