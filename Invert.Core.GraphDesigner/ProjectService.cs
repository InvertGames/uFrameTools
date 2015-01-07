//using System.Collections.Generic;
//using System.Linq;

//namespace Invert.Core.GraphDesigner
//{
//    public class ProjectService : IFeature
//    {
//        private readonly IProjectEvents[] _eventHandlers;
//        private readonly IAssetManager _assetManager;
//        private IProjectRepository[] _projects;

//        public IProjectRepository[] Projects
//        {
//            get { return _projects ?? (_projects = QueryProjects().ToArray()); }
//            set { _projects = value; }
//        }

//        public IProjectRepository CurrentProject { get; set; }


//        public ProjectService(IProjectEvents[] eventHandlers, IAssetManager assetManager)
//        {
//            _eventHandlers = eventHandlers;
//            _assetManager = assetManager;
//        }

//        public IEnumerable<IProjectRepository> QueryProjects()
//        {
//            var projects = _assetManager.GetAssets(typeof(IProjectRepository)).Cast<IProjectRepository>().ToArray();
//            foreach (var project in projects)
//            {
//                if (!_projects.Contains(project))
//                {
//                    ProjectAdded(project);
//                }
//            }
//            foreach (var project in _projects)
//            {
//                if (!projects.Contains(project))
//                {
//                    ProjectRemoved(project);
//                }
//            }
//        }

//        private void ProjectAdded(IProjectRepository project)
//        {
//            foreach (var handler in _eventHandlers)
//            {
//                handler.ProjectLoaded(project);
//            }
//        }
//        private void ProjectRemoved(IProjectRepository project)
//        {
//            foreach (var handler in _eventHandlers)
//            {
//                handler.ProjectRemoved(project);
//            }
//        }

//        public void LoadProject(IProjectRepository project)
//        {
//            foreach (var eventHandler in this._eventHandlers)
//            {
//                eventHandler.ProjectLoaded(project);
//            }
//        }

//        public void UnloadProject(IProjectRepository project)
//        {
//            foreach (var eventHandler in this._eventHandlers)
//            {
//                eventHandler.ProjectUnloaded(project);
//            }
//        }

//        public void InvalidateProjects()
//        {
            
//        }
//    }
//}