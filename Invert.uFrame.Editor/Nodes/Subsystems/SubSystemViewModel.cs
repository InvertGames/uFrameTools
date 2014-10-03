using System;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ViewModels;

public class SubSystemViewModel : DiagramNodeViewModel<SubSystemData>
{
    public SubSystemViewModel(SubSystemData data,DiagramViewModel diagramViewModel) : base(data,diagramViewModel)
    {
        
    }

    public override Type ExportGraphType
    {
        get { return typeof (ExternalSubsystemGraph); }
    }

    public override ConnectorViewModel InputConnector
    {
        get
        {
            if (DiagramViewModel.DiagramData.CurrentFilter == this.GraphItem) return null;
            return base.InputConnector;
        }
    }

    public override ConnectorViewModel OutputConnector
    {
        get
        {
            if (DiagramViewModel.DiagramData.CurrentFilter == this.GraphItem) return null;
            return base.OutputConnector;
        }
    }

    public override bool AllowCollapsing
    {
        get { return true; }
    }

    public void Export(IGraphData data = null)
    {
        var d = data ?? uFrameEditor.CurrentProject.CreateNewDiagram();
        d.AddNode(GraphItemObject);
        d.PositionData[d.RootFilter, GraphItemObject.Identifier] = Position;
        uFrameEditor.CurrentProject.RemoveNode(GraphItemObject);
        uFrameEditor.DesignerWindow.SwitchDiagram(data);
    }

    public void AddInstance(ElementData registeredInstanceData)
    {
        GraphItem.Instances.Add(new RegisteredInstanceData()
        {
            RelatedType = registeredInstanceData.Identifier,
            Name = registeredInstanceData.Name,
            Node = GraphItem,

        });
    }
}