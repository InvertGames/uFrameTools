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
            return Data.Settings;
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

    public IElementDesignerData Data
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
            return uFrameEditor.GetAllCodeGenerators(CurrentRepository.GeneratorSettings, Settings.CodePathStrategy, Data);
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
        Data.Settings.CodePathStrategy =
             uFrameEditor.Container.Resolve<ICodePathStrategy>(Data.Settings.CodePathStrategyName ?? "Default");

        if (Data.Settings.CodePathStrategy == null)
        {
            Data.Settings.CodePathStrategy = uFrameEditor.Container.Resolve<ICodePathStrategy>("Default");
        }

        Data.Settings.CodePathStrategy.Data = Data;
        Data.Settings.CodePathStrategy.AssetPath =
            assetPath.Replace(string.Format("{0}{1}", Path.GetFileNameWithoutExtension(assetPath), fileExtension), "").Replace("/", Path.DirectorySeparatorChar.ToString());
        Data.Prepare();

    }



    public void Load()
    {
        GraphItems.Clear();
        var connectors = new List<ConnectorViewModel>();

        CurrentNodes = Data.CurrentFilter.FilterItems(CurrentRepository).ToArray();

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
            return Data.RefactorCount;
        }
    }

    public string Title
    {
        get
        {
            return Data.Name;
        }
    }

    public bool HasErrors
    {
        get
        {
            if (Data is ElementsGraph)
            {
                var dd = Data as ElementsGraph;
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
            var jsonElementDesignerData = (Data) as ElementsGraph;
            if (jsonElementDesignerData != null)
                return jsonElementDesignerData.Error;
            return null;
        }
    }

    public bool NeedsUpgrade
    {
        get
        {
            return string.IsNullOrEmpty(Data.Version) || (Convert.ToDouble(Data.Version) < uFrameVersionProcessor.CURRENT_VERSION_NUMBER && uFrameVersionProcessor.REQUIRE_UPGRADE);
        }
    }

    public void Navigate()
    {
        if (SelectedNode == null) return;
        if (SelectedNode.IsFilter)
        {
            if (SelectedNode.GraphItemObject == Data.CurrentFilter)
            {
                Data.PopFilter(null);
                Data.UpdateLinks();
            }
            else
            {
                Data.PushFilter(SelectedNode.GraphItemObject as IDiagramFilter);
                Data.UpdateLinks();
            }
        }
    }

    public void Save()
    {
        CurrentRepository.SaveDiagram(Data);
    }

    public void MarkDirty()
    {
        CurrentRepository.MarkDirty(Data);
    }

    public void RecordUndo(string title)
    {
        CurrentRepository.RecordUndo(Data, title);
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
        return CurrentRepository.GetImportableItems(Data.CurrentFilter);
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
        var nodes = Data.NodeItems.ToArray();
        foreach (var node in nodes.OfType<ElementData>())
        {

            var foundElement =
                nodes.OfType<ElementData>().FirstOrDefault(p => p.AssemblyQualifiedName == node.BaseType);
            
            if (foundElement != null)
                node.BaseIdentifier = foundElement.Identifier;

            foreach (var item in node.ContainedItems.OfType<ITypeDiagramItem>())
            {
                var uFrameType = Data.NodeItems.FirstOrDefault(p => p.Name == item.RelatedType);
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

            // Upgrade bindings
            foreach (var item in viewData.ReflectionBindingMethods)
            {
                viewData.Bindings.Add(new ViewBindingData()
                {
                    Name = item.Name,
                    Node = viewData
                });
            }
        }



        Data.Version = uFrameVersionProcessor.CURRENT_VERSION_NUMBER.ToString();
        AssetDatabase.SaveAssets();
    }
    public void AddNode(IDiagramNode newNodeData)
    {
        CurrentRepository.AddNode(newNodeData);
    }
}