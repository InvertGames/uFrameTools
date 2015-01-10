namespace Invert.uFrame.Editor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Invert.Core.GraphDesigner;
    
    
    public class StateMachineTransitionChildItem : StateMachineTransitionChildItemBase {
        public override void Validate(List<ErrorInfo> info)
        {
            base.Validate(info);
            var computedPropertyNode = this.InputFrom<ElementComputedPropertyNode>();
            if (computedPropertyNode != null)
            {
                if (computedPropertyNode.Type != typeof(bool).Name)
                {
                    info.AddError("Computed property transitions must be of type Boolean.",this.Identifier, () =>
                    {
                        computedPropertyNode.Type = typeof (bool).Name;
                    });

                }
            }
           
        }
    }
}
