﻿using Invert.Common;
using Invert.uFrame;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ViewModels;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Invert.IOC;
using UnityEngine;
#if !UNITY_DLL
using KeyCode = System.Windows.Forms.Keys;
#endif
namespace Invert.Core.GraphDesigner
{
    public static class InvertGraphEditor
    {
        public const string CURRENT_VERSION = "1.501";
        public const double CURRENT_VERSION_NUMBER = 1.501;
        public const bool REQUIRE_UPGRADE = true;


        private static IAssetManager _assetManager;

        private static IEditorCommand[] _commands;

        private static IConnectionStrategy[] _connectionStrategies;

        private static IProjectRepository _currentProject;

        private static IProjectRepository[] _projects;

        private static IGraphEditorSettings _settings;

        private static UFrameContainer _TypesContainer;

        private static IWindowManager _windowManager;
        private static MouseEvent _currentMouseEvent;
        private static IGraphWindow _designerWindow;
        private static IStyleProvider _styleProvider;
        private static IUFrameContainer _uiContainer;

        public static IPlatformOperations Platform { get; set; }
        public static IPlatformPreferences Prefs { get; set; }

        public static IStyleProvider StyleProvider
        {
            get { return _styleProvider ?? (_styleProvider = Container.Resolve<IStyleProvider>()); }
            set { _styleProvider = value; }
        }



        public static IAssetManager AssetManager
        {
            get { return _assetManager ?? (_assetManager = Container.Resolve<IAssetManager>()); }
            set { _assetManager = value; }
        }

        public static IEditorCommand[] Commands
        {
            get
            {
                return _commands ?? (_commands = Container.ResolveAll<IEditorCommand>().ToArray());
            }
        }

        public static IConnectionStrategy[] ConnectionStrategies
        {
            get { return _connectionStrategies ?? (_connectionStrategies = Container.ResolveAll<IConnectionStrategy>().ToArray()); }
            set { _connectionStrategies = value; }
        }

        public static IUFrameContainer Container
        {
            get { return InvertApplication.Container; }
        }
        public static IUFrameContainer UIContainer
        {
            get { return _uiContainer ?? (_uiContainer = new UFrameContainer()); }
        }


        public static DiagramViewModel CurrentDiagramViewModel
        {
            get
            {
                if (DesignerWindow == null) return null;
                return DesignerWindow.DiagramViewModel;
            }
        }

        //public static MouseEvent CurrentMouseEvent
        //{
        //    // TODO
        //    get { return _currentMouseEvent; }
        //    set { _currentMouseEvent = value; }
        //}

        //public static IProjectRepository CurrentProject
        //{
        //    get { return _currentProject; }
        //    set
        //    {
        //        _currentProject = value;
        //        if (value != null)
        //        {
        //            foreach (var diagram in _currentProject.Graphs)
        //            {
        //                diagram.SetProject(value);
        //            }
        //        }
        //    }
        //}

        public static IGraphWindow DesignerWindow
        {
            get { return _designerWindow; }
            set
            {
                _designerWindow = value;
                //Container.Inject(_designerWindow);
            }
        }

        public static IKeyBinding[] KeyBindings { get; set; }

        //public static IProjectRepository[] Projects
        //{
        //    get { return _projects ?? (_projects = AssetManager.GetAssets(typeof(IProjectRepository)).Cast<IProjectRepository>().ToArray()); }
        //    set { _projects = value; }
        //}

        //public static IProjectRepository[] GetAllProjects()
        //{

        //}
        public static IGraphEditorSettings Settings
        {
            get { return _settings ?? (_settings = Container.Resolve<IGraphEditorSettings>()); }
            set { _settings = value; }
        }

        public static UFrameContainer TypesContainer
        {
            get
            {
                if (_TypesContainer != null) return _TypesContainer;
                _TypesContainer = new UFrameContainer();
                InitializeTypesContainer(_TypesContainer);
                return _TypesContainer;
            }
            set { _TypesContainer = value; }
        }

