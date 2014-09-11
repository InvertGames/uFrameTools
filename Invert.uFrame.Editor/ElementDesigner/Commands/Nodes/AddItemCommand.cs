using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.uFrame.Editor.ViewModels;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public abstract class AddItemCommand<TType> : AddNewCommand, IDiagramContextCommand
    {
        public override void Execute(object item)
        {
            base.Execute(item);
            var node = item as DiagramViewModel;
            if (node == null) return;

            var data = node.Data.NodeItems.LastOrDefault();
       
            if (data == null) return;
            data.BeginEditing();
        }

        public override string CanPerform(DiagramViewModel node)
        {
            if (node == null) return "Diagram must be loaded first.";

            if (!node.Data.CurrentFilter.IsAllowed(null, typeof(TType)))
                return "Item is not allowed in this part of the diagram.";

            return null;
        }

        public override string Path
        {
            get { return  "Add New/" + Title; }
        }
    }
    public class AddItemCommand2 : EditorCommand<DiagramViewModel>, IDiagramContextCommand,IDynamicOptionsCommand
    {
        public override void Execute(object item)
        {
            
            base.Execute(item);
            //var node = item as DiagramViewModel;
            //if (node == null) return;

            //var data = node.Data.NodeItems.LastOrDefault();

            //if (data == null) return;
            //data.BeginEditing();
        }

        public override void Perform(DiagramViewModel node)
        {
            if (SelectedOption != null)
            {
                var newNodeData = Activator.CreateInstance(SelectedOption.Value as Type) as IDiagramNode;
                newNodeData.Name = uFrameEditor.CurrentProject.GetUniqueName(SelectedOption.Name.Replace("Add ",""));
                node.CurrentRepository.SetItemLocation(newNodeData,uFrameEditor.CurrentMouseEvent.MouseDownPosition);
                node.AddNode(newNodeData);
            }
        }

        public override string CanPerform(DiagramViewModel node)
        {
            if (node == null) return "Diagram must be loaded first.";

            //if (!node.Data.CurrentFilter.IsAllowed(null, typeof(TType)))
            //    return "Item is not allowed in this part of the diagram.";

            return null;
        }

        public override string Path
        {
            get { return "Add New/" + Title; }
        }

        public IEnumerable<UFContextMenuItem> GetOptions(object item)
        {
            var viewModel = item as DiagramViewModel;

            foreach (var nodeType in uFrameEditor.AllowedFilterNodes[viewModel.CurrentRepository.CurrentFilter.GetType()])
            {
                yield return new UFContextMenuItem()
                {
                    Name = "Add " + nodeType.Name.Replace("Data",""),
                    Value = nodeType
                };
            }
        }

        public UFContextMenuItem SelectedOption { get; set; }
        public MultiOptionType OptionsType { get; private set; }
    }
    //public class DeleteCommand : EditorCommand<ISelectable>, IDiagramNodeCommand, IDiagramNodeItemCommand
    //{

    //}
}
