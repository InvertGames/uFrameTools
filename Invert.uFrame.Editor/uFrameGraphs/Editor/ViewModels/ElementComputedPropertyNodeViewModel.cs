namespace Invert.uFrame.Editor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    
    
    public class ElementComputedPropertyNodeViewModel : ElementComputedPropertyNodeViewModelBase {
        
        public ElementComputedPropertyNodeViewModel(ElementComputedPropertyNode graphItemObject, Invert.Core.GraphDesigner.DiagramViewModel diagramViewModel) : 
                base(graphItemObject, diagramViewModel) {
        }
    }
}