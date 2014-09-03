using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Invert.MVVM;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ViewModels;
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
        get { return GraphItems.Where(p => p.IsSelected); }
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
            return uFrameEditor.GetAllCodeGenerators(Settings.CodePathStrategy, Data);
        }
    }
    protected override void DataObjectChanged()
    {
        base.DataObjectChanged();
        GraphItems.Clear();
        foreach (var item in Data.GetDiagramItems())
        {
            uFrameEditor.Container.ResolveRelation(item.GetType(), typeof(ViewModel));
        }
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

    public IElementsDataRepository Repository
    {
        get;
        set;
    }

    public DiagramViewModel(string assetPath)
    {

        var fileExtension = Path.GetExtension(assetPath);
        if (string.IsNullOrEmpty(fileExtension)) fileExtension = ".asset";
        var repositories = uFrameEditor.Container.ResolveAll<IElementsDataRepository>();
        foreach (var elementsDataRepository in repositories)
        {
            var diagram = elementsDataRepository.LoadDiagram(assetPath);

            if (diagram == null) continue;
            Repository = elementsDataRepository;
            DataObject = diagram;
            break;
        }


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
        var connectors = new List<ConnectorViewModel>();
        foreach (var item in Data.GetDiagramItems())
        {
            // Get the ViewModel for the data
            var vm = uFrameEditor.Container.ResolveRelation<ViewModel>(item.GetType(), item, this) as GraphItemViewModel;
            if (vm == null)
                Debug.Log(string.Format("Couldn't find view-model for {0}", item.GetType()));
            
            GraphItems.Add(vm);

            foreach (var strategy in uFrameEditor.ConnectionStrategies)
            {
                strategy.GetConnectors(connectors, vm);
            }
            
        }
        foreach (var item in connectors)
        {
            GraphItems.Add(item);
        }

    }

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
            if (Data is JsonElementDesignerData)
            {
                var dd = Data as JsonElementDesignerData;
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
            var jsonElementDesignerData = (Data) as JsonElementDesignerData;
            if (jsonElementDesignerData != null)
                return jsonElementDesignerData.Error;
            return null;
        }
    }

    public bool NeedsUpgrade
    {
        get
        {
            return Data is ElementDesignerData;
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
        Repository.SaveDiagram(Data);
    }

    public void MarkDirty()
    {
        Repository.MarkDirty(Data);
    }

    public void RecordUndo(string title)
    {
        Repository.RecordUndo(Data, title);
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

    public void UpgradeProject()
    {
        uFrameEditor.ExecuteCommand(new ConvertToJSON());
    }

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
        return Data.GetImportableItems();
    }
}