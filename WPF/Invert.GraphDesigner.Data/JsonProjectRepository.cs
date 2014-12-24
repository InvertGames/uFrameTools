using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.GraphDesigner.WPF.Annotations;
using Invert.Json;

namespace Invert.GraphDesigner.WPF
{
    public class JsonProjectRepository : DefaultProjectRepository, IJsonObject, INotifyPropertyChanged
    {
        private List<IGraphData> _includedGraphs;
        private string _name;
        private IGraphData _currentGraph1;

        public JsonProjectRepository(FileInfo projectFileInfo, IGraphData currentGraph, IEnumerable<IGraphData> graphs)
        {
            ProjectFileInfo = projectFileInfo;
            CurrentGraph = currentGraph;
            Graphs = graphs;
            if (projectFileInfo.Exists)
            {
                Deserialize(JSON.Parse(File.ReadAllText(projectFileInfo.FullName)).AsObject,this);
                foreach (var graph in Directory.GetFiles(projectFileInfo.Directory.FullName,"*.graph"))
                {
                    var graphJson = JSON.Parse(File.ReadAllText(graph));
                    var type = InvertApplication.FindType(graphJson["Type"].Value);
                    var instance = Activator.CreateInstance(type) as InvertGraph;
                    if (instance == null) continue;
                    instance.Path = graph;
                    instance.DeserializeFromJson(graphJson);
                    instance.SetProject(this);
                    IncludedGraphs.Add(instance);
                    CurrentGraph = instance;
                    
                }
            }
        }

        public FileInfo ProjectFileInfo { get; set; }

        [JsonProperty]
        public override string LastLoadedDiagram { get; set; }

        [JsonProperty]
        public override string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        public override void Save()
        {
            base.Save();
            var jsonClass = new JSONClass();
            Serialize(jsonClass);
            File.WriteAllText(ProjectFileInfo.FullName,jsonClass.ToString());
            foreach (var graph in Graphs)
            {
                File.WriteAllText(graph.Path.Replace(".graph","") + ".graph", InvertGraph.Serialize(graph).ToString());
            }
        }

        public override void SaveDiagram(INodeRepository data)
        {
            
        }

        public override void RecordUndo(INodeRepository data, string title)
        {
           
        }

        public override void Refresh()
        {
           
        }

        public override IGraphData CreateNewDiagram(Type diagramType, IDiagramFilter defaultFilter = null)
        {
            var graph = diagramType == null ? Activator.CreateInstance(diagramType) as InvertGraph : new InvertGraph();
            graph.Name = string.Format("{0}{1}", diagramType.Name, IncludedGraphs.Count);
            if (defaultFilter != null)
            {
                graph.RootFilter = defaultFilter;
                defaultFilter.Name = graph.Name;
            }
          
         
            graph.SetProject(this);
            IncludedGraphs.Add(graph);
            Save();
            return graph;
        }

        public override IGraphData CurrentGraph
        {
            get { return _currentGraph1; }
            set
            {
                _currentGraph1 = value;
                OnPropertyChanged();
            }
        }

        public override IEnumerable<IGraphData> Graphs
        {
            get { return IncludedGraphs; }
            set
            {
                if (value != null)
                    IncludedGraphs = value.ToList();
            }
        }

        public List<IGraphData> IncludedGraphs
        {
            get { return _includedGraphs ?? (_includedGraphs = new List<IGraphData>()); }
            set { _includedGraphs = value; }
        }

        public void Serialize(JSONClass cls)
        {
            var includedGraphs = new JSONArray();
            foreach (var item in IncludedGraphs)
            {
                var itemCls = new JSONClass();
                itemCls["Path"] = item.Path;
                includedGraphs.Add(itemCls);
            }
            cls.Add("Graphs", includedGraphs);
            var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.IsDefined(typeof(JsonProperty))).ToArray();
            foreach (var property in properties)
            {
                
                this.SerializeProperty(property,cls);
            }
        }

        public void Deserialize(JSONClass cls, INodeRepository repository)
        {
            var properties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.IsDefined(typeof(JsonProperty))).ToArray();
            foreach (var property in properties)
            {
                this.DeserializeProperty(property,cls);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}