using System;
using Invert.Core.GraphDesigner;

[Serializable]
public class SceneFlowFilter : DiagramFilter
{
    public override bool ImportedOnly
    {
        get { return true; }
    }

    public override string Name
    {
        get { return "Root"; }
    }

    public override bool IsItemAllowed(object item, Type t)
    {
        if (t == typeof (IDiagramNode))
        {
            return true;
        }
        if (t == typeof (ViewModelCommandData))
        {
            return true;
        }
 
        return base.IsItemAllowed(item, t);
    }
}