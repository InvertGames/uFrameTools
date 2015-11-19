using System.Collections.Generic;
using Invert.Json;

namespace Invert.Core.GraphDesigner
{
    public abstract class FilterDictionary<TValue> : Dictionary<string, TValue>, IJsonObject
    {
        public TValue this[IDiagramNode node]
        {
            get
            {
                TValue val;
                if (!TryGetValue(node.Identifier, out val))
                    return default(TValue);

                return val;
            }
            set
            {
                this[node.Identifier] = value;
            }
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
            foreach (KeyValuePair<string, TValue> pair in this) {
                cls.Add(pair.Key, SerializeValue(pair.Value));
            }
        }

        public void Deserialize(JSONClass cls)
        {
            foreach (KeyValuePair<string, JSONNode> cl in cls)
            {
                Add(cl.Key, DeserializeValue(cl.Value));
            }
        }

        protected abstract TValue DeserializeValue(JSONNode value);
    }
}