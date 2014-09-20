using System.Collections.Generic;
using Invert.uFrame.Editor;

public class MissingNodeData : DiagramNode
{
    public JSONClass _CachedData;
    public override void Deserialize(JSONClass cls, INodeRepository repository)
    {
        //base.Deserialize(cls, repository);
        _CachedData = cls;
    }

    public override void Serialize(JSONClass cls)
    {
        //base.Serialize(cls);
        foreach (KeyValuePair<string,JSONNode> item in _CachedData)
        {
            cls.Add(item.Key,item.Value);
        }
    }

    public override IEnumerable<IDiagramNodeItem> Items
    {
        get { yield break; }
    }

    public override string Label
    {
        get { return "Missing Type " + _CachedData["_CLRType"].Value; }
    }

    public override string Name
    {
        get { return Label; }

    }

    public override IEnumerable<IDiagramNodeItem> ContainedItems
    {
        get { yield break; }
        set {  }
    }
}