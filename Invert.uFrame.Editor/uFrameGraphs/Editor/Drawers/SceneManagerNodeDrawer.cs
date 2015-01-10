using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    
    
    public class SceneManagerNodeDrawer : GenericNodeDrawer<SceneManagerNode,SceneManagerNodeViewModel> {
        
        public SceneManagerNodeDrawer(SceneManagerNodeViewModel viewModel) : 
                base(viewModel) {
        }
    }
}
