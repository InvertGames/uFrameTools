using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.Common;
using UnityEngine;

public class ModelClassNodeData : ClassNodeData
{
    public string NameAsAssetClass
    {
        get
        {
            return string.Format("{0}Asset", Name);
        }
    }

    public string NameAsInterface
    {
        get
        {
            return string.Format("I{0}", this.Name);
        }
    }
}

public class ModelClassNodeViewModel : ClassNodeViewModel
{
    public ModelClassNodeViewModel(ClassNodeData graphItemObject, DiagramViewModel diagramViewModel) : base(graphItemObject, diagramViewModel)
    {
    }
}
public class ModelClassNodeDrawer : ClassNodeDrawer
{
    public ModelClassNodeDrawer(ClassNodeViewModel viewModelObject) : base(viewModelObject)
    {
    }

    protected override GUIStyle HeaderStyle
    {
        get { return ElementDesignerStyles.NodeHeader11; }
    }
}
