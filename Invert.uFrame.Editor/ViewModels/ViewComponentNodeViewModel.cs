namespace Invert.uFrame.Editor.ViewModels
{
    public class ViewComponentNodeViewModel : DiagramNodeViewModel
    {
        public ViewComponentNodeViewModel(ViewComponentData data, DiagramViewModel diagramViewModel)
            : base(data,diagramViewModel)
        {
        
        }

        public override bool AllowCollapsing
        {
            get { return false; }
        }

    }
}