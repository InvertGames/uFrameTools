using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.GraphDesigner.WPF;

namespace Invert.Platform.Gwen
{
    public class WindowsAssetManager : IAssetManager
    {
        private Dictionary<Type, string> _assetFileMappings = new Dictionary<Type, string>()
        {
            {typeof(IProjectRepository), ".ufproject"}
        };

        private List<IAssetEvents> _listeners;

        public Dictionary<Type, string> AssetFileMappings
        {
            get { return _assetFileMappings; }
            set { _assetFileMappings = value; }
        }

        public WindowsAssetManager()
        {
            Watcher = new FileSystemWatcher(Application.StartupPath);
            Watcher.EnableRaisingEvents = true;
            Watcher.Created += delegate(object sender, FileSystemEventArgs args)
            {

            };
            Watcher.Changed += delegate(object sender, FileSystemEventArgs args)
            {

            };
            Watcher.Renamed += delegate(object sender, RenamedEventArgs args) { };
            Watcher.Deleted += delegate(object sender, FileSystemEventArgs args)
            {

            };
            Watcher.IncludeSubdirectories = true;
        }

        public FileSystemWatcher Watcher { get; set; }

        public object CreateAsset(Type type)
        {

            return null;
        }

        public object LoadAssetAtPath(string path, Type repositoryFor)
        {
            return null;
        }

        public JsonProjectRepository DummyRepository
        {
            get
            {
                var graphData = new PluginGraphData()
                {
                    GraphFileInfo = new FileInfo("MyGraph.json")
                };

                var project = new JsonProjectRepository(new FileInfo("MyProject.jsonproj"), new IGraphData[] {graphData});   
                graphData.Deserialize(Settings1.Default.TestJson);
                return project;
            }
        }
        public IEnumerable<object> GetAssets(Type type)
        {
            string extension;
            if (_assetFileMappings.TryGetValue(type, out extension))
            {
                yield return DummyRepository;
            }

            yield break;
        }

        public List<IAssetEvents> Listeners
        {
            get { return _listeners ?? (_listeners = new List<IAssetEvents>()); }
            set { _listeners = value; }
        }

        public Action Subscribe(IAssetEvents handler)
        {
            Listeners.Add(handler);
            return () => Unsubscribe(handler);
        }

        public void Unsubscribe(IAssetEvents handler)
        {
            Listeners.Remove(handler);
        }
    }
}