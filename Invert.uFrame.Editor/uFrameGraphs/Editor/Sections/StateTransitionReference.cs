namespace Invert.uFrame.Editor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Invert.Core.GraphDesigner;
    
    
    public class StateTransitionReference : StateTransitionReferenceBase {
        public override void Validate(List<ErrorInfo> info)
        {
            base.Validate(info);
            if (ToState == null)
            {
                info.AddError(string.Format("Transition {0} doesn't have an endpoint.", Name), Identifier);
            }
        }

        private StateNode ToState
        {
            get { return this.OutputTo<StateNode>(); }
        }
    }
    
    public partial interface IStateTransition : Invert.Core.GraphDesigner.IDiagramNodeItem, Invert.Core.GraphDesigner.IConnectable {
    }
}
