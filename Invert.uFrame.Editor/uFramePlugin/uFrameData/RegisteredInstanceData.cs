using System;
using System.CodeDom;
using Invert.Core.GraphDesigner;
using Invert.Json;
using Invert.uFrame.Editor;

public class RegisteredInstanceData : DiagramNodeItem,IBindableTypedItem
{
    public override string FullLabel
    {
        get { return Name; }
    }

    public override void Remove(IDiagramNode diagramNode)
    {
        var data = Node as SubSystemData;
        if (data != null)
        {
            data.Instances.Remove(this);
            data.Dirty = true;
        }
    }

    public override string Label
    {
        get { return Name; }
    }

    public string RelatedType { get; set; }

    public string RelatedTypeName
    {
        get
        {
            var node = this.RelatedNode() as ElementData;
            if (node == null)
            {
                return "ERROR";
            }
            return node.NameAsViewModel;
        }
    }

    public bool AllowEmptyRelatedType
    {
        get { return false; }
    }

    public string FieldName
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public string NameAsChangedMethod
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public string ViewFieldName
    {
        get
        {
            throw new NotImplementedException();
            
        }
    }

    public void SetType(IDesignerType input)
    {
        this.RelatedType = input.Identifier;
    }

    public void RemoveType()
    {
        this.Remove(Node);
    }

    public CodeTypeReference GetFieldType()
    {
        throw new NotImplementedException();
    }

    public CodeTypeReference GetPropertyType()
    {
        throw new NotImplementedException();
    }

    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        cls.Add("RegisterType",new JSONData(RelatedType));

    }

    public override void Deserialize(JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);
        if (cls["RegisterType"] != null)
        {
            RelatedType = cls["RegisterType"].Value;
        }
    }
}