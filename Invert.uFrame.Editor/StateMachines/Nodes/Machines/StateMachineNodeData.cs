using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor;
using JSONData = Invert.uFrame.Editor.JSONData;

public class StateMachineNodeData : DiagramNode,IDesignerType
{
    //private List<StateMachineVariableData> _variables;

    public override string Label
    {
        get { return Name; }
    }

    public override IEnumerable<IDiagramNodeItem> ContainedItems
    {
        get
        {
            yield break;
            //return Variables.Cast<IDiagramNodeItem>();
        }
        set {  }
    }

    public override void RemoveFromDiagram()
    {
        base.RemoveFromDiagram();
        Data.RemoveNode(this);
    }

    public IEnumerable<StateMachineStateData> States
    {
        get { return this.GetContainingNodes(Data).OfType<StateMachineStateData>(); }
    }

    public IEnumerable<StateMachineTransition> Transitions
    {
        get { return this.States.SelectMany(p => p.Transitions); }
    } 
    public override CodeTypeReference GetFieldType(ITypeDiagramItem itemData)
    {
        return new CodeTypeReference(Name);
    }

    public override CodeTypeReference GetPropertyType(ITypeDiagramItem itemData)
    {
        return new CodeTypeReference(uFrameEditor.UFrameTypes.State);
    }

    //public List<StateMachineVariableData> Variables
    //{
    //    get { return _variables ?? (_variables = new List<StateMachineVariableData>()); }
    //    set { _variables = value; }
    //}

    public ElementData Element
    {
        get { return Data.NodeItems.OfType<ElementData>().FirstOrDefault(p => p.Identifier == ElementIdentifier); }
    }
    public string ElementIdentifier { get; set; }
    public string StatePropertyIdentifier { get; set; }

    public StateMachineStateData StartState
    {
        get { return States.FirstOrDefault(p => p.Identifier == StartStateIdentifier); }
    }

    public string StartStateIdentifier { get; set; }
    public override void Serialize(Invert.uFrame.Editor.JSONClass cls)
    {
        base.Serialize(cls);
        cls.Add("StartStateIdentifier",new Invert.uFrame.Editor.JSONData(StartStateIdentifier ?? string.Empty));
    }

    public override void Deserialize(Invert.uFrame.Editor.JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);
        if (cls["StartStateIdentifier"] != null)
        {
            StartStateIdentifier = cls["StartStateIdentifier"].Value;
        }
    }
}