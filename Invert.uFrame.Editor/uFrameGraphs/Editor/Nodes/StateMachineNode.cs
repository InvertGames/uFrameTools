namespace Invert.uFrame.Editor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Invert.Core.GraphDesigner;
    
    
    public class StateMachineNode : StateMachineNodeBase {
        public override void Validate(List<ErrorInfo> errors)
        {
            base.Validate(errors);
            if (StartOutputSlot.OutputTo<StateNode>() == null)
            {
                errors.AddError("State Machine requires a start state.",this.Identifier);
            }
        }
    }
}
