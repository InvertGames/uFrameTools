using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Common;
using Invert.uFrame;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEngine;

public class ViewDrawer : DiagramNodeDrawer<ViewNodeViewModel>
{
    private ElementDataBase _forElement;

    public ViewDrawer()
    {
    }

    protected override GUIStyle HeaderStyle
    {
        get { return ElementDesignerStyles.NodeHeader2; }
    }

    public ViewDrawer(ViewNodeViewModel viewModel)
    {
        ViewModel = viewModel;
    }

    private NodeItemHeader _behavioursHeader;
    private NodeItemHeader _propertiesHeader;
    private NodeItemHeader _bindingsHeader;

    public NodeItemHeader BindingsHeader
    {
        get
        {
            if (_bindingsHeader == null)
            {
                _bindingsHeader = Container.Resolve<NodeItemHeader>();
                _bindingsHeader.Label = "Bindings";
                _bindingsHeader.HeaderType = typeof(string);
                _bindingsHeader.AddCommand = Container.Resolve<AddBindingCommand>();
            }
            return _bindingsHeader;
        }
        set { _propertiesHeader = value; }
    }

    public NodeItemHeader PropertiesHeader
    {
        get
        {
            if (_propertiesHeader == null)
            {
                _propertiesHeader = Container.Resolve<NodeItemHeader>();

                _propertiesHeader.Label = "2-Way Properties";

                _propertiesHeader.HeaderType = typeof(ViewModelPropertyData);
                _propertiesHeader.AddCommand = Container.Resolve<AddViewPropertyCommand>();
            }
            return _propertiesHeader;
        }
        set { _propertiesHeader = value; }
    }

    protected override void DrawSelectedItemLabel(IDiagramNodeItem nodeItem)
    {
        //var  bindingDiagramItem = nodeItem as BindingDiagramItem;
        //if (bindingDiagramItem != null)
        //{
        //    DrawItemLabel(bindingDiagramItem);
        //}
        //else
        //{
            base.DrawSelectedItemLabel(nodeItem);
        //}
        
    }

    protected override void GetContentDrawers(List<IDrawer> drawers)
    {
        base.GetContentDrawers(drawers);
        //if (NodeViewModel.GraphItem.BaseNode is ElementData)
        //{
        //    yield return new DiagramSubItemGroup()
        //    {
        //        Header = PropertiesHeader,
        //        Items = ViewModel.ContainedItems.ToArray()
        //    };

        //    var vForElement = NodeViewModel.GraphItem.ViewForElement;

        //    if (vForElement != null)
        //    {
        //        var existing =
        //            NodeViewModel.GraphItem.BindingMethods.Select(p => (IDiagramNodeItem)(new BindingDiagramItem(p.Name) {View = NodeViewModel.GraphItem,MethodInfo = p}));
        //        var adding =
        //            NodeViewModel.GraphItem.NewBindings.Select(p => (IDiagramNodeItem)(new BindingDiagramItem("[Added] " + p.MethodName) { View = NodeViewModel.GraphItem, Generator = p }));

        //        yield return new DiagramSubItemGroup()
        //        {
        //            Header = BindingsHeader,
        //            Items = existing.Concat(adding).ToArray()
        //        };
        //    }
        //}

    }

    public override GUIStyle ItemStyle
    {
        get { return ElementDesignerStyles.Item4; }
    }

    [Inject("ViewDoubleClick")]
    public IEditorCommand DoubleClickCommand { get; set; }

    public override void OnMouseDoubleClick(MouseEvent mouseEvent)
    {
        base.OnMouseDoubleClick(mouseEvent);
    }
}