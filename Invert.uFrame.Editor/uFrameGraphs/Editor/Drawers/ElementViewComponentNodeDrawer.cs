using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    
    
    public class ElementViewComponentNodeDrawer : GenericNodeDrawer<ElementViewComponentNode,ElementViewComponentNodeViewModel> {
        
        public ElementViewComponentNodeDrawer(ElementViewComponentNodeViewModel viewModel) : 
                base(viewModel) {
        }
    }
}
