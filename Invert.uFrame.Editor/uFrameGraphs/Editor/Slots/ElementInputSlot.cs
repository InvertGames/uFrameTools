namespace Invert.uFrame.Editor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Invert.Core.GraphDesigner;
    
    
    public class ElementInputSlot : ElementInputSlotBase {
    }
    
    public partial interface IElementInputSlot : Invert.Core.GraphDesigner.IDiagramNodeItem, Invert.Core.GraphDesigner.IConnectable {
    }
}