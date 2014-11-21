using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor.ViewModels;

namespace Invert.uFrame.Editor.ElementDesigner.Commands
{
    public class AddItemCommand2 : EditorCommand<DiagramViewModel>, IDiagramContextCommand,IDynamicOptionsCommand
    {
        public override void Perform(DiagramViewModel node)
        {
            if (SelectedOption != null)
            {
                var newNodeData = Activator.CreateInstance(SelectedOption.Value as Type) as IDiagramNode;
                
                node.AddNode(newNodeData);
                newNodeData.BeginEditing();
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

            foreach (var nodeType in InvertGraphEditor.AllowedFilterNodes[viewModel.CurrentRepository.CurrentFilter.GetType()])
            {
                if (nodeType.IsAbstract) continue;
                yield return new UFContextMenuItem()
                {
                    Name = "Add " + GetName(nodeType),
                    Value = nodeType
                };
            }
        }

        public string GetName(Type nodeType)
        {
            var config = InvertGraphEditor.Container.Resolve<NodeConfig>(nodeType.Name);
            if (config != null)
            {
                return config.Name;
            }
            return nodeType.Name.Replace("Data", "").Replace("Node", "");
        }
        public UFContextMenuItem SelectedOption { get; set; }
        public MultiOptionType OptionsType { get; private set; }
    }

    public abstract class CrossDiagramCommand : EditorCommand<DiagramNodeViewModel>, IDiagramContextCommand, IDynamicOptionsCommand, IDiagramNodeCommand
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

        public override void Perform(DiagramNodeViewModel diagram)
        {
            if (SelectedOption != null)
            {
                var targetDiagram = SelectedOption.Value as IGraphData;
                var sourceDiagram = diagram.DiagramViewModel.DiagramData;
                var selectedNode = diagram.GraphItemObject;
                Perform(sourceDiagram, selectedNode, targetDiagram);

                //diagram.CurrentRepository.SetItemLocation(newNodeData, uFrameEditor.CurrentMouseEvent.MouseDownPosition);
                //diagram.AddNode(newNodeData);
            }
        }

        public override string CanPerform(DiagramNodeViewModel node)
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
            var viewModel = item as DiagramNodeViewModel;
            if (viewModel == null) yield break;
            var diagrams = viewModel.DiagramViewModel.CurrentRepository.Diagrams;

            foreach (var diagram in diagrams)
            {
                yield return new UFContextMenuItem()
                {
                    Name = this.Path + "/ " + diagram.Name,
                    Value = diagram
                };
            }
        }

        protected abstract void Perform(IGraphData sourceDiagram, IDiagramNode selectedNode, IGraphData targetDiagram);
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

        public override void Perform(DiagramNodeViewModel diagram)
        {
            base.Perform(diagram);
            diagram.IsLocal = false;
        }

        protected override void Perform(IGraphData sourceDiagram, IDiagramNode selectedNode,
            IGraphData targetDiagram)
        {
            sourceDiagram.PushNode(targetDiagram,selectedNode);

        }

        public override string CanPerform(DiagramNodeViewModel node)
        {
            if (!node.IsLocal) return "Node must be local to push it.";
            return base.CanPerform(node);
        }
    }
    public class PullFromCommand : EditorCommand<DiagramNodeViewModel>,  IDiagramNodeCommand
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

        public override void Perform(DiagramNodeViewModel node)
        {
            var sourceDiagram = node.DiagramViewModel.DiagramData;
            var targetDiagram = node.DiagramViewModel.CurrentRepository.Diagrams.FirstOrDefault(p=>p.NodeItems.Contains(node.GraphItemObject));
            if (targetDiagram == null) return;

            targetDiagram.RemoveNode(node.GraphItemObject);
            sourceDiagram.AddNode(node.GraphItemObject);
            node.IsLocal = true;
        }

        public override string CanPerform(DiagramNodeViewModel node)
        {

            if (node.GraphItemObject == node.DiagramViewModel.DiagramData.RootFilter)
                return "This node is the main part of a diagram and can't be removed.";
            if(node.IsLocal) 
                return "The node must be external to pull it.";
            return null;
        }

    }

}
