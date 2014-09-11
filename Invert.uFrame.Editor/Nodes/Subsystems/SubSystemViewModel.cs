using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ViewModels;

public class SubSystemViewModel : DiagramNodeViewModel
{
    public SubSystemViewModel(SubSystemData data,DiagramViewModel diagramViewModel) : base(data,diagramViewModel)
    {
        
    }
    public override bool AllowCollapsing
    {
        get { return false; }
    }

    public void Export(IElementDesignerData data = null)
    {
        var d = data ?? uFrameEditor.CurrentProject.CreateNewDiagram();
        d.AddNode(GraphItemObject);
        d.PositionData[d.SceneFlowFilter, GraphItemObject.Identifier] = Position;
        uFrameEditor.CurrentProject.RemoveNode(GraphItemObject);
        uFrameEditor.DesignerWindow.SwitchDiagram(data);
    }
}