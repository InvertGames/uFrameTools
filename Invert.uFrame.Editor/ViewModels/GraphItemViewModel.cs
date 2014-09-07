namespace Invert.uFrame.Editor.ViewModels
{
    public abstract class GraphItemViewModel<TData> : GraphItemViewModel
    {
        public TData Data
        {
            get { return (TData)DataObject; }
        }
    }
}