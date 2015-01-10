using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    
    
    public class SubsystemNodeDrawer : GenericNodeDrawer<SubsystemNode,SubsystemNodeViewModel> {
        
        public SubsystemNodeDrawer(SubsystemNodeViewModel viewModel) : 
                base(viewModel) {
        }
    }
}
