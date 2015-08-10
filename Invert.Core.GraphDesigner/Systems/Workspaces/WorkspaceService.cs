using System;
using System.Collections.Generic;
using System.Linq;
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
        IExecuteCommand<CreateWorkspaceCommand>
    {
        public IEnumerable<Workspace> Workspaces
        {
            get { return Repository.All<Workspace>(); }
        }

 
        public void RemoveWorkspace(string name)
        {
            RemoveWorkspace(Workspaces.FirstOrDefault(p=>p.Name == name));
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
        }



        public void QueryContextMenu(ContextMenuUI ui, MouseEvent evt, object obj)
        {
            var selectProject = obj as SelectWorkspaceCommand;
            if (selectProject != null)
            {
                foreach (var item in Workspaces)
                {
                    ui.AddCommand(new ContextMenuItem()
                    {
                        Title = item.Name,
                        Command = new OpenWorkspaceCommand()
                        {
                            Workspace = item
                        }
                    });
                }
                ui.AddCommand(new ContextMenuItem()
                {
                    Title = "Create New Workspace",
                    Command = new CreateWorkspaceCommand()
                    {
                        Name = "My Workspace"
                    }
                });
            }
        }

        public void Execute(SelectWorkspaceCommand command)
        {
            Signal<IShowContextMenu>(_ => _.Show(null, command));
        }

        public void Execute(SelectGraphCommand command)
        {
            Signal<IShowContextMenu>(_=>_.Show(null, command));
        }

        public void QueryToolbarCommands(ToolbarUI ui)
        {
            ui.AddCommand(new ToolbarItem()
            {
                Title = CurrentWorkspace.Name,
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
            var workspace = Repository.Create<Workspace>();
            workspace.Name = command.Name;
            Repository.Commit();
        }
    }

    public class OpenWorkspaceCommand : Command
    {
        public Workspace Workspace { get; set; }
    }
    public class CreateWorkspaceCommand : Command
    {
        public string Name { get; set; }
    }
    public class OpenGraphCommand : Command
    {
        
    }

}
