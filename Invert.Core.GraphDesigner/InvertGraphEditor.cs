using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Invert.Core.GraphDesigner.Settings;
using Invert.uFrame;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public interface IGraphWindow : ICommandHandler
    {
        DiagramViewModel DiagramViewModel { get; }
    }
    public static class InvertGraphEditor
    {
        public const bool REQUIRE_UPGRADE = true;
        public const string CURRENT_VERSION = "1.501";
        public const double CURRENT_VERSION_NUMBER = 1.501;
        

        public static IAssetManager AssetManager { get; set; }

        private static Dictionary<Type, List<Type>> _allowedFilterNodes;
        private static Dictionary<Type, List<Type>> _allowedFilterItems;

        public static IProjectRepository[] Projects
        {
            get { return _projects ?? (_projects = AssetManager.GetAssets(typeof(IProjectRepository)).Cast<IProjectRepository>().ToArray()); }
            set { _projects = value; }
        }

        public static DiagramViewModel CurrentDiagramViewModel
        {
            get { return DesignerWindow.DiagramViewModel; }
        }

        public static IUFrameContainer Container
        {
            get { return InvertApplication.Container; }
        }

        public static IProjectRepository CurrentProject { get; set; }

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

        public static IGraphWindow DesignerWindow { get; set; }

        public static void HookCommand<TFor>(string name, IEditorCommand hook) where TFor : class, IEditorCommand
        {
            var command = Container.Resolve<TFor>(name);
            command.Hooks.Add(hook);
        }

        public static void RegisterKeyBinding(IEditorCommand command, string name, KeyCode code, bool control = false, bool alt = false, bool shift = false)
        {
            Container.RegisterInstance<IKeyBinding>(new SimpleKeyBinding(command, name, code, control, alt, shift), name);
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
                            Debug.Log(p.Name);
                            ExecuteCommand(handler, p);
                        });
                    handler.CommandExecuted(command);
                }
            }
            CurrentProject.MarkDirty(CurrentProject.CurrentGraph);
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
        private static IConnectionStrategy[] _connectionStrategies;
        public static IConnectionStrategy[] ConnectionStrategies
        {
            get { return _connectionStrategies ?? (_connectionStrategies = Container.ResolveAll<IConnectionStrategy>().ToArray()); }
            set { _connectionStrategies = value; }
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

        public static IEnumerable<IEditorCommand> GetContextCommandsFor<T>()
        {
            return Enumerable.Where(Commands, p => p is IContextMenuItemCommand && typeof(T).IsAssignableFrom(p.For));
        }

        public static IEnumerable<IEditorCommand> CreateCommandsFor<T>()
        {
            var commands = Container.ResolveAll<T>();

            return Enumerable.Where(Commands, p => typeof(T).IsAssignableFrom(p.For));
        }

        private static IEditorCommand[] _commands;
        private static IProjectRepository[] _projects;
        private static IGraphEditorSettings _settings;

        public static IEditorCommand[] Commands
        {
            get
            {
                return _commands ?? (_commands = Container.ResolveAll<IEditorCommand>().ToArray());
            }
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
        public static void RegisterDrawer<TViewModel, TDrawer>(this uFrameContainer container)
        {
            container.RegisterRelation<TViewModel, IDrawer, TDrawer>();
        }
        [Obsolete]
        public static void RegisterDrawer<TViewModel, TDrawer>()
        {
            Container.RegisterRelation<TViewModel, IDrawer, TDrawer>();
        }
        public static void RegisterItemDrawer<TViewModel, TDrawer>(this uFrameContainer container)
        {
            Container.RegisterRelation<TViewModel, IDrawer, TDrawer>();
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
            RegisterDrawer<TViewModel, TDrawer>();
            return container;
        }
        public static IUFrameContainer RegisterChildGraphItem<TModel, TViewModel, TDrawer>(this IUFrameContainer container)
        {
            container.RegisterRelation<TModel, ItemViewModel, TViewModel>();
            RegisterDrawer<TViewModel, TDrawer>();
            return container;
        }
        public static IUFrameContainer RegisterGraphItem<TModel, TViewModel, TDrawer>()
        {
            Container.RegisterRelation<TModel, ViewModel, TViewModel>();
            RegisterDrawer<TViewModel, TDrawer>();
            return Container;
        }
        public static IUFrameContainer RegisterConnection<TSource, TTarget>(this IUFrameContainer container, bool oneToMany = true)
            where TSource : class, IConnectable
            where TTarget : class, IConnectable
        {
            if (oneToMany)
                container.RegisterInstance<IConnectionStrategy>(new OneToManyConnectionStrategy<TSource, TTarget>(), typeof(TSource).Name + "_" + typeof(TTarget).Name + "Connection");
            else
            {
                container.RegisterInstance<IConnectionStrategy>(new OneToOneConnectionStrategy<TSource, TTarget>(), typeof(TSource).Name + "_" + typeof(TTarget).Name + "Connection");
            }
            return container;
        }
        public static void RegisterFilterNode<TFilterData, TAllowedItem>(this IUFrameContainer container)
        {
            if (!AllowedFilterNodes.ContainsKey(typeof(TFilterData)))
            {
                AllowedFilterNodes.Add(typeof(TFilterData), new List<Type>());
            }
            AllowedFilterNodes[typeof(TFilterData)].Add(typeof(TAllowedItem));
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
        public static void RegisterFilterItem<TFilterData, TAllowedItem>(this IUFrameContainer container)
        {
            container.RegisterRelation<TFilterData, IDiagramNodeItem, TAllowedItem>();
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
        public static bool IsFilter(Type type)
        {
            return AllowedFilterNodes.ContainsKey(type);
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
        public static IGraphEditorSettings Settings
        {
            get { return _settings ?? (_settings = Container.Resolve<IGraphEditorSettings>()); }
            set { _settings = value; }
        }
        public static MouseEvent CurrentMouseEvent
        {
            // TODO
            get; set; }

        public static IKeyBinding[] KeyBindings { get; set; }

        public static IWindowManager WindowManager
        {
            get { return _windowManager ?? (_windowManager = Container.Resolve<IWindowManager>()); }
        }

        private static uFrameContainer _TypesContainer;
        private static IWindowManager _windowManager;

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

    }

    public class DefaultGraphSettings : IGraphEditorSettings
    {
        public bool UseGrid
        {
            get { return true; }
            set { }
        }

        public bool ShowHelp
        {
            get { return false; }
            set
            {
                
            }
        }
    }
    public class ScaffoldNode<TData> where TData : GenericNode
    {
        public class ViewModel : GenericNodeViewModel<TData>
        {

            public ViewModel(TData graphItemObject, DiagramViewModel diagramViewModel)
                : base(graphItemObject, diagramViewModel)
            {
            }
        }

        public class Drawer : GenericNodeDrawer<TData, ViewModel>
        {
            private GUIStyle _headerStyle;

            protected override GUIStyle HeaderStyle
            {
                get { return _headerStyle; }
            }

            public Drawer(ViewModel viewModel)
                : base(viewModel)
            {
                _headerStyle = InvertGraphEditor.Container.GetNodeConfig<TData>().NodeStyle;
            }
        }

        public class TypedItemViewModel : ScaffoldNodeTypedChildItem<TData>.ViewModel
        {
            public TypedItemViewModel(TData graphItemObject, DiagramNodeViewModel diagramViewModel) : base(graphItemObject, diagramViewModel)
            {
            }
        }

        public class TypedItemDrawer : ScaffoldNodeTypedChildItem<TData>.Drawer
        {
            public TypedItemDrawer(ScaffoldNodeTypedChildItem<TData>.ViewModel viewModel) : base(viewModel)
            {
            }
        }
        public class ItemViewModel : ScaffoldNodeChildItem<TData>.ViewModel
        {
            public ItemViewModel(TData graphItemObject, DiagramNodeViewModel diagramViewModel) : base(graphItemObject, diagramViewModel)
            {
            }
        }
        public class ItemDrawer : ScaffoldNodeChildItem<TData>.ViewModel
        {
            public ItemDrawer(TData graphItemObject, DiagramNodeViewModel diagramViewModel) : base(graphItemObject, diagramViewModel)
            {
            }
        }
    }
    public class ScaffoldNodeChildItem<TData> where TData : IDiagramNodeItem
    {
        public class ViewModel : GenericItemViewModel<TData>
        {

            public ViewModel(TData graphItemObject, DiagramNodeViewModel diagramViewModel)
                : base(graphItemObject, diagramViewModel)
            {
            }
        }

        public class Drawer : ItemDrawer
        {

            public Drawer(ViewModel viewModel)
                : base(viewModel)
            {

            }
        }
    }
    public class ScaffoldNodeTypedChildItem<TData> where TData : ITypedItem
    {
        public class ViewModel : TypedItemViewModel
        {

            public ViewModel(TData graphItemObject, DiagramNodeViewModel diagramViewModel)
                : base(graphItemObject, diagramViewModel)
            {
            }

            public override string TypeLabel
            {
                get { return Data.RelatedTypeName; }
            }
        }

        public class Drawer : ElementItemDrawer
        {

            public Drawer(ViewModel viewModel)
                : base(viewModel)
            {

            }
        }
    }
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
}

