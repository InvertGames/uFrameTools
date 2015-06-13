using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Invert.IOC;
using UnityEngine;
#if !UNITY_DLL
using KeyCode = System.Windows.Forms.Keys;
#endif
namespace Invert.Core.GraphDesigner
{
    public class GraphDesignerPlugin : DiagramPlugin, IPrefabNodeProvider, ICommandEvents, IConnectionEvents
    {
        public override decimal LoadPriority
        {
            get { return -100; }
        }
        public override bool Required
        {
            get { return true; }
        }

        public override void Initialize(UFrameContainer container)
        {
            ListenFor<ICommandEvents>();
            ListenFor<IConnectionEvents>();
//#if UNITY_DLL
        
//            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
//            {
//                InvertApplication.CachedAssemblies.Add(assembly);
//            }
//#endif
            var typeContainer = InvertGraphEditor.TypesContainer;
            // Drawers
            container.Register<DiagramViewModel,DiagramViewModel>();
            container.RegisterDrawer<PropertyFieldViewModel, PropertyFieldDrawer>();
            container.Register<SectionHeaderDrawer, SectionHeaderDrawer>();
            container.RegisterItemDrawer<GenericItemHeaderViewModel, GenericChildItemHeaderDrawer>();
             
            container.RegisterDrawer<SectionHeaderViewModel, SectionHeaderDrawer>();
            container.RegisterDrawer<ConnectorViewModel, ConnectorDrawer>();
            container.RegisterDrawer<ConnectionViewModel, ConnectionDrawer>();
            container.RegisterDrawer<InputOutputViewModel, SlotDrawer>();
            container.RegisterDrawer<DiagramViewModel, DiagramDrawer>();
            //typeContainer.AddItem<GenericSlot,InputOutputViewModel,SlotDrawer>();
            //typeContainer.AddItem<BaseClassReference, InputOutputViewModel, SlotDrawer>();

            container.RegisterInstance<IConnectionStrategy>(new InputOutputStrategy(),"InputOutputStrategy");
            //container.RegisterConnectable<GenericTypedChildItem, IClassTypeNode>();
            container.RegisterConnectable<GenericInheritableNode, GenericInheritableNode>();
            container.RegisterInstance<IConnectionStrategy>(new TypedItemConnectionStrategy(), "TypedConnectionStrategy");
            //container.RegisterInstance<IConnectionStrategy>(new RegisteredConnectionStrategy(),"RegisteredConnectablesStrategy");

            container.AddNode<EnumNode>("Enum")
                .AddCodeTemplate<EnumNodeGenerator>();
            container.AddItem<EnumChildItem>();

            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(int), Group = "", Label = "int", IsPrimitive = true }, "int");
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(string), Group = "", Label = "string", IsPrimitive = true }, "string");
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(decimal), Group = "", Label = "decimal", IsPrimitive = true }, "decimal");
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(float), Group = "", Label = "float", IsPrimitive = true }, "float");
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(bool), Group = "", Label = "bool", IsPrimitive = true }, "bool");
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(char), Group = "", Label = "char", IsPrimitive = true }, "char");
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(DateTime), Group = "", Label = "date", IsPrimitive = true }, "date");
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(Vector2), Group = "", Label = "Vector2", IsPrimitive = true }, "Vector2");
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(Vector3), Group = "", Label = "Vector3", IsPrimitive = true }, "Vector3");
   
            container.Register<DesignerGeneratorFactory, RegisteredTemplateGeneratorsFactory>("TemplateGenerators");
            
#if UNITY_DLL        
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(Quaternion), Group = "", Label = "Quaternion", IsPrimitive = true }, "Quaternion");
            container.Register<DesignerGeneratorFactory, Invert.uFrame.CodeGen.ClassNodeGenerators.SimpleClassNodeCodeFactory>("ClassNodeData");
            
            // Enums
            container.RegisterGraphItem<EnumData, EnumNodeViewModel>();
            container.RegisterChildGraphItem<EnumItem, EnumItemViewModel>();
            //container.RegisterInstance(new AddEnumItemCommand());

            // Class Nodes
            container.RegisterGraphItem<ClassPropertyData, ClassPropertyItemViewModel>();
            container.RegisterGraphItem<ClassCollectionData, ClassCollectionItemViewModel>();
            container.RegisterGraphItem<ClassNodeData, ClassNodeViewModel>();
            

#endif
            // Register the container itself
            container.RegisterInstance<IUFrameContainer>(container);
            container.RegisterInstance<UFrameContainer>(container);

            container.AddNode<TypeReferenceNode, TypeReferenceNodeViewModel, TypeReferenceNodeDrawer>("Type Reference");

            // Toolbar commands
            container.RegisterInstance<IToolbarCommand>(new SelectProjectCommand(), "SelectProjectCommand");
            container.RegisterInstance<IToolbarCommand>(new SelectDiagramCommand(), "SelectDiagramCommand");
            container.RegisterInstance<IToolbarCommand>(new PopToFilterCommand(), "PopToFilterCommand");
            container.RegisterInstance<IToolbarCommand>(new SaveCommand(), "SaveCommand");
