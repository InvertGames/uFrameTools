namespace Invert.uFrame.Editor.ViewModels
{
    public class ItemViewModel<TData> : ItemViewModel
    {
        public ItemViewModel(DiagramNodeViewModel nodeViewModel)
            : base(nodeViewModel)
        {

        }


        public TData Data
        {
            get { return (TData)DataObject; }
            set { DataObject = value; }
        }
    }
}