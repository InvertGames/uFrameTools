using Invert.uFrame.Editor.ViewModels;

public class SubSystemViewModel : DiagramNodeViewModel
{
    public SubSystemViewModel(SubSystemData data) : base(data)
    {
        
    }
    public override bool AllowCollapsing
    {
        get { return false; }
    }
}