        public static IWindowManager WindowManager
        {
            get { return _windowManager ?? (_windowManager = Container.Resolve<IWindowManager>()); }
        }

        public static IPlatformDrawer PlatformDrawer { get; set; }

        public static IUFrameContainer Connectable<TSource, TTarget>(this IUFrameContainer container, bool oneToMany = true)
            where TSource : class, IConnectable
            where TTarget : class, IConnectable
        {
            return Connectable<TSource, TTarget>(container, Color.white, oneToMany);
        }

        public static IUFrameContainer Connectable<TSource, TTarget>(this IUFrameContainer container, Color color, bool oneToMany = true)
            where TSource : class, IConnectable
            where TTarget : class, IConnectable
        {

            //if (oneToMany)
            container.RegisterInstance<IConnectionStrategy>(new CustomInputOutputStrategy<TSource, TTarget>(color), typeof(TSource).Name + "_" + typeof(TTarget).Name + "Connection");
            //else
            //{
            //    container.RegisterInstance<IConnectionStrategy>(new OneToOneConnectionStrategy<TSource, TTarget>(), typeof(TSource).Name + "_" + typeof(TTarget).Name + "Connection");
            //}
            return container;
        }
        public static IUFrameContainer AddNodeFlag<TSource>(this IUFrameContainer container, string flag) where TSource : class, IDiagramNodeItem
        {
            container.RegisterInstance<IDiagramNodeCommand>(new GraphItemFlagCommand<TSource>(flag, flag), typeof(TSource).Name + flag + "FlagCommand");
            return container;
        }
        public static IUFrameContainer AddItemFlag<TSource>(this IUFrameContainer container, string flag, Color color) where TSource : class, IDiagramNodeItem
        {
            var command = new GraphItemFlagCommand<TSource>(flag, flag) {Color = color};

            container.RegisterInstance<IDiagramNodeItemCommand>(command,typeof (TSource).Name + flag + "FlagCommand");
            container.RegisterInstance<IFlagCommand>(command, flag);
            return container;
        }
        public static IEnumerable<IEditorCommand> CreateCommandsFor<T>()
        {
            var commands = Container.ResolveAll<T>();

            return Enumerable.Where(Commands, p => typeof(T).IsAssignableFrom(p.For));
        }
        public static TCommandUI CreateCommandUIWithCommands<TCommandUI>(params IEditorCommand[] actions) where TCommandUI : class,ICommandUI
        {
            var ui = Container.Resolve<TCommandUI>() as ICommandUI;
            ui.Handler = DesignerWindow;
            foreach (var action in actions)
            {
                if (action.CanExecute(DesignerWindow) == null)
                ui.AddCommand(action);
            }
            return (TCommandUI)ui;
        }
        public static TCommandUI CreateCommandUI<TCommandUI>(params Type[] contextTypes) where TCommandUI : class,ICommandUI
        {
            var ui = Container.Resolve<TCommandUI>() as ICommandUI;
            ui.Handler = DesignerWindow;
            foreach (var contextType in contextTypes)
            {
                var commands = Container.ResolveAll(contextType).Cast<IEditorCommand>().ToArray();

                foreach (var command in commands)
                {
                    if (command.CanExecute(DesignerWindow) == null)
                    {
                       
                        ui.AddCommand(command);
                    }

                   
                }
            }
            return (TCommandUI)ui;
        }


        public static void DesignerPluginLoaded()
        {

            Settings = Container.Resolve<IGraphEditorSettings>();
            AssetManager = Container.Resolve<IAssetManager>();
            OrganizeFilters();
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
        }

        public static void ExecuteCommand(IEditorCommand action)
        {
            ExecuteCommand(DesignerWindow, action);
        }

