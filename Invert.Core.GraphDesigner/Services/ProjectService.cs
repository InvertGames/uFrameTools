using System;
using System.Linq;
using Invert.IOC;

namespace Invert.Core.GraphDesigner
{
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
}