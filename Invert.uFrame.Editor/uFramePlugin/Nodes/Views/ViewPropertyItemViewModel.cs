namespace Invert.uFrame.Editor.ViewModels
{
    public class ViewPropertyItemViewModel : ItemViewModel<ViewPropertyData>
    {
        public ViewPropertyItemViewModel(ViewPropertyData data, DiagramNodeViewModel nodeViewModel)
            : base(nodeViewModel)
        {
            DataObject = data;
        }
        
    }
}