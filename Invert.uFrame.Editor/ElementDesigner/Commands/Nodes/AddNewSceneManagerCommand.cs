using UnityEngine;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddNewSceneManagerCommand : AddItemCommand<SceneManagerData>
    {
        public override string Title
        {
            get { return "Add New Scene Manager"; }
        }
        public override void Perform(DiagramViewModel node)
        {
            var data = new SceneManagerData()
            {
                //Data = node.Data,
                Name = node.DiagramData.GetUniqueName("NewSceneManager"),
                Location = new Vector2(15, 15)
            };
            node.CurrentRepository.AddNode(data);
            //data.Location = node.LastMouseDownPosition;
        }
    }
}