using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Invert.Data;
using Invert.Json;

namespace Invert.Core.GraphDesigner
{
    public class GenericReferenceItem : GenericSlot, ITypedItem, IDataRecordRemoved
    {
        private string _sourceIdentifier;
        [Browsable(false)]
        public override string Label
        {
            get { return SourceItemObject.Name + ": " + base.Label; }
        }

        public override string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(base.Name))
                {
                    return base.Name;
                }
                if (SourceItemObject == null)
                {
                    return "Missing";
                }
                return SourceItemObject.Name;
            }
            set { base.Name = value;
                
            }
        }
        [JsonProperty]
        public string SourceIdentifier
        {
            get { return _sourceIdentifier; }
            set {
                this.Changed("SourceIdentifier",ref _sourceIdentifier, value);
            }
        }
        [Browsable(false)]
        public virtual IDiagramNodeItem SourceItemObject
        {
            get
            {
                return Repository.GetById<IDiagramNodeItem>(SourceIdentifier);
            }
        }

        public override void Deserialize(JSONClass cls)
        {
            base.Deserialize(cls);
            SourceIdentifier = cls["SourceIdentifier"].Value;
        }

        public override void Serialize(JSONClass cls)
        {
            base.Serialize(cls);
            if (!string.IsNullOrEmpty(SourceIdentifier))
                cls.Add("SourceIdentifier", SourceIdentifier);
        }

        public string RelatedType
        {
            get
            {
                var source = SourceItemObject;
                if (source == null)
                {
                    return "Missing";
                }
                var classItem = source as IClassTypeNode;
                if (classItem != null)
                {
                    return classItem.ClassName;
                }
                return source.Name;
            }
            set
            {
                
            }
        }

        public string RelatedTypeName
        {
            get
            {
                return RelatedType;
            }
        }

        public void RemoveType()
        {
            Repository.Remove(this);
        }

        public override void RecordRemoved(IDataRecord record)
        {
            if (record.Identifier == this.SourceIdentifier)
            {
                Repository.Remove(this);
            }
        }
    }

    public class SelectionFor<TFor, TValue> : GenericSlot where TValue : InputSelectionValue, new() where TFor : class,IDataRecord
    {
        public override bool AllowMultipleInputs
        {
            get { return false; }
        }

        public override bool AllowMultipleOutputs
        {
            get { return false; }
        }

        public TFor Item
        {
            get
            {
                if (typeof (IConnectable).IsAssignableFrom(typeof (TFor)))
                {
                    return this.InputFrom<TFor>() ?? SelectedItem;
                }
                return SelectedItem;
            }
        }

        public override bool AllowInputs
        {
            get { return typeof (IConnectable).IsAssignableFrom(typeof(TFor)); }
        }

        public override bool AllowSelection
        {
            get { return true; }
        }

        protected virtual TFor SelectedItem
        {
            get
            {
                if (SelectedValue == null) return null;
                return GetAllowed().OfType<TFor>().FirstOrDefault(p=>p.Identifier == SelectedValue.ValueId);
            }
        }

        public TValue SelectedValue
        {
            get
            {
                return Repository.All<TValue>().FirstOrDefault(p => p.NodeId == this.NodeId && p.ItemId == this.Identifier);
            }
        }

        public override string SelectedDisplayName
        {
            get
            {
                
                var item = Item;
                if (item == null) return "...";
               
                return ItemDisplayName(item);
            }
        }

        public virtual string ItemDisplayName(TFor item)
        {
            var xItem = item as IDiagramNodeItem;
            if (xItem != null)
            {
                return xItem.Name;
            }
            return item.Identifier;
        }
        public override void SetInput(IDataRecord item)
        {
            base.SetInput(item);
            var selected = SelectedValue;
            if (selected != null)
            {
                selected.ValueId = item.Identifier;
            }
            else
            {
                var selectedItem = Repository.Create<TValue>();
                selectedItem.NodeId = this.NodeId;
                selectedItem.ItemId = this.Identifier;
                selectedItem.ValueId = item.Identifier;
            }

        }

        public override IEnumerable<IGraphItem> GetAllowed()
        {
            yield break;
        }
    }

    public class InputSelectionValue : IDataRecord
    {
        private string _nodeId;
        private string _itemId;
        private string _valueId;
        public IRepository Repository { get; set; }

        [JsonProperty]
        public string Identifier { get; set; }

        public bool Changed { get; set; }

        [JsonProperty]
        public string NodeId
        {
            get { return _nodeId; }
            set { this.Changed("NodeId", ref _nodeId, value); }
        }

        [JsonProperty]
        public string ItemId
        {
            get { return _itemId; }
            set { this.Changed("ItemId", ref _itemId, value); }
        }

        [JsonProperty]
        public string ValueId
        {
            get { return _valueId; }
            set { this.Changed("ValueId", ref _valueId, value); }
        }

        
        
    }


    
}