using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Invert.MVVM;
using Invert.uFrame.Code.Bindings;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using Invert.uFrame.Editor.Refactoring;
using UnityEngine;

namespace Invert.uFrame.Editor.ViewModels
{
    public class ViewNodeViewModel : DiagramNodeViewModel<ViewData>
    {
        private string _preview;
        private BindingDiagramItem[] _bindings;

        public ViewNodeViewModel(ViewData data, DiagramViewModel diagramViewModel)
            : base(data, diagramViewModel)
        {

        }

        protected override void DataObjectChanged()
        {
            base.DataObjectChanged();

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
                return GraphItem.NewBindings.Select(p => p.Generator);
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
            GraphItem.Bindings.Add(new ViewBindingData()
            {
                Name = lastSelected.MethodName,
                Generator = lastSelected,
                Node = GraphItem
            });
            Preview = null;
            _bindings = null;
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
                return GraphItem.ReflectionBindingMethods;
            }
        }
        public BindingDiagramItem[] Bindings
        {
            get { return Items.OfType<BindingDiagramItem>().ToArray(); }
        }

        public IEnumerable<ViewPropertyData> Properties
        {
            get { return GraphItem.Properties; }
        }

        public void RemoveBinding(IBindingGenerator item)
        {

            GraphItem.Bindings.RemoveAll(p => p.Generator == item);
            Preview = null;
            _bindings = null;
        }

        public void AddProperty(ViewPropertyData viewPropertyData)
        {
            viewPropertyData.Node = GraphItem;
            GraphItem.Properties.Add(viewPropertyData);
        }

        public void RemoveProperty(MemberInfo memberInfo)
        {
            GraphItem.Properties.RemoveAll(p => p.MemberInfo == memberInfo);
        }

        public void AddProperty()
        {
            var property = new ViewPropertyData()
            {
                Name = GraphItem.Data.GetUniqueName("NewProperty"),
                Node = GraphItem,

            };
            GraphItem.Properties.Add(property);
            Debug.Log("Property Added");
        }
    }
}

