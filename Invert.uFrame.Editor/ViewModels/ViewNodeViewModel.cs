using System.Collections;
using System.Collections.Generic;
using Invert.MVVM;
using Invert.uFrame.Code.Bindings;

namespace Invert.uFrame.Editor.ViewModels
{
    public class ViewNodeViewModel : DiagramNodeViewModel<ViewData>
    {
        public ViewNodeViewModel(ViewData data) : base(data)
        {
        
        }

        public override bool AllowCollapsing
        {
            get { return true; }
        }

        public override bool ShowSubtitle
        {
            get { return true; }
        }

        public bool HasElement
        {
            get { return GraphItem.ViewForElement != null; }
        }

        public void AddNewBinding(IBindingGenerator lastSelected)
        {
            GraphItem.NewBindings.Add(lastSelected);
        }
    }

    public class EnumItemViewModel : ItemViewModel<EnumItem>
    {
        public EnumItemViewModel(EnumItem item)
        {
            DataObject = item;
        }

        public override string Name
        {
            get { return Data.Name; }
            set { Data.Name = value; }
        }
    }

    public class EnumNodeViewModel : DiagramNodeViewModel<EnumData>
    {
        public EnumNodeViewModel(EnumData data)
            : base(data)
        {

        }
        public override bool AllowCollapsing
        {
            get { return true; }
        }

        public IEnumerable<EnumItem> EnumItems
        {
            get { return GraphItem.EnumItems; }
        }
    }

    public class ElementNodeViewModel : DiagramNodeViewModel<ElementData>
    {
        public ElementNodeViewModel(ElementData data)
            : base(data)
        {

        }

        public override bool ShowSubtitle
        {
            get { return !string.IsNullOrEmpty(SubTitle); }
        }

        public override bool AllowCollapsing
        {
            get { return true; }
        }

        public bool IsTemplate
        {
            get { return GraphItem.IsTemplate; }
        }

        public bool IsMultiInstance
        {
            get { return GraphItem.IsMultiInstance; }
        }

        public IEnumerable<IViewModelItem> ViewModelItems
        {
            get { return GraphItem.ViewModelItems; }
        }

        public ICollection<ViewModelPropertyData> Properties
        {
            get { return GraphItem.Properties; }
        }
        public ICollection<ViewModelCommandData> Commands
        {
            get { return GraphItem.Commands; }
        }
        public ICollection<ViewModelCollectionData> Collections
        {
            get { return GraphItem.Collections; }
        }

        

    }
}