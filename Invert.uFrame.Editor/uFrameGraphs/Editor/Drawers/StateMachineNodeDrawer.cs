using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    
    
    public class StateMachineNodeDrawer : GenericNodeDrawer<StateMachineNode,StateMachineNodeViewModel> {
        
        public StateMachineNodeDrawer(StateMachineNodeViewModel viewModel) : 
                base(viewModel) {
        }
    }
}
