using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor;

public class GenericInheritableNode : GenericNode, IInhertable
{
    private string _baseIdentifier;

    public string BaseIdentifier
    {
        get { return _baseIdentifier; }
        private set { _baseIdentifier = value; }
    }

    public void SetBaseType(GenericInheritableNode baseItem)
    {
        if (baseItem == null)
        {
            BaseIdentifier = null;
        }
        else
        {
            BaseIdentifier = baseItem.Identifier;
        }
        
    }

    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        if (BaseIdentifier != null)
        {
            cls.Add("BaseIdentifier",BaseIdentifier);
        }
    }

    public override void Deserialize(JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);
        if (cls["BaseIdentifier"] != null)
        {
            BaseIdentifier = cls["BaseIdentifier"].Value;
        }
    }

    public GenericInheritableNode BaseNode
    {
        get { return Project.NodeItems.FirstOrDefault(p => p.Identifier == BaseIdentifier) as GenericInheritableNode; }
    }

    public IEnumerable<GenericInheritableNode> BaseNodes
    {
        get
        {
            var baseType = BaseNode;
            while (baseType != null)
            {
                yield return baseType;
                baseType = baseType.BaseNode;
            }
        }
    }
    public IEnumerable<GenericInheritableNode> BaseNodesWithThis
    {
        get
        {
            yield return this;
            var baseType = BaseNode;
            while (baseType != null)
            {
                yield return baseType;
                baseType = baseType.BaseNode;
            }
        }
    }
    public IEnumerable<GenericInheritableNode> DerivedNodes
    {
        get
        {
            var derived = Project.NodeItems.OfType<GenericInheritableNode>().Where(p => p.BaseIdentifier == Identifier);
            foreach (var derivedItem in derived)
            {
                yield return derivedItem;
                foreach (var another in derivedItem.DerivedNodes)
                {
                    yield return another;
                }
            }
        }
    }


    //public IEnumerable<TChildItem> GetInheritedChildren<TChildItem>()
    //{
    //    return BaseNodesWithThis
    //        .SelectMany(p =>
    //            p.InputGraphItems
    //                .OfType<GenericInheritableNode>()
    //                .Where(o => o.GetType() == this.GetType())
    //                .SelectMany(z => z.ChildItems.OfType<GenericNodeChildItem>()));
    //}
}