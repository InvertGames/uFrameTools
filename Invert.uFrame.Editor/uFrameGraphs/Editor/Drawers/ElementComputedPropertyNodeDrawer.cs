using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    
    
    public class ElementComputedPropertyNodeDrawer : GenericNodeDrawer<ElementComputedPropertyNode,ElementComputedPropertyNodeViewModel> {
        
        public ElementComputedPropertyNodeDrawer(ElementComputedPropertyNodeViewModel viewModel) : 
                base(viewModel) {
        }
    }
}