        public static void ExecuteCommand(Action<DiagramViewModel> action, bool recordUndo = false)
        {
            ExecuteCommand(DesignerWindow, new SimpleEditorCommand<DiagramViewModel>(action), recordUndo);
        }

  
        private static void ExecuteCommand(this ICommandHandler handler, IEditorCommand command, bool recordUndo = true)
        {
            var objs = handler.ContextObjects.ToArray();
            if (recordUndo && DesignerWindow != null && DesignerWindow.DiagramViewModel != null)
            {
                DesignerWindow.DiagramViewModel.CurrentRepository.RecordUndo(DesignerWindow.DiagramViewModel.GraphData, command.Name);
            }
           
            command.Execute(handler);


            if (recordUndo && DesignerWindow != null && DesignerWindow.DiagramViewModel != null)
            {
                DesignerWindow.DiagramViewModel.CurrentRepository.MarkDirty(DesignerWindow.DiagramViewModel.GraphData);
            }
                
            //CurrentProject.MarkDirty(CurrentProject.CurrentGraph);
        }

        public static IEnumerable<OutputGenerator> GetAllCodeGenerators(GeneratorSettings settings, INodeRepository project, bool includeDisabled = false)
        {
            // Grab all the code generators
            var diagramItemGenerators = Container.ResolveAll<DesignerGeneratorFactory>().ToArray();
            var proj = project as IProjectRepository;
            var diagrams = new[] { project as IGraphData };
            if (proj != null)
            {
                diagrams = proj.Graphs.ToArray();
            }

            foreach (var diagramItemGenerator in diagramItemGenerators)
            {
                DesignerGeneratorFactory generator = diagramItemGenerator;
                // If its a generator for the entire diagram

                if (typeof(IGraphData).IsAssignableFrom(generator.DiagramItemType) && project != null)
                {
                    foreach (var diagram in diagrams)
                    {
                        if (diagram.Precompiled) continue;
                        if (diagram.Settings.CodeGenDisabled && !includeDisabled) continue;

                        if (generator.DiagramItemType.IsAssignableFrom(diagram.GetType()))
                        {
                            var codeGenerators = generator.GetGenerators(settings, diagram.CodePathStrategy, project, diagram);
                            foreach (var codeGenerator in codeGenerators)
                            {
                                // TODO Had to remove this?
                                //if (!codeGenerator.IsEnabled(project)) continue;

                                codeGenerator.AssetPath = diagram.CodePathStrategy.AssetPath;
                                codeGenerator.Settings = settings;
                                if (codeGenerator.ObjectData == null)
                                    codeGenerator.ObjectData = diagram;
                                codeGenerator.GeneratorFor = diagramItemGenerator.DiagramItemType;
                                yield return codeGenerator;
                            }
                        }
                    }
                }
                else if (typeof(INodeRepository).IsAssignableFrom(generator.DiagramItemType))
                {
                    foreach (var diagram in diagrams)
                    {
                        if (diagram.Precompiled) continue;
                        if (diagram.Settings.CodeGenDisabled && !includeDisabled) continue;
                        var codeGenerators = generator.GetGenerators(settings, diagram.CodePathStrategy, project, diagram);
                        foreach (var codeGenerator in codeGenerators)
                        {
                            // TODO had to remove this?
                            //if (!codeGenerator.IsEnabled(project)) continue;
                            codeGenerator.AssetPath = diagram.CodePathStrategy.AssetPath;
                            codeGenerator.Settings = settings;
                            if (codeGenerator.ObjectData == null)
                                codeGenerator.ObjectData = project;
                            codeGenerator.GeneratorFor = diagramItemGenerator.DiagramItemType;
                            yield return codeGenerator;
                        }
                    }
                }
                // If its a generator for a specific node type
                else
                {
                    foreach (var codeGenerator1 in GetCodeGeneratorsForNodes(settings, project, generator, diagramItemGenerator, includeDisabled)) yield return codeGenerator1;
                }
            }
        }

        /// <summary>
        /// Get all of the output generators for a node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static IEnumerable<OutputGenerator> GetCodeGeneratorsForNode(this IDiagramNodeItem node)
        {
            return GetAllCodeGenerators(null, node.Node.Project).Where(p => p.ObjectData == node);
        }

