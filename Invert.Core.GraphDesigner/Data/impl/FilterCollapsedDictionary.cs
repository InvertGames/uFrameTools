using System.Collections.Generic;
using Invert.Json;

namespace Invert.Core.GraphDesigner
{

    public class FilterCollapsedDictionary : FilterDictionary<bool>
    {
        protected override JSONNode SerializeValue(bool value)
        {
            return new JSONData(value);
        }

        protected override bool DeserializeValue(JSONNode value)
        {
            return value.AsBool;
        }
    }


    public class FlagsDictionary : Dictionary<string,bool>, IJsonObject
    {


        public void Serialize(JSONClass cls)
        {
            foreach (var item in this)
            {
                cls.Add(item.Key,new JSONData(item.Value));
            }
        }

        public void Deserialize(JSONClass cls, INodeRepository repository)
        {
            this.Clear();
            foreach (KeyValuePair<string,JSONNode> jsonNode in cls)
            {
                this.Add(jsonNode.Key,jsonNode.Value.AsBool);
            }
        }
    }

    public class DataBag : IJsonObject
    {
        Dictionary<string,string>  _dict = new Dictionary<string, string>();

        public string this[string key]
        {
            get
            {
                if (_dict.ContainsKey(key))
                    return _dict[key];

                return null;
            }
            set
            {
                AddOrReplace(key,value);
            }
        }

        public void AddOrReplace(string key, string value)
        {
            if (_dict.ContainsKey(key))
            {
                if (value == null)
                {
                    _dict.Remove(key);
                    return;
                }
                _dict[key] = value;
            }
            else
            {
                _dict.Add(key, value);
            }
        }
        public void Serialize(JSONClass cls)
        {
            foreach (var item in _dict)
            {
                cls.Add(item.Key, new JSONData(item.Value));
            }
        }

        public void Deserialize(JSONClass cls, INodeRepository repository)
        {
            _dict.Clear();
            foreach (KeyValuePair<string, JSONNode> jsonNode in cls)
            {
                _dict.Add(jsonNode.Key, jsonNode.Value.Value);
            }
        }
    }
}