using Invert.Common;
using Invert.uFrame;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public enum NodeColor
    {
        Gray,
        DarkGray,
        Blue,
        LightGray,
        Black,
        DarkDarkGray,
        Orange,
        Red,
        Yellow,
        Green,
        Purple,
        Pink,
        YellowGreen
    }

    public interface IGraphWindow : ICommandHandler
    {
        DiagramViewModel DiagramViewModel { get; }
        void RefreshContent();
    }

    public static class InvertGraphEditor
    {
        public const string CURRENT_VERSION = "1.501";
        public const double CURRENT_VERSION_NUMBER = 1.501;
        public const bool REQUIRE_UPGRADE = true;
        private static Dictionary<Type, List<Type>> _allowedFilterItems;

        private static Dictionary<Type, List<Type>> _allowedFilterNodes;

        private static IAssetManager _assetManager;

        private static IEditorCommand[] _commands;

        private static IConnectionStrategy[] _connectionStrategies;

        private static IProjectRepository _currentProject;

        private static IProjectRepository[] _projects;

        private static IGraphEditorSettings _settings;

        private static uFrameContainer _TypesContainer;

        private static IWindowManager _windowManager;

        public static IPlatformOperations Platform { get; set; }
        public static IPlatformPreferences Prefs { get; set; }

        public static Dictionary<Type, List<Type>> AllowedFilterItems
        {
            get { return _allowedFilterItems ?? (_allowedFilterItems = new Dictionary<Type, List<Type>>()); }
            set { _allowedFilterItems = value; }
        }

        public static Dictionary<Type, List<Type>> AllowedFilterNodes
        {
            get { return _allowedFilterNodes ?? (_allowedFilterNodes = new Dictionary<Type, List<Type>>()); }
            set { _allowedFilterNodes = value; }
        }

        public static IAssetManager AssetManager
        {
            get { return _assetManager = Container.Resolve<IAssetManager>(); }
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

        public static DiagramViewModel CurrentDiagramViewModel
        {
            get
            {
                if (DesignerWindow == null) return null;
                return DesignerWindow.DiagramViewModel;
            }
        }

        public static MouseEvent CurrentMouseEvent
        {
            // TODO
            get;
            set;
        }

        public static IProjectRepository CurrentProject
        {
            get { return _currentProject; }
            set
            {
                _currentProject = value;
                if (value != null)
                {
                    foreach (var diagram in _currentProject.Diagrams)
                    {
                        diagram.SetProject(value);
                    }
                }
            }
        }

        public static IGraphWindow DesignerWindow { get; set; }

        public static IKeyBinding[] KeyBindings { get; set; }

        public static IProjectRepository[] Projects
        {
            get { return _projects ?? (_projects = AssetManager.GetAssets(typeof(IProjectRepository)).Cast<IProjectRepository>().ToArray()); }
            set { _projects = value; }
        }

        public static IGraphEditorSettings Settings
        {
            get { return _settings ?? (_settings = Container.Resolve<IGraphEditorSettings>()); }
            set { _settings = value; }
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

        public static IWindowManager WindowManager
        {
            get { return _windowManager ?? (_windowManager = Container.Resolve<IWindowManager>()); }
        }

        public static IUFrameContainer Connectable<TSource, TTarget>(this IUFrameContainer container, bool oneToMany = true)
            where TSource : class, IConnectable
            where TTarget : class, IConnectable
        {
            return Connectable<TSource, TTarget>(container, Color.white, oneToMany);
        }

        public static IUFrameContainer Connectable<TSource, TTarget>(this IUFrameContainer container,Color color, bool oneToMany = true)
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

        public static IEnumerable<IEditorCommand> CreateCommandsFor<T>()
        {
            var commands = Container.ResolveAll<T>();

            return Enumerable.Where(Commands, p => typeof(T).IsAssignableFrom(p.For));
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
            return CreateDrawer<IDrawer>(viewModel);
        }

        public static IDrawer CreateDrawer<TDrawerBase>(ViewModel viewModel) where TDrawerBase : IDrawer
        {
            if (viewModel == null)
            {
                Debug.LogError("Data is null.");
                return null;
            }
            var drawer = Container.ResolveRelation<TDrawerBase>(viewModel.GetType(), viewModel);
            if (drawer == null)
            {
                Debug.Log(String.Format("Couldn't Create drawer for {0}.", viewModel.GetType()));
            }
            return drawer;
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

        public static void ExecuteCommand(Action<DiagramViewModel> action)
        {
            ExecuteCommand(DesignerWindow, new SimpleEditorCommand<DiagramViewModel>(action));
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
                            ExecuteCommand(handler, p);
                        });
                    handler.CommandExecuted(command);
                }
            }
            CurrentProject.MarkDirty(CurrentProject.CurrentGraph);
        }

        public static IEnumerable<CodeGenerator> GetAllCodeGenerators(GeneratorSettings settings, IProjectRepository project, bool includeDisabled = false)
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
                        if (diagram.Settings.CodeGenDisabled && !includeDisabled) continue;

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

                        if (diagram.Settings.CodeGenDisabled && !includeDisabled) continue;
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
                    foreach (var codeGenerator1 in GetCodeGeneratorsForNodes(settings, project, generator, diagramItemGenerator, includeDisabled)) yield return codeGenerator1;
                }
            }
        }

        public static IEnumerable<CodeGenerator> GetCodeGeneratorsForNode(this IDiagramNode node)
        {
            return GetAllCodeGenerators(node.Project.GeneratorSettings,node.Project).Where(p=>p.ObjectData == node);
        }

        private static IEnumerable<CodeGenerator> GetCodeGeneratorsForNodes(GeneratorSettings settings, IProjectRepository project,
            DesignerGeneratorFactory generator, DesignerGeneratorFactory diagramItemGenerator, bool includeDisabled = false)
        {
            foreach (var diagram in project.Diagrams)
            {

                if (diagram.Settings.CodeGenDisabled && !includeDisabled) continue;
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

        public static IEnumerable<CodeFileGenerator> GetAllFileGenerators(GeneratorSettings settings)
        {
            return GetAllFileGenerators(settings, CurrentProject);
        }

        public static IEnumerable<CodeFileGenerator> GetAllFileGenerators(GeneratorSettings settings, IProjectRepository project, bool includeDisabled = false)
        {
            var codeGenerators = GetAllCodeGenerators(settings, project, includeDisabled).ToArray();
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
            return AllowedFilterNodes.ContainsKey(type);
        }

        public static void OrganizeFilters()
        {
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

        public static IUFrameContainer RegisterChildGraphItem<TModel, TViewModel, TDrawer>(this IUFrameContainer container)
        {
            container.RegisterRelation<TModel, ItemViewModel, TViewModel>();
            container.RegisterItemDrawer<TViewModel, TDrawer>();
            return container;
        }

        public static void RegisterDrawer<TViewModel, TDrawer>(this IUFrameContainer container)
        {
            container.RegisterRelation<TViewModel, IDrawer, TDrawer>();
        }

        [Obsolete]
        public static void RegisterDrawer<TViewModel, TDrawer>()
        {
            Container.RegisterRelation<TViewModel, IDrawer, TDrawer>();
        }

        public static void RegisterFilterItem<TFilterData, TAllowedItem>()
        {
            Container.RegisterRelation<TFilterData, IDiagramNodeItem, TAllowedItem>();
        }

        public static void RegisterFilterItem<TFilterData, TAllowedItem>(this IUFrameContainer container)
        {
            container.RegisterRelation<TFilterData, IDiagramNodeItem, TAllowedItem>();
        }

        //public static IUFrameContainer ConnectionStrategy<TSource, TTarget>(this IUFrameContainer container, Color connectionColor,
        //    Func<TSource, TTarget, bool> isConnected, Action<TSource, TTarget> apply, Action<TSource, TTarget> remove) where TSource : class, IConnectable where TTarget : class, IConnectable
        //{
        //    container.RegisterInstance<IConnectionStrategy>(new CustomConnectionStrategy<TSource, TTarget>(connectionColor,isConnected,apply,remove), typeof(TSource).Name + "_" + typeof(TTarget).Name + "CustomConnection");
        //    return container;
        //}
        public static void RegisterFilterNode<TFilterData, TAllowedItem>(this IUFrameContainer container)
        {
            if (!AllowedFilterNodes.ContainsKey(typeof(TFilterData)))
            {
                AllowedFilterNodes.Add(typeof(TFilterData), new List<Type>());
            }
            AllowedFilterNodes[typeof(TFilterData)].Add(typeof(TAllowedItem));
        }
        public static void RegisterFilterNode(this IUFrameContainer container,Type filter, Type tnode)
        {
            if (!AllowedFilterNodes.ContainsKey(filter))
            {
                AllowedFilterNodes.Add(filter, new List<Type>());
            }
            AllowedFilterNodes[filter].Add(tnode);
        }
        public static void RegisterFilterNode<TFilterData, TAllowedItem>()
        {
            if (!AllowedFilterNodes.ContainsKey(typeof(TFilterData)))
            {
                AllowedFilterNodes.Add(typeof(TFilterData), new List<Type>());
            }
            AllowedFilterNodes[typeof(TFilterData)].Add(typeof(TAllowedItem));
        }

        public static IUFrameContainer RegisterGraphItem<TModel>(this uFrameContainer container) where TModel : GenericNode
        {
            container.RegisterGraphItem<TModel, ScaffoldNode<TModel>.ViewModel, ScaffoldNode<TModel>.Drawer>();
            //RegisterDrawer();
            return container;
        }

        public static IUFrameContainer RegisterGraphItem<TModel, TViewModel, TDrawer>(this IUFrameContainer container)
        {
            container.RegisterRelation<TModel, ViewModel, TViewModel>();
            container.RegisterDrawer<TViewModel, TDrawer>();
            return container;
        }

        public static IUFrameContainer RegisterGraphItem<TModel, TViewModel, TDrawer>()
        {
            Container.RegisterRelation<TModel, ViewModel, TViewModel>();
            RegisterDrawer<TViewModel, TDrawer>();
            return Container;
        }

        public static void RegisterItemDrawer<TViewModel, TDrawer>(this IUFrameContainer container)
        {
            Container.RegisterRelation<TViewModel, IDrawer, TDrawer>();
        }

        public static void RegisterKeyBinding(IEditorCommand command, string name, KeyCode code, bool control = false, bool alt = false, bool shift = false)
        {
            Container.RegisterInstance<IKeyBinding>(new SimpleKeyBinding(command, name, code, control, alt, shift), name);
        }

        private static void InitializeTypesContainer(uFrameContainer container)
        {
        }
    }

    public class DefaultGraphSettings : IGraphEditorSettings
    {
        public Color BackgroundColor { get; set; }

        public Color GridLinesColor { get; set; }

        public Color GridLinesColorSecondary { get; set; }

        public bool ShowGraphDebug { get; set; }

        public bool ShowHelp
        {
            get { return false; }
            set
            {
            }
        }

        public bool UseGrid
        {
            get { return true; }
            set { }
        }

        public DefaultGraphSettings()
        {
            BackgroundColor = new Color(0.13f, 0.13f, 0.13f);
            GridLinesColor = new Color(0.1f, 0.1f, 0.1f);
            GridLinesColorSecondary = new Color(0.08f, 0.08f, 0.08f);
        }
    }

    public class ScaffoldNode<TData> where TData : GenericNode
    {
      
        public class Drawer : GenericNodeDrawer<TData, ViewModel>
        {


            public Drawer(ViewModel viewModel)
                : base(viewModel)
            {
               
            }
        }

        public class ItemDrawer : ScaffoldNodeChildItem<TData>.Drawer
        {
            public ItemDrawer(ScaffoldNodeChildItem<TData>.ViewModel viewModel)
                : base(viewModel)
            {
            }
        }

        public class ItemViewModel : ScaffoldNodeChildItem<TData>.ViewModel
        {
            public ItemViewModel(TData graphItemObject, DiagramNodeViewModel diagramViewModel)
                : base(graphItemObject, diagramViewModel)
            {
            }
        }

        public class ScaffoldTypedItemDrawer : ScaffoldNodeTypedChildItem<TData>.Drawer
        {
            public ScaffoldTypedItemDrawer(ScaffoldNodeTypedChildItem<TData>.ViewModel viewModel)
                : base(viewModel)
            {
            }
        }

        public class TypedItemViewModel : ScaffoldNodeTypedChildItem<TData>.ViewModel
        {
            public TypedItemViewModel(TData graphItemObject, DiagramNodeViewModel diagramViewModel)
                : base(graphItemObject, diagramViewModel)
            {
            }
        }

        public class ViewModel : GenericNodeViewModel<TData>
        {
            public ViewModel(TData graphItemObject, DiagramViewModel diagramViewModel)
                : base(graphItemObject, diagramViewModel)
            {
            }
        }
    }

    public class ScaffoldNodeChildItem<TData> where TData : IDiagramNodeItem
    {
        public class Drawer : ItemDrawer
        {
            public Drawer(ViewModel viewModel)
                : base(viewModel)
            {
            }
        }

        public class ViewModel : GenericItemViewModel<TData>
        {
            public ViewModel(TData graphItemObject, DiagramNodeViewModel diagramViewModel)
                : base(graphItemObject, diagramViewModel)
            {
            }
        }
    }

    public class ScaffoldNodeTypedChildItem<TData> where TData : ITypedItem
    {
        public class Drawer : ElementItemDrawer
        {
            public Drawer(ViewModel viewModel)
                : base(viewModel)
            {
            }
        }

        public class ViewModel : TypedItemViewModel
        {
            public override string TypeLabel
            {
                get { return Data.RelatedTypeName; }
            }

            public ViewModel(TData graphItemObject, DiagramNodeViewModel diagramViewModel)
                : base(graphItemObject, diagramViewModel)
            {
            }
        }

      
    }

    //public class CustomItemDrawer<TData> : ItemDrawer
    //{
    //    public CustomItemDrawer()
    //    {
    //        ViewModelObject = new ItemViewModel();
    //    }
    //    public Func<CustomItemDrawer<TData>,>
    //    public override void Draw(float scale)
    //    {
    //        base.Draw(scale);
    //    }
    //}
}