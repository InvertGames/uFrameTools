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
            var value = property.GetValue(this, null);
            if (value != null)
            {
                var propertyName = property.Name;
                var propertyType = property.PropertyType;
                if (propertyType == typeof(int))
                {
                    cls.Add(propertyName, new JSONData((int)value));
                }
                else if (propertyType == typeof(string))
                {
                    cls.Add(propertyName, new JSONData((string)value));
                }
                else if (propertyType == typeof(float))
                {
                    cls.Add(propertyName, new JSONData((float)value));
                }
                else if (propertyType == typeof(bool))
                {
                    cls.Add(propertyName, new JSONData((bool)value));
                }
                else if (propertyType == typeof(double))
                {
                    cls.Add(propertyName, new JSONData((double)value));
                }
                else if (propertyType == typeof(Vector2))
                {
                    cls.Add(propertyName, new JSONData((Vector2)value));
                }
                else if (propertyType == typeof(Vector3))
                {
                    cls.Add(propertyName, new JSONData((Vector3)value));
                }
                else if (propertyType == typeof(Quaternion))
                {
                    cls.Add(propertyName, new JSONData((Quaternion)value));
                }
                else
                {
                    throw new Exception(string.Format("{0} property can't be serialized. Override Serialize method to serialize it."));
                }
            }
        }

    }
    public override void Deserialize(JSONClass cls, INodeRepository repository)
    {
        base.Deserialize(cls, repository);
        var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in properties)
        {
            if (property.GetCustomAttributes(typeof(JsonProperty), true).Length < 1) continue;
            var propertyName = property.Name;
            if (cls[propertyName] == null) continue;
            var propertyType = property.PropertyType;
            if (propertyType == typeof(int))
            {
                property.SetValue(this, cls[propertyName].AsInt, null);
            }
            else if (propertyType == typeof(string))
            {
                property.SetValue(this, cls[propertyName].Value, null);
            }
            else if (propertyType == typeof(float))
            {
                property.SetValue(this, cls[propertyName].AsFloat, null);
            }
            else if (propertyType == typeof(bool))
            {
                property.SetValue(this, cls[propertyName].AsBool, null);
            }
            else if (propertyType == typeof(double))
            {
                property.SetValue(this, cls[propertyName].AsDouble, null);
            }
            else if (propertyType == typeof(Vector2))
            {
                property.SetValue(this, cls[propertyName].AsVector2, null);
            }
            else if (propertyType == typeof(Vector3))
            {
                property.SetValue(this, cls[propertyName].AsVector3, null);
            }
            else if (propertyType == typeof(Quaternion))
            {
                property.SetValue(this, cls[propertyName].AsQuaternion, null);
            }
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