        /// <summary>
        /// Get all of the output generators that are generated only the first time, AKA: Editable Files
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static IEnumerable<OutputGenerator> GetAllEditableFilesForNode(this IDiagramNodeItem node)
        {
            return GetAllCodeGenerators(null, node.Node.Project).Where(p => p.ObjectData == node && !p.AlwaysRegenerate);
        }

        /// <summary>
        /// This method gets every method, property, or constructor generated for a node, pass in an item filter to filter only to specific items
        /// that are set up as iterators on templates.
        /// 
        /// For Example:
        /// You can get only the members of a class that have been added since last save and compile by comparing with with the change tracking data.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="itemFilter"></param>
        /// <returns></returns>
        public static IEnumerable<TemplateMemberResult> GetEditableOutputMembers(this IDiagramNodeItem node, Predicate<IDiagramNodeItem> itemFilter = null)
        {
          
            foreach (var item in GetAllEditableFilesForNode(node).OfType<ITemplateClassGenerator>())
            {
                var gen = new CodeFileGenerator()
                {
                    Generators = new[] {item as OutputGenerator},
                    SystemPath = string.Empty
                };
                gen.Namespace = new CodeNamespace();
                gen.Unit = new CodeCompileUnit();
                gen.Unit.Namespaces.Add(gen.Namespace);
                item.ItemFilter = itemFilter;
                item.Initialize(gen);
                foreach (var result in item.Results)
                {
                    yield return result;
                }
            }
        }
        public static IEnumerable<IClassTemplate> GetTemplates(this IDiagramNodeItem node, Predicate<IDiagramNodeItem> itemFilter = null)
        {

            foreach (var item in node.GetCodeGeneratorsForNode().OfType<ITemplateClassGenerator>())
            {
                yield return item.Template;
            }
        }
        /// <summary>
        /// Grab all of the output generators that are always regenerated on a node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static IEnumerable<OutputGenerator> GetAllDesignerFilesForNode(this IDiagramNodeItem node)
        {
            return GetAllCodeGenerators(null, node.Node.Project).Where(p => p.ObjectData == node && p.AlwaysRegenerate);
        }

        /// <summary>
        /// Get all of the code generators for a repository IE: (Graph, or Project)
        /// </summary>
        /// <param name="settings">Obsolete pass null</param>
        /// <param name="project">Project or graph</param>
        /// <param name="generator"></param>
        /// <param name="diagramItemGenerator"></param>
        /// <param name="includeDisabled"></param>
        /// <returns></returns>
        private static IEnumerable<OutputGenerator> GetCodeGeneratorsForNodes(GeneratorSettings settings, INodeRepository project,
            DesignerGeneratorFactory generator, DesignerGeneratorFactory diagramItemGenerator, bool includeDisabled = false)
        {
            var proj = project as IProjectRepository;
            var diagrams = new[] { project as IGraphData };
            if (proj != null)
            {
                diagrams = proj.Graphs.ToArray();
            }


            foreach (var diagram in diagrams)
            {
                if (diagram.Precompiled) continue;
                if (diagram.Settings.CodeGenDisabled && !includeDisabled) continue;
                var items = diagram.AllGraphItems.OfType<IDiagramNodeItem>().Where(p => p.GetType() == generator.DiagramItemType);

                foreach (var item in items)
                {
                    if (item.Precompiled) continue;
                    var codeGenerators = generator.GetGenerators(settings, diagram.CodePathStrategy, project, item);
                    foreach (var codeGenerator in codeGenerators)
                    {
                        // TODO had to remove this?
                        //if (!codeGenerator.IsEnabled(project)) continue;
                        codeGenerator.AssetPath = diagram.CodePathStrategy.AssetPath;
                        codeGenerator.Settings = settings;
                        if (codeGenerator.ObjectData == null)
                            codeGenerator.ObjectData = item;
                        codeGenerator.GeneratorFor = diagramItemGenerator.DiagramItemType;
                        yield return codeGenerator;
                    }
                }
            }
        }

