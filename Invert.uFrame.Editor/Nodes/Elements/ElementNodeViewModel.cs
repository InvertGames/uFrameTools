using System.Collections.Generic;

namespace Invert.uFrame.Editor.ViewModels
{
    public class ElementNodeViewModel : DiagramNodeViewModel<ElementData>
    {
        

        public ElementNodeViewModel(ElementData data, DiagramViewModel diagramViewModel)
            : base(data, diagramViewModel)
        {

        }

        public ConnectorViewModel InheritanceOutput { get; set; }
        
        protected override void DataObjectChanged()
        {
            base.DataObjectChanged();
            
        }

        public override bool ShowSubtitle
        {
            get { return !string.IsNullOrEmpty(SubTitle); }
        }

        public override bool AllowCollapsing
        {
            get { return true; }
        }

        public bool IsTemplate
        {
            get { return GraphItem.IsTemplate; }
        }

        public bool IsMultiInstance
        {
            get { return GraphItem.IsMultiInstance; }
        }

        public IEnumerable<IViewModelItem> ViewModelItems
        {
            get { return GraphItem.ViewModelItems; }
        }

        public ICollection<ViewModelPropertyData> Properties
        {
            get { return GraphItem.Properties; }
        }
        public ICollection<ViewModelCommandData> Commands
        {
            get { return GraphItem.Commands; }
        }
        public ICollection<ViewModelCollectionData> Collections
        {
            get { return GraphItem.Collections; }
        }

        public void AddProperty()
        {
            var property = new ViewModelPropertyData()
            {
                Node = GraphItem,
                DefaultValue = string.Empty,
                Name = GraphItem.Data.GetUniqueName("String1"),
                RelatedType = typeof(string).Name
            };
            this.GraphItem.Properties.Add(property);
        }

        public void AddCommand()
        {
            var property = new ViewModelCommandData()
            {
                Node = GraphItem,
                Name = uFrameEditor.CurrentProject.GetUniqueName("Command"),
            };

            this.GraphItem.Commands.Add(property);
        }
        public void AddCollection()
        {
            var property = new ViewModelCollectionData()
            {
                Node = GraphItem,
                Name = GraphItem.Data.GetUniqueName("Collection"),
                RelatedType = typeof(string).Name
            };
            this.GraphItem.Collections.Add(property);
        }

    }
}