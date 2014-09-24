using System;
using System.Collections.Generic;
using System.Linq;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
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
                newNodeData.Name = uFrameEditor.CurrentProject.GetUniqueName(SelectedOption.Name.Replace("Add ","New"));
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

    public abstract class CrossDiagramCommand : EditorCommand<DiagramViewModel>, IDiagramContextCommand, IDynamicOptionsCommand, IDiagramNodeCommand
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

        public override void Perform(DiagramViewModel diagram)
        {
            if (SelectedOption != null)
            {
                var targetDiagram = SelectedOption.Value as IElementDesignerData;
                var sourceDiagram = diagram.DiagramData;
                var selectedNode = diagram.SelectedNode.GraphItemObject;
                Perform(sourceDiagram, selectedNode, targetDiagram);

                //diagram.CurrentRepository.SetItemLocation(newNodeData, uFrameEditor.CurrentMouseEvent.MouseDownPosition);
                //diagram.AddNode(newNodeData);
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
            get { return "Move To"; }
        }

        public UFContextMenuItem SelectedOption { get; set; }
        public MultiOptionType OptionsType { get; private set; }

        public IEnumerable<UFContextMenuItem> GetOptions(object item)
        {
            var viewModel = item as DiagramViewModel;
            if (viewModel == null) yield break;
            var diagrams = viewModel.CurrentRepository.Diagrams;

            foreach (var diagram in diagrams)
            {
                yield return new UFContextMenuItem()
                {
                    Name = this.Path + "/ " + diagram.Name,
                    Value = diagram
                };
            }
        }

        protected abstract void Perform(IElementDesignerData sourceDiagram, IDiagramNode selectedNode, IElementDesignerData targetDiagram);
    }

    public class PushToCommand : CrossDiagramCommand
    {
        public override string Group
        {
            get { return "Moving"; }
        }
        public override string Path
        {
            get { return "Push To"; }
        }

        protected override void Perform(IElementDesignerData sourceDiagram, IDiagramNode selectedNode,
            IElementDesignerData targetDiagram)
        {
            var position = sourceDiagram.PositionData[sourceDiagram.CurrentFilter, selectedNode.Identifier];
            var sourcePathStrategy = sourceDiagram.CodePathStrategy;
            var targetPathStrategy = targetDiagram.CodePathStrategy;

            var sourceFiles = uFrameEditor.GetAllFileGenerators(uFrameEditor.CurrentProject.GeneratorSettings).Where(p=>!p.AssetPath.EndsWith(".designer.cs"));
            
            sourceDiagram.RemoveNode(selectedNode);
            sourceDiagram.PositionData[sourceDiagram.CurrentFilter, selectedNode.Identifier] = position;
            targetDiagram.AddNode(selectedNode);

        }
    }
    public class PullFromCommand : EditorCommand<DiagramViewModel>,  IDiagramNodeCommand
    {
        public override string Group
        {
            get { return "Moving"; }
        }

        public override string Path
        {
            get { return "Pull From"; }
        }

        public override string Name
        {
            get { return "Pull From Command"; }
        }

        //protected override void Perform(IElementDesignerData sourceDiagram, IDiagramNode selectedNode,
        //    INodeRepository targetDiagram)
        //{
            
        //}

        public override void Perform(DiagramViewModel node)
        {
            var sourceDiagram = node.DiagramData;
            var targetDiagram = node.CurrentRepository.Diagrams.FirstOrDefault(p=>p.NodeItems.Contains(node.SelectedNode.GraphItemObject));
            if (targetDiagram == null) return;

            targetDiagram.RemoveNode(node.SelectedNode.GraphItemObject);
            sourceDiagram.AddNode(node.SelectedNode.GraphItemObject);
        }

        public override string CanPerform(DiagramViewModel node)
        {
            if (node == null) return "Can't be null.";
            return null;
        }

    }
}