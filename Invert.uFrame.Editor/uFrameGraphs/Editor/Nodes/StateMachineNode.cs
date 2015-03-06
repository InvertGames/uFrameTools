namespace Invert.uFrame.Editor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Invert.Core.GraphDesigner;
    
    
    public class StateMachineNode : StateMachineNodeBase, IClassTypeNode {
        public override void Validate(List<ErrorInfo> errors)
        {
            base.Validate(errors);

            if (this.InputFrom<PropertyChildItem>() == null)
            {
                errors.AddWarning(string.Format("StateMachine {0} is not used.", Name), this.Identifier);
            }
            if (StartOutputSlot == null) return;
            if (StartOutputSlot.OutputTo<StateNode>() == null)
            {
                errors.AddError("State Machine requires a start state.",this.Identifier);
            }
        }

        public IEnumerable<StateNode> States
        {
            get { return this.GetContainingNodes(Graph).OfType<StateNode>(); }
        }

        public string ClassName
        {
            get { return Name; }
        }
    }
}