        //public static IEnumerable<CodeFileGenerator> GetAllFileGenerators(GeneratorSettings settings)
        //{
        //    return GetAllFileGenerators(settings, CurrentProject);
        //}
        /// <summary>
        /// Get all of the merged generators for a project, this will merge any output generators with the same filename into a combined "CodeFileGenerator".
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="project"></param>
        /// <param name="includeDisabled"></param>
        /// <param name="systemPath"></param>
        /// <returns></returns>
        public static IEnumerable<CodeFileGenerator> GetAllFileGenerators(GeneratorSettings settings, INodeRepository project, bool includeDisabled = false, string systemPath = null)
        {
            var codeGenerators = GetAllCodeGenerators(settings, project, includeDisabled).ToArray();
            var groups = codeGenerators.GroupBy(p => p.FullPathName).Distinct();
            foreach (var @group in groups)
            {
                var generator = new CodeFileGenerator(project.Namespace)
                {
                    AssetPath = @group.Key.Replace("\\", "/"),
#if UNITY_DLL
                    SystemPath = Path.Combine(Application.dataPath, @group.Key.Substring(7)).Replace("\\", "/"),
#else
                    SystemPath = Path.Combine(systemPath,@group.Key),
#endif
                    Generators = @group.ToArray()
                };
                yield return generator;
            }
        }


        public static GraphItemViewModel GetNodeViewModel(this IUFrameContainer container, IGraphItem item, DiagramViewModel diagram)
        {
            var vm = InvertApplication.Container.ResolveRelation<ViewModel>(item.GetType(), item, diagram) as
                           GraphItemViewModel;
            return vm;
        }
        public static IEnumerable<Type> GetAllowedFilterItems(Type filterType)
        {
            return Container.RelationshipMappings.Where(
                p => p.From == filterType && p.To == typeof(IDiagramNodeItem)).Select(p => p.Concrete);
        }

        public static IEnumerable<Type> GetAllowedFilterNodes(Type filterType)
        {
            return Container.RelationshipMappings.Where(
                p => p.From == filterType && p.To == typeof(IDiagramNode)).Select(p => p.Concrete);
        }

        public static IEnumerable<IEditorCommand> GetContextCommandsFor<T>()
        {
            return Enumerable.Where(Commands, p => p is IContextMenuItemCommand && typeof(T).IsAssignableFrom(p.For));
        }

        public static void HookCommand<TFor>(string name, IEditorCommand hook) where TFor : class, IEditorCommand
        {
            var command = Container.Resolve<TFor>(name);
            command.Hooks.Add(hook);
        }

        public static bool IsFilter(Type type)
        {
            return FilterExtensions.AllowedFilterNodes.ContainsKey(type);
        }

        public static void OrganizeFilters()
        {
            var filterTypes = Container.RelationshipMappings.Where(
               p => typeof(IDiagramFilter).IsAssignableFrom(p.From) && p.To == typeof(IDiagramNode));
            var filterTypeItems = Container.RelationshipMappings.Where(
                p => typeof(IDiagramFilter).IsAssignableFrom(p.From) && p.To == typeof(IDiagramNodeItem));

            foreach (var filterMapping in filterTypes)
            {
                if (!FilterExtensions.AllowedFilterNodes.ContainsKey(filterMapping.From))
                {
                    FilterExtensions.AllowedFilterNodes.Add(filterMapping.From, new List<Type>());
                }
                FilterExtensions.AllowedFilterNodes[filterMapping.From].Add(filterMapping.Concrete);
            }

            foreach (var filterMapping in filterTypeItems)
            {
                if (!FilterExtensions.AllowedFilterItems.ContainsKey(filterMapping.From))
                {
                    FilterExtensions.AllowedFilterItems.Add(filterMapping.From, new List<Type>());
                }
                FilterExtensions.AllowedFilterItems[filterMapping.From].Add(filterMapping.Concrete);
            }
        }

