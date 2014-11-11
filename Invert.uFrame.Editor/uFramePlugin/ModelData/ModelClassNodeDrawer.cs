using Invert.Common;
using UnityEngine;

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