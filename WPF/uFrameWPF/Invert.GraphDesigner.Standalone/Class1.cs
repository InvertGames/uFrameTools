using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.GraphDesigner.Standalone.Platform;
using Invert.GraphDesigner.WPF;
using UnityEngine;

namespace Invert.GraphDesigner.Standalone
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }

    public class GwenPlugin : DiagramPlugin
    {
        static GwenPlugin()
        {
            InvertGraphEditor.Prefs = new WindowsPrefs();
        }

        public override void Initialize(uFrameContainer container)
        {
            container.RegisterInstance<IStyleProvider>(new GwenStyleProvider());
            container.RegisterInstance<IAssetManager>(new WindowsAssetManager());
            container.RegisterInstance<IGraphEditorSettings>(new DefaultGraphSettings());
            container.Register<ContextMenuUI, WindowsContextMenu>();
        }
    }

    public class WindowsContextMenu : ContextMenuUI
    {
        public static ContextMenuStrip ContextMenu { get; set; }
        public static Control Control { get; set; }
        public override void AddSeparator(string empty)
        {
            base.AddSeparator(empty);
            Commands.Add(null);
        }

        public override void Go()
        {
            base.Go();
             
            ContextMenu.Items.Clear();
            foreach (var item in this.Commands)
            {
                if (item == null)
                {
                    ContextMenu.Items.Add(new ToolStripSeparator());
                    continue;
                }
                var obj = Handler.ContextObjects.FirstOrDefault(p => item.For.IsAssignableFrom(p.GetType()));

                var item1 = item;
                var dynamicCommand = item as IDynamicOptionsCommand;
                if (dynamicCommand != null)
                {
                   
                    if (obj == null) continue;

                    foreach (var option in dynamicCommand.GetOptions(obj))
                    {
                        var option1 = option;
                        AddByPath(option.Name, () =>
                        {
                            dynamicCommand.SelectedOption = option1;
                            InvertGraphEditor.ExecuteCommand(item1);
                        });
                    }
                }
                else
                {
                    if (item.CanPerform(obj) == null)
                    {
                        AddByPath(item.Path, () =>
                        {
                            InvertGraphEditor.ExecuteCommand(item1);
                        });
                    }
                }
               
            }
            if (InvertGraphEditor.DesignerWindow.DiagramViewModel.LastMouseEvent != null)
            {
                var mousePosition = InvertGraphEditor.DesignerWindow.DiagramViewModel.LastMouseEvent.MousePosition;
                ContextMenu.Show(Control, new Point(Mathf.RoundToInt(mousePosition.x)
                    , Mathf.RoundToInt(mousePosition.y)));
            }
            
        }

        public void AddByPath(string commandPath, Action execute)
        {
            var path = commandPath.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            ToolStripMenuItem menuItem = null;
            var i = 0;
            for (int index = 0; index < path.Length; index++, i++)
            {
                var item = path[index];
                if (menuItem != null)
                {
                    var find = menuItem.DropDownItems.Find(item, true).FirstOrDefault() as ToolStripMenuItem;
                    if (find == null)
                    {
                        break;
                    }
                    menuItem = find;
                }
                else
                {
                    var find = ContextMenu.Items.Find(item, true).FirstOrDefault() as ToolStripMenuItem;
                    if (find == null) break;
                    menuItem = find;
                }
            }

            for (int index = i; index < path.Length; index++)
            {
                var item = path[index];
                var oldMenuItem = menuItem;

                menuItem = new ToolStripMenuItem(item, null, (s, e) => { execute(); })
                {
                    Name = item
                };
                if (oldMenuItem != null)
                {
                    oldMenuItem.DropDownItems.Add(menuItem);
                }
                else
                {
                    ContextMenu.Items.Add(menuItem);
                }
            }
        }
       
    }

    public interface IAssetEvents
    {

    }
    public class WindowsAssetManager : IAssetManager, ISubscribable<IAssetEvents>
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
