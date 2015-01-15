using System.Linq;
using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor {
    public partial class PropertyChildItem : PropertyChildItemBase {
        public override string DefaultTypeName
        {
            get { return typeof(string).Name; }
        }

        public override void OnConnectionApplied(IConnectable output, IConnectable input)
        {
            base.OnConnectionApplied(output, input);

            if (output == this && input is IClassTypeNode)
            {
                foreach (var item in Outputs.ToArray())
                {
                    if (item.Input is IClassTypeNode && item.Input != input)
                        Node.Graph.RemoveConnection(item.Output,item.Input);
                }
            }

        }
    }
}
