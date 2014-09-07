using System.Collections.Generic;
using System.Reflection;
using Invert.uFrame.Code.Bindings;

public class BindingDiagramItem : DiagramNodeItem
{
    public ViewData View { get; set; }
    public IBindingGenerator Generator { get; set; }
    public MethodInfo MethodInfo { get; set; }
    public BindingDiagramItem(string methodName)
    {
        MethodName = methodName;
    }

    public string MethodName { get; set; }
    public override string FullLabel
    {
        get { return MethodName; }
    }

    public override string Label
    {
        get { return MethodName; }
    }

    public override string Name
    {
        get { return MethodName; }
    }

    //public override bool CanCreateLink(IGraphItem target)
    //{
    //    return false;
    //}

    //public override IEnumerable<IDiagramLink> GetLinks(IDiagramNode[] diagramNode)
    //{
    //    yield break;
    //}

    public override void Remove(IDiagramNode diagramNode)
    {
        View.BindingMethods.Remove(MethodInfo);
        View.NewBindings.Remove(Generator);
    }

    //public override void RemoveLink(IDiagramNode target)
    //{

    //}

    //public override void CreateLink(IDiagramNode container, IGraphItem target)
    //{

    //}
}