        public static IUFrameContainer RegisterChildGraphItem<TModel, TViewModel>(this IUFrameContainer container)
        {
            container.RegisterRelation<TModel, ItemViewModel, TViewModel>();
            return container;
        }

        public static IUFrameContainer RegisterCodeTemplate<TFor, TTemplateType>(this IUFrameContainer container)
        {
            container.RegisterRelation<TFor, CodeGenerator, TTemplateType>();
            return container;
        }
        public static void RegisterConnectable<TOutput, TInput>(this IUFrameContainer container)
        {
            container.RegisterInstance<RegisteredConnection>(new RegisteredConnection() { TInputType = typeof(TInput), TOutputType = typeof(TOutput) }, typeof(TOutput).Name + typeof(TInput).Name);

        }
        public static void RegisterConnectable(this IUFrameContainer container, Type outputType, Type inputType)
        {
            container.RegisterInstance<RegisteredConnection>(new RegisteredConnection() { TInputType = inputType, TOutputType = outputType }, outputType.Name + inputType.Name);

        }
        public static void RegisterFilterItem<TFilterData, TAllowedItem>()
        {
            Container.RegisterRelation<TFilterData, IDiagramNodeItem, TAllowedItem>();
        }

        public static void RegisterFilterItem<TFilterData, TAllowedItem>(this IUFrameContainer container)
        {
            container.RegisterRelation<TFilterData, IDiagramNodeItem, TAllowedItem>();
        }
        public static IUFrameContainer RegisterDataViewModel<TModel, TViewModel>(this IUFrameContainer container)
        {
            container.RegisterRelation<TModel, ViewModel, TViewModel>();
            return container;
        }
        public static IUFrameContainer RegisterDataChildViewModel<TModel, TViewModel>(this IUFrameContainer container)
        {
            container.RegisterRelation<TModel, ItemViewModel, TViewModel>();
            return container;
        }

        public static IUFrameContainer RegisterConnectionStrategy<TConnectionStrategy>(this IUFrameContainer container)
            where TConnectionStrategy : IConnectionStrategy, new()
        {
            container.RegisterInstance<IConnectionStrategy>(new TConnectionStrategy(), typeof(TConnectionStrategy).Name);
            return container;
        }

        public static IUFrameContainer RegisterNodeCommand<TCommand>(this IUFrameContainer container)
            where TCommand : IDiagramNodeCommand, new()
        {
            //container.RegisterInstance<IDiagramContextCommand>(new AddNodeToGraph(), "AddItemCommand");
            container.RegisterInstance<IDiagramNodeCommand>(new TCommand(), typeof(TCommand).Name);
            return container;
        }
        public static IUFrameContainer RegisterNodeItemCommand<TCommand>(this IUFrameContainer container)
        where TCommand : IDiagramNodeItemCommand, new()
        {
            //container.RegisterInstance<IDiagramContextCommand>(new AddNodeToGraph(), "AddItemCommand");
            container.RegisterInstance<IDiagramNodeItemCommand>(new TCommand(), typeof(TCommand).Name);
            return container;
        }
        public static IUFrameContainer RegisterGraphCommand<TCommand>(this IUFrameContainer container)
             where TCommand : IDiagramContextCommand, new()
        {
            //container.RegisterInstance<IDiagramContextCommand>(new AddNodeToGraph(), "AddItemCommand");
            container.RegisterInstance<IDiagramContextCommand>(new TCommand(), typeof(TCommand).Name);
            return container;
        }
        public static IUFrameContainer RegisterToolbarCommand<TCommand>(this IUFrameContainer container)
             where TCommand : IToolbarCommand, new()
        {
            //container.RegisterInstance<IDiagramContextCommand>(new AddNodeToGraph(), "AddItemCommand");
            container.RegisterInstance<IToolbarCommand>(new TCommand(), typeof(TCommand).Name);
            return container;
        }

