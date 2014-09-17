using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

public class StateMachineNodeViewModel : DiagramNodeViewModel<StateMachineNodeData>
{
    private ConnectorHeaderViewModel _headerViewModel;
    private ConnectorViewModel _elementConnector;
    private ConnectorViewModel _stateVariableConnector;
    private ConnectorHeaderViewModel _stateVariableHeader;

    public StateMachineNodeViewModel(StateMachineNodeData graphItemObject, DiagramViewModel diagramViewModel)
        : base(graphItemObject, diagramViewModel)
    {
    }

    public override ConnectorViewModel InputConnector
    {
        get { return base.InputConnector; }
    }

    protected override void DataObjectChanged()
    {
        base.DataObjectChanged();
        if (uFrameEditor.CurrentProject.CurrentFilter == this.GraphItem) return;
        foreach (var item in GraphItem.GetContainingNodes(uFrameEditor.CurrentProject).OfType<StateMachineStateData>().SelectMany(p=>p.Transitions))
        {
            var vm = GetDataViewModel(item);
            if (vm == null)
            {
                Debug.LogError(string.Format("Couldn't find view-model for {0}", item.GetType()));
                continue;
            }
            ContentItems.Add(vm);
        }
       // ContentItems.Insert(0,ElementConnectionHeader);
      //  ContentItems.Insert(0, StateVariableHeader);
    }
    public ConnectorViewModel StateVariableConnector
    {
        get
        {
            return _stateVariableConnector ?? (_stateVariableConnector = new ConnectorViewModel()
            {
                DataObject = DataObject,
                Direction = ConnectorDirection.Input,
                ConnectorFor = StateVariableHeader,
                Side = ConnectorSide.Left,
                SidePercentage = 0.5f,
            });
        }
        set { _elementConnector = value; }
    }
    public ConnectorViewModel ElementConnector
    {
        get
        {
            return _elementConnector ?? (_elementConnector = new ConnectorViewModel()
            {
                DataObject = DataObject,
                Direction = ConnectorDirection.Input,
                ConnectorFor = ElementConnectionHeader,
                Side = ConnectorSide.Left,
                SidePercentage = 0.5f,
            });
        }
        set { _elementConnector = value; }
    }
    public ConnectorHeaderViewModel StateVariableHeader
    {
        get
        {
            return _stateVariableHeader ?? (_stateVariableHeader = new ConnectorHeaderViewModel()
            {
                DataObject = GraphItem,
                Name = "State Variable Input",
            });
        }
    }
    public ConnectorHeaderViewModel ElementConnectionHeader
    {
        get
        {
            return _headerViewModel ?? (_headerViewModel = new ConnectorHeaderViewModel()
            {
                DataObject = GraphItem,
                Name = "Element Input",
            });
        }
    }

    public bool IsCurrentFilter
    {
        get { return DiagramViewModel.Data.CurrentFilter == GraphItem; }
    }

    //public void AddVariable()
    //{
    //    GraphItem.Variables.Add(new StateMachineVariableData()
    //    {
    //        RelatedType = typeof(string).FullName,
    //        Name = GraphItem.Data.GetUniqueName("Property"),
    //        Node = GraphItem,
            
    //    });
    //}

    public override void GetConnectors(List<ConnectorViewModel> list)
    {
        //list.Add(ElementConnector);
        //list.Add(StateVariableConnector);
        base.GetConnectors(list);
       
    }
}