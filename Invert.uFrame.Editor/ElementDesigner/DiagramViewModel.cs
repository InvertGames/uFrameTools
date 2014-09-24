using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Invert.MVVM;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEngine;
using ViewModel = Invert.uFrame.Editor.ViewModels.ViewModel;

public class DiagramViewModel : ViewModel
{
    private ModelCollection<GraphItemViewModel> _graphItems = new ModelCollection<GraphItemViewModel>();

    public ElementDiagramSettings Settings
    {
        get
        {
            return DiagramData.Settings;
        }
    }
    public IEnumerable<GraphItemViewModel> SelectedGraphItems
    {
        get
        {
            foreach (var item in GraphItems)
            {
                foreach (var child in item.ContentItems)
                {
                    if (child.IsSelected) yield return child;
                }
                if (item.IsSelected)
                {
                    yield return item;
                }

            }
        }
    }
    public GraphItemViewModel SelectedGraphItem
    {
        get { return SelectedGraphItems.FirstOrDefault(); }
    }

    public DiagramNodeViewModel SelectedNode
    {
        get { return SelectedGraphItems.OfType<DiagramNodeViewModel>().FirstOrDefault(); }
    }

    public IEnumerable<GraphItemViewModel> SelectedNodeItems
    {
        get
        {
            return GraphItems.OfType<DiagramNodeViewModel>().SelectMany(p => p.ContentItems).Where(p => p.IsSelected);
        }
    }
    public GraphItemViewModel SelectedNodeItem
    {
        get
        {
            return SelectedNodeItems.FirstOrDefault();
        }
    }

    public IElementDesignerData DiagramData
    {
        get
        {
            return DataObject as IElementDesignerData;
        }
    }

    public IEnumerable<CodeGenerator> CodeGenerators
    {
        get
        {
            return uFrameEditor.GetAllCodeGenerators(CurrentRepository.GeneratorSettings, uFrameEditor.CurrentProject);
        }
    }
    protected override void DataObjectChanged()
    {
        base.DataObjectChanged();
        GraphItems.Clear();

        //foreach (var item in Data.GetDiagramItems())
        //{
        //    uFrameEditor.Container.ResolveRelation(item.GetType(), typeof(ViewModel));
        //}
        //_graphItems.CollectionChangedWith += GraphItemsChanged;

    }

    //private void GraphItemsChanged(ModelCollectionChangeEventWith<GraphItemViewModel> changeArgs)
    //{
    //    foreach (var graphItem in changeArgs.NewItemsOfT)
    //    {
    //        GraphItemAdded(graphItem);
    //    }
    //    foreach (var graphItem in changeArgs.OldItemsOfT)
    //    {
    //        GraphItemRemoved(graphItem);
    //    }
    //}

    //private void GraphItemAdded(GraphItemViewModel graphItem)
    //{

    //}



    //private void GraphItemRemoved(GraphItemViewModel graphItem)
    //{

    //}

    public IProjectRepository CurrentRepository
    {
        get;
        set;
    }

    public DiagramViewModel(IElementDesignerData diagram, IProjectRepository currentRepository)
    {
        var assetPath = AssetDatabase.GetAssetPath(diagram as GraphData);
        var fileExtension = Path.GetExtension(assetPath);


        if (diagram == null) throw new Exception("Diagram not found");
        CurrentRepository = currentRepository;
        DataObject = diagram;
      


     
        DiagramData.Prepare();

    }



    public void Load()
    {
        GraphItems.Clear();
        var connectors = new List<ConnectorViewModel>();

        CurrentNodes = DiagramData.CurrentFilter.FilterItems(CurrentRepository).ToArray();

        foreach (var item in CurrentNodes)
        {
            // Get the ViewModel for the data
            var vm = uFrameEditor.Container.ResolveRelation<ViewModel>(item.GetType(), item, this) as GraphItemViewModel;
            if (vm == null)
            {
                Debug.LogError(string.Format("Couldn't find view-model for {0}", item.GetType()));
                continue;
            }
            GraphItems.Add(vm);
            // Clear the connections on the view-model
            vm.Connectors.Clear();
            vm.GetConnectors(vm.Connectors);
            connectors.AddRange(vm.Connectors);

        }
        foreach (var item in connectors)
        {
            GraphItems.Add(item);
        }

        var connections = new List<ConnectionViewModel>();
        var connectorInfo = new ConnectorInfo(connectors.ToArray());
        foreach (var strategy in uFrameEditor.ConnectionStrategies)
        {
            strategy.GetConnections(connections, connectorInfo);
        }

        foreach (var item in connections)
        {
            GraphItems.Add(item);
            item.ConnectorA.HasConnections = true;
            item.ConnectorB.HasConnections = true;
        }
    }

    public IDiagramNode[] CurrentNodes { get; set; }

    public ModelCollection<GraphItemViewModel> GraphItems
    {
        get { return _graphItems; }
        set { _graphItems = value; }
    }

    public int RefactorCount
    {
        get
        {
            return DiagramData.RefactorCount;
        }
    }

    public string Title
    {
        get
        {
            return DiagramData.Name;
        }
    }

