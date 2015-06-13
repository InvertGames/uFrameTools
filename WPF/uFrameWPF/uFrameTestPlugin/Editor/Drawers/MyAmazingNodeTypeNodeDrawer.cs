namespace uFrameTestPlugin {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Invert.Core.GraphDesigner;
    
    
    public class MyAmazingNodeTypeNodeDrawer : GenericNodeDrawer<MyAmazingNodeTypeNode,MyAmazingNodeTypeNodeViewModel> {
        
        public MyAmazingNodeTypeNodeDrawer(MyAmazingNodeTypeNodeViewModel viewModel) : 
                base(viewModel) {
        }
    }
}
