using System.Linq;
using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor {
    public partial class ViewBindingsReference : ViewBindingsReferenceBase,IViewBindings {
        private uFrameBindingType _bindingType;

        [JsonProperty]
        public string BindingName { get; set; }

        public override string Label
        {
            get { return base.Label; }
        }

        public override string Name
        {
            get { return Title; }
            set { base.Name = value; }
        }

        public override IDiagramNodeItem SourceItemObject
        {
            get
            {
                return
                    ((ElementViewNode) Node).Element.PersistedItems.FirstOrDefault(p => p.Identifier == SourceIdentifier);
            }
        }

        //public override string Name
        //{
        //    get { return Title; }
        //    set { base.Name = value; }
        //}

        public uFrameBindingType BindingType
        {
            get
            {
                return
                    _bindingType ?? (_bindingType = uFramePlugin.BindingTypes.Where(p => p.Name == BindingName).Select(p => p.Instance).FirstOrDefault() as uFrameBindingType);
            }
            set { _bindingType = value; }
        }

        public override string Title
        {
            get
            {
                return string.Format(BindingType.DisplayFormat, SourceItem.Name);
            }
        }
    }
    
    public partial interface IViewBindings : Invert.Core.GraphDesigner.IDiagramNodeItem, Invert.Core.GraphDesigner.IConnectable {
    }
}
