using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.Core.GraphDesigner.Two;
using Invert.Data;
using Invert.IOC;

namespace Invert.Core.GraphDesigner
{
    public interface ICreateWorkspace
    {
        Workspace CreateWorkspace(string name);
    }

    public interface IRemoveWorkspace
    {
        void RemoveWorkspace(string name);
        void RemoveWorkspace(Workspace workspace);
    }

    public interface IOpenWorkspace
    {
        void OpenWorkspace(string name);
        void OpenWorkspace(Workspace workspace);
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
        ICreateWorkspace, 
        IRemoveWorkspace,
        IOpenWorkspace
    {
        public IEnumerable<Workspace> Workspaces
        {
            get { return Repository.All<Workspace>(); }
        }

        public Workspace CreateWorkspace(string name)
        {
            var workspace = Repository.Create<Workspace>();
            workspace.Name = name;
            Repository.Commit();
            return workspace;
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

        public void OpenWorkspace(string name)
        {
            OpenWorkspace(Workspaces.FirstOrDefault(p => p.Name == name));
        }

        public void OpenWorkspace(Workspace workspace)
        {
            if (workspace == CurrentWorkspace) return;
            CurrentWorkspace = workspace;
            InvertApplication.SignalEvent<IWorkspaceChanged>(_ => _.WorkspaceChanged(CurrentWorkspace));
        }
    }


}