        //public static IUFrameContainer ConnectionStrategy<TSource, TTarget>(this IUFrameContainer container, Color connectionColor,
        //    Func<TSource, TTarget, bool> isConnected, Action<TSource, TTarget> apply, Action<TSource, TTarget> remove) where TSource : class, IConnectable where TTarget : class, IConnectable
        //{
        //    container.RegisterInstance<IConnectionStrategy>(new CustomConnectionStrategy<TSource, TTarget>(connectionColor,isConnected,apply,remove), typeof(TSource).Name + "_" + typeof(TTarget).Name + "CustomConnection");
        //    return container;
        //}
        public static void RegisterFilterNode<TFilterData, TAllowedItem>(this IUFrameContainer container)
        {
            if (!FilterExtensions.AllowedFilterNodes.ContainsKey(typeof(TFilterData)))
            {
                FilterExtensions.AllowedFilterNodes.Add(typeof(TFilterData), new List<Type>());
            }
            FilterExtensions.AllowedFilterNodes[typeof(TFilterData)].Add(typeof(TAllowedItem));
        }
        public static void RegisterFilterNode(this IUFrameContainer container, Type filter, Type tnode)
        {
            if (!FilterExtensions.AllowedFilterNodes.ContainsKey(filter))
            {
                FilterExtensions.AllowedFilterNodes.Add(filter, new List<Type>());
            }
            FilterExtensions.AllowedFilterNodes[filter].Add(tnode);
        }
        public static void RegisterFilterNode<TFilterData, TAllowedItem>()
        {
            if (!FilterExtensions.AllowedFilterNodes.ContainsKey(typeof(TFilterData)))
            {
                FilterExtensions.AllowedFilterNodes.Add(typeof(TFilterData), new List<Type>());
            }
            FilterExtensions.AllowedFilterNodes[typeof(TFilterData)].Add(typeof(TAllowedItem));
        }


        public static IUFrameContainer RegisterGraphItem<TModel, TViewModel>(this IUFrameContainer container)
        {
            container.RegisterRelation<TModel, ViewModel, TViewModel>();
            return container;
        }

        public static IUFrameContainer RegisterKeyBinding(this IUFrameContainer container, IEditorCommand command, string name, KeyCode code, bool control = false, bool alt = false, bool shift = false)
        {
            container.RegisterInstance<IKeyBinding>(new SimpleKeyBinding(command, name, code, control, alt, shift), name);
            return container;
        }
        public static void RegisterKeyBinding(IEditorCommand command, string name, KeyCode code, bool control = false, bool alt = false, bool shift = false)
        {
            Container.RegisterInstance<IKeyBinding>(new SimpleKeyBinding(command, name, code, control, alt, shift), name);
        }
        public static IDrawer CreateDrawer(this IUFrameContainer container, ViewModel viewModel)
        {
            return CreateDrawer<IDrawer>(container, viewModel);
        }

        public static Dictionary<Type, Type> _drawers;
        public static IDrawer CreateDrawer<TDrawerBase>(this IUFrameContainer container, ViewModel viewModel) where TDrawerBase : IDrawer
        {
            if (_drawers != null)
            {
                
            }
            if (viewModel == null)
            {
                InvertApplication.LogError("Data is null.");
                return null;
            }
            var drawer = container.ResolveRelation<TDrawerBase>(viewModel.GetType(), viewModel);
            if (drawer == null)
            {
                InvertApplication.Log(String.Format("Couldn't Create drawer for {0}.", viewModel.GetType()));
            }
            return drawer;
        }

        public static void ListenToProjectEvents(this IUFrameContainer container, IProjectEvents events)
        {
            container.RegisterInstance<IProjectEvents>(events, events.GetType().Name);
        }

        public static GraphItemViewModel CreateViewModel(this IUFrameContainer container, object data)
        {
            return container.ResolveRelation<ViewModel>(data.GetType(), data, null) as GraphItemViewModel;
        }
        private static void InitializeTypesContainer(UFrameContainer container)
        {

        }

        public static void NavigateTo(string identifier)
        {
            if (CurrentDiagramViewModel == null) return;
            ExecuteCommand(_ =>
            {
                _.NavigateTo(identifier);
            });
        }
    }

