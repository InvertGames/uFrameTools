namespace Invert.uFrame.Editor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Invert.Core.GraphDesigner;
    
    
    public class ElementComponentNodeDrawer : GenericNodeDrawer<ElementComponentNode,ElementComponentNodeViewModel> {
        
        public ElementComponentNodeDrawer(ElementComponentNodeViewModel viewModel) : 
                base(viewModel) {
        }
    }
}
