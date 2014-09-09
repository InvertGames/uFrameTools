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

    public override GUIStyle ItemStyle
    {
        get { return ElementDesignerStyles.Item4; }
    }
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
                _bindingsHeader.ViewModelObject = ViewModel;
                _bindingsHeader.HeaderType = typeof(string);
                if (NodeViewModel.IsLocal)
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
                _propertiesHeader.ViewModelObject = ViewModel;
                _propertiesHeader.Label = "Scene Properties";

                _propertiesHeader.HeaderType = typeof(ViewModelPropertyData);
                if (NodeViewModel.IsLocal)
                _propertiesHeader.AddCommand = Container.Resolve<AddViewPropertyCommand>();
            }
            return _propertiesHeader;
        }
        set { _propertiesHeader = value; }
    }

    //protected override void DrawSelectedItemLabel(IDiagramNodeItem nodeItem)
    //{
    //    //var  bindingDiagramItem = nodeItem as BindingDiagramItem;
    //    //if (bindingDiagramItem != null)
    //    //{
    //    //    DrawItemLabel(bindingDiagramItem);
    //    //}
    //    //else
    //    //{
    //        base.DrawSelectedItemLabel(nodeItem);
    //    //}
        
    //}

    protected override void GetContentDrawers(List<IDrawer> drawers)
    {
        //base.GetContentDrawers(drawers);
        drawers.Add(PropertiesHeader);
        foreach (var item in ViewModel.ContentItems.OfType<ViewPropertyItemViewModel>())
        {
                var drawer = uFrameEditor.CreateDrawer(item);
            if (drawer == null) Debug.Log(string.Format("Couldn't create drawer for {0} make sure it is registered.", item.GetType().Name));
            drawers.Add(drawer);
        }

        drawers.Add(BindingsHeader);

        //foreach (var item in this.NodeViewModel.Bindings)
        //{
        //    var drawer = uFrameEditor.CreateDrawer(item);
        //    if (drawer == null) Debug.Log(string.Format("Couldn't create drawer for {0} make sure it is registered.", item.GetType().Name));
        //    drawers.Add(drawer);
        //}
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


    [Inject("ViewDoubleClick")]
    public IEditorCommand DoubleClickCommand { get; set; }

    public override void OnMouseDoubleClick(MouseEvent mouseEvent)
    {
        base.OnMouseDoubleClick(mouseEvent);
    }
}