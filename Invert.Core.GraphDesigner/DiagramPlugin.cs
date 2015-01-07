using System;
using System.Collections.Generic;
using System.Linq;

namespace Invert.Core.GraphDesigner
{
    public abstract class DiagramPlugin : CorePlugin, IDiagramPlugin
    {
        public override bool Enabled
        {
            get { return InvertGraphEditor.Prefs.GetBool("UFRAME_PLUGIN_" + this.GetType().Name, EnabledByDefault); }
            set { InvertGraphEditor.Prefs.SetBool("UFRAME_PLUGIN_" + this.GetType().Name, value); }
        }

        public override void Loaded(uFrameContainer container)
        {

        }

        public virtual void CommandExecuted(ICommandHandler handler, IEditorCommand command)
        {

        }
    }

    public interface ICommandEvents
    {
        void CommandExecuting(ICommandHandler handler, IEditorCommand command);
        void CommandExecuted(ICommandHandler handler, IEditorCommand command);
    }

    public interface IGraphItemEvents
    {
        void GraphItemCreated(IGraphItem node);
        void GraphItemRemoved(IGraphItem node);
        void GraphItemRenamed(IGraphItem node);
    }

    public interface IGraphEvents
    {


        void GraphCreated(IProjectRepository project, IGraphData graph);
        void GraphLoaded(IProjectRepository project, IGraphData graph);
        void GraphDeleted(IProjectRepository project, IGraphData graph);

    }

    public interface IProjectEvents
    {
        void ProjectLoaded(IProjectRepository project);
        void ProjectUnloaded(IProjectRepository project);
        void ProjectRemoved(IProjectRepository project);
    }

    public class ProjectService : DiagramPlugin, ISubscribable<IProjectEvents>
    {
        private IProjectRepository[] _projects;
        private List<IProjectEvents> _listeners;

        public override bool Enabled
        {
            get { return true; }
            set { throw new System.NotImplementedException(); }
        }

        public override bool Required
        {
            get { return true; }
        }

        public IProjectRepository[] Projects
        {
            get
            {
                if (_projects == null) LoadProjects();
                return _projects;
            }
            private set { _projects = value; }
        }

        [Inject]
        public IAssetManager AssetManager { get; set; }

        public override void Initialize(uFrameContainer container)
        {

        }

        public override void Loaded(uFrameContainer container)
        {
            container.Inject(this);
            Listeners.AddRange(container.ResolveAll<IProjectEvents>());
        }

        private void LoadProjects()
        {
            var projects = AssetManager.GetAssets(typeof(IProjectRepository)).Cast<IProjectRepository>().ToArray();
            if (_projects == null)
            {
                _projects = new IProjectRepository[] { };
            }
            foreach (var projectRepository in projects.Where(p => _projects.All(x => x != p)))
            {
                var repository = projectRepository;
                this.Signal(p => p.ProjectLoaded(repository));
            }

            foreach (var projectRepository in _projects.Where(p => projects.All(x => x != p)))
            {
                var repository = projectRepository;
                this.Signal(p => p.ProjectRemoved(repository));
            }

            _projects = projects;
        }

        public List<IProjectEvents> Listeners
        {
            get { return _listeners ?? (_listeners = new List<IProjectEvents>()); }
            set { _listeners = value; }
        }

        public Action Subscribe(IProjectEvents handler)
        {
            Listeners.Add(handler);
            return () => Unsubscribe(handler);
        }

        public void Unsubscribe(IProjectEvents handler)
        {
            Listeners.Remove(handler);
        }

        public void RefreshProjects()
        {
            LoadProjects();
        }
    }
}