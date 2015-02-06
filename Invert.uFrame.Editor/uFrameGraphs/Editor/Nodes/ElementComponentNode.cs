namespace Invert.uFrame.Editor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Invert.Core.GraphDesigner;
    
    
    public class ElementComponentNode : ElementComponentNodeBase, IClassTypeNode {
        public string ClassName
        {
            get { return string.Format("I{0}", Name); }
        }
    }
}
