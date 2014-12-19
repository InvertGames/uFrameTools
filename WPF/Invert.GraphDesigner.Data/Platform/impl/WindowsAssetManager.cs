using System;
using System.Collections.Generic;
using Invert.Core;

namespace DiagramDesigner.Platform
{
    public class WindowsAssetManager : IAssetManager
    {
        public object CreateAsset(Type type)
        {
            throw new NotImplementedException();
        }

        public object LoadAssetAtPath(string path, Type repositoryFor)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> GetAssets(Type type)
        {
            //if (type == typeof(IProjectRepository))
            //{
            //    foreach (var item in ProjectUtilities.LoadedProjects
            //        .Select(p => new VisualStudioProjectRepository(p))
            //        .ToArray())
            //    {
            //        foreach (var graph in item.Graphs)
            //        {
            //            graph.SetProject(item);
            //        }
            //        yield return item;
            //    }
            //}
            yield break;
        }
    }
}