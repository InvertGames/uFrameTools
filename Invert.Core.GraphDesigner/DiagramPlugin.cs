using System;
using System.Collections.Generic;
using System.Linq;
using Invert.IOC;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public interface IPrefabNodeProvider
    {
        IEnumerable<QuickAddItem> PrefabNodes(INodeRepository nodeRepository);
    }

    public class QuickAddItem :IItem
    {
        private string _searchTag;

        public QuickAddItem(string @group, string title, Action<QuickAddItem> action)
        {
            Group = @group;
            Title = title;
            Action = action;
        }
        public DiagramViewModel Diagram { get; set; }
        public Vector2 MousePosition { get; set; }
        public IDiagramNodeItem Item { get; set; }
        public Action<QuickAddItem> Action { get; set; }

        public virtual string Title { get; set; }

        public string Group { get; set; }

        public string SearchTag
        {
            get { return _searchTag ?? Group + Title; }
            set { _searchTag = value; }
        }
    }
    public abstract class DiagramPlugin : CorePlugin, IDiagramPlugin
    {
        public void ListenFor<TEvents>() where TEvents : class
        {
            InvertApplication.ListenFor<TEvents>(this);
        }
        public override bool Enabled
        {
            get { return InvertGraphEditor.Prefs.GetBool("UFRAME_PLUGIN_" + this.GetType().Name, EnabledByDefault); }
            set { InvertGraphEditor.Prefs.SetBool("UFRAME_PLUGIN_" + this.GetType().Name, value); }
        }

        public override void Loaded(UFrameContainer container)
        {

        }

   
    }

    public abstract class Feature : CorePlugin
    {
        public override bool EnabledByDefault
        {
            get { return true; }
        }
        
        public override bool Required
        {
            get { return true; }
        }

        public override bool Enabled
        {
            get { return true; }
            set
            {
                
            }
        }
    }
    public interface ICommandEvents
    {
        void CommandExecuting(ICommandHandler handler, IEditorCommand command, object o);
        void CommandExecuted(ICommandHandler handler, IEditorCommand command, object o);
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

    public interface INodeItemEvents
    {
        void Deleted(IDiagramNodeItem node);
        void Hidden(IDiagramNodeItem node);
        void Renamed(IDiagramNodeItem node, string previousName, string newName);
    }
    public interface IProjectEvents
    {
        void ProjectLoaded(IProjectRepository project);
        void ProjectUnloaded(IProjectRepository project);
        void ProjectRemoved(IProjectRepository project);
        void ProjectChanged(IProjectRepository project);
        void ProjectsRefreshed(ProjectService service);
    }

    public class ProjectService : DiagramPlugin
    {
        private IProjectRepository[] _projects;
        
        private IProjectRepository _currentProject;
        public string LastLoadedProject
        {
            get
            {
                return InvertGraphEditor.Prefs.GetString("UF_LastLoadedProject", String.Empty);
            }
            set { InvertGraphEditor.Prefs.SetString("UF_LastLoadedProject", value); }
        }

        public IProjectRepository CurrentProject
        {
            get
            {
                if (_currentProject == null || _currentProject.Equals(null))
                {
                
                    if (!String.IsNullOrEmpty(LastLoadedProject))
                    {
                        CurrentProject = this.Projects.FirstOrDefault(p =>p != null && !p.Equals(null) && p.Name == LastLoadedProject);
                    }
                    if (_currentProject == null || _currentProject.Equals(null))
                    {
                        CurrentProject = this.Projects.FirstOrDefault(p => p != null && !p.Equals(null));
                    }
                }
                return _currentProject;
            }
            set
            {
                var changed = _currentProject != value;

                _currentProject = value;

                if (value != null && !value.Equals(null))
                {
                 
                   
                    LastLoadedProject = value.Name;
                    if (_currentProject.CurrentGraph != null)
                    _currentProject.CurrentGraph.SetProject(_currentProject);

                    if (changed)
                    {
                        InvertApplication.SignalEvent<IProjectEvents>(_ => _.ProjectChanged(value));
                    }
                }
            }
        }

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

        
        public IAssetManager AssetManager {
            get { return InvertGraphEditor.Container.Resolve<IAssetManager>(); }}

        public override void Initialize(UFrameContainer container)
        {
        }

        public override void Loaded(UFrameContainer container)
        {
            
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
                InvertApplication.SignalEvent<IProjectEvents>(p => p.ProjectLoaded(repository));
            }

            foreach (var projectRepository in _projects.Where(p => projects.All(x => x != p)))
            {
                var repository = projectRepository;
                InvertApplication.SignalEvent<IProjectEvents>(p => p.ProjectRemoved(repository));
            }

            _projects = projects;
        }

        public void RefreshProjects()
        {
            if (CurrentProject != null)
                CurrentProject.CurrentGraph = null;
            _currentProject = null;
            LoadProjects();
            InvertApplication.SignalEvent<IProjectEvents>(p => p.ProjectsRefreshed(this));
        }
    }

    
    public interface ISelectionEvents {
        void SelectionChanged(object[] value);
    }

    public interface IChangeTrackingEvents
    {
        void ChangeOccured(IChangeData data);
    }

    public interface IConnectionEvents
    {
        void ConnectionApplying(IGraphData graph, IConnectable output, IConnectable input);
        void ConnectionApplied(IGraphData graph, IConnectable output, IConnectable input);
    }
}