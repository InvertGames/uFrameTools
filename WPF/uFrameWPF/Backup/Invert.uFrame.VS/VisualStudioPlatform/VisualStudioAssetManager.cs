using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Microsoft.VisualStudio.OLE.Interop;

namespace Invert.uFrame.VS
{
    public class VisualStudioAssetManager : IAssetManager
    {
        private static List<VisualStudioProjectRepository> _cachedProjects = new List<VisualStudioProjectRepository>();

        public object CreateAsset(Type type)
        {
            return null;
        }

        public object LoadAssetAtPath(string path, Type repositoryFor)
        {
            return null;
        }

        private static List<VisualStudioProjectRepository> CachedProjects
        {
            get { return _cachedProjects; }
            set { _cachedProjects = value; }
        }

        public IEnumerable<object> GetAssets(Type type)
        {
            if (type == typeof (IProjectRepository))
            {
                var loadedProjects = ProjectUtilities.LoadedProjects;
                foreach (var project in ProjectUtilities.LoadedProjects)
                {
                    var cached = _cachedProjects.FirstOrDefault(p => p.Project == project);
                    if (cached != null)
                    {
                        yield return cached;
                    }
                    else
                    {
                        cached = new VisualStudioProjectRepository(project);
                        _cachedProjects.Add(cached);
                        foreach (var graph in cached.Graphs)
                        {
                            graph.SetProject(cached);
                        }
                        yield return cached;
                    }
                }
                _cachedProjects.RemoveAll(_ => loadedProjects.All(p => p != _.Project));
            }
            yield break;
        }
    }
}