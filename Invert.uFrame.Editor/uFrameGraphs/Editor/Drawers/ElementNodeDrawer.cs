using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    
    
    public class ElementNodeDrawer : GenericNodeDrawer<ElementNode,ElementNodeViewModel> {
        
        public ElementNodeDrawer(ElementNodeViewModel viewModel) : 
                base(viewModel) {
        }
    }
}
