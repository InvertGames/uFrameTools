using System.Windows.Controls;
using Invert.Core.GraphDesigner;

namespace DiagramDesigner.Controls
{
    public class InvertControl<TViewModel> : Control
    {
        public TViewModel ViewModel
        {
            get { return (TViewModel)DataContext; }
        }
    }

    public class ItemControl<TViewModel> : InvertControl<ItemViewModel> where TViewModel : ItemViewModel
    {
        
    }
}