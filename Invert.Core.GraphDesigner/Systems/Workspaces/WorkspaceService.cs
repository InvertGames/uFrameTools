using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Invert.Core.GraphDesigner;
using Invert.Data;
using Invert.IOC;

namespace Invert.Core.GraphDesigner
{

    public interface IRemoveWorkspace
    {
        void RemoveWorkspace(string name);
        void RemoveWorkspace(Workspace workspace);
    }

    public interface IWorkspaceChanged
    {
        void WorkspaceChanged(Workspace workspace);
    }

    public class RepoService : DiagramPlugin
    {
        public IRepository Repository { get; set; }

        public override void Initialize(UFrameContainer container)
        {
            base.Initialize(container);

        }

        public override void Loaded(UFrameContainer container)
        {
            base.Loaded(container);
            Repository = container.Resolve<IRepository>();
        }
    }
    public class WorkspaceService : RepoService,
        IRemoveWorkspace,
        IContextMenuQuery,
        IToolbarQuery,
        IExecuteCommand<SelectWorkspaceCommand>,
        IExecuteCommand<SelectGraphCommand>,
        IExecuteCommand<OpenWorkspaceCommand>,
        IExecuteCommand<CreateWorkspaceCommand>,
        IExecuteCommand<RemoveWorkspaceCommand>
    {
        public IEnumerable<Workspace> Workspaces
        {
            get { return Repository.AllOf<Workspace>(); }
        }


        public void RemoveWorkspace(string name)
        {
            RemoveWorkspace(Workspaces.FirstOrDefault(p => p.Name == name));
        }

        public void RemoveWorkspace(Workspace workspace)
        {
            Repository.Remove(workspace);
        }

        public Workspace CurrentWorkspace { get; set; }
        public override void Loaded(UFrameContainer container)
        {
            base.Loaded(container);
            if (CurrentWorkspace == null && InvertGraphEditor.Prefs != null)
            {
                CurrentWorkspace = Workspaces.FirstOrDefault(p => p.Identifier == InvertGraphEditor.Prefs.GetString("LastLoadedWorkspace", string.Empty));
            }
            Configurations = container.ResolveAll<WorkspaceConfiguration>().ToDictionary(p => p.WorkspaceType);

        }

        public WorkspaceConfiguration CurrentConfiguration
        {
            get
            {
                if (Configurations == null || CurrentWorkspace == null) return null;
                return Configurations[CurrentWorkspace.GetType()];
            }
        }
        public Dictionary<Type, WorkspaceConfiguration> Configurations { get; set; }


        public void QueryContextMenu(ContextMenuUI ui, MouseEvent evt, object obj)
        {
           
        }

        public void Execute(SelectWorkspaceCommand command)
        {
            Signal<IShowContextMenu>(_ => _.Show(null, command));
        }

        public void Execute(SelectGraphCommand command)
        {
            Signal<IShowContextMenu>(_ => _.Show(null, command));
        }

        public void QueryToolbarCommands(ToolbarUI ui)
        {

            ui.AddCommand(new ToolbarItem()
            {
                Title = CurrentWorkspace == null ? "--Choose Workspace--" : CurrentWorkspace.Name,
                IsDropdown = true,
                Command = new SelectWorkspaceCommand(),
                Position = ToolbarPosition.Left
            });
        }

        public void Execute(OpenWorkspaceCommand command)
        {
            if (command.Workspace == CurrentWorkspace) return;
            CurrentWorkspace = command.Workspace;
            InvertGraphEditor.Prefs.SetString("LastLoadedWorkspace", command.Workspace.Identifier);
            Signal<IWorkspaceChanged>(_ => _.WorkspaceChanged(CurrentWorkspace));
        }

        public void Execute(CreateWorkspaceCommand command)
        {
            var workspace = Activator.CreateInstance(command.WorkspaceType) as Workspace;
            workspace.Name = command.Name;
            command.Result = workspace;
            Repository.Add(workspace);
            Execute(new OpenWorkspaceCommand()
            {
                Workspace = workspace
            });
        }

        public void Execute(RemoveWorkspaceCommand command)
        {
            Repository.Remove(command.Workspace);
        }
    }
    public class RemoveWorkspaceCommand : Command
    {
        public Workspace Workspace { get; set; }
    }
    public class OpenWorkspaceCommand : Command
    {
        public Workspace Workspace { get; set; }
    }
    public class CreateWorkspaceCommand : Command
    {
        public string Name { get; set; }
        public Workspace Result { get; set; }
        public Type WorkspaceType { get; set; }
    }
    public class OpenGraphCommand : Command
    {

    }

    public class WorkspaceConfiguration
    {
        private List<WorkspaceGraphConfiguration> _graphTypes;
        public string Title { get; set; }
        public string Description { get; set; }
        public Type WorkspaceType { get; set; }

        public List<WorkspaceGraphConfiguration> GraphTypes
        {
            get { return _graphTypes ?? (_graphTypes = new List<WorkspaceGraphConfiguration>()); }
            set { _graphTypes = value; }
        }

        public WorkspaceConfiguration(Type workspaceType, string title)
            : this(workspaceType, title, null)
        {
        }

        public WorkspaceConfiguration(Type workspaceType, string title, string description)
        {
            WorkspaceType = workspaceType;
            Title = title;
            Description = description;
        }

        public WorkspaceConfiguration(List<WorkspaceGraphConfiguration> graphTypes, string title, string description, Type workspaceType)
        {
            _graphTypes = graphTypes;
            Title = title;
            Description = description;
            WorkspaceType = workspaceType;
        }

        public WorkspaceConfiguration WithGraph<TGraphType>(string title, string description = null)
        {
            GraphTypes.Add(new WorkspaceGraphConfiguration()
            {
                Title = title,
                Description = description,
                GraphType = typeof(TGraphType)
            });
            return this;
        }
    }

    public static class ConfigurationExtensions
    {
        public static WorkspaceConfiguration AddWorkspaceConfig<TWorkspaceType>(this IUFrameContainer container, string title, string description = null)
        {
            var config = new WorkspaceConfiguration(typeof(TWorkspaceType), title, description);
            container.RegisterInstance(config, typeof(TWorkspaceType).Name);
            return config;
        }

        public static WorkspaceConfiguration WorkspaceConfig<TWorkspaceType>(this IUFrameContainer container)
        {
            return container.Resolve<WorkspaceConfiguration>(typeof(TWorkspaceType).Name);
        }
    }
    public class WorkspaceGraphConfiguration
    {
        public Type GraphType { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
