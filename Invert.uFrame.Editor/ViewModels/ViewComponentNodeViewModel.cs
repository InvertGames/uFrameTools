namespace Invert.uFrame.Editor.ViewModels
{
    public class ViewComponentNodeViewModel : DiagramNodeViewModel
    {
        public ViewComponentNodeViewModel(ViewComponentData data) : base(data)
        {
        
        }

        public override bool AllowCollapsing
        {
            get { return false; }
        }

    }
}