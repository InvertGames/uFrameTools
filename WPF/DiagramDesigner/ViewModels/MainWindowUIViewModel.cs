using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.GraphDesigner.WPF;

namespace DiagramDesigner.Platform
{
    public class MainWindowUIViewModel : ViewModel, ICommandHandler
    {
        public MainWindowUIViewModel()
        {
       
        }

        private IEnumerable<IToolbarCommand> _toolbarCommands;

        public IEnumerable<IToolbarCommand> ToolbarCommands
        {
            get { return _toolbarCommands ?? (_toolbarCommands = InvertGraphEditor.Container.ResolveAll<IToolbarCommand>()); }
        }

        //if (_loadedTextGraphs == null)
        //{
        //    _loadedTextGraphs = new List<IGraphData>();
        //    foreach (var item in TextGraphs)
        //    {
        //        var graphData = JSON.Parse(item.text);

        //        var name = graphData["Name"].Value;
        //        var type = InvertApplication.FindType(graphData["Type"].Value);
        //        if (type == null)
        //        {
        //            Debug.LogError(string.Format("Couldn't find graph type {0}", graphData["Type"].Value));
        //        }
        //        var graph = Activator.CreateInstance(type) as IGraphData;
        //        graph.Name = item.name; 
        //        if (graph == null)
        //        { 

        //            Debug.LogError(string.Format("Couldn't load graph {0}", name));
        //            continue;
        //            ;
        //        }
        //        graph.Path = AssetDatabase.GetAssetPath(item);
        //        graph.DeserializeFromJson(graphData);
        //        Debug.Log(string.Format("Loaded graph {0}", name));
        //        _loadedTextGraphs.Add(graph);
        //    }
        //}
        //foreach (var item in _loadedTextGraphs)
        //{
        //    yield return item;
        //}

        public IProjectRepository CurrentProject
        {
            get
            {
                if (_currentProject == null)
                {
                    CreateProject();
                }
                return _currentProject;
            }
            set { _currentProject = value; }
        }

        private DiagramViewModel _currentDiagram;
        private ObservableCollection<ProjectGraphViewModel> _projectGraphs;
        private IProjectRepository _currentProject;

        public DiagramViewModel CurrentDiagram
        {
            get
            {
                return _currentDiagram;
            }
            set
            {
                _currentDiagram = value;
                if (value != null)
                {
                    value.Load();
                }
                OnPropertyChanged("CurrentDiagram");
            }
        }

        public void LoadDiagram(IGraphData data)
        {
            if (data == null) return;
            if (CurrentProject == null)
            {
                throw new Exception("Current Project must be set before loading a diagram");
            }
        
       
            if (ProjectGraphs.All(p => p.Graph != data))
            ProjectGraphs.Add(new ProjectGraphViewModel()
            {
                Graph = data,
                ShowCommand = new SimpleEditorCommand<DiagramViewModel>(model =>
                {
                    LoadDiagram(data);
                })
            });
            CurrentProject.CurrentGraph = data;
            
            var diagram = new DiagramViewModel(data,CurrentProject);
            CurrentDiagram = diagram;
            
        }

        public ObservableCollection<ProjectGraphViewModel> ProjectGraphs
        {
            get { return _projectGraphs ?? (_projectGraphs = new ObservableCollection<ProjectGraphViewModel>()); }
            set { _projectGraphs = value; }
        }

        public void CreateProject()
        {
            CurrentProject = new JsonProjectRepository(new FileInfo("TestProject.ufproj"),null,null )
            {
                Name = "New Test Project"
            };
            foreach (var item in CurrentProject.Graphs)
            {
                IGraphData item1 = item;
                ProjectGraphs.Add(new ProjectGraphViewModel() { Graph = item, ShowCommand = new SimpleEditorCommand<DiagramViewModel>(
                    (_) =>
                    {
                        LoadDiagram(item1);
                    })});
            }
            LoadDiagram(CurrentProject.Graphs.FirstOrDefault());
            foreach (var item in CurrentProject.Graphs)
            {
                
            }
        }

        public void CreateGraph()
        {
            if (CurrentProject == null)
            {
                throw new Exception("Current Project can't be null");
            }
            var graph = CurrentProject.CreateNewDiagram(typeof(PluginGraphData), new ShellPluginNode());
            LoadDiagram(graph);
        }

        public void SaveProject()
        {
            CurrentProject.Save();
        }

        public IEnumerable<object> ContextObjects
        {
            get
            {
                yield return this;
                if (CurrentDiagram != null)
                {
                    foreach (var item in CurrentDiagram.ContextObjects)
                    {
                        yield return item;
                    }
                }
            }
        }

        public void CommandExecuted(IEditorCommand command)
        {
            if (CurrentDiagram != null)
            {
                CurrentDiagram.Load();
            }
        }

        public void CommandExecuting(IEditorCommand command)
        {
            
        }
    }

    public class ClassGraph : GenericGraphData<ClassDiagramNode>
    {
        
    }
    public class ClassDiagramNode : GenericNode
    {
        public override string Name
        {
            get
            {
                if (Graph == null)
                {
                    return "Loading";
                }
                return Graph.Name;
            }
        }
    }
    public class ClassNode : GenericInheritableNode
    {
        [Section("Properties", SectionVisibility.Always)]
        public IEnumerable<ClassNodeProperty> Properties
        {
            get { return ChildItems.OfType<ClassNodeProperty>(); }
        }
    }

    public class ClassNodeProperty : GenericNodeChildItem
    {
        
    }
    public class ProjectGraphViewModel : ViewModel
    {
        public IGraphData Graph { get; set; }
        
        public string Name
        {
            get { return Graph.Name; }
        }

        public ICommand ShowCommand { get; set; }

    }
}