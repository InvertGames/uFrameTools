using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    
    
    public class StateNodeDrawer : GenericNodeDrawer<StateNode,StateNodeViewModel> {
        
        public StateNodeDrawer(StateNodeViewModel viewModel) : 
                base(viewModel) {
        }
    }
}
