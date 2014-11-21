using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Core.GraphDesigner.Settings;
using Invert.uFrame.Code.Bindings;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ElementDesigner.Commands;

using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEngine;

namespace Invert.uFrame.Editor
{
    public class uFrameCorePlugin : DiagramPlugin
    {    
        public override decimal LoadPriority
        {
            get { return -90; }
        }

   
        public override void Initialize(uFrameContainer container)
        {

            container.RegisterInstance<IDiagramNodeCommand>(new SelectViewBaseElement(), "SelectView");
            container.RegisterInstance<IDiagramNodeCommand>(new MarkIsTemplateCommand(), "MarkAsTemplate");

            // Graph Diagrams
            container.Register<GraphData, ElementsGraph>("Graph");
            container.Register<GraphData, ExternalSubsystemGraph>("External Subsystem Graph");
            container.Register<GraphData, ExternalElementGraph>("External Element Graph");
            container.Register<GraphData, ExternalStateMachineGraph>("External State Machine Graph");

            // Scene Managers
            container.RegisterGraphItem<SceneManagerData, SceneManagerViewModel, SceneManagerDrawer>();
            container.RegisterChildGraphItem<SceneManagerTransition, SceneTransitionItemViewModel, ItemDrawer>();
            container.RegisterFilterNode<SceneFlowFilter, SceneManagerData>();
            //container.RegisterInstance<IConnectionStrategy>(new SceneTransitionConnectionStrategy(), "SceneTransitionConnectionStrategy");
            //container.RegisterInstance<IConnectionStrategy>(new SceneManagerSubsystemConnectionStrategy(), "SceneManagerSubsystemConnectionStrategy");
            container.RegisterInstance(new AddTransitionCommand());

            // Sub Systems
            container.RegisterGraphItem<SubSystemData, SubSystemViewModel, SubSystemDrawer>();
            container.RegisterChildGraphItem<RegisteredInstanceData, RegisterInstanceItemViewModel, TypedItemDrawer>();
            container.RegisterFilterNode<SceneFlowFilter, SubSystemData>();
            //container.RegisterInstance<IConnectionStrategy>(new SubsystemConnectionStrategy(), "SubsystemConnectionStrategy");
            container.RegisterInstance(new AddInstanceCommand());

            // Elements
            container.RegisterGraphItem<ElementData, ElementNodeViewModel, ElementDrawer>();
            container.RegisterChildGraphItem<ViewModelPropertyData, ElementPropertyItemViewModel, TypedItemDrawer>();
            container.RegisterChildGraphItem<ViewModelCommandData, ElementCommandItemViewModel, TypedItemDrawer>();
            container.RegisterChildGraphItem<ViewModelCollectionData, ElementCollectionItemViewModel, TypedItemDrawer>();
            container.RegisterFilterNode<SubSystemData, ElementData>();
            container.RegisterInstance(new AddElementCommandCommand());
            container.RegisterInstance(new AddElementCollectionCommand());
            container.RegisterInstance(new AddElementPropertyCommand());
            //container.RegisterInstance<IConnectionStrategy>(new ElementInheritanceConnectionStrategy(), "ElementInheritanceConnectionStrategy");
            //container.RegisterInstance<IConnectionStrategy>(new ElementViewConnectionStrategy(), "ElementViewConnectionStrategy");

            // Computed Properties
            container.RegisterGraphItem<ComputedPropertyData, ComputedPropertyNodeViewModel, ComputedPropertyDrawer>();
            container.RegisterFilterNode<ElementData, ComputedPropertyData>();
            //container.RegisterInstance<IConnectionStrategy>(new ComputedPropertyInputsConnectionStrategy(), "ComputedPropertyInputsConnectionStrategy");

            // Views
            container.RegisterGraphItem<ViewData, ViewNodeViewModel, ViewDrawer>();
            container.RegisterChildGraphItem<ViewPropertyData, ElementViewPropertyItemViewModel, ItemDrawer>();
            container.RegisterChildGraphItem<ViewBindingData, ViewBindingItemViewModel, ItemDrawer>();
            container.RegisterFilterNode<ElementData, ViewData>();
            container.RegisterInstance(new AddBindingCommand());
            //container.RegisterInstance<IConnectionStrategy>(new TwoWayPropertyConnectionStrategy(), "TwoWayPropertyConnectionStrategy");
            //container.RegisterInstance<IConnectionStrategy>(new ViewInheritanceConnectionStrategy(), "ViewInheritanceConnectionStrategy");

            // View Components
            container.RegisterGraphItem<ViewComponentData, ViewComponentNodeViewModel, ViewComponentDrawer>();
            container.RegisterFilterNode<ElementData, ViewComponentData>();
            //container.RegisterInstance<IConnectionStrategy>(new ViewComponentElementConnectionStrategy(), "ViewComponentElementConnectionStrategy");
            //container.RegisterInstance<IConnectionStrategy>(new ViewComponentInheritanceConnectionStrategy(), "ViewComponentInheritanceConnectionStrategy");

            // Enums
            container.RegisterFilterNode<SubSystemData, EnumData>();
            container.RegisterFilterNode<ElementData, EnumData>();

            // State Machines
            container.RegisterGraphItem<StateMachineNodeData, StateMachineNodeViewModel, StateMachineNodeDrawer>();
            container.RegisterGraphItem<StateMachineStateData, StateMachineStateNodeViewModel, StateMachineStateNodeDrawer>();
            container.RegisterChildGraphItem<StateMachineTransition, StateMachineTransitionViewModel, ItemDrawer>();
            container.RegisterChildGraphItem<StateTransitionData, StateTransitionViewModel, ItemDrawer>();
            container.RegisterGraphItem<StateMachineActionData, StateActionNodeViewModel, StateActionNodeDrawer>();
            container.RegisterFilterNode<ElementData, StateMachineNodeData>();
            container.RegisterFilterNode<StateMachineNodeData, StateMachineStateData>();

            //container.RegisterInstance<IConnectionStrategy>(new StartStateConnectionStrategy(), "StartStateConnectionStrategy");
            //container.RegisterInstance<IConnectionStrategy>(new StateMachineTransitionConnectionStrategy(), "StateMachineTransitionConnectionStrategy");
            //container.RegisterInstance<IConnectionStrategy>(new ComputedTransitionConnectionStrategy(), "ComputedTransitionConnectionStrategy");
            //container.RegisterInstance<IConnectionStrategy>(new ElementStateMachineConnectionStrategy(), "ElementStateMachineConnectionStrategy");
            //container.RegisterInstance<IConnectionStrategy>(new ElementStateVariableConnectionStrategy(), "ElementStateVariableConnectionStrategy");

            // Simple Classes
            
            container.RegisterFilterNode<ElementData, ClassNodeData>();
            container.RegisterFilterNode<SubSystemData, ClassNodeData>();

#if DEBUG
            // Model Classes ^^
            container.RegisterGraphItem<ModelClassNodeData, ModelClassNodeViewModel, ModelClassNodeDrawer>();
            container.RegisterFilterNode<ElementData, ModelClassNodeData>();
            container.RegisterFilterNode<SubSystemData, ModelClassNodeData>();
#endif
            // General Connections
            //container.RegisterInstance<IConnectionStrategy>(new AssociationConnectionStrategy(), "AssociationConnectionStrategy");
            //container.RegisterInstance<IConnectionStrategy>(new ComputedPropertyInputStrategy(), "ComputedPropertyInputStrategy");


            // Type selections
            container.RegisterInstance<IEditorCommand>(new SelectItemTypeCommand() { AllowNone = false, PrimitiveOnly = false }, "ViewModelPropertyTypeSelection");
            container.RegisterInstance<IEditorCommand>(new SelectItemTypeCommand() { AllowNone = false, PrimitiveOnly = false }, "ClassPropertyTypeSelection");
            container.RegisterInstance<IEditorCommand>(new SelectItemTypeCommand() { AllowNone = false, PrimitiveOnly = false }, "ClassCollectionTypeSelection");
            container.RegisterInstance<IEditorCommand>(new SelectItemTypeCommand() { AllowNone = true, PrimitiveOnly = false }, "ViewModelCommandTypeSelection");
            container.RegisterInstance<IEditorCommand>(new SelectItemTypeCommand() { AllowNone = false, PrimitiveOnly = false }, "ViewModelCollectionTypeSelection");
            container.RegisterInstance<IEditorCommand>(new SelectItemTypeCommand() { AllowNone = false, PrimitiveOnly = false }, "ComputedPropertyTypeSelection");
            container.RegisterInstance<IEditorCommand>(new SelectItemTypeCommand() { AllowNone = false, PrimitiveOnly = false }, "StateMachineVariableTypeSelection");


            container.RegisterInstance<IUFrameTypeProvider>(new uFrameStringTypeProvider());


            // Where the generated code files are placed
            container.Register<ICodePathStrategy, DefaultCodePathStrategy>("Default");
            container.Register<ICodePathStrategy, SubSystemPathStrategy>("By Subsystem");


            container.Register<DesignerGeneratorFactory, ElementDataGeneratorFactory>("ElementData");
            container.Register<DesignerGeneratorFactory, EnumDataGeneratorFactory>("EnumData");
            container.Register<DesignerGeneratorFactory, ViewDataGeneratorFactory>("ViewData");
            container.Register<DesignerGeneratorFactory, ViewComponentDataGeneratorFactory>("ViewComponentData");
            container.Register<DesignerGeneratorFactory, SceneManagerDataGeneratorFactory>("SceneManagerData");
            

#if DEBUG
            container.Register<DesignerGeneratorFactory, ModelClassNodeCodeFactory>("ModelClassNodeData");
#endif

            container.Register<IBindingGenerator, StandardPropertyBindingGenerator>("StandardPropertyBindingGenerator");
            container.Register<IBindingGenerator, ComputedPropertyBindingGenerator>("ComputedPropertyBindingGenerator");
            container.Register<IBindingGenerator, ViewCollectionBindingGenerator>("ViewCollectionBindingGenerator");
            container.Register<IBindingGenerator, DefaultCollectionBindingGenerator>("DefaultCollectionBindingGenerator");
            //container.Register<IBindingGenerator, InstantiateViewPropertyBindingGenerator>("InstantiateViewPropertyBindingGenerator");
            container.Register<IBindingGenerator, CommandExecutedBindingGenerator>("CommandExecutedBindingGenerator");
            container.Register<IBindingGenerator, StateMachinePropertyBindingGenerator>("StateMachinePropertyBindingGenerator");

            container.Register<DesignerGeneratorFactory, StateMachineCodeFactory>("StateMachineCodeFactory");
            container.Register<DesignerGeneratorFactory, StateMachineStateCodeFactory>("StateMachineStateCodeFactory");
            //container.RegisterInstance<ITypeGeneratorPostProcessor>(new StateMachineViewModelProcessor(),
            //    "StateMachineViewModelPostProcessor");
        }

        public override void Loaded()
        {
            base.Loaded();
            
            uFrameEditor.Loaded();
        }
    }


}
