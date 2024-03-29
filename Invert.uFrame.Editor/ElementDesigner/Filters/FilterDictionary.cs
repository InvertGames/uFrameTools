using System;
using System.Collections.Generic;
using Invert.uFrame.Editor;
using UnityEngine;

[Serializable]
public abstract class FilterDictionary<TValue> : IJsonObject
{
    [SerializeField]
    private List<string> _keys = new List<string>();

    [SerializeField]
    private List<TValue> _values = new List<TValue>();

    public TValue this[IDiagramNode node]
    {
        get
        {
            var indexOf = Keys.IndexOf(node.Identifier);
            if (indexOf > -1)
            {
                return Values[indexOf];
            }

            return default(TValue);
        }
        set
        {
            var indexOf = Keys.IndexOf(node.Identifier);
            if (indexOf > -1)
            {
                Values[indexOf] = value;
            }
            else
            {
                Add(node.Identifier, value);
            }
        }
    }

    public List<string> Keys
    {
        get { return _keys; }
        set { _keys = value; }
    }

    public List<TValue> Values
    {
        get { return _values; }
        set { _values = value; }
    }

    public void Remove(string key)
    {
        var index = Keys.IndexOf(key);
        Keys.RemoveAt(index);
        Values.RemoveAt(index);
    }

    protected void Add(string key, TValue value)
    {
        Keys.Add(key);
        Values.Add(value);
    }

    public JSONClass Serialize()
    {
        var cls = new JSONClass();
        Serialize(cls);
        return cls;
    }

    protected abstract JSONNode SerializeValue(TValue value);

    public void Serialize(JSONClass cls)
    {
        for (int index = 0; index < _keys.Count; index++)
        {
            var key = _keys[index];
            var value = _values[index];
            cls.Add(key, SerializeValue(value));
        }
    }

    public void Deserialize(JSONClass cls, INodeRepository repository)
    {
        foreach (KeyValuePair<string, JSONNode> cl in cls)
        {
            
           Add(cl.Key,DeserializeValue(cl.Value));
        }
    }

    protected abstract TValue DeserializeValue(JSONNode value);
}