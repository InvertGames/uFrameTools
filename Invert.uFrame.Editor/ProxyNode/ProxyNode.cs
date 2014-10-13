using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.uFrame.Editor.ViewModels;

namespace Invert.uFrame.Editor.ProxyNode
{
    public class ProxyNode : DiagramNode
    {

        public string ItemIdentifier
        {
            get; set;
        }
        public ITypeDiagramItem TypedDiagramItem
        {
            get; set;
        }
        
        public override IEnumerable<IDiagramNodeItem> Items
        {
            get
            {
                yield break;
            }
        }

        public override IEnumerable<IDiagramNodeItem> ContainedItems
        {
            get
            {
                yield break;
            }
            set
            {
                
            }
        }

        public override string Label
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class ProxyNodeViewModel : DiagramNodeViewModel<ProxyNode>
    {
        public ProxyNodeViewModel(ProxyNode graphItemObject, DiagramViewModel diagramViewModel) : base(graphItemObject, diagramViewModel)
        {
        }
    }

    public class ProxyNodeDrawer : DiagramNodeDrawer<ProxyNodeViewModel>
    {
        public ProxyNodeDrawer(ProxyNodeViewModel viewModel) : base(viewModel)
        {
        }

    }
}
