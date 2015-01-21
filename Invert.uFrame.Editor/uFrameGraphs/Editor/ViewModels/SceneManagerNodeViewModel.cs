namespace Invert.uFrame.Editor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    
    
    public class SceneManagerNodeViewModel : SceneManagerNodeViewModelBase{
        
        public SceneManagerNodeViewModel(SceneManagerNode graphItemObject, Invert.Core.GraphDesigner.DiagramViewModel diagramViewModel) : 
                base(graphItemObject, diagramViewModel) {
        }

        public Type CurrentType
        {
            get { return GraphItem.CurrentType; }
        }
    }
}
