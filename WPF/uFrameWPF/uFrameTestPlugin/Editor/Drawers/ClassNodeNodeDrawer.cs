namespace uFrameTestPlugin {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Invert.Core.GraphDesigner;
    
    
    public class ClassNodeNodeDrawer : GenericNodeDrawer<ClassNodeNode,ClassNodeNodeViewModel> {
        
        public ClassNodeNodeDrawer(ClassNodeNodeViewModel viewModel) : 
                base(viewModel) {
        }
    }
}
