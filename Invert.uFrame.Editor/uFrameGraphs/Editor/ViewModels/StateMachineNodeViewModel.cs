namespace Invert.uFrame.Editor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    
    
    public class StateMachineNodeViewModel : StateMachineNodeViewModelBase {
        
        public StateMachineNodeViewModel(StateMachineNode graphItemObject, Invert.Core.GraphDesigner.DiagramViewModel diagramViewModel) : 
                base(graphItemObject, diagramViewModel) {
        }
    }
}