using System.IO;
using System.Runtime.InteropServices;
using Invert.uFrame.Code.Bindings;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

using Invert.uFrame.Editor.Nodes;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Invert.uFrame.Editor
{

    public static class uFrameEditor
    {

        public static ProjectRepository[] Projects
        {
            get { return _projects ?? (_projects = GetAssets(typeof(ProjectRepository)).Cast<ProjectRepository>().ToArray()); }
            set { _projects = value; }
        }

        public static INamespaceProvider NamespaceProvider
        {
            get
            {
                return Container.Resolve<INamespaceProvider>();
            }
        }
        private static IEditorCommand[] _commands;

        private static uFrameContainer _container;

        private static IEnumerable<CodeGenerator> _generators;

        private static IDiagramPlugin[] _plugins;

        private static IToolbarCommand[] _toolbarCommands;
        private static IProjectRepository _repository;

        private static Dictionary<Type, List<Type>> _allowedFilterNodes;
        private static Dictionary<Type, List<Type>> _allowedFilterItems;
        private static ProjectRepository[] _projects;
        private static UFrameSettings _settings;
        private static IUFrameTypeProvider _uFrameTypes;
        private static IConnectionStrategy[] _connectionStrategies;
        private static Type[] _codeGenerators;
        private static uFrameContainer _TypesContainer;

        public static IEditorCommand[] Commands
        {
            get
            {
                return _commands ?? (_commands = Container.ResolveAll<IEditorCommand>().ToArray());
            }
        }

        //private static UFrameApplicationViewModel _application;
        public static uFrameContainer Container
        {
            get
            {
                if (_container != null) return _container;
                _container = new uFrameContainer();
                InitializeContainer(_container);
                return _container;
            }
            set { _container = value; }
        }
        public static uFrameContainer TypesContainer
        {
            get
            {
                if (_TypesContainer != null) return _TypesContainer;
                _TypesContainer = new uFrameContainer();
                InitializeTypesContainer(_TypesContainer);
                return _TypesContainer;
            }
            set { _TypesContainer = value; }
        }

        private static void InitializeTypesContainer(uFrameContainer container)
        {
            
        }

        public static DiagramViewModel CurrentDiagramViewModel
        {
            get { return CurrentDiagram.DiagramViewModel; }
        }

        public static ElementsDiagram CurrentDiagram
        {
            get
            {
                return DesignerWindow.DiagramDrawer;
            }
        }

        public static MouseEvent CurrentMouseEvent
        {
            get { return DesignerWindow.MouseEvent; }
        }

        public static ElementsDesigner DesignerWindow { get; set; }

        public static IKeyBinding[] KeyBindings { get; set; }

        public static IDiagramPlugin[] Plugins
        {
            get
            {
                return _plugins ?? (_plugins = Container.ResolveAll<IDiagramPlugin>().ToArray());
            }
            set { _plugins = value; }
        }

        public static bool ShowHelp
        {
            get
            {
                return EditorPrefs.GetBool("UFRAME_ShowHelp", true);
            }
            set
            {
                EditorPrefs.SetBool("UFRAME_ShowHelp", value);
            }
        }

        public static bool ShowInfoLabels
        {
            get
            {
                return EditorPrefs.GetBool("UFRAME_ShowInfoLabels", true);
            }
            set
            {
                EditorPrefs.SetBool("UFRAME_ShowInfoLabels", value);
            }
        }

        public static IUFrameTypeProvider UFrameTypes
        {
            get { return _uFrameTypes ?? (_uFrameTypes = Container.Resolve<IUFrameTypeProvider>()); }
            set { _uFrameTypes = value; }
        }

        private static IBindingGenerator[] BindingGenerators { get; set; }

        public static IEnumerable<IEditorCommand> CreateCommandsFor<T>()
        {
            var commands = Container.ResolveAll<T>();

            return Commands.Where(p => typeof(T).IsAssignableFrom(p.For));
        }

        public static TCommandUI CreateCommandUI<TCommandUI>(ICommandHandler handler, params Type[] contextTypes) where TCommandUI : class,ICommandUI
        {
            var ui = Container.Resolve<TCommandUI>() as ICommandUI;
            ui.Handler = handler;
            foreach (var contextType in contextTypes)
            {
                var commands = Container.ResolveAll(contextType).Cast<IEditorCommand>().ToArray();

                foreach (var command in commands)
                {
                    ui.AddCommand(command);
                }
            }
            return (TCommandUI)ui;
        }

        public static IDrawer CreateDrawer(ViewModel viewModel)
        {
            if (viewModel == null)
            {
                Debug.LogError("Data is null.");
                return null;
            }
            var drawer = Container.ResolveRelation<IDrawer>(viewModel.GetType(), viewModel);
            if (drawer == null)
            {
                Debug.Log(String.Format("Couldn't Create drawer for {0}.", viewModel.GetType()));
            }
            return drawer;
        }
        public static void ExecuteCommand(IEditorCommand action)
        {
            DesignerWindow.ExecuteCommand(action);
        }
        public static void ExecuteCommand(Action<DiagramViewModel> action)
        {
            DesignerWindow.ExecuteCommand(new SimpleEditorCommand<DiagramViewModel>(action));
        }
        public static void ExecuteCommand(this ICommandHandler handler, IEditorCommand command)
        {
            var objs = handler.ContextObjects.ToArray();

            CurrentProject.RecordUndo(CurrentProject.CurrentGraph, command.Title);
            foreach (var o in objs)
            {
                if (o == null) continue;


                if (command.For.IsAssignableFrom(o.GetType()))
                {
                    if (command.CanPerform(o) != null) continue;
                    //handler.CommandExecuting(command);
                    command.Execute(o);
                    if (command.Hooks != null)
                        command.Hooks.ForEach(p =>
                        {
                            Debug.Log(p.Name);
                            ExecuteCommand(handler, p);
                        });
                    handler.CommandExecuted(command);
                }
            }
            CurrentProject.MarkDirty(CurrentProject.CurrentGraph);
        }

        public static Type FindType(string name)
        {
            //if (string.IsNullOrEmpty(name)) return null;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = assembly.GetType(name);
                if (t != null)
                {
                    return t;
                }
            }
            return null;
        }
        public static Type FindTypeByName(string name)
        {
            //if (string.IsNullOrEmpty(name)) return null;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var item in assembly.GetTypes())
                {
                    if (item.Name == name)
                        return item;
                }
               
            }
            return null;
        }
        public static IEnumerable<CodeGenerator> GetAllCodeGenerators(GeneratorSettings settings, IProjectRepository project)
        {
            // Grab all the code generators
            var diagramItemGenerators = Container.ResolveAll<DesignerGeneratorFactory>().ToArray();

            foreach (var diagramItemGenerator in diagramItemGenerators)
            {
                DesignerGeneratorFactory generator = diagramItemGenerator;
                // If its a generator for the entire diagram
               
                if (typeof(GraphData).IsAssignableFrom(generator.DiagramItemType) && project != null)
                {
                    var diagrams = project.Diagrams;
                    foreach (var diagram in diagrams)
                    {
                        if (generator.DiagramItemType.IsAssignableFrom(diagram.GetType()))
                        {
                            var codeGenerators = generator.GetGenerators(settings, diagram.CodePathStrategy, project, diagram);
                            foreach (var codeGenerator in codeGenerators)
                            {
                                if (!codeGenerator.IsEnabled(project)) continue;
                                
                                codeGenerator.AssetPath = diagram.CodePathStrategy.AssetPath;
                                codeGenerator.Settings = settings;
                                codeGenerator.ObjectData = project;
                                codeGenerator.GeneratorFor = diagramItemGenerator.DiagramItemType;
                                yield return codeGenerator;
                            }
                        }
                    }
                }
                else if (typeof(INodeRepository).IsAssignableFrom(generator.DiagramItemType))
                {
                    foreach (var diagram in project.Diagrams)
                    {
                        var codeGenerators = generator.GetGenerators(settings, diagram.CodePathStrategy, project, diagram);
                        foreach (var codeGenerator in codeGenerators)
                        {
                            if (!codeGenerator.IsEnabled(project)) continue;
                            codeGenerator.AssetPath = diagram.CodePathStrategy.AssetPath;
                            codeGenerator.Settings = settings;
                            codeGenerator.ObjectData = project;
                            codeGenerator.GeneratorFor = diagramItemGenerator.DiagramItemType;
                            yield return codeGenerator;
                        }

                    }


                }
                // If its a generator for a specific node type
                else
                {
                    foreach (var diagram in project.Diagrams)
                    {
                        var items = diagram.NodeItems.Where(p => p.GetType() == generator.DiagramItemType);

                        foreach (var item in items)
                        {
                            var codeGenerators = generator.GetGenerators(settings, diagram.CodePathStrategy, project, item);
                            foreach (var codeGenerator in codeGenerators)
                            {
                                if (!codeGenerator.IsEnabled(project)) continue;
                                codeGenerator.AssetPath = diagram.CodePathStrategy.AssetPath;
                                codeGenerator.Settings = settings;
                                codeGenerator.ObjectData = item;
                                codeGenerator.GeneratorFor = diagramItemGenerator.DiagramItemType;
                                yield return codeGenerator;
                            }
                        }
                    }

                }
            }
        }

        public static IEnumerable<CodeFileGenerator> GetAllFileGenerators(GeneratorSettings settings)
        {
            return GetAllFileGenerators(settings, CurrentProject);
        }

        public static IEnumerable<CodeFileGenerator> GetAllFileGenerators(GeneratorSettings settings, IProjectRepository project)
        {
            var codeGenerators = GetAllCodeGenerators(settings, project).ToArray();
            var groups = codeGenerators.GroupBy(p => p.FullPathName);
            foreach (var @group in groups)
            {
                var generator = new CodeFileGenerator(settings.NamespaceProvider.RootNamespace)
                {
                    AssetPath = @group.Key.Replace("\\", "/"),
                    SystemPath = Path.Combine(Application.dataPath, @group.Key.Substring(7)).Replace("\\", "/"),
                    Generators = @group.ToArray()
                };
                yield return generator;
            }
        }

        public static IEnumerable<IBindingGenerator> GetBindingGeneratorsForView(ViewData view)
        {
            foreach (var binding in view.Bindings.ToArray())
            {
                var generator = Container.Resolve<IBindingGenerator>(binding.GeneratorType);
                if (generator == null)
                {
                    Debug.LogError("Binding Generator Not Found: " + binding.GeneratorType);
                    continue;
                }
                if (binding.Property == null)
                {
                    binding.PropertyIdentifier = null;
                    view.Bindings.Remove(binding);
                    Debug.Log("Couldnt find property for " + binding.Name);
                    continue;
                }
                generator.Element = view.ViewForElement;

                generator.Item = binding.Property;

                generator.GenerateDefaultImplementation = false;
                yield return generator;
            }

        }

        public static IEnumerable<IBindingGenerator> GetPossibleBindingGenerators(ViewData view, bool isOverride = true, bool generateDefaultBindings = true, bool includeBaseItems = true, bool callBase = true)
        {
            var mainElement = view.ViewForElement;
            
            if (view.BaseView != null)
            {
                foreach (var viewModelItem in mainElement.ViewModelItems)
                {
                    var bindingGenerators = Container.ResolveAll<IBindingGenerator>();
                    foreach (var bindingGenerator in bindingGenerators)
                    {
                        bindingGenerator.IsBase = mainElement != view.ViewForElement;
                        bindingGenerator.IsOverride = isOverride;
                        bindingGenerator.Item = viewModelItem;
                        bindingGenerator.Element = mainElement;
                        bindingGenerator.GenerateDefaultImplementation = generateDefaultBindings;
                        
                        if (bindingGenerator.IsApplicable)
                            yield return bindingGenerator;
                    }
                }
            }
            else
            {
                foreach (var element in mainElement.AllBaseTypes.Concat(new[] {mainElement}))
                {
                    foreach (var viewModelItem in element.ViewModelItems)
                    {
                        var bindingGenerators = Container.ResolveAll<IBindingGenerator>();
                        foreach (var bindingGenerator in bindingGenerators)
                        {
                            bindingGenerator.IsBase = mainElement != view.ViewForElement;
                            bindingGenerator.IsOverride = isOverride;
                            bindingGenerator.Item = viewModelItem;
                            bindingGenerator.Element = mainElement;
                            bindingGenerator.GenerateDefaultImplementation = generateDefaultBindings;

                            if (bindingGenerator.IsApplicable)
                                yield return bindingGenerator;
                        }
                    }
                }
               
            }

          


        }

        public static IEnumerable<IEditorCommand> GetContextCommandsFor<T>()
        {
            return Commands.Where(p => p is IContextMenuItemCommand && typeof(T).IsAssignableFrom(p.For));
        }

        public static IEnumerable<Type> GetDerivedTypes<T>(bool includeAbstract = false, bool includeBase = true)
        {
            var type = typeof(T);
            if (includeBase)
                yield return type;
            if (includeAbstract)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var t in assembly
                        .GetTypes()
                        .Where(x => x.IsSubclassOf(type)))
                    {
                        yield return t;
                    }
                }
            }
            else
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var t in assembly
                        .GetTypes()
                        .Where(x => x.IsSubclassOf(type) && !x.IsAbstract))
                    {
                        yield return t;
                    }
                }
            }
        }

        public static void HookCommand<TFor>(string name, IEditorCommand hook) where TFor : class, IEditorCommand
        {
            var command = Container.Resolve<TFor>(name);
            command.Hooks.Add(hook);
        }

        public static void RegisterKeyBinding(IEditorCommand command, string name, KeyCode code, bool control = false, bool alt = false, bool shift = false)
        {
            Container.RegisterInstance<IKeyBinding>(new SimpleKeyBinding(command, name, code, control, alt, shift), name);
        }

        public static Object[] GetAssets(Type assetType)
        {
            var tempObjects = new List<Object>();
            var directory = new DirectoryInfo(Application.dataPath);
            FileInfo[] goFileInfo = directory.GetFiles("*" + ".asset", SearchOption.AllDirectories);

            int i = 0; int goFileInfoLength = goFileInfo.Length;
            for (; i < goFileInfoLength; i++)
            {
                FileInfo tempGoFileInfo = goFileInfo[i];
                if (tempGoFileInfo == null)
                    continue;

                string tempFilePath = tempGoFileInfo.FullName;
                tempFilePath = tempFilePath.Replace(@"\", "/").Replace(Application.dataPath, "Assets");
                try
                {

                    var tempGo = AssetDatabase.LoadAssetAtPath(tempFilePath, assetType) as Object;
                    if (tempGo == null)
                    {

                    }
                    else
                    {
                        tempObjects.Add(tempGo);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    continue;
                }

            }

            return tempObjects.ToArray();
        }

        public static TGraphType CreateGraph<TGraphType>() where TGraphType : GraphData
        {
            return UFrameAssetManager.CreateAsset<TGraphType>();
        }

        private static void InitializeContainer(uFrameContainer container)
        {
#if DEBUG
            // Obsolete
            container.RegisterInstance<IDiagramNodeItemCommand>(new MarkIsYieldCommand(), "MarkIsYield");

            container.RegisterInstance<IToolbarCommand>(new PrintPlugins(), "Print Plugins");
            container.RegisterInstance<IToolbarCommand>(new ForceUpgradeDiagram(), "Force Upgrade");
            container.RegisterInstance<IToolbarCommand>(new DebugCommand("Identifier", d =>
            {
                Debug.Log(d.SelectedNode.GraphItemObject.Identifier);
            }), "Identifer");

            container.RegisterInstance<IToolbarCommand>(new DebugCommand("Print Objects", d =>
            {
                foreach (var item in d.DiagramData.NodeItems)
                {
                    Debug.Log(item.Name + ": " + item.Identifier);
                }
            }), "Print Objects");
            container.RegisterInstance<IToolbarCommand>(new DebugCommand("Print Generators", d =>
            {
                var view = d.SelectedNode.GraphItemObject as ViewData;

                var generators = GetBindingGeneratorsForView(view);
                foreach (var generator in generators)
                {
                    Debug.Log(generator.GetType().Name);
                }
            }), "Print Objects");
#endif

            container.Register<NodeItemHeader, NodeItemHeader>();

            // Register the container itself
            container.RegisterInstance<IUFrameContainer>(container);
            container.RegisterInstance<uFrameContainer>(container);

            // Register the diagram type
            container.Register<ElementsDiagram, ElementsDiagram>();

            // Command Drawers
            container.Register<ToolbarUI, ToolbarUI>();
            container.Register<ContextMenuUI, ContextMenuUI>();

            // Toolbar commands
            container.RegisterInstance<IToolbarCommand>(new PopToFilterCommand(), "PopToFilterCommand");
            container.RegisterInstance<IToolbarCommand>(new SaveCommand(), "SaveCommand");
            
            container.RegisterInstance<IToolbarCommand>(new AddNewCommand(), "AddNewCommand");
            container.RegisterInstance<IToolbarCommand>(new DiagramSettingsCommand() { Title = "Settings" }, "SettingsCommand");

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
            container.RegisterInstance<IDiagramNodeCommand>(new SelectViewBaseElement(), "SelectView");
            container.RegisterInstance<IDiagramNodeCommand>(new MarkIsTemplateCommand(), "MarkAsTemplate");

            // All Node Items
            container.RegisterInstance<IDiagramNodeItemCommand>(new DeleteItemCommand(), "Delete");
            container.RegisterInstance<IDiagramNodeItemCommand>(new MoveUpCommand(), "MoveItemUp");
            container.RegisterInstance<IDiagramNodeItemCommand>(new MoveDownCommand(), "MoveItemDown");
            container.RegisterInstance<IEditorCommand>(new RemoveNodeItemCommand(), "RemoveNodeItem");

            // Drawers
            RegisterDrawer<ConnectorViewModel, ConnectorDrawer>();
            RegisterDrawer<ConnectionViewModel, ConnectionDrawer>();
            RegisterDrawer<ConnectorHeaderViewModel, InputHeaderDrawer>();

            // Graph Diagrams
            container.Register<GraphData, ElementsGraph>("Graph");
            container.Register<GraphData, ExternalSubsystemGraph>("External Subsystem Graph");
            container.Register<GraphData, ExternalElementGraph>("External Element Graph");
            container.Register<GraphData, ExternalStateMachineGraph>("External State Machine Graph");

            // Scene Managers
            RegisterGraphItem<SceneManagerData, SceneManagerViewModel, SceneManagerDrawer>();
            RegisterGraphItem<SceneManagerTransition, SceneTransitionItemViewModel, ItemDrawer>();
            RegisterFilterNode<SceneFlowFilter, SceneManagerData>();
            container.RegisterInstance<IConnectionStrategy>(new SceneTransitionConnectionStrategy(), "SceneTransitionConnectionStrategy");
            container.RegisterInstance<IConnectionStrategy>(new SceneManagerSubsystemConnectionStrategy(), "SceneManagerSubsystemConnectionStrategy");
            container.RegisterInstance(new AddTransitionCommand());

            // Sub Systems
            RegisterGraphItem<SubSystemData, SubSystemViewModel, SubSystemDrawer>();
            RegisterGraphItem<RegisteredInstanceData, RegisterInstanceItemViewModel, ElementItemDrawer>();
            RegisterFilterNode<SceneFlowFilter, SubSystemData>();
            container.RegisterInstance<IConnectionStrategy>(new SubsystemConnectionStrategy(), "SubsystemConnectionStrategy");
            container.RegisterInstance(new AddInstanceCommand());

            // Elements
            RegisterGraphItem<ElementData, ElementNodeViewModel, ElementDrawer>();
            RegisterGraphItem<ViewModelPropertyData, ElementPropertyItemViewModel, ElementItemDrawer>();
            RegisterGraphItem<ViewModelCommandData, ElementCommandItemViewModel, ElementItemDrawer>();
            RegisterGraphItem<ViewModelCollectionData, ElementCollectionItemViewModel, ElementItemDrawer>();
            RegisterFilterNode<SubSystemData, ElementData>();
            container.RegisterInstance(new AddElementCommandCommand());
            container.RegisterInstance(new AddElementCollectionCommand());
            container.RegisterInstance(new AddElementPropertyCommand());
            container.RegisterInstance<IConnectionStrategy>(new ElementInheritanceConnectionStrategy(), "ElementInheritanceConnectionStrategy");
            container.RegisterInstance<IConnectionStrategy>(new ElementViewConnectionStrategy(), "ElementViewConnectionStrategy");

            // Computed Properties
            RegisterGraphItem<ComputedPropertyData, ComputedPropertyNodeViewModel, ComputedPropertyDrawer>();
            RegisterFilterNode<ElementData, ComputedPropertyData>();
            container.RegisterInstance<IConnectionStrategy>(new ComputedPropertyInputsConnectionStrategy(), "ComputedPropertyInputsConnectionStrategy");

            // Views
            RegisterGraphItem<ViewData, ViewNodeViewModel, ViewDrawer>();
            RegisterGraphItem<ViewPropertyData, ElementViewPropertyItemViewModel, ItemDrawer>();
            RegisterGraphItem<ViewBindingData, ViewBindingItemViewModel, ItemDrawer>();
            RegisterFilterNode<ElementData, ViewData>();
            container.RegisterInstance(new AddBindingCommand());
            container.RegisterInstance<IConnectionStrategy>(new TwoWayPropertyConnectionStrategy(), "TwoWayPropertyConnectionStrategy");
            container.RegisterInstance<IConnectionStrategy>(new ViewInheritanceConnectionStrategy(), "ViewInheritanceConnectionStrategy");

            // View Components
            RegisterGraphItem<ViewComponentData, ViewComponentNodeViewModel, ViewComponentDrawer>();
            RegisterFilterNode<ElementData, ViewComponentData>();
            container.RegisterInstance<IConnectionStrategy>(new ViewComponentElementConnectionStrategy(), "ViewComponentElementConnectionStrategy");
            container.RegisterInstance<IConnectionStrategy>(new ViewComponentInheritanceConnectionStrategy(), "ViewComponentInheritanceConnectionStrategy");

            // Enums
            RegisterGraphItem<EnumData, EnumNodeViewModel, DiagramEnumDrawer>();
            RegisterGraphItem<EnumItem, EnumItemViewModel, EnumItemDrawer>();
            RegisterFilterNode<SubSystemData, EnumData>();
            RegisterFilterNode<ElementData, EnumData>();
            container.RegisterInstance(new AddEnumItemCommand());

            // State Machines
            RegisterGraphItem<StateMachineNodeData, StateMachineNodeViewModel, StateMachineNodeDrawer>();
            RegisterGraphItem<StateMachineStateData, StateMachineStateNodeViewModel, StateMachineStateNodeDrawer>();
            RegisterGraphItem<StateMachineTransition, StateMachineTransitionViewModel, ItemDrawer>();
            RegisterGraphItem<StateTransitionData, StateTransitionViewModel, ItemDrawer>();
            RegisterGraphItem<StateMachineActionData, StateActionNodeViewModel, StateActionNodeDrawer>();
            RegisterFilterNode<ElementData, StateMachineNodeData>();
            RegisterFilterNode<StateMachineNodeData, StateMachineStateData>();

            container.RegisterInstance<IConnectionStrategy>(new StartStateConnectionStrategy(), "StartStateConnectionStrategy");
            container.RegisterInstance<IConnectionStrategy>(new StateMachineTransitionConnectionStrategy(), "StateMachineTransitionConnectionStrategy");
            container.RegisterInstance<IConnectionStrategy>(new ComputedTransitionConnectionStrategy(), "ComputedTransitionConnectionStrategy");
            //container.RegisterInstance<IConnectionStrategy>(new ElementStateMachineConnectionStrategy(), "ElementStateMachineConnectionStrategy");
            //container.RegisterInstance<IConnectionStrategy>(new ElementStateVariableConnectionStrategy(), "ElementStateVariableConnectionStrategy");

            // Simple Classes
            RegisterGraphItem<ClassPropertyData, ClassPropertyItemViewModel, ElementItemDrawer>();
            RegisterGraphItem<ClassCollectionData, ClassCollectionItemViewModel, ElementItemDrawer>();
            RegisterGraphItem<ClassNodeData, ClassNodeViewModel, ClassNodeDrawer>();
            RegisterFilterNode<ElementData, ClassNodeData>();
            RegisterFilterNode<SubSystemData, ClassNodeData>();
            container.RegisterInstance<IConnectionStrategy>(new ClassNodeInheritanceConnectionStrategy(), "ClassNodeInheritanceConnectionStrategy");

#if DEBUG
            // Model Classes ^^
            RegisterGraphItem<ModelClassNodeData, ModelClassNodeViewModel, ModelClassNodeDrawer>();
            RegisterFilterNode<ElementData, ModelClassNodeData>();
            RegisterFilterNode<SubSystemData, ModelClassNodeData>();
#endif
            // General Connections
            container.RegisterInstance<IConnectionStrategy>(new AssociationConnectionStrategy(), "AssociationConnectionStrategy");
            //container.RegisterInstance<IConnectionStrategy>(new ComputedPropertyInputStrategy(), "ComputedPropertyInputStrategy");

            container.RegisterInstance<IUFrameTypeProvider>(new uFrameStringTypeProvider());
            container.RegisterInstance<UFrameSettings>(new UFrameSettings());

            // Where the generated code files are placed
            container.Register<ICodePathStrategy, DefaultCodePathStrategy>("Default");
            container.Register<ICodePathStrategy, SubSystemPathStrategy>("By Subsystem");

            // Load all plugins
            foreach (var diagramPlugin in GetDerivedTypes<DiagramPlugin>(false, false))
            {
                container.RegisterInstance(Activator.CreateInstance((Type)diagramPlugin) as IDiagramPlugin, diagramPlugin.Name, false);
            }

            container.InjectAll();

            foreach (var diagramPlugin in Plugins.OrderBy(p => p.LoadPriority))
            {
                if (diagramPlugin.Enabled)
                    diagramPlugin.Initialize(Container);
            }

            var commandKeyBindings = new List<IKeyBinding>();
            foreach (var item in Container.Instances)
            {
                if (typeof(IEditorCommand).IsAssignableFrom(item.Base))
                {
                    if (item.Instance != null)
                    {
                        var command = item.Instance as IEditorCommand;
                        if (command != null)
                        {
                            var keyBinding = command.GetKeyBinding();
                            if (keyBinding != null)
                                commandKeyBindings.Add(keyBinding);
                        }
                    }
                }
            }
            ConnectionStrategies = Container.ResolveAll<IConnectionStrategy>().ToArray();
            KeyBindings = Container.ResolveAll<IKeyBinding>().Concat(commandKeyBindings).ToArray();
            BindingGenerators = Container.ResolveAll<IBindingGenerator>().ToArray();
            Settings = container.Resolve<UFrameSettings>();


            var filterTypes = Container.RelationshipMappings.Where(
                p => typeof(IDiagramFilter).IsAssignableFrom(p.From) && p.To == typeof(IDiagramNode));
            var filterTypeItems = Container.RelationshipMappings.Where(
                p => typeof(IDiagramFilter).IsAssignableFrom(p.From) && p.To == typeof(IDiagramNodeItem));

            foreach (var filterMapping in filterTypes)
            {
                if (!AllowedFilterNodes.ContainsKey(filterMapping.From))
                {
                    AllowedFilterNodes.Add(filterMapping.From, new List<Type>());
                }
                AllowedFilterNodes[filterMapping.From].Add(filterMapping.Concrete);
            }

            foreach (var filterMapping in filterTypeItems)
            {
                if (!AllowedFilterItems.ContainsKey(filterMapping.From))
                {
                    AllowedFilterItems.Add(filterMapping.From, new List<Type>());
                }
                AllowedFilterItems[filterMapping.From].Add(filterMapping.Concrete);
            }
        }

        public static Type[] CodeGenerators
        {
            get { return _codeGenerators ?? (_codeGenerators = GetDerivedTypes<CodeGenerator>().ToArray()); }
            set { _codeGenerators = value; }
        }


        public static UFrameSettings Settings
        {
            get { return _settings ?? (Container.Resolve<UFrameSettings>()); }
            set { _settings = value; }
        }

        public static IConnectionStrategy[] ConnectionStrategies
        {
            get { return _connectionStrategies ?? (_connectionStrategies = Container.ResolveAll<IConnectionStrategy>().ToArray()); }
            set { _connectionStrategies = value; }
        }

        public static void RegisterDrawer<TViewModel, TDrawer>()
        {
            Container.RegisterRelation<TViewModel, IDrawer, TDrawer>();
        }
        public static void RegisterGraphItem<TModel, TViewModel, TDrawer>()
        {
            Container.RegisterRelation<TModel, ViewModel, TViewModel>();
            RegisterDrawer<TViewModel, TDrawer>();
        }

        public static void RegisterFilterNode<TFilterData, TAllowedItem>()
        {
            if (!AllowedFilterNodes.ContainsKey(typeof(TFilterData)))
            {
                AllowedFilterNodes.Add(typeof(TFilterData), new List<Type>());
            }
            AllowedFilterNodes[typeof(TFilterData)].Add(typeof(TAllowedItem));
        }

        public static void RegisterFilterItem<TFilterData, TAllowedItem>()
        {
            Container.RegisterRelation<TFilterData, IDiagramNodeItem, TAllowedItem>();
        }

        public static IEnumerable<Type> GetAllowedFilterNodes(Type filterType)
        {
            return Container.RelationshipMappings.Where(
                p => p.From == filterType && p.To == typeof(IDiagramNode)).Select(p => p.Concrete);
        }
        public static IEnumerable<Type> GetAllowedFilterItems(Type filterType)
        {
            return Container.RelationshipMappings.Where(
                p => p.From == filterType && p.To == typeof(IDiagramNodeItem)).Select(p => p.Concrete);
        }

        public static Dictionary<Type, List<Type>> AllowedFilterNodes
        {
            get { return _allowedFilterNodes ?? (_allowedFilterNodes = new Dictionary<Type, List<Type>>()); }
            set { _allowedFilterNodes = value; }
        }

        public static Dictionary<Type, List<Type>> AllowedFilterItems
        {
            get { return _allowedFilterItems ?? (_allowedFilterItems = new Dictionary<Type, List<Type>>()); }
            set { _allowedFilterItems = value; }
        }

        public static IProjectRepository CurrentProject { get; set; }

        public static bool IsFilter(Type type)
        {
            return AllowedFilterNodes.ContainsKey(type);
        }

        public static void Log(string s)
        {
#if DEBUG
            File.AppendAllText("uframe-log.txt", s + "\r\n\r\n");
            Debug.Log(s);
#endif
        }
    }

}