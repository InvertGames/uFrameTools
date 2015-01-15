using System.Collections.Generic;
using System.Linq;
using Invert.Core;
using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor {
    public partial class ScenePropertiesSlot : ScenePropertiesSlotBase {
        public override void Validate(List<ErrorInfo> info)
        {
            base.Validate(info);
            
            foreach (var input in this.InputsFrom<PropertyChildItem>())
            {
                
                if (input.RelatedTypeNode is ElementNode)
                {
                    var input1 = input;
                    info.AddError("Element Properties as Scene Properties is not supported.", this.Node.Identifier,
                        () =>
                        {
                            this.Node.Graph.RemoveConnection(input1,this);
                        });
                }
            }
        }
    }
    
    public partial interface IScenePropertiesSlot : Invert.Core.GraphDesigner.IDiagramNodeItem, Invert.Core.GraphDesigner.IConnectable {
    }
}
