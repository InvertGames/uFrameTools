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
}