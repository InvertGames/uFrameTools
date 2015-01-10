using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    
    
    public class ElementsGraphRootNodeDrawer : GenericNodeDrawer<ElementsGraphRootNode,ElementsGraphRootNodeViewModel> {
        
        public ElementsGraphRootNodeDrawer(ElementsGraphRootNodeViewModel viewModel) : 
                base(viewModel) {
        }
    }
}
