using System.Collections.Generic;
using System.Linq;
using Invert.Core.GraphDesigner;

namespace Invert.uFrame.Editor {
    public class SubsystemNode : SubsystemNodeBase {
        
        public override IEnumerable<RegisteredInstanceReference> AvailableInstances {
            get {
                foreach (var item in RegisteredInstance)
                {
                    yield return item;
                }
                foreach (var item in ImportedSystems)
                {
                    foreach (var instance in item.RegisteredInstance)
                    {
                        yield return instance;
                    }
                }
                
            }
        }

        public IEnumerable<SubsystemNode> ImportedSystems
        {
            get
            {
                foreach (var item in ImportInputSlot.Items.OfType<ExportSubSystemSlot>().Select(p=>p.Node).OfType<SubsystemNode>())
                {
                    yield return item;
                    foreach (var x in item.ImportedSystems)
                    {
                        yield return x;
                    }
                }
            }
        }
        public IEnumerable<SubsystemNode> ImportedSystemsWithThis
        {
            get
            {
                yield return this;
                foreach (var item in ImportInputSlot.Items.OfType<ExportSubSystemSlot>().Select(p => p.Node).OfType<SubsystemNode>())
                {
                    yield return item;
                    foreach (var x in item.ImportedSystems)
                    {
                        yield return x;
                    }
                }
            }
        }

    }

    public class SubsystemNodeViewModel : GenericNodeViewModel<SubsystemNode>
    {
        public SubsystemNodeViewModel(SubsystemNode graphItemObject, DiagramViewModel diagramViewModel) : base(graphItemObject, diagramViewModel)
        {

        }

        protected override void CreateContent()
        {
            base.CreateContent();

            foreach (var item in GraphItem.GetContainingNodes(DiagramViewModel.CurrentRepository).OfType<GenericNode>())
            {
                if (!IsVisible(SectionVisibility.WhenNodeIsNotFilter)) continue;
                CreateContentByConfiguration(item.SlotConfigurations.Where(p=>p.IsInput).Cast<GraphItemConfiguration>(), item);
            }

        }
    }
#if UNITY_DLL
    public class SubsystemNodeDrawer : GenericNodeDrawer<SubsystemNode,SubsystemNodeViewModel>
    {
        public SubsystemNodeDrawer(SubsystemNodeViewModel viewModel)
            : base(viewModel)
        {
        }
    }
#endif
}
