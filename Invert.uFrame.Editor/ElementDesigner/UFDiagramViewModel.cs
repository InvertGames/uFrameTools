using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Invert.MVVM;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;
using ViewModel = Invert.uFrame.Editor.ViewModels.ViewModel;

public class UFDiagramViewModel : ViewModel
{
    private ModelCollection<GraphItemViewModel> _graphItems = new ModelCollection<GraphItemViewModel>();

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
    //    foreach (var changeArg in changeArgs.NewItemsOfT)
    //    {
    //        GraphItemAdded(changeArg);
    //    }
    //    foreach (var graphItemViewModel in changeArgs.OldItemsOfT)
    //    {
    //        GraphItemRemoved(graphItemViewModel);
    //    }
    //}

    //private void GraphItemAdded(GraphItemViewModel changeArg)
    //{
    //    throw new NotImplementedException();
    //}

    //private void GraphItemRemoved(GraphItemViewModel graphItemViewModel)
    //{
    //    throw new NotImplementedException();
    //}


    public UFDiagramViewModel(IElementDesignerData data)
    {
        DataObject = data;
    }


    public ModelCollection<GraphItemViewModel> GraphItems
    {
        get { return _graphItems; }
        set { _graphItems = value; }
    }
}