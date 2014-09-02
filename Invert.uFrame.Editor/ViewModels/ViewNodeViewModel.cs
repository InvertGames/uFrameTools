using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Invert.MVVM;
using Invert.uFrame.Code.Bindings;
using Invert.uFrame.Editor.Refactoring;

namespace Invert.uFrame.Editor.ViewModels
{
    public class ViewNodeViewModel : DiagramNodeViewModel<ViewData>
    {
        private string _preview;

        public ViewNodeViewModel(ViewData data,DiagramViewModel diagramViewModel) : base(data,diagramViewModel)
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

        public IEnumerable<IBindingGenerator> NewBindings
        {
            get
            {
                return GraphItem.NewBindings;
            }
        }
        public string GetViewPreview()
        {

            var refactorContext = new RefactorContext(GraphItem.BindingInsertMethodRefactorer);
            var settings = GraphItem.Data.Settings;
            var pathStrategy = settings.CodePathStrategy;
            var viewFilePath = System.IO.Path.Combine(settings.CodePathStrategy.AssetPath, pathStrategy.GetEditableViewFilename(GraphItem));
            return refactorContext.RefactorFile(viewFilePath, false);
        }

        public string Preview
        {
            get { return _preview ?? (_preview = GetViewPreview()); }
            set { _preview = value; }
        }

        public void AddNewBinding(IBindingGenerator lastSelected)
        {
            GraphItem.NewBindings.Add(lastSelected);
            Preview = null;
        }

        public IEnumerable<IBindingGenerator> BindingGenerators
        {
            get
            {
                return uFrameEditor.GetBindingGeneratorsFor(GraphItem.ViewForElement, true, false, true, false);
            }
        }

        public IEnumerable<MethodInfo> BindingMethods
        {
            get
            {
                return GraphItem.BindingMethods;
            }
        }

        public void RemoveBinding(IBindingGenerator item)
        {
            GraphItem.NewBindings.Remove(item);
            Preview = null;
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
        public EnumNodeViewModel(EnumData data,DiagramViewModel diagramViewModel)
            : base(data,diagramViewModel)
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
        public ElementNodeViewModel(ElementData data, DiagramViewModel diagramViewModel)
            : base(data,diagramViewModel)
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