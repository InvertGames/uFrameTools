using System.IO;
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
            foreach (var o in objs)
            {
                if (o == null) continue;

                if (command.For.IsAssignableFrom(o.GetType()))
                {
                    if (command.CanPerform(o) != null) continue;
                    handler.CommandExecuting(command);
                    command.Execute(o);
                    if (command.Hooks != null)
                        command.Hooks.ForEach(p => ExecuteCommand(handler, p));
                    handler.CommandExecuted(command);
                }
            }
            
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

        public static IEnumerable<CodeGenerator> GetAllCodeGenerators(GeneratorSettings settings, ICodePathStrategy pathStrategy, INodeRepository diagramData)
        {
            // Grab all the code generators
            var diagramItemGenerators = Container.ResolveAll<DesignerGeneratorFactory>().ToArray();

            foreach (var diagramItemGenerator in diagramItemGenerators)
            {
                DesignerGeneratorFactory generator = diagramItemGenerator;
                // If its a generator for the entire diagram
                var project = diagramData as IProjectRepository;
                if (typeof (GraphData).IsAssignableFrom(generator.DiagramItemType) && project != null)
                {
                    var items = project.Diagrams;
                    foreach (var item in items)
                    {
                        if (generator.DiagramItemType.IsAssignableFrom(item.GetType()))
                        {
                            var codeGenerators = generator.GetGenerators(settings, pathStrategy, diagramData, item);
                            foreach (var codeGenerator in codeGenerators)
                            {
                                codeGenerator.Settings = settings;
                                codeGenerator.ObjectData = diagramData;
                                codeGenerator.GeneratorFor = diagramItemGenerator.DiagramItemType;
                                yield return codeGenerator;
                            }    
                        }
                    }
                }
                else if (typeof(INodeRepository).IsAssignableFrom(generator.DiagramItemType))
                {
                    var codeGenerators = generator.GetGenerators(settings, pathStrategy, diagramData, diagramData);
                    foreach (var codeGenerator in codeGenerators)
                    {
                        codeGenerator.Settings = settings;
                        codeGenerator.ObjectData = diagramData;
                        codeGenerator.GeneratorFor = diagramItemGenerator.DiagramItemType;
                        yield return codeGenerator;
                    }
                } 
                // If its a generator for a specific node type
                else
                {
                    var items = diagramData.NodeItems.Where(p => p.GetType() == generator.DiagramItemType);

                    foreach (var item in items)
                    {
                        var codeGenerators = generator.GetGenerators(settings, pathStrategy, diagramData, item);
                        foreach (var codeGenerator in codeGenerators)
                        {
                            codeGenerator.Settings = settings;
                            codeGenerator.ObjectData = item;
                            codeGenerator.GeneratorFor = diagramItemGenerator.DiagramItemType;
                            yield return codeGenerator;
                        }
                    }
                }
            }
        }

        public static IEnumerable<CodeFileGenerator> GetAllFileGenerators(GeneratorSettings settings,ICodePathStrategy strategy = null)
        {
            return GetAllFileGenerators(settings, CurrentProject, strategy);
        }

        public static IEnumerable<CodeFileGenerator> GetAllFileGenerators(GeneratorSettings settings,INodeRepository diagramData, ICodePathStrategy strategy = null)
        {
            var codeGenerators = GetAllCodeGenerators(settings,strategy ?? diagramData.Settings.CodePathStrategy, diagramData).ToArray();
            var groups = codeGenerators.GroupBy(p => p.Filename);
            foreach (var @group in groups)
            {
                var generator = new CodeFileGenerator(settings.NamespaceProvider.RootNamespace)
                {
                    Filename = @group.Key,
                    Generators = @group.ToArray()
                };
                yield return generator;
            }
        }

        public static IEnumerable<IBindingGenerator> GetBindingGeneratorsFor(ElementData element, bool isOverride = true, bool generateDefaultBindings = true, bool includeBaseItems = true, bool callBase = true)
        {
            IEnumerable<ITypeDiagramItem> items = element.ViewModelItems;
            if (includeBaseItems)
            {
                items = new[] { element }.Concat(element.AllBaseTypes).SelectMany(p => p.ViewModelItems);
            }
            //var vmItems = new[] {element}.Concat(element.AllBaseTypes).SelectMany(p => p.ViewModelItems);
            foreach (var viewModelItem in items)
            {
                var bindingGenerators = Container.ResolveAll<IBindingGenerator>();
                foreach (var bindingGenerator in bindingGenerators)
                {
                    bindingGenerator.IsOverride = isOverride;
                    bindingGenerator.Item = viewModelItem;
                    bindingGenerator.GenerateDefaultImplementation = generateDefaultBindings;
                    if (bindingGenerator.IsApplicable)
                        yield return bindingGenerator;
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
            container.RegisterInstance<IToolbarCommand>(new PrintPlugins(),"Print Plugins");
#endif

            container.Register<NodeItemHeader, NodeItemHeader>();

            container.RegisterInstance<IUFrameContainer>(container);
            container.RegisterInstance<uFrameContainer>(container);

            // Register the diagram type
            container.Register<ElementsDiagram, ElementsDiagram>();

            // Command Drawers
            container.Register<ToolbarUI, ToolbarUI>();
            container.Register<ContextMenuUI, ContextMenuUI>();

            container.RegisterInstance(new AddElementCommandCommand());
            container.RegisterInstance(new AddElementCollectionCommand());
            container.RegisterInstance(new AddElementPropertyCommand());

            container.RegisterInstance(new AddEnumItemCommand());

            container.RegisterInstance(new AddViewPropertyCommand());
            container.RegisterInstance(new AddBindingCommand());
            container.RegisterInstance(new AddTransitionCommand());
            container.RegisterInstance(new AddInstanceCommand());

            container.RegisterInstance<IEditorCommand>(new RemoveNodeItemCommand(), "RemoveNodeItem");

            // Toolbar commands
            container.RegisterInstance<IToolbarCommand>(new PopToFilterCommand(), "PopToFilterCommand");
            container.RegisterInstance<IToolbarCommand>(new SaveCommand(), "SaveCommand");
            container.RegisterInstance<IToolbarCommand>(new AutoLayoutCommand(), "AutoLayoutCommand");
            container.RegisterInstance<IToolbarCommand>(new AddNewCommand(), "AddNewCommand");
            container.RegisterInstance<IToolbarCommand>(new DiagramSettingsCommand() { Title = "Settings" }, "SettingsCommand");

            // For the add new menu
            container.RegisterInstance<AddNewCommand>(new AddNewSceneManagerCommand(), "AddNewSceneManagerCommand");
            container.RegisterInstance<AddNewCommand>(new AddNewSubSystemCommand(), "AddNewSubSystemCommand");
            container.RegisterInstance<AddNewCommand>(new AddNewElementCommand(), "AddNewElementCommand");
            container.RegisterInstance<AddNewCommand>(new AddNewEnumCommand(), "AddNewEnumCommand");
            container.RegisterInstance<AddNewCommand>(new AddNewViewCommand(), "AddNewViewCommand");
            container.RegisterInstance<AddNewCommand>(new AddNewViewComponentCommand(), "AddNewViewComponentCommand");

            // For no selection diagram context menu
            container.RegisterInstance<IDiagramContextCommand>(new AddItemCommand2(), "AddNewFilterItemCommand");
            //container.RegisterInstance<IDiagramContextCommand>(new AddNewSceneManagerCommand(), "AddNewSceneManagerCommand");
            //container.RegisterInstance<IDiagramContextCommand>(new AddNewSubSystemCommand(), "AddNewSubSystemCommand");
            //container.RegisterInstance<IDiagramContextCommand>(new AddNewElementCommand(), "AddNewElementCommand");
            //container.RegisterInstance<IDiagramContextCommand>(new AddNewEnumCommand(), "AddNewEnumCommand");
            //container.RegisterInstance<IDiagramContextCommand>(new AddNewViewCommand(), "AddNewViewCommand");
            //container.RegisterInstance<IDiagramContextCommand>(new AddNewViewComponentCommand(), "AddNewViewComponentCommand");
            container.RegisterInstance<IDiagramContextCommand>(new ShowItemCommand(), "ShowItem");

            // For node context menu
            container.RegisterInstance<IDiagramNodeCommand>(new OpenCommand(), "OpenCode");
            container.RegisterInstance<IDiagramNodeCommand>(new DeleteCommand(), "Delete");
            container.RegisterInstance<IDiagramNodeCommand>(new RenameCommand(), "Reanme");
            container.RegisterInstance<IDiagramNodeCommand>(new HideCommand(), "Hide");
            container.RegisterInstance<IDiagramNodeCommand>(new RemoveLinkCommand(), "RemoveLink");
            container.RegisterInstance<IDiagramNodeCommand>(new SelectViewBaseElement(), "SelectView");
            container.RegisterInstance<IDiagramNodeCommand>(new MarkIsTemplateCommand(), "MarkAsTemplate");
            //container.RegisterInstance<IDiagramNodeCommand>(new MarkIsMultiInstanceCommand(), "MarkAsMulti");
            

            // For node item context menu
            container.RegisterInstance<IDiagramNodeItemCommand>(new MarkIsYieldCommand(), "MarkIsYield");
            container.RegisterInstance<IDiagramNodeItemCommand>(new DeleteItemCommand(), "Delete");
            //container.RegisterInstance<IDiagramNodeItemCommand>(new SelectDependantPropertiesCommand(), "DependantOn");

            container.RegisterInstance<IDiagramNodeItemCommand>(new MoveUpCommand(), "MoveItemUp");
            container.RegisterInstance<IDiagramNodeItemCommand>(new MoveDownCommand(), "MoveItemDown");

            //container.RegisterInstance<IEditorCommand>(,"Show/Hide This Help"),"ShowHideHelp");

            // Drawers
            RegisterDrawer<ConnectorViewModel, ConnectorDrawer>();
            RegisterDrawer<ConnectionViewModel, ConnectionDrawer>();

            container.Register<GraphData,ElementsGraph>("Element Graph");

            RegisterGraphItem<ViewModelPropertyData, TypedItemViewModel, ElementItemDrawer>();
            RegisterGraphItem<SceneManagerData, SceneManagerViewModel, SceneManagerDrawer>();
            RegisterGraphItem<SubSystemData, SubSystemViewModel, SubSystemDrawer>();
            RegisterGraphItem<ElementData, ElementNodeViewModel, ElementDrawer>();
            RegisterGraphItem<EnumData, EnumNodeViewModel, DiagramEnumDrawer>();
            RegisterGraphItem<ViewData, ViewNodeViewModel, ViewDrawer>();
            RegisterGraphItem<ViewComponentData, ViewComponentNodeViewModel, ViewComponentDrawer>();

            RegisterGraphItem<ViewModelPropertyData, ElementPropertyItemViewModel, ElementItemDrawer>();
            RegisterGraphItem<ViewModelCommandData, ElementCommandItemViewModel, ElementItemDrawer>();
            RegisterGraphItem<ViewModelCollectionData, ElementCollectionItemViewModel, ElementItemDrawer>();
            RegisterGraphItem<EnumItem, EnumItemViewModel, EnumItemDrawer>();
            RegisterGraphItem<SceneManagerTransition, SceneTransitionItemViewModel, ItemDrawer>();
            RegisterGraphItem<ViewPropertyData, ViewPropertyItemViewModel, ItemDrawer>();
            RegisterGraphItem<ViewBindingItemViewModel, SceneTransitionItemViewModel, ItemDrawer>();
            RegisterGraphItem<SceneManagerTransition, SceneTransitionItemViewModel, ItemDrawer>();
            RegisterGraphItem<RegisteredInstanceData, RegisterInstanceItemViewModel, ElementItemDrawer>();


            // Filters
            RegisterFilterNode<SceneFlowFilter, SceneManagerData>();
            RegisterFilterNode<SceneFlowFilter, SubSystemData>();

            RegisterFilterNode<SubSystemData, ElementData>();
            RegisterFilterNode<SubSystemData, EnumData>();

            RegisterFilterNode<ElementData, EnumData>();
            RegisterFilterNode<ElementData, ViewData>();
            RegisterFilterNode<ElementData, ViewComponentData>();

            RegisterDrawer<ConnectorHeaderViewModel, InputHeaderDrawer>();

            // Connections
            container.RegisterInstance<IConnectionStrategy>(new ElementInheritanceConnectionStrategy(), "ElementInheritanceConnectionStrategy");
            container.RegisterInstance<IConnectionStrategy>(new ElementViewConnectionStrategy(), "ElementViewConnectionStrategy");
            container.RegisterInstance<IConnectionStrategy>(new SceneTransitionConnectionStrategy(), "SceneTransitionConnectionStrategy");
            container.RegisterInstance<IConnectionStrategy>(new ViewInheritanceConnectionStrategy(), "ViewInheritanceConnectionStrategy");
            container.RegisterInstance<IConnectionStrategy>(new SceneManagerSubsystemConnectionStrategy(), "SceneManagerSubsystemConnectionStrategy");
            container.RegisterInstance<IConnectionStrategy>(new SubsystemConnectionStrategy(), "SubsystemConnectionStrategy");
            container.RegisterInstance<IConnectionStrategy>(new AssociationConnectionStrategy(), "AssociationConnectionStrategy");
            container.RegisterInstance<IConnectionStrategy>(new ComputedPropertyInputStrategy(), "ComputedPropertyInputStrategy");
            container.RegisterInstance<IConnectionStrategy>(new ViewComponentElementConnectionStrategy(), "ViewComponentElementConnectionStrategy");
            container.RegisterInstance<IConnectionStrategy>(new ViewComponentInheritanceConnectionStrategy(), "ViewComponentInheritanceConnectionStrategy");


            container.RegisterInstance<IUFrameTypeProvider>(new uFrameStringTypeProvider());
            container.RegisterInstance<UFrameSettings>(new UFrameSettings());

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

            ConnectionStrategies = Container.ResolveAll<IConnectionStrategy>().ToArray();
            KeyBindings = Container.ResolveAll<IKeyBinding>().ToArray();
            BindingGenerators = Container.ResolveAll<IBindingGenerator>().ToArray();
           // uFrameTypes = Container.Resolve<IUFrameTypeProvider>();
            
            Settings = container.Resolve<UFrameSettings>();


            var filterTypes = Container.RelationshipMappings.Where(
                p => typeof (IDiagramFilter).IsAssignableFrom(p.From) && p.To == typeof (IDiagramNode));
            var filterTypeItems = Container.RelationshipMappings.Where(
                p => typeof(IDiagramFilter).IsAssignableFrom(p.From) && p.To == typeof(IDiagramNodeItem));

            foreach (var filterMapping in filterTypes)
            {
                if (!AllowedFilterNodes.ContainsKey(filterMapping.From))
                {
                    AllowedFilterNodes.Add(filterMapping.From,new List<Type>());
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


        public static UFrameSettings Settings
        {
            get { return _settings ?? (Container.Resolve<UFrameSettings>()); }
            set { _settings = value; }
        }

        public static IConnectionStrategy[] ConnectionStrategies { get; set; }

        public static void RegisterDrawer<TViewModel, TDrawer>()
        {
            Container.RegisterRelation<TViewModel,IDrawer,TDrawer>();
        }
        public static void RegisterGraphItem<TModel, TViewModel, TDrawer>()
        {
            Container.RegisterRelation<TModel, ViewModel, TViewModel>();
            RegisterDrawer<TViewModel, TDrawer>();
        }

        public static void RegisterFilterNode<TFilterData, TAllowedItem>()
        {
            if (!AllowedFilterNodes.ContainsKey(typeof (TFilterData)))
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