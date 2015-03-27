using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

public class StateMachineNodeViewModel : DiagramNodeViewModel<StateMachineNodeData>
{
    private InputOutputViewModel _headerViewModel;
    private ConnectorViewModel _elementConnector;
    private ConnectorViewModel _stateVariableConnector;
    private InputOutputViewModel _stateVariableHeader;

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
        //if (uFrameEditor.CurrentProject.CurrentFilter == this.GraphItem) return;

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
    public InputOutputViewModel StateVariableHeader
    {
        get
        {
            return _stateVariableHeader ?? (_stateVariableHeader = new InputOutputViewModel()
            {
                DataObject = GraphItem,
                Name = "State Variable Input",
            });
        }
    }
    public InputOutputViewModel ElementConnectionHeader
    {
        get
        {
            return _headerViewModel ?? (_headerViewModel = new InputOutputViewModel()
            {
                DataObject = GraphItem,
                Name = "Element Input",
            });
        }
    }

    public bool IsCurrentFilter
    {
        get { return DiagramViewModel.GraphData.CurrentFilter == GraphItem; }
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

    public void AddTransition()
    {
        GraphItem.Transitions.Add(new StateMachineTransition()
        {
            Node = GraphItem,
            Name = GraphItem.Project.GetUniqueName("Transition"),
            
        });
    }
}