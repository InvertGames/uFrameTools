using Invert.Common;
using Invert.Core.GraphDesigner;
using UnityEngine;

public class ModelClassNodeDrawer : ClassNodeDrawer
{
    public ModelClassNodeDrawer(ClassNodeViewModel viewModelObject) : base(viewModelObject)
    {
    }

    protected override object HeaderStyle
    {
        get { return CachedStyles.NodeHeader11; }
    }
}