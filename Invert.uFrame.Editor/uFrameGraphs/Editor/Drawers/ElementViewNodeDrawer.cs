using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    
    
    public class ElementViewNodeDrawer : GenericNodeDrawer<ElementViewNode,ElementViewNodeViewModel> {
        
        public ElementViewNodeDrawer(ElementViewNodeViewModel viewModel) : 
                base(viewModel) {
        }
    }
}
