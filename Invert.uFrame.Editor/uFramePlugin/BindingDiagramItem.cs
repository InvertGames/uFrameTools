using System.Collections.Generic;
using System.Reflection;
using Invert.uFrame.Code.Bindings;
using Invert.uFrame.Editor.ViewModels;

public class BindingDiagramItem : DiagramNodeItem
{
    public ViewData View { get; set; }
    public IBindingGenerator Generator { get; set; }
    public MethodInfo MethodInfo { get; set; }

    public string MethodName { get; set; }

    public override string Label
    {
        get { return Name; }
    }

    public override string Name
    {
        get { return MethodName; }
    }

    public override string FullLabel
    {
        get { return Name; }
    }

    public override void Remove(IDiagramNode diagramNode)
    {
        
    }
}