    public bool HasErrors
    {
        get
        {
            if (DiagramData is ElementsGraph)
            {
                var dd = DiagramData as ElementsGraph;
                if (dd.Errors)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public Exception Errors
    {
        get
        {
            var jsonElementDesignerData = (DiagramData) as ElementsGraph;
            if (jsonElementDesignerData != null)
                return jsonElementDesignerData.Error;
            return null;
        }
    }

    public bool NeedsUpgrade
    {
        get
        {
            return string.IsNullOrEmpty(DiagramData.Version) || (Convert.ToDouble(DiagramData.Version) < uFrameVersionProcessor.CURRENT_VERSION_NUMBER && uFrameVersionProcessor.REQUIRE_UPGRADE);
        }
    }

    public void Navigate()
    {
        if (SelectedNode == null) return;
        if (SelectedNode.IsFilter)
        {
            if (SelectedNode.GraphItemObject == DiagramData.CurrentFilter)
            {
                DiagramData.PopFilter(null);
                DiagramData.UpdateLinks();
            }
            else
            {
                DiagramData.PushFilter(SelectedNode.GraphItemObject as IDiagramFilter);
                DiagramData.UpdateLinks();
            }
        }
    }

    public void Save()
    {
        CurrentRepository.SaveDiagram(DiagramData);
    }

    public void MarkDirty()
    {
        CurrentRepository.MarkDirty(DiagramData);
    }

    public void RecordUndo(string title)
    {
        CurrentRepository.RecordUndo(DiagramData, title);
    }

    public void DeselectAll()
    {
        foreach (var item in SelectedGraphItems)
        {
            item.IsSelected = false;
        }
        foreach (var item in GraphItems.OfType<DiagramNodeViewModel>())
        {
            item.EndEditing();
        }
    }

    //public void UpgradeProject()
    //{
    //    uFrameEditor.ExecuteCommand(new ConvertToJSON());
    //}

    public void NothingSelected()
    {
        DeselectAll();
    }

    public void Select(GraphItemViewModel viewModelObject)
    {
        if (SelectedGraphItems.Count() <= 1)
            DeselectAll();

        viewModelObject.IsSelected = true;
    }

    public IEnumerable<IDiagramNode> GetImportableItems()
    {
        return CurrentRepository.GetImportableItems(DiagramData.CurrentFilter);
    }

    public void UpgradeProject()
    {
        uFrameEditor.ExecuteCommand((n) =>
        {
            Process15Uprade();
        });
      
    }

    public void Process15Uprade()
    {
        var nodes = DiagramData.NodeItems.ToArray();
        foreach (var node in nodes.OfType<ElementData>())
        {

            var foundElement =
                nodes.OfType<ElementData>().FirstOrDefault(p => p.AssemblyQualifiedName == node.BaseType);
            
            if (foundElement != null)
                node.BaseIdentifier = foundElement.Identifier;

            foreach (var item in node.ContainedItems.OfType<ITypeDiagramItem>())
            {
                var uFrameType = DiagramData.NodeItems.FirstOrDefault(p => p.Name == item.RelatedType);
                if (uFrameType != null)
                    item.RelatedType = uFrameType.Identifier;
            }
        }

        foreach (var subsystem in nodes.OfType<SubSystemData>())
        {
            var containing = subsystem.GetContainingNodes(CurrentRepository).OfType<ElementData>();
            foreach (var node in containing)
            {
                if (!node.IsMultiInstance)
                {
                    subsystem.Instances.Add(new RegisteredInstanceData()
                    {
                        Node = subsystem,
                        Name = node.Name,
                        RelatedType = node.Identifier
                    });
                }
            }
        }

        foreach (var viewData in nodes.OfType<ViewData>())
        {
            
            
            var newElement = nodes.OfType<ElementData>().FirstOrDefault(p => p.AssemblyQualifiedName == viewData.ForAssemblyQualifiedName);
            if (newElement != null)
            {
                viewData.ForElementIdentifier = newElement.Identifier;
            }

            if (viewData.ViewForElement == null) continue;
            var generators = uFrameEditor.GetBindingGeneratorsFor(viewData.ViewForElement, true, false, true, false).ToArray();

            // Upgrade bindings
            foreach (var item in viewData.ReflectionBindingMethods)
            {
                var generator = generators.FirstOrDefault(p => p.MethodName == item.Name);
                if (generator == null || generator.MethodName.EndsWith("Added") || generator.MethodName.EndsWith("Removed"))
                {
                    //Debug.Log("Generator not found for " + item.Name + " Method. You might need to re-add the binding.");
                    continue;
                }

                viewData.Bindings.Add(new ViewBindingData()
                {
                    GeneratorType = generator.GetType().Name,
                    Name = item.Name,
                    Node = viewData
                });
            }
        }



        DiagramData.Version = uFrameVersionProcessor.CURRENT_VERSION_NUMBER.ToString();
        AssetDatabase.SaveAssets();
    }
    public void AddNode(IDiagramNode newNodeData)
    {
        CurrentRepository.AddNode(newNodeData);
    }
}