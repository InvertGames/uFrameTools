using System;
using System.CodeDom;
using System.Linq;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Json;

public class GenericTypedChildItem : GenericNodeChildItem, IBindableTypedItem, ISerializeablePropertyData
{
    protected string _type = string.Empty;

    public Type Type
    {
        get
        {
            if (string.IsNullOrEmpty(RelatedType)) return null;

            return InvertApplication.FindTypeByName(RelatedType);
        }
    }

    public string NameAsChangedMethod
    {
        get { return string.Format("{0}Changed", Name); }
    }

    public string RelatedType
    {
        get { return _type; }
        set
        {
            if (this.Node.Graph != null && this.Node.Project 
                 != null)
            this.Node.TrackChange(new TypeChange(this, _type, RelatedTypeName));
            _type = value;
            
        }
    }

    public virtual string DefaultTypeName
    {
        get { return string.Empty; }
    }

    public virtual string RelatedTypeName
    {
        get
        {
            var outputClass = RelatedTypeNode;
            if (outputClass != null)
            {
                return outputClass.ClassName;
            }

            var relatedNode = this.RelatedNode();

            if (relatedNode != null)
                return relatedNode.Name;

            return string.IsNullOrEmpty(RelatedType) ?  DefaultTypeName : RelatedType;
        }
    }

    public virtual IClassTypeNode RelatedTypeNode
    {
        get
        {
            if (Node == null)
            {
                throw new Exception("NODE IS NULL");
            }
            else if (Node.Project == null)
            {
                throw new Exception("PROJECT IS NULL");
            }
            var result =  this.OutputTo<IClassTypeNode>();
            if (result == null)
            {
                return this.Node.Project.NodeItems.FirstOrDefault(p => p.Identifier == RelatedType) as IClassTypeNode;
            }
            return result;
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
            return string.Format("_{0}Property", Name);
        }
    }

    public string ViewFieldName
    {
        get
        {
            return string.Format("_{0}", Name);
        }
    }

    public void SetType(IDesignerType input)
    {
        this.RelatedType = input.Identifier;
    }

    public void RemoveType()
    {
        this.RelatedType = typeof(string).Name;
    }

    public CodeTypeReference GetFieldType()
    {
        var relatedNode = this.RelatedNode();
        if (relatedNode != null)
        {
            return relatedNode.GetFieldType(this);
        }
        var t = new CodeTypeReference(RelatedTypeName);
        return t;
    }

    public CodeTypeReference GetPropertyType()
    {
        var relatedNode = this.RelatedNode();
        if (relatedNode != null)
        {
            return relatedNode.GetPropertyType(this);
        }
        return new CodeTypeReference(RelatedTypeName);
    }

    public IDiagramNode TypeNode()
    {
        return this.RelatedNode();
    }

    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        cls.Add("ItemType", new JSONData(_type ?? string.Empty));
    }

    public override void Deserialize(JSONClass cls)
    {
        base.Deserialize(cls);

        if (cls["ItemType"] != null)
            _type = cls["ItemType"].Value.Split(',')[0].Split('.').Last();
    }

    public override string FullLabel
    {
        get { return Name; }
    }

    public override string Label
    {
        get { return Name; }
    }

    public override void Remove(IDiagramNode diagramNode)
    {

    }

}