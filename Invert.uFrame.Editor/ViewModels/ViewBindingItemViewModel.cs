using Invert.uFrame.Code.Bindings;

namespace Invert.uFrame.Editor.ViewModels
{
    public class ViewBindingItemViewModel : ItemViewModel<IBindingGenerator>
    {
        public ViewBindingItemViewModel(IBindingGenerator data, DiagramNodeViewModel nodeViewModel)
            : base(nodeViewModel)
        {
            DataObject = data;
        }
    }
}