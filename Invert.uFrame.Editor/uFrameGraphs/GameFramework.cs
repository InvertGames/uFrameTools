//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.0
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Invert.uFrame.Editor {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Invert.Core;
    using Invert.Core.GraphDesigner;
    
    
    public class GameFrameworkBase : Invert.Core.GraphDesigner.DiagramPlugin {
        
        private Invert.Core.GraphDesigner.NodeConfig<ElementNode> _Element;
        
        private Invert.Core.GraphDesigner.NodeConfig<ElementComputedPropertyNode> _ElementComputedProperty;
        
        private Invert.Core.GraphDesigner.NodeConfig<SceneManagerNode> _SceneManager;
        
        private Invert.Core.GraphDesigner.NodeConfig<SubsystemNode> _Subsystem;
        
        private Invert.Core.GraphDesigner.NodeConfig<ElementViewNode> _ElementView;
        
        private Invert.Core.GraphDesigner.NodeConfig<ElementViewComponentNode> _ElementViewComponent;
        
        private Invert.Core.GraphDesigner.NodeConfig<ElementsGraphRootNode> _ElementsGraphRoot;
        
        private Invert.Core.GraphDesigner.NodeConfig<StateMachineNode> _StateMachine;
        
        private Invert.Core.GraphDesigner.NodeConfig<StateNode> _State;
        
        public Invert.Core.GraphDesigner.NodeConfig<ElementNode> Element {
            get {
                return _Element;
            }
            set {
                _Element = value;
            }
        }
        
        public Invert.Core.GraphDesigner.NodeConfig<ElementComputedPropertyNode> ElementComputedProperty {
            get {
                return _ElementComputedProperty;
            }
            set {
                _ElementComputedProperty = value;
            }
        }
        
        public Invert.Core.GraphDesigner.NodeConfig<SceneManagerNode> SceneManager {
            get {
                return _SceneManager;
            }
            set {
                _SceneManager = value;
            }
        }
        
        public Invert.Core.GraphDesigner.NodeConfig<SubsystemNode> Subsystem {
            get {
                return _Subsystem;
            }
            set {
                _Subsystem = value;
            }
        }
        
        public Invert.Core.GraphDesigner.NodeConfig<ElementViewNode> ElementView {
            get {
                return _ElementView;
            }
            set {
                _ElementView = value;
            }
        }
        
        public Invert.Core.GraphDesigner.NodeConfig<ElementViewComponentNode> ElementViewComponent {
            get {
                return _ElementViewComponent;
            }
            set {
                _ElementViewComponent = value;
            }
        }
        
        public Invert.Core.GraphDesigner.NodeConfig<ElementsGraphRootNode> ElementsGraphRoot {
            get {
                return _ElementsGraphRoot;
            }
            set {
                _ElementsGraphRoot = value;
            }
        }
        
        public Invert.Core.GraphDesigner.NodeConfig<StateMachineNode> StateMachine {
            get {
                return _StateMachine;
            }
            set {
                _StateMachine = value;
            }
        }
        
        public Invert.Core.GraphDesigner.NodeConfig<StateNode> State {
            get {
                return _State;
            }
            set {
                _State = value;
            }
        }
        
        public virtual Invert.Core.GraphDesigner.SelectItemTypeCommand GetPropertySelectionCommand() {
            return new SelectItemTypeCommand() { IncludePrimitives = true, AllowNone = false };
        }
        
        public virtual Invert.Core.GraphDesigner.SelectItemTypeCommand GetCollectionSelectionCommand() {
            return new SelectItemTypeCommand() { IncludePrimitives = true, AllowNone = false };
        }
        
        public virtual Invert.Core.GraphDesigner.SelectItemTypeCommand GetCommandSelectionCommand() {
            return new SelectItemTypeCommand() { IncludePrimitives = true, AllowNone = false };
        }
        
        public override void Initialize(Invert.Core.uFrameContainer container) {
            container.RegisterInstance<IEditorCommand>(GetPropertySelectionCommand(), typeof(PropertyChildItem).Name + "TypeSelection");;
            container.RegisterInstance<IEditorCommand>(GetCollectionSelectionCommand(), typeof(CollectionChildItem).Name + "TypeSelection");;
            container.RegisterInstance<IEditorCommand>(GetCommandSelectionCommand(), typeof(CommandChildItem).Name + "TypeSelection");;
            container.AddTypeItem<PropertyChildItem>();
            container.AddTypeItem<CollectionChildItem>();
            container.AddTypeItem<CommandChildItem>();
            container.AddItem<SubsystemSlot>();
            container.AddItem<RegisteredInstanceReference>();
            container.AddItem<ExportSubSystemSlot>();
            container.AddItem<ImportSubSystemSlot>();
            container.AddItem<SceneManagerTransitionReference>();
            container.AddItem<ScenePropertiesSlot>();
            container.AddItem<ViewBindingsReference>();
            container.AddItem<ComputedSubPropertiesReference>();
            container.AddItem<StateMachineTransitionChildItem>();
            container.AddItem<StateTransitionReference>();
            container.AddItem<StartStateSlot>();
            container.AddItem<ElementInputSlot>();
            Element = container.AddNode<ElementNode,ElementNodeViewModel,ElementNodeDrawer>("Element");
            Element.Inheritable();
            Element.HasSubNode<ElementViewNode>();
            Element.HasSubNode<ElementViewComponentNode>();
            Element.HasSubNode<ElementComputedPropertyNode>();
            Element.HasSubNode<StateMachineNode>();
            ElementComputedProperty = container.AddNode<ElementComputedPropertyNode,ElementComputedPropertyNodeViewModel,ElementComputedPropertyNodeDrawer>("ElementComputedProperty");
            ElementComputedProperty.Color(NodeColor.Green);
            SceneManager = container.AddNode<SceneManagerNode,SceneManagerNodeViewModel,SceneManagerNodeDrawer>("SceneManager");
            SceneManager.Color(NodeColor.Black);
            Subsystem = container.AddNode<SubsystemNode,SubsystemNodeViewModel,SubsystemNodeDrawer>("Subsystem");
            Subsystem.Color(NodeColor.DarkGray);
            Subsystem.HasSubNode<ElementNode>();
            ElementView = container.AddNode<ElementViewNode,ElementViewNodeViewModel,ElementViewNodeDrawer>("ElementView");
            ElementView.Inheritable();
            ElementView.Color(NodeColor.Blue);
            ElementViewComponent = container.AddNode<ElementViewComponentNode,ElementViewComponentNodeViewModel,ElementViewComponentNodeDrawer>("ElementViewComponent");
            ElementViewComponent.Inheritable();
            ElementViewComponent.Color(NodeColor.Orange);
            ElementsGraphRoot = container.AddGraph<ElementsGraph, ElementsGraphRootNode>("ElementsGraph");
            ElementsGraphRoot.HasSubNode<SceneManagerNode>();
            ElementsGraphRoot.HasSubNode<SubsystemNode>();
            StateMachine = container.AddGraph<StateMachineGraph, StateMachineNode>("StateMachineGraph");
            StateMachine.Color(NodeColor.Purple);
            StateMachine.HasSubNode<StateNode>();
            State = container.AddNode<StateNode,StateNodeViewModel,StateNodeDrawer>("State");
            State.Color(NodeColor.DarkGray);
            container.Connectable<ElementNode,ElementViewNode>();
            container.Connectable<ElementComputedPropertyNode,ElementComputedPropertyNode>();
            container.Connectable<ElementComputedPropertyNode,StateMachineTransitionChildItem>();
            container.Connectable<ElementViewNode,ElementViewComponentNode>();
            container.Connectable<PropertyChildItem,ElementNode>();
            container.Connectable<PropertyChildItem,ElementComputedPropertyNode>();
            container.Connectable<PropertyChildItem,PropertyChildItem>();
            container.Connectable<CollectionChildItem,ElementNode>();
            container.Connectable<CommandChildItem,ElementNode>();
            container.Connectable<CommandChildItem,StateMachineTransitionChildItem>();
            container.Connectable<SceneManagerTransitionReference,SceneManagerNode>();
            container.Connectable<StateTransitionReference,StateNode>();
            container.Connectable<IRegisteredInstance,RegisteredInstanceReference>();
            container.Connectable<ISceneManagerTransition,SceneManagerTransitionReference>();
            container.Connectable<IViewBindings,ViewBindingsReference>();
            container.Connectable<IComputedSubProperties,ComputedSubPropertiesReference>();
            container.Connectable<IStateTransition,StateTransitionReference>();
        }
    }
    
    public class ElementsGraphBase : GenericGraphData<ElementsGraphRootNode> {
    }
    
    public class StateMachineGraphBase : GenericGraphData<StateMachineNode> {
    }
    
    public class ElementNodeBase : Invert.Core.GraphDesigner.GenericInheritableNode, IRegisteredInstance, IElementInputSlot {
        
        public virtual bool AllowMultipleInputs {
            get {
                return false;
            }
        }
        
        public virtual bool AllowMultipleOutputs {
            get {
                return true;
            }
        }
    }
    
    public class ElementComputedPropertyNodeBase : Invert.Core.GraphDesigner.GenericNode {
        
        public virtual bool AllowMultipleInputs {
            get {
                return false;
            }
        }
        
        public virtual bool AllowMultipleOutputs {
            get {
                return false;
            }
        }
        
        public virtual System.Collections.Generic.IEnumerable<IComputedSubProperties> PossibleProperties {
            get {
                return this.Project.AllGraphItems.OfType<IComputedSubProperties>();
            }
        }
        
        [Invert.Core.GraphDesigner.ReferenceSection("Properties", SectionVisibility.Always, false, false, typeof(IComputedSubProperties), false, OrderIndex=0, HasPredefinedOptions=false)]
        public virtual System.Collections.Generic.IEnumerable<ComputedSubPropertiesReference> Properties {
            get {
                return ChildItems.OfType<ComputedSubPropertiesReference>();
            }
        }
    }
    
    public class SceneManagerNodeBase : Invert.Core.GraphDesigner.GenericNode {
        
        private SubsystemSlot _SubsystemInputSlot;
        
        public virtual bool AllowMultipleInputs {
            get {
                return true;
            }
        }
        
        public virtual bool AllowMultipleOutputs {
            get {
                return true;
            }
        }
        
        public virtual System.Collections.Generic.IEnumerable<ISceneManagerTransition> PossibleTransitions {
            get {
                return this.Project.AllGraphItems.OfType<ISceneManagerTransition>();
            }
        }
        
        [Invert.Core.GraphDesigner.ReferenceSection("Transitions", SectionVisibility.Always, false, false, typeof(ISceneManagerTransition), false, OrderIndex=0, HasPredefinedOptions=false)]
        public virtual System.Collections.Generic.IEnumerable<SceneManagerTransitionReference> Transitions {
            get {
                return ChildItems.OfType<SceneManagerTransitionReference>();
            }
        }
        
        public virtual System.Collections.Generic.IEnumerable<RegisteredInstanceReference> ImportedItems {
            get {
                yield break;
            }
        }
        
        [Invert.Core.GraphDesigner.InputSlot("Subsystem", false, SectionVisibility.Always, OrderIndex=0)]
        public virtual SubsystemSlot SubsystemInputSlot {
            get {
                return _SubsystemInputSlot;
            }
            set {
                _SubsystemInputSlot = value;
            }
        }
    }
    
    public class SubsystemNodeBase : Invert.Core.GraphDesigner.GenericNode {
        
        private ImportSubSystemSlot _ImportInputSlot;
        
        private ExportSubSystemSlot _ExportOutputSlot;
        
        public virtual bool AllowMultipleInputs {
            get {
                return true;
            }
        }
        
        public virtual bool AllowMultipleOutputs {
            get {
                return true;
            }
        }
        
        public virtual System.Collections.Generic.IEnumerable<IRegisteredInstance> PossibleRegisteredInstance {
            get {
                return this.Project.AllGraphItems.OfType<IRegisteredInstance>();
            }
        }
        
        [Invert.Core.GraphDesigner.ReferenceSection("RegisteredInstance", SectionVisibility.Always, false, false, typeof(IRegisteredInstance), true, OrderIndex=0, HasPredefinedOptions=false)]
        public virtual System.Collections.Generic.IEnumerable<RegisteredInstanceReference> RegisteredInstance {
            get {
                return ChildItems.OfType<RegisteredInstanceReference>();
            }
        }
        
        public virtual System.Collections.Generic.IEnumerable<RegisteredInstanceReference> AvailableInstances {
            get {
                yield break;
            }
        }
        
        [Invert.Core.GraphDesigner.InputSlot("Import", true, SectionVisibility.WhenNodeIsNotFilter, OrderIndex=0)]
        public virtual ImportSubSystemSlot ImportInputSlot {
            get {
                return _ImportInputSlot;
            }
            set {
                _ImportInputSlot = value;
            }
        }
        
        [Invert.Core.GraphDesigner.OutputSlot("Export", true, SectionVisibility.WhenNodeIsNotFilter, OrderIndex=0)]
        public virtual ExportSubSystemSlot ExportOutputSlot {
            get {
                return _ExportOutputSlot;
            }
            set {
                _ExportOutputSlot = value;
            }
        }
    }
    
    public class ElementViewNodeBase : Invert.Core.GraphDesigner.GenericInheritableNode {
        
        private ScenePropertiesSlot _ScenePropertiesInputSlot;
        
        private ElementInputSlot _ElementInputSlot;
        
        public virtual bool AllowMultipleInputs {
            get {
                return true;
            }
        }
        
        public virtual bool AllowMultipleOutputs {
            get {
                return true;
            }
        }
        
        public virtual System.Collections.Generic.IEnumerable<IViewBindings> PossibleBindings {
            get {
                return this.Project.AllGraphItems.OfType<IViewBindings>();
            }
        }
        
        [Invert.Core.GraphDesigner.ReferenceSection("Bindings", SectionVisibility.Always, false, false, typeof(IViewBindings), false, OrderIndex=2, HasPredefinedOptions=true)]
        public virtual System.Collections.Generic.IEnumerable<ViewBindingsReference> Bindings {
            get {
                return ChildItems.OfType<ViewBindingsReference>();
            }
        }
        
        [Invert.Core.GraphDesigner.InputSlot("SceneProperties", true, SectionVisibility.Always, OrderIndex=0)]
        public virtual ScenePropertiesSlot ScenePropertiesInputSlot {
            get {
                return _ScenePropertiesInputSlot;
            }
            set {
                _ScenePropertiesInputSlot = value;
            }
        }
        
        [Invert.Core.GraphDesigner.InputSlot("Element", false, SectionVisibility.Always, OrderIndex=0)]
        public virtual ElementInputSlot ElementInputSlot {
            get {
                return _ElementInputSlot;
            }
            set {
                _ElementInputSlot = value;
            }
        }
    }
    
    public class ElementViewComponentNodeBase : Invert.Core.GraphDesigner.GenericInheritableNode {
        
        public virtual bool AllowMultipleInputs {
            get {
                return false;
            }
        }
        
        public virtual bool AllowMultipleOutputs {
            get {
                return true;
            }
        }
    }
    
    public class ElementsGraphRootNodeBase : Invert.Core.GraphDesigner.GenericNode {
        
        public virtual bool AllowMultipleInputs {
            get {
                return false;
            }
        }
        
        public virtual bool AllowMultipleOutputs {
            get {
                return false;
            }
        }
    }
    
    public class StateMachineNodeBase : Invert.Core.GraphDesigner.GenericNode {
        
        private StartStateSlot _StartOutputSlot;
        
        public virtual bool AllowMultipleInputs {
            get {
                return false;
            }
        }
        
        public virtual bool AllowMultipleOutputs {
            get {
                return false;
            }
        }
        
        [Invert.Core.GraphDesigner.Section("Transitions", SectionVisibility.Always, OrderIndex=0)]
        public virtual System.Collections.Generic.IEnumerable<StateMachineTransitionChildItem> Transitions {
            get {
                return ChildItems.OfType<StateMachineTransitionChildItem>();
            }
        }
        
        [Invert.Core.GraphDesigner.OutputSlot("Start", false, SectionVisibility.WhenNodeIsFilter, OrderIndex=0)]
        public virtual StartStateSlot StartOutputSlot {
            get {
                return _StartOutputSlot;
            }
            set {
                _StartOutputSlot = value;
            }
        }
    }
    
    public class StateNodeBase : Invert.Core.GraphDesigner.GenericNode, IStartStateSlot {
        
        public virtual bool AllowMultipleInputs {
            get {
                return false;
            }
        }
        
        public virtual bool AllowMultipleOutputs {
            get {
                return false;
            }
        }
        
        public virtual System.Collections.Generic.IEnumerable<IStateTransition> PossibleTransitions {
            get {
                return this.Project.AllGraphItems.OfType<IStateTransition>();
            }
        }
        
        [Invert.Core.GraphDesigner.ReferenceSection("Transitions", SectionVisibility.Always, false, false, typeof(IStateTransition), false, OrderIndex=0, HasPredefinedOptions=false)]
        public virtual System.Collections.Generic.IEnumerable<StateTransitionReference> Transitions {
            get {
                return ChildItems.OfType<StateTransitionReference>();
            }
        }
    }
    
    public class ElementNodeViewModelBase : Invert.Core.GraphDesigner.GenericNodeViewModel<ElementNode> {
        
        public ElementNodeViewModelBase(ElementNode graphItemObject, Invert.Core.GraphDesigner.DiagramViewModel diagramViewModel) : 
                base(graphItemObject, diagramViewModel) {
        }
    }
    
    public class ElementComputedPropertyNodeViewModelBase : Invert.Core.GraphDesigner.GenericNodeViewModel<ElementComputedPropertyNode> {
        
        public ElementComputedPropertyNodeViewModelBase(ElementComputedPropertyNode graphItemObject, Invert.Core.GraphDesigner.DiagramViewModel diagramViewModel) : 
                base(graphItemObject, diagramViewModel) {
        }
    }
    
    public class SceneManagerNodeViewModelBase : Invert.Core.GraphDesigner.GenericNodeViewModel<SceneManagerNode> {
        
        public SceneManagerNodeViewModelBase(SceneManagerNode graphItemObject, Invert.Core.GraphDesigner.DiagramViewModel diagramViewModel) : 
                base(graphItemObject, diagramViewModel) {
        }
    }
    
    public class SubsystemNodeViewModelBase : Invert.Core.GraphDesigner.GenericNodeViewModel<SubsystemNode> {
        
        public SubsystemNodeViewModelBase(SubsystemNode graphItemObject, Invert.Core.GraphDesigner.DiagramViewModel diagramViewModel) : 
                base(graphItemObject, diagramViewModel) {
        }
    }
    
    public class ElementViewNodeViewModelBase : Invert.Core.GraphDesigner.GenericNodeViewModel<ElementViewNode> {
        
        public ElementViewNodeViewModelBase(ElementViewNode graphItemObject, Invert.Core.GraphDesigner.DiagramViewModel diagramViewModel) : 
                base(graphItemObject, diagramViewModel) {
        }
    }
    
    public class ElementViewComponentNodeViewModelBase : Invert.Core.GraphDesigner.GenericNodeViewModel<ElementViewComponentNode> {
        
        public ElementViewComponentNodeViewModelBase(ElementViewComponentNode graphItemObject, Invert.Core.GraphDesigner.DiagramViewModel diagramViewModel) : 
                base(graphItemObject, diagramViewModel) {
        }
    }
    
    public class ElementsGraphRootNodeViewModelBase : Invert.Core.GraphDesigner.GenericNodeViewModel<ElementsGraphRootNode> {
        
        public ElementsGraphRootNodeViewModelBase(ElementsGraphRootNode graphItemObject, Invert.Core.GraphDesigner.DiagramViewModel diagramViewModel) : 
                base(graphItemObject, diagramViewModel) {
        }
    }
    
    public class StateMachineNodeViewModelBase : Invert.Core.GraphDesigner.GenericNodeViewModel<StateMachineNode> {
        
        public StateMachineNodeViewModelBase(StateMachineNode graphItemObject, Invert.Core.GraphDesigner.DiagramViewModel diagramViewModel) : 
                base(graphItemObject, diagramViewModel) {
        }
    }
    
    public class StateNodeViewModelBase : Invert.Core.GraphDesigner.GenericNodeViewModel<StateNode> {
        
        public StateNodeViewModelBase(StateNode graphItemObject, Invert.Core.GraphDesigner.DiagramViewModel diagramViewModel) : 
                base(graphItemObject, diagramViewModel) {
        }
    }
    
    public class ElementNodeDrawerBase : GenericNodeDrawer<ElementNode,ElementNodeViewModel> {
        
        public ElementNodeDrawerBase(ElementNodeViewModel viewModel) : 
                base(viewModel) {
        }
    }
    
    public class ElementComputedPropertyNodeDrawerBase : GenericNodeDrawer<ElementComputedPropertyNode,ElementComputedPropertyNodeViewModel> {
        
        public ElementComputedPropertyNodeDrawerBase(ElementComputedPropertyNodeViewModel viewModel) : 
                base(viewModel) {
        }
    }
    
    public class SceneManagerNodeDrawerBase : GenericNodeDrawer<SceneManagerNode,SceneManagerNodeViewModel> {
        
        public SceneManagerNodeDrawerBase(SceneManagerNodeViewModel viewModel) : 
                base(viewModel) {
        }
    }
    
    public class SubsystemNodeDrawerBase : GenericNodeDrawer<SubsystemNode,SubsystemNodeViewModel> {
        
        public SubsystemNodeDrawerBase(SubsystemNodeViewModel viewModel) : 
                base(viewModel) {
        }
    }
    
    public class ElementViewNodeDrawerBase : GenericNodeDrawer<ElementViewNode,ElementViewNodeViewModel> {
        
        public ElementViewNodeDrawerBase(ElementViewNodeViewModel viewModel) : 
                base(viewModel) {
        }
    }
    
    public class ElementViewComponentNodeDrawerBase : GenericNodeDrawer<ElementViewComponentNode,ElementViewComponentNodeViewModel> {
        
        public ElementViewComponentNodeDrawerBase(ElementViewComponentNodeViewModel viewModel) : 
                base(viewModel) {
        }
    }
    
    public class ElementsGraphRootNodeDrawerBase : GenericNodeDrawer<ElementsGraphRootNode,ElementsGraphRootNodeViewModel> {
        
        public ElementsGraphRootNodeDrawerBase(ElementsGraphRootNodeViewModel viewModel) : 
                base(viewModel) {
        }
    }
    
    public class StateMachineNodeDrawerBase : GenericNodeDrawer<StateMachineNode,StateMachineNodeViewModel> {
        
        public StateMachineNodeDrawerBase(StateMachineNodeViewModel viewModel) : 
                base(viewModel) {
        }
    }
    
    public class StateNodeDrawerBase : GenericNodeDrawer<StateNode,StateNodeViewModel> {
        
        public StateNodeDrawerBase(StateNodeViewModel viewModel) : 
                base(viewModel) {
        }
    }
    
    public class SubsystemSlotBase : SingleInputSlot<ISubsystemSlot> {
        
        public virtual bool AllowMultipleInputs {
            get {
                return true;
            }
        }
        
        public virtual bool AllowMultipleOutputs {
            get {
                return true;
            }
        }
    }
    
    public partial interface ISubsystemSlot : Invert.Core.GraphDesigner.IDiagramNodeItem, Invert.Core.GraphDesigner.IConnectable {
    }
    
    public class ExportSubSystemSlotBase : MultiOutputSlot<IExportSubSystemSlot>, ISubsystemSlot, IImportSubSystemSlot {
        
        public virtual bool AllowMultipleInputs {
            get {
                return true;
            }
        }
        
        public virtual bool AllowMultipleOutputs {
            get {
                return true;
            }
        }
    }
    
    public partial interface IExportSubSystemSlot : Invert.Core.GraphDesigner.IDiagramNodeItem, Invert.Core.GraphDesigner.IConnectable {
    }
    
    public class ImportSubSystemSlotBase : MultiInputSlot<IImportSubSystemSlot>, IExportSubSystemSlot {
        
        public virtual bool AllowMultipleInputs {
            get {
                return true;
            }
        }
        
        public virtual bool AllowMultipleOutputs {
            get {
                return true;
            }
        }
    }
    
    public partial interface IImportSubSystemSlot : Invert.Core.GraphDesigner.IDiagramNodeItem, Invert.Core.GraphDesigner.IConnectable {
    }
    
    public class ScenePropertiesSlotBase : MultiInputSlot<IScenePropertiesSlot> {
        
        public virtual bool AllowMultipleInputs {
            get {
                return true;
            }
        }
        
        public virtual bool AllowMultipleOutputs {
            get {
                return true;
            }
        }
    }
    
    public partial interface IScenePropertiesSlot : Invert.Core.GraphDesigner.IDiagramNodeItem, Invert.Core.GraphDesigner.IConnectable {
    }
    
    public class StartStateSlotBase : SingleOutputSlot<IStartStateSlot> {
        
        public virtual bool AllowMultipleInputs {
            get {
                return true;
            }
        }
        
        public virtual bool AllowMultipleOutputs {
            get {
                return true;
            }
        }
    }
    
    public partial interface IStartStateSlot : Invert.Core.GraphDesigner.IDiagramNodeItem, Invert.Core.GraphDesigner.IConnectable {
    }
    
    public class ElementInputSlotBase : SingleInputSlot<IElementInputSlot> {
        
        public virtual bool AllowMultipleInputs {
            get {
                return true;
            }
        }
        
        public virtual bool AllowMultipleOutputs {
            get {
                return true;
            }
        }
    }
    
    public partial interface IElementInputSlot : Invert.Core.GraphDesigner.IDiagramNodeItem, Invert.Core.GraphDesigner.IConnectable {
    }
    
    public class RegisteredInstanceReferenceBase : Invert.Core.GraphDesigner.GenericReferenceItem<IRegisteredInstance> {
        
        public virtual bool AllowMultipleInputs {
            get {
                return true;
            }
        }
        
        public virtual bool AllowMultipleOutputs {
            get {
                return true;
            }
        }
    }
    
    public partial interface IRegisteredInstance : Invert.Core.GraphDesigner.IDiagramNodeItem, Invert.Core.GraphDesigner.IConnectable {
    }
    
    public class SceneManagerTransitionReferenceBase : Invert.Core.GraphDesigner.GenericReferenceItem<ISceneManagerTransition> {
        
        public virtual bool AllowMultipleInputs {
            get {
                return true;
            }
        }
        
        public virtual bool AllowMultipleOutputs {
            get {
                return true;
            }
        }
    }
    
    public partial interface ISceneManagerTransition : Invert.Core.GraphDesigner.IDiagramNodeItem, Invert.Core.GraphDesigner.IConnectable {
    }
    
    public class ViewBindingsReferenceBase : Invert.Core.GraphDesigner.GenericReferenceItem<IViewBindings> {
        
        public virtual bool AllowMultipleInputs {
            get {
                return true;
            }
        }
        
        public virtual bool AllowMultipleOutputs {
            get {
                return true;
            }
        }
    }
    
    public partial interface IViewBindings : Invert.Core.GraphDesigner.IDiagramNodeItem, Invert.Core.GraphDesigner.IConnectable {
    }
    
    public class ComputedSubPropertiesReferenceBase : Invert.Core.GraphDesigner.GenericReferenceItem<IComputedSubProperties> {
        
        public virtual bool AllowMultipleInputs {
            get {
                return true;
            }
        }
        
        public virtual bool AllowMultipleOutputs {
            get {
                return true;
            }
        }
    }
    
    public partial interface IComputedSubProperties : Invert.Core.GraphDesigner.IDiagramNodeItem, Invert.Core.GraphDesigner.IConnectable {
    }
    
    public class StateTransitionReferenceBase : Invert.Core.GraphDesigner.GenericReferenceItem<IStateTransition> {
        
        public virtual bool AllowMultipleInputs {
            get {
                return true;
            }
        }
        
        public virtual bool AllowMultipleOutputs {
            get {
                return true;
            }
        }
    }
    
    public partial interface IStateTransition : Invert.Core.GraphDesigner.IDiagramNodeItem, Invert.Core.GraphDesigner.IConnectable {
    }
    
    public class PropertyChildItemBase : GenericTypedChildItem, IScenePropertiesSlot, IViewBindings, IComputedSubProperties {
    }
    
    public class CollectionChildItemBase : GenericTypedChildItem, IViewBindings {
    }
    
    public class CommandChildItemBase : GenericTypedChildItem, ISceneManagerTransition, IViewBindings {
    }
    
    public class StateMachineTransitionChildItemBase : Invert.Core.GraphDesigner.GenericNodeChildItem, IStateTransition {
    }
}
