using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.Core.GraphDesigner.Settings;
using Invert.Core.GraphDesigner.UnitySpecific;
using Invert.uFrame;
using Invert.uFrame.CodeGen.ClassNodeGenerators;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class BaseClassReference : GenericSlot { }
    public class GraphDesignerPlugin : DiagramPlugin
    {
        public override decimal LoadPriority
        {
            get { return -100; }
        }
        public override bool Required
        {
            get { return true; }
        }

        public override void Initialize(uFrameContainer container)
        {
            var typeContainer = InvertGraphEditor.TypesContainer;
            
            typeContainer.AddItem<GenericSlot,InputOutputViewModel,SlotDrawer>();
            typeContainer.AddItem<BaseClassReference, InputOutputViewModel, SlotDrawer>();

            container.RegisterInstance<IConnectionStrategy>(new InputOutputStrategy(),"InputOutputStrategy");
            container.RegisterDrawer<SectionHeaderViewModel,SectionHeaderDrawer>();


            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(int), Group = "", Label = "int", IsPrimitive = true }, "int");
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(string), Group = "", Label = "string", IsPrimitive = true }, "string");
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(decimal), Group = "", Label = "decimal", IsPrimitive = true }, "decimal");
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(float), Group = "", Label = "float", IsPrimitive = true }, "float");
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(bool), Group = "", Label = "bool", IsPrimitive = true }, "bool");
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(char), Group = "", Label = "char", IsPrimitive = true }, "char");
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(DateTime), Group = "", Label = "date", IsPrimitive = true }, "date");
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(Vector2), Group = "", Label = "Vector2", IsPrimitive = true }, "Vector2");
            typeContainer.RegisterInstance(new GraphTypeInfo() { Type = typeof(Vector3), Group = "", Label = "Vector3", IsPrimitive = true }, "Vector3");

            
            container.RegisterInstance<IAssetManager>(new UnityAssetManager());
            container.Register<DesignerGeneratorFactory, SimpleClassNodeCodeFactory>("ClassNodeData");
            
            // Register the container itself
            container.RegisterInstance<IUFrameContainer>(container);
            container.RegisterInstance<uFrameContainer>(container);
            
            container.Register<SectionHeaderDrawer, SectionHeaderDrawer>();
            container.RegisterItemDrawer<GenericItemHeaderViewModel, GenericChildItemHeaderDrawer>();

            // Toolbar commands
            container.RegisterInstance<IToolbarCommand>(new PopToFilterCommand(), "PopToFilterCommand");
            container.RegisterInstance<IToolbarCommand>(new SaveCommand(), "SaveCommand");

            container.RegisterInstance<IToolbarCommand>(new AddNewCommand(), "AddNewCommand");
            

            // For no selection diagram context menu
            container.RegisterInstance<IDiagramContextCommand>(new AddItemCommand2(), "AddItemCommand");
            container.RegisterInstance<IDiagramContextCommand>(new ShowItemCommand(), "ShowItem");

            container.RegisterInstance<IDiagramNodeCommand>(new PushToCommand(), "Push To Command");
            container.RegisterInstance<IDiagramNodeCommand>(new PullFromCommand(), "Pull From Command");
            //container.RegisterInstance<IDiagramNodeCommand>(new ExportCommand(), "Export");

            // All Nodes
            container.RegisterInstance<IDiagramNodeCommand>(new OpenCommand(), "OpenCode");
            container.RegisterInstance<IDiagramNodeCommand>(new DeleteCommand(), "Delete");
            container.RegisterInstance<IDiagramNodeCommand>(new RenameCommand(), "Reanme");
            container.RegisterInstance<IDiagramNodeCommand>(new HideCommand(), "Hide");
            container.RegisterInstance<IDiagramNodeCommand>(new RemoveLinkCommand(), "RemoveLink");

            // All Node Items
            container.RegisterInstance<IDiagramNodeItemCommand>(new DeleteItemCommand(), "Delete");
            container.RegisterInstance<IDiagramNodeItemCommand>(new MoveUpCommand(), "MoveItemUp");
            container.RegisterInstance<IDiagramNodeItemCommand>(new MoveDownCommand(), "MoveItemDown");
            container.RegisterInstance<IEditorCommand>(new RemoveNodeItemCommand(), "RemoveNodeItem");

            // Drawers
            container.RegisterDrawer<ConnectorViewModel, ConnectorDrawer>();
            container.RegisterDrawer<ConnectionViewModel, ConnectionDrawer>();
            container.RegisterDrawer<InputOutputViewModel, SlotDrawer>();


            // Enums
            container.RegisterGraphItem<EnumData, EnumNodeViewModel, DiagramEnumDrawer>();
            container.RegisterChildGraphItem<EnumItem, EnumItemViewModel, EnumItemDrawer>();
            container.RegisterInstance(new AddEnumItemCommand());

            // Class Nodes
            container.RegisterGraphItem<ClassPropertyData, ClassPropertyItemViewModel, ElementItemDrawer>();
            container.RegisterGraphItem<ClassCollectionData, ClassCollectionItemViewModel, ElementItemDrawer>();
            container.RegisterGraphItem<ClassNodeData, ClassNodeViewModel, ClassNodeDrawer>();
            container.RegisterInstance<IConnectionStrategy>(new ClassNodeInheritanceConnectionStrategy(), "ClassNodeInheritanceConnectionStrategy");

            InvertGraphEditor.RegisterKeyBinding(new RenameCommand(), "Rename", KeyCode.F2);
            InvertGraphEditor.RegisterKeyBinding(new SimpleEditorCommand<DiagramViewModel>((p) =>
            {
                p.DeselectAll();
            }), "End All Editing", KeyCode.Return);

            InvertGraphEditor.RegisterKeyBinding(new DeleteItemCommand(), "Delete Item", KeyCode.X, true);
            InvertGraphEditor.RegisterKeyBinding(new DeleteCommand(), "Delete", KeyCode.Delete);
            InvertGraphEditor.RegisterKeyBinding(new MoveUpCommand(), "Move Up", KeyCode.UpArrow);
            InvertGraphEditor.RegisterKeyBinding(new MoveDownCommand(), "Move Down", KeyCode.DownArrow);


            InvertGraphEditor.RegisterKeyBinding(new SimpleEditorCommand<DiagramViewModel>((p) =>
            {
                InvertGraphEditor.Settings.ShowHelp = !InvertGraphEditor.Settings.ShowHelp;
            }), "Show/Hide This Help", KeyCode.F1);
#if DEBUG
            InvertGraphEditor.RegisterKeyBinding(new SimpleEditorCommand<DiagramViewModel>((p) =>
            {
                InvertGraphEditor.Settings.ShowGraphDebug = !InvertGraphEditor.Settings.ShowGraphDebug;
            }), "Show/Hide Debug", KeyCode.F3);
#endif
            InvertGraphEditor.RegisterKeyBinding(new SimpleEditorCommand<DiagramViewModel>((p) =>
            {
                var saveCommand = InvertApplication.Container.Resolve<IToolbarCommand>("Save");
                InvertGraphEditor.ExecuteCommand(saveCommand);
            }), "Save & Compile", KeyCode.S, true, true);

          
        }

        public override void Loaded()
        {
            InvertGraphEditor.DesignerPluginLoaded();
        }
    }
}
