namespace Invert.uFrame.Editor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Invert.Core.GraphDesigner;
    
    
    public class HandlerNode : HandlerNodeBase {
        public Handlers HandlersSlot
        {
            get { return this.InputFrom<Handlers>(); }
        }
        public ElementComponentNode Component
        {
            get
            {
                
                return HandlersSlot.Node as ElementComponentNode;
            }
        }

        public override void Validate(List<ErrorInfo> errors)
        {
            base.Validate(errors);
            if (Component == null)
            {
                errors.AddWarning(string.Format("Component not associated with {0} handler.", this.Name));
            }
        }

    }
}
