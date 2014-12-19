using System;
using System.Collections.Generic;
using System.Linq;
using Invert.Core;
using Invert.Core.GraphDesigner;

namespace Invert.uFrame.VS
{
    public class VisualStudioAssetManager : IAssetManager
    {
        public object CreateAsset(Type type)
        {
            return null;
        }

        public object LoadAssetAtPath(string path, Type repositoryFor)
        {
            return null;
        }

        public IEnumerable<object> GetAssets(Type type)
        {
            if (type == typeof (IProjectRepository))
            {
                foreach (var item in ProjectUtilities.LoadedProjects
                    .Select(p => new VisualStudioProjectRepository(p))
                    .ToArray())
                {
                    foreach (var graph in item.Graphs)
                    {
                        graph.SetProject(item);
                    }
                    yield return item;
                }
            }
            yield break;
        }
    }
}