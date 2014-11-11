using System;
using System.Collections;
using System.Linq;
using System.Text;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;


public class ClassNodeInheritanceConnectionStrategy : DefaultConnectionStrategy<ClassNodeData, ClassNodeData>
{
    public override Color ConnectionColor
    {
        get { return Color.green; }
    }
    
    protected override bool CanConnect(ClassNodeData output, ClassNodeData input)
    {
        if (output.Identifier == input.Identifier) return false;
        if (input.DerivedElements.Any(p => p.Identifier == output.Identifier)) return false;
        return base.CanConnect(output, input);
    }

    protected override bool IsConnected(ClassNodeData output, ClassNodeData input)
    {
        return input.BaseIdentifier == output.Identifier;
    }

    protected override void ApplyConnection(ClassNodeData output, ClassNodeData input)
    {
        input.SetBaseClass(output);
    }

    protected override void RemoveConnection(ClassNodeData output, ClassNodeData input)
    {
        input.RemoveBaseClass();
    }
}