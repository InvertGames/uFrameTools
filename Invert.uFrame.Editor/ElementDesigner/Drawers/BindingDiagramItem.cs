using System.Collections.Generic;
using System.Reflection;
using Invert.uFrame.Code.Bindings;
using Invert.uFrame.Editor.ViewModels;

public class BindingDiagramItem : ItemViewModel
{
    public BindingDiagramItem(DiagramNodeViewModel nodeViewModel, string methodName) : base(nodeViewModel)
    {
        MethodName = methodName;
    }

    public ViewData View { get; set; }
    public IBindingGenerator Generator { get; set; }
    public MethodInfo MethodInfo { get; set; }

    public string MethodName { get; set; }

}