using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Common;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.IOC;
using Invert.uFrame;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using Invert.uFrame.Editor.ViewModels;
using Invert.IOC;
using UnityEditor;
using UnityEngine;

public class ViewDrawer : DiagramNodeDrawer<ViewNodeViewModel>
{
    private ElementDataBase _forElement;

    public override object ItemStyle
    {
        get { return CachedStyles.Item4; }
    }
    public ViewDrawer()
    {
    }

    protected override object HeaderStyle
    {
        get { return CachedStyles.NodeHeader2; }
    }

    public ViewDrawer(ViewNodeViewModel viewModel)
    {
        ViewModel = viewModel;
    }

    private SectionHeaderDrawer _behavioursHeader;
    private SectionHeaderDrawer _propertiesHeader;
    private SectionHeaderDrawer _bindingsHeader;

  
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

 


    [Inject("ViewDoubleClick")]
    public IEditorCommand DoubleClickCommand { get; set; }

    public override void OnMouseDoubleClick(MouseEvent mouseEvent)
    {
        base.OnMouseDoubleClick(mouseEvent);
    }
}