using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Invert.Common;
using Invert.Common.UI;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.uFrame;
using Invert.uFrame.Editor;
using UnityEditor;
using UnityEngine;
using Object = System.Object;


[Serializable]
public class ProjectRepository : DefaultProjectRepository, IProjectRepository, ISerializationCallbackReceiver
{
    [SerializeField, HideInInspector]
    private string[] _settingsKeys;
    [SerializeField, HideInInspector]
    private bool[] _settingsValues;

   

    public void OnBeforeSerialize()
    {
        if (SettingsBag == null) return;
        _settingsKeys = SettingsBag.Keys.ToArray();
        _settingsValues = SettingsBag.Values.ToArray();
    }

    public void OnAfterDeserialize()
    {
        if (_settingsKeys == null) return;
        SettingsBag.Clear();
        for (var i = 0; i < _settingsKeys.Length; i++)
        {
            if (i >= _settingsValues.Length)
            {
                break;
            }
            var key = _settingsKeys[i];
            var value = _settingsValues[i];
            SettingsBag.Add(key, value);

        }

        OpenTabs.RemoveAll(p => string.IsNullOrEmpty(p.GraphIdentifier));
    }

    private Dictionary<string, string> _derivedTypes;

    public override void MarkDirty(INodeRepository data)
    {
        if (data != null)
            EditorUtility.SetDirty(data as UnityEngine.Object);
    }

    protected string[] _diagramNames;

    [SerializeField, HideInInspector]
    protected List<ScriptableObject> _diagrams;


    public void RecacheAssets()
    {

    }

    //public Dictionary<string, string> GetProjectDiagrams()
    //{
    //    var items = new Dictionary<string, string>();
    //    foreach (var elementDesignerData in Diagrams.Where(p => p.GetType() == RepositoryFor))
    //    {
    //        var asset = AssetDatabase.GetAssetPath(elementDesignerData as UnityEngine.Object);

    //        items.Add(elementDesignerData.Name, asset);
    //    }
    //    return items;
    //}

    [NonSerialized] private List<IGraphData> _loadedTextGraphs;
    public override IGraphData CreateNewDiagram(Type diagramType, IDiagramFilter defaultFilter = null)
    {
        Selection.activeObject = this;
        var t = diagramType;
        var diagram = InvertGraphEditor.AssetManager.CreateAsset(t) as IGraphData;
        if (defaultFilter != null && diagram != null)
        {
            diagram.RootFilter = defaultFilter;
            var nodeItem = defaultFilter as IDiagramNode;
            if (nodeItem != null)
            {
                nodeItem.Graph = diagram;
            }
        }
        diagram.Version = InvertGraphEditor.CURRENT_VERSION_NUMBER.ToString();
        Diagrams.Add(diagram as ScriptableObject);
        EditorUtility.SetDirty(diagram as ScriptableObject);
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        Refresh();
        return diagram;
    }

    public override string Name
    {
        get
        {
            if (!InvertApplication.IsMainThread)
            {
                return _name;
            }
            return (_name = name);
        }
        set
        {
            name = value;
            _name = value;
        }
    }



    [SerializeField]
    private string _projectNamespace;

    public override string Namespace
    {
        get { return _projectNamespace; }
        set { _projectNamespace = value; }
    }

    [SerializeField]
    private List<OpenGraph> _currentTabs;

    public override List<OpenGraph> OpenTabs
    {
        get { return _currentTabs ?? (_currentTabs = new List<OpenGraph>()); }
        set { _currentTabs = value; }
    }

    [NonSerialized]
    private List<TextAsset> _textGraphs;

    private string _name;

    public override string LastLoadedDiagram
    {
        get { return EditorPrefs.GetString("UF_LastLoadedDiagram" + this.name, string.Empty); }
        set { EditorPrefs.SetString("UF_LastLoadedDiagram" + this.name, value); }
    }
    public List<ScriptableObject> Diagrams
    {
        get
        {
            if (_diagrams == null)
            {
                _diagrams = new List<ScriptableObject>();
            }
            return _diagrams;
        }
        set { _diagrams = value; }
    }


    public List<TextAsset> TextGraphs
    {
        get { return _textGraphs; }
        set { _textGraphs = value; }
    }

    public override IGraphData CurrentGraph
    {
        get
        {
            if (Diagrams == null) return null;
            if (_currentGraph == null)
            {
                if (!String.IsNullOrEmpty(LastLoadedDiagram))
                {
                    CurrentGraph = Enumerable.FirstOrDefault<ScriptableObject>(Diagrams, p => p != null && p.name == LastLoadedDiagram) as IGraphData;
                }
                if (_currentGraph == null)
                {
                    CurrentGraph = Enumerable.FirstOrDefault<ScriptableObject>(Diagrams) as IGraphData;
                }
            }
            return _currentGraph;
        }
        set
        {
            _currentGraph = value;
            if (value == null) return;
            var openGraph = Enumerable.FirstOrDefault<OpenGraph>(OpenGraphs, p => p.GraphName == value.Name);
            if (openGraph == null)
            {
                OpenTabs.Add(new OpenGraph()
                {
                    GraphName = value.Name,
                    GraphIdentifier = value.Identifier
                });
                Debug.Log("Opened Tab");
            }

            LastLoadedDiagram = value.Name;
        }
    }

    public override IEnumerable<IGraphData> Graphs
    {
        get
        {
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
            foreach (var item in Diagrams)
                yield return item as IGraphData; //Diagrams.Cast<IGraphData>();
        }
        set { Diagrams = value.Cast<ScriptableObject>().ToList(); }
    }


    public override void SaveDiagram(INodeRepository data)
    {
        if (data != null)
        {
            EditorUtility.SetDirty(data as UnityEngine.Object);
        }
        AssetDatabase.SaveAssets();
    }

    public override void RecordUndo(INodeRepository data, string title)
    {
        if (data != null)
            Undo.RecordObject(data as UnityEngine.Object, title);
    }

    public override void Refresh()
    {
        //var assets = uFrameEditor.GetAssets(typeof(GraphData));
        //Diagrams = assets.OfType<GraphData>().ToList();
        _loadedTextGraphs = null;
        CurrentGraph = null;
        _nodeItems = null;

        foreach (var diagram in Diagrams)
        {
            (diagram as IGraphData).Prepare();
        }

    }
}