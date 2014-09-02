using System;
using System.Collections;
using System.Collections.Generic;
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

    protected override void DataObjectChanged()
    {
        base.DataObjectChanged();
        GraphItems.Clear();
        foreach (var item in Data.GetDiagramItems())
        {
            uFrameEditor.Container.ResolveRelation(item.GetType(), typeof (ViewModel));
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

    

    public DiagramViewModel(IElementDesignerData data)
    {
        DataObject = data;
        
    }


    public ModelCollection<GraphItemViewModel> GraphItems
    {
        get { return _graphItems; }
        set { _graphItems = value; }
    }

    public void Navigate()
    {
        if (SelectedNode == null) return;
        if (SelectedNode.IsFilter)
        {
            if (SelectedNode.GraphItemObject == Data.CurrentFilter)
            {
                Data.PopFilter(null);
            }
            else
            {
                Data.PushFilter(SelectedNode.GraphItemObject as IDiagramFilter);
            }
        }
    }
}