using System.Collections.Generic;

namespace Invert.uFrame.Editor.ViewModels
{
    public class EnumNodeViewModel : DiagramNodeViewModel<EnumData>
    {
        public EnumNodeViewModel(EnumData data, DiagramViewModel diagramViewModel)
            : base(data, diagramViewModel)
        {

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
            GraphItem.EnumItems.Add(new EnumItem()
            {
                Node = GraphItem,
                Name = GraphItem.Data.GetUniqueName("Item")
            });
        }
    }
}