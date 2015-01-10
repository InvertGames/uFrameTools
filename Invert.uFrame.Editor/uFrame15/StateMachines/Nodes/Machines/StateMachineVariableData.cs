
//public class StateMachineVariableData : DiagramNodeItem, ITypeDiagramItem
//{
//    public override string FullLabel
//    {
//        get { return Name; }
//    }

//    public override void Remove(IDiagramNode diagramNode)
//    {
//        var node = diagramNode as StateMachineNodeData;
//        if (node != null)
//            node.Variables.Remove(this);
//    }

//    public override string Label
//    {
//        get { return Name; }
//    }

//    public string Title
//    {
//        get { return Name; }
//    }

//    public string SearchTag { get { return Name; } }
//    public string RelatedType { get; set; }

//    public bool AllowEmptyRelatedType
//    {
//        get { return false; }
//    }
//    public string RelatedTypeName
//    {
//        get
//        {
//            var relatedNode = this.RelatedNode();
//            if (relatedNode != null)
//                return relatedNode.Name;

//            return RelatedType;
//        }
//    }
//    public void SetType(IDesignerType input)
//    {
//        RelatedType = input.Identifier;
//    }

//    public void RemoveType()
//    {
//        RelatedType = null;
//    }

//    public override void Serialize(Invert.uFrame.Editor.JSONClass cls)
//    {
//        base.Serialize(cls);
//        cls.Add("RelatedType",new Invert.uFrame.Editor.JSONData(RelatedType ?? string.Empty));
//    }

//    public override void Deserialize(Invert.uFrame.Editor.JSONClass cls, INodeRepository repository)
//    {
//        base.Deserialize(cls, repository);
//        if (cls["RelatedType"] != null)
//        {
//            RelatedType = cls["RelatedType"].Value;
//        }

//    }
//}