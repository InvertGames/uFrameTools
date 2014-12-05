using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;
using UnityEngine;

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
    public override void Serialize(JSONClass cls)
    {
        base.Serialize(cls);
        var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
         
            if (property.GetCustomAttributes(typeof(JsonProperty), true).Length < 1) continue;
            this.SerializeProperty(property, cls);
           
            
        }

    }
    public override void Deserialize(JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);
        var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (property.GetCustomAttributes(typeof(JsonProperty), true).Length < 1) continue;
           this.DeserializeProperty(property,cls);
        }

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


    public IEnumerable<ConnectionData> Inputs
    {
        get
        {
            if (Node == null)
            {
                throw new Exception("NODE IS NULL");
            }
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
            //if (Node == null) yield break;
            //if (Node.Project == null) yield break;
            if (Node == null)
            {
                throw new Exception("NODE IS NULL");
            }
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