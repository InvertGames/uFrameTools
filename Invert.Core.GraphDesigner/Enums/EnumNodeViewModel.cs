using System.Collections.Generic;
using Invert.uFrame.Editor.ElementDesigner;

namespace Invert.uFrame.Editor.ViewModels
{
    public class EnumNodeViewModel : DiagramNodeViewModel<EnumData>
    {
        public EnumNodeViewModel(EnumData data, DiagramViewModel diagramViewModel)
            : base(data, diagramViewModel)
        {

        }

        protected override void CreateContent()
        {
            ContentItems.Add(new SectionHeaderViewModel()
            {
                Name = "Items",
                AddCommand = new SimpleEditorCommand<EnumNodeViewModel>(_=>_.AddNew())
            });
            base.CreateContent();

        }

        public override ConnectorViewModel OutputConnector
        {
            get { return null; }
        }

        public override bool AllowCollapsing
        {
            get { return true; }
        }

        public IEnumerable<EnumItem> EnumItems
        {
            get { return GraphItem.EnumItems; }
        }

        public void AddNew()
        {
            GraphItem.Project.AddItem(new EnumItem()
            {
                Node = GraphItem,
                Name = GraphItem.Project.GetUniqueName("Item")
            });
        }
    }
}