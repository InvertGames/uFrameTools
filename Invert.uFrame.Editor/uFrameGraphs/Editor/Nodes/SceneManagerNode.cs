using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor {
    public class SceneManagerNode : SceneManagerNodeBase {
        
        public override IEnumerable<RegisteredInstanceReference> ImportedItems {
            get { return Subsystem.AvailableInstances; }
        }

        public IEnumerable<ElementNode> IncludedElements
        {
            get
            {
                return Subsystem.ImportedSystemsWithThis.SelectMany(p => p.GetContainingNodes(Project)).OfType<ElementNode>();
                
            }
        }
        public SubsystemNode Subsystem
        {
            get
            {
                var subsystemSlot = SubsystemInputSlot.Item as ExportSubSystemSlot;
                if (subsystemSlot == null) return null;
                var node = subsystemSlot.Node;

                return node as SubsystemNode;
            }
        }
    }
}
