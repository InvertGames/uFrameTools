using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


namespace Invert.Core.GraphDesigner
{
    public class AddNodeToGraph : EditorCommand<DiagramViewModel>, IDiagramContextCommand,IDynamicOptionsCommand
    {
        public override string Name
        {
            get { return "Add"; }
        }

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
            var filterType = viewModel.CurrentRepository.CurrentFilter.GetType();
            if (!FilterExtensions.AllowedFilterNodes.ContainsKey(filterType))
            {
                InvertApplication.Log(string.Format("The filter type {0} was not find. Make sure the filter is registered, or it has sub nodes for it.", filterType.Name));
                yield break;
            }
            foreach (var nodeType in FilterExtensions.AllowedFilterNodes[filterType])
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
            var config = InvertGraphEditor.Container.Resolve<NodeConfigBase>(nodeType.Name);
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
        public override void Execute(ICommandHandler handler)
        {

            base.Execute(handler);
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
                var sourceDiagram = diagram.DiagramViewModel.GraphData;
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
            var diagrams = viewModel.GraphItemObject.Project.Graphs;

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

    //public class ToExternalGraph :PullFromCommand
    //{
    //    public override string Name
    //    {
    //        get { return "To External Graph"; }
    //    }
    //    public override string Path
    //    {
    //        get { return "To External Graph"; }
    //    }
    //    protected override void Process(DiagramNode node, IGraphData sourceDiagram, IGraphData targetDiagram)
    //    {
    //       var graph = node.Project.CreateNewDiagram(typeof (InvertGraph), node) as IGraphData;
    //       graph.Name = node.Name;

    //       // MoveNode(node, targetDiagram, graph);
    //        var allchildren = GetAllChildNodes(node).ToArray();
    //        foreach (var item in allchildren)
    //        {
    //            graph.AddNode(item);
    //            Debug.Log(string.Format("moving {0} in graph {1} to graph {2}",  item.Name, targetDiagram.Name, node.Name));
    //        }
    //        foreach (var item in allchildren)
    //        {
    //            targetDiagram.RemoveNode(item);
    //        }
    //        // targetDiagram.RemoveNode(node);

    //    }

    //    private IEnumerable<DiagramNode> GetAllChildNodes(DiagramNode node)
    //    {
    //        foreach (var item in node.GetContainingNodesInProject(node.Project).OfType<DiagramNode>())
    //        {
    //            if (item == node) continue;
    //            if (item.Graph != node.Graph) continue;
    //            //if (item.GetParentNodes().Count() > 1)
    //            //{
    //            //    UnityEngine.Debug.Log(string.Format("Skipping {0} because it is located in more than one place.", item.Name));
    //            //    continue;
    //            //}
    //            yield return item;
    //            foreach (var x in GetAllChildNodes(item))
    //            {
    //                yield return x;
    //            }
    //        }
    //    }
    //    private void MoveNode(DiagramNode node, IGraphData sourceDiagram, IGraphData targetDiagram)
    //    {
    //        foreach (var item in node.GetContainingNodes(sourceDiagram).Where(p=>p.Graph == sourceDiagram).OfType<DiagramNode>())
    //        {
    //            if (item == node) continue; 
    //            MoveNode(item, sourceDiagram, targetDiagram);
    //            var positionData = sourceDiagram.PositionData[node, item];
    //            targetDiagram.AddNode(item);
    //            targetDiagram.PositionData[node, item] = positionData;
    //            sourceDiagram.RemoveNode(item);
               
    //        }
    //    }

    //    public override string CanPerform(DiagramNode node)
    //    {
    //        //if (node.Graph != InvertGraphEditor.DesignerWindow.DiagramViewModel.DiagramData)
    //        //{
    //        //    return "Node must be local.";
    //        //}
    //        return null;
    //    }
    //}

    public class PullFromCommand : EditorCommand<DiagramNode>,  IDiagramNodeCommand
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
        public override bool CanProcessMultiple
        {
            get { return false; }
        }

        public override void Perform(DiagramNode node)
        {
            var sourceDiagram = node.Graph;
            var targetDiagram = InvertGraphEditor.DesignerWindow.DiagramViewModel.GraphData;
            if (targetDiagram == null) return;
            var editableFilesBefore = InvertGraphEditor.GetAllFileGenerators(null, node.Project, false).Where(p=>p.Generators.Any(x=>!x.AlwaysRegenerate)).ToArray();
            
            Process(node, sourceDiagram, targetDiagram);

            var editableFilesAfter = InvertGraphEditor.GetAllFileGenerators(null, node.Project, false).Where(p => p.Generators.Any(x => !x.AlwaysRegenerate)).ToArray();
            //if (editableFilesBefore.Any(p => File.Exists(System.IO.Path.Combine(p.AssetPath, p.Filename))))
            //{
            //    InvertGraphEditor.Platform.MessageBox("Move Files",
            //        "Pulling this item causes some output paths to change. I'm going to move them now.", "OK");
            //}
            foreach (var beforeFile in editableFilesBefore)
            {
                var before = beforeFile.Generators.FirstOrDefault();
                if (before == null) continue;
                foreach (var afterFile in editableFilesAfter)
                {
                    var after = afterFile.Generators.FirstOrDefault();
                    if (after == null) continue;

                    if (before.ObjectData == after.ObjectData)
                    {
                      
                        var beforeFilename = beforeFile.SystemPath;
                        var afterFilename = afterFile.SystemPath;
                        if (beforeFilename == afterFilename) continue; // No change in path
                        if (System.IO.Path.GetFileName(beforeFilename) != System.IO.Path.GetFileName(afterFilename))
                            continue; // Filenames aren't the same
                        //InvertApplication.Log(string.Format("Moving {0} to {1}", beforeFilename, afterFilename));
                        if (File.Exists(beforeFilename))
                        File.Move(beforeFilename, afterFilename);
                        var beforeMetaFilename = beforeFilename + ".meta";
                        var afterMetaFilename = afterFilename + ".meta";

                        if (File.Exists(beforeMetaFilename))
                        File.Move(beforeMetaFilename, afterMetaFilename);
                    }
                }
            }
            sourceDiagram.Project.MarkDirty(sourceDiagram);
            sourceDiagram.Project.MarkDirty(targetDiagram);
#if UNITY_DLL
       
            UnityEditor.AssetDatabase.Refresh();
#endif

        }

        protected virtual void Process(DiagramNode node, IGraphData sourceDiagram, IGraphData targetDiagram)
        {
            sourceDiagram.RemoveNode(node, false);
            targetDiagram.AddNode(node);
        }

        public override string CanPerform(DiagramNode node)
        {
            if (node == null) return "Invalid input";
            //if (node == node.Graph.RootFilter)
            //    return "This node is the main part of a diagram and can't be removed.";
            //if (node.Graph != InvertGraphEditor.DesignerWindow.DiagramViewModel.DiagramData) 
            //    return "The node must be external to pull it.";
            return null;
        }


    }

    public class AddReferenceNode : EditorCommand<DiagramViewModel>
    {
      
        public override string Name
        {
            get
            {
                return "Add Type Reference";
            }
        }
        public override void Perform(DiagramViewModel diagram)
        {
    
            InvertGraphEditor.WindowManager.InitItemWindow(InvertApplication.CachedAssemblies.SelectMany(p=>p.GetTypes()).Select(p=>new GraphTypeInfo()
            {
                Name = p.FullName,
                Label = p.Name
            }), _ =>
            {
                var node = new TypeReferenceNode();
                diagram.AddNode(node, new Vector2(15f, 15f));
                node.Name = _.Label;
                node.FullName = _.Name;
            });
            //InvertGraphEditor.WindowManager.InitTypeListWindow(InvertApplication.CachedAssemblies.SelectMany(p=>p=>new GraphTypeInfo()p.DefinedTypes));
            //InvertGraphEditor.WindowManager.TypeInputWindow((g) =>
            //{
            //    var node = new TypeReferenceNode();
            //    diagram.AddNode(node, new Vector2(15f, 15f));
            //    node.Name = g.Name;
                

            //});
        }

        public override string CanPerform(DiagramViewModel node)
        {
            return null;
        }
    }

}
