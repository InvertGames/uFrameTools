using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor;

public class GenericNodeChildItem : DiagramNodeItem, IConnectable
{
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
    //private List<string> _connectedGraphItemIds = new List<string>();

    //public IEnumerable<IGraphItem> ConnectedGraphItems
    //{
    //    get
    //    {
    //        foreach (var item in Node.Project.NodeItems)
    //        {
    //            if (ConnectedGraphItemIds.Contains(item.Identifier))
    //                yield return item;

    //            foreach (var child in item.ContainedItems)
    //            {
    //                if (ConnectedGraphItemIds.Contains(child.Identifier))
    //                {
    //                    yield return child;
    //                }
    //            }
    //        }
    //    }
    //}

    //public List<string> ConnectedGraphItemIds
    //{
    //    get { return _connectedGraphItemIds; }
    //    set { _connectedGraphItemIds = value; }
    //}
    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
       //cls.AddPrimitiveArray("ConnectedGraphItems", ConnectedGraphItemIds, i => new JSONData(i));
    }


    public override void Deserialize(JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);
        //if (cls["ConnectedGraphItems"] != null)
        //{
        //    ConnectedGraphItemIds = cls["ConnectedGraphItems"].DeserializePrimitiveArray(n => n.Value).ToList();
        //}
    }

    public IEnumerable<ConnectionData> Inputs
    {
        get
        {

            foreach (var connectionData in Node.Project.Connections)
            {
                if (connectionData.InputIdentifier == this.Identifier)
                {
                    yield return connectionData;
                }
            }
        }
    }
    public IEnumerable<ConnectionData> Outputs
    {
        get
        {
            if (Node == null) yield break;
            if (Node.Project == null) yield break;
            
            foreach (var connectionData in Node.Project.Connections)
            {
                if (connectionData.OutputIdentifier == this.Identifier)
                {

                    yield return connectionData;
                }
            }
        }
    }

}