#if UNITY_DLL
            container.RegisterToolbarCommand<HelpCommand>();
            container.RegisterNodeCommand<NodeHelpCommand>();
#endif
            //container.RegisterInstance<IToolbarCommand>(new AddNewCommand(), "AddNewCommand");
            

            // For no selection diagram context menu
            container.RegisterGraphCommand<AddNodeToGraph>();
            container.RegisterGraphCommand<AddReferenceNode>();
            container.RegisterGraphCommand<ShowItemCommand>();
            container.RegisterNodeCommand<PullFromCommand>();
            //container.RegisterNodeCommand<ToExternalGraph>();
            container.RegisterNodeCommand<OpenCommand>();
            container.RegisterNodeCommand<DeleteCommand>();
            container.RegisterNodeCommand<RenameCommand>();
            container.RegisterNodeCommand<HideCommand>();
            //container.RegisterNodeCommand<RemoveLinkCommand>();

            container.RegisterNodeItemCommand<DeleteItemCommand>();
            container.RegisterNodeItemCommand<MoveUpCommand>();
            container.RegisterNodeItemCommand<MoveDownCommand>();

            container.RegisterInstance<IEditorCommand>(new RemoveNodeItemCommand(), "RemoveNodeItem");

            container.RegisterKeyBinding(new RenameCommand(), "Rename", KeyCode.F2);
            container.RegisterKeyBinding(new SimpleEditorCommand<DiagramViewModel>((p) =>
            {
                p.DeselectAll();
            }), "End All Editing", KeyCode.Return);
      
            container.RegisterKeyBinding(new DeleteItemCommand(), "Delete Item", KeyCode.X, true);
            container.RegisterKeyBinding(new DeleteCommand(), "Delete", KeyCode.Delete);
#if UNITY_DLL
            container.RegisterKeyBinding(new MoveUpCommand(), "Move Up", KeyCode.UpArrow);
            container.RegisterKeyBinding(new MoveDownCommand(), "Move Down", KeyCode.DownArrow);
#endif


            container.RegisterKeyBinding(new SimpleEditorCommand<DiagramViewModel>((p) =>
            {
                InvertGraphEditor.Settings.ShowHelp = !InvertGraphEditor.Settings.ShowHelp;
            }), "Show/Hide This Help", KeyCode.F1);
#if DEBUG
            container.RegisterKeyBinding(new SimpleEditorCommand<DiagramViewModel>((p) =>
            {
                InvertGraphEditor.Settings.ShowGraphDebug = !InvertGraphEditor.Settings.ShowGraphDebug;
            }), "Show/Hide Debug", KeyCode.F3);
#endif
            container.RegisterKeyBinding(new SimpleEditorCommand<DiagramViewModel>((p) =>
            {
                var saveCommand = InvertApplication.Container.Resolve<IToolbarCommand>("Save");
                InvertGraphEditor.ExecuteCommand(saveCommand);
            }), "Save & Compile", KeyCode.S, true, true);

          
        }

        public override void Loaded(UFrameContainer container)
        {
            InvertGraphEditor.DesignerPluginLoaded();
        }

        public IEnumerable<QuickAddItem> PrefabNodes(INodeRepository nodeRepository)
        {
            return nodeRepository.GetImportableItems(nodeRepository.CurrentFilter).OfType<DiagramNode>().Select(p=>new QuickAddItem("Show Item",p.Name,
                _ =>
                {
                    nodeRepository.SetItemLocation(p, _.MousePosition);
                }));
        }

        public void CommandExecuting(ICommandHandler handler, IEditorCommand command, object o)
        {
            
        }

        public void CommandExecuted(ICommandHandler handler, IEditorCommand command, object o)
        {
#if UNITY_DLL
            var item = o as IDiagramNodeItem;
            if (item != null)
            {
                var projectService = InvertApplication.Container.Resolve<ProjectService>();
                foreach (var graph in projectService.CurrentProject.Graphs)
                {
                    if (graph.Identifier == item.Node.Graph.Identifier)
                    {
                        UnityEditor.EditorUtility.SetDirty(graph as UnityEngine.Object);
                    }
                }
            }
#endif
        }

        public void ConnectionApplying(IGraphData graph, IConnectable output, IConnectable input)
        {
            
        }

        public void ConnectionApplied(IGraphData g, IConnectable output, IConnectable input)
        {
            #if UNITY_DLL
            var projectService = InvertApplication.Container.Resolve<ProjectService>();
            foreach (var graph in projectService.CurrentProject.Graphs)
            {
                if (graph.Identifier == g.Identifier)
                {
                    UnityEditor.EditorUtility.SetDirty(graph as UnityEngine.Object);
                }
            }
            #endif
        }
    }
}