    public class RegisteredTemplateGeneratorsFactory : DesignerGeneratorFactory<IGraphData>
    {
        private static Dictionary<Type, List<Type>> _registeredTemplates = new Dictionary<Type, List<Type>>();

        protected static Dictionary<Type, List<Type>> RegisteredTemplates
        {
            get { return _registeredTemplates; }
            set { _registeredTemplates = value; }
        }

        public static void RegisterTemplate<TFor, TTemplate>()
            where TTemplate : class, IClassTemplate<TFor>, new()
            where TFor : class, IDiagramNodeItem
        {
            var type = typeof(TemplateClassGenerator<TFor, TTemplate>);
            List<Type> list;
            if (!RegisteredTemplates.TryGetValue(typeof(TFor), out list))
            {
                RegisteredTemplates.Add(typeof(TFor), list = new List<Type>());
            }
            if (!list.Contains(type))
                list.Add(type);
        }
        public override IEnumerable<OutputGenerator> CreateGenerators(GeneratorSettings settings, ICodePathStrategy pathStrategy, INodeRepository diagramData,
            IGraphData item)
        {
            foreach (var graphItem in item.AllGraphItems.OfType<IDiagramNodeItem>())
            {
                foreach (var template in RegisteredTemplates)
                {
                    if (template.Key.IsAssignableFrom(graphItem.GetType()))
                    {
                        foreach (var templateType in template.Value)
                        {
                            foreach (var t in CreateTemplateGenerators(pathStrategy, graphItem, templateType))
                            {
                                yield return t;
                            }
                        }
                    }
                }
            }

        }

        private IEnumerable<OutputGenerator> CreateTemplateGenerators(ICodePathStrategy pathStrategy, IDiagramNodeItem graphItem, Type templateType)
        {
            var templateClassType = templateType.GetGenericArguments()[1];
            var templateAttribute = templateClassType.GetCustomAttributes(typeof(TemplateClass), true)
                .OfType<TemplateClass>()
                .FirstOrDefault();
            if (templateAttribute == null)
            {
                InvertApplication.Log(string.Format("ClassTemplate attribute not found on {0} ", templateClassType.Name));
                yield break;
            }

           // var outputFolderName = templateAttribute.OutputFolderName ?? templateType.Name;
            if (templateAttribute.Location == TemplateLocation.DesignerFile || templateAttribute.Location == TemplateLocation.Both)
            {
                var template = Activator.CreateInstance(templateType) as CodeGenerator;
                template.ObjectData = graphItem;
                template.IsDesignerFile = true;
                template.AssetPath = pathStrategy.AssetPath;

                //if (templateAttribute.IsEditorExtension)
                //{
                //    template.Filename = Path.Combine(Path.Combine("_DesignerFiles", "Editor"),
                //        outputFolderName + ".designer.cs");
                //}
                //else
                //{
                //    template.Filename = Path.Combine("_DesignerFiles", template. + ".designer.cs");
                //}
                if (template.IsValid())
                {
                    yield return template;
                }


            }
            if (templateAttribute.Location == TemplateLocation.EditableFile || templateAttribute.Location == TemplateLocation.Both)
            {
                var template = Activator.CreateInstance(templateType) as CodeGenerator;
                template.ObjectData = graphItem;
                template.IsDesignerFile = false;
                template.AssetPath = pathStrategy.AssetPath;

                //var className = string.Format(templateAttribute.ClassNameFormat, graphItem.Name);
                //var typeItem = graphItem as IClassTypeNode;
                //if (typeItem != null)
                //{
                //    className = typeItem.ClassName;
                //}

                //if (templateAttribute.IsEditorExtension)
                //{
                //    template.Filename = Path.Combine("Editor", Path.Combine(outputFolderName, className +".cs"));

                //}
                //else
                //{
                //    template.Filename = Path.Combine(outputFolderName,
                //        className + ".cs");

                //}
                if (template.IsValid())
                {
                    yield return template;
                }

            }
        }
    }
}