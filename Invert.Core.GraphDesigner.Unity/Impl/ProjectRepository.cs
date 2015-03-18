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
        //InvalidateCache();
        _allGraphItems = null;
        _nodeItems = null;

        OpenTabs.RemoveAll(p => string.IsNullOrEmpty(p.GraphIdentifier));
    }

    private Dictionary<string, string> _derivedTypes;

    public override void MarkDirty(INodeRepository data)
    {
        base.MarkDirty(data);
        if (data is InvertGraph)
        {
            foreach (var graph in Diagrams.OfType<UnityGraphData>())
            {
                if (graph.Graph == data)
                {
                    EditorUtility.SetDirty(graph);
                }
            }
        }
        if (data is UnityEngine.Object)
           EditorUtility.SetDirty((UnityEngine.Object) data);
    }

    protected string[] _diagramNames;

    [SerializeField, HideInInspector]
    protected List<ScriptableObject> _diagrams;

    [NonSerialized]
    private List<IGraphData> _loadedTextGraphs;

    /// <summary>
    /// Creates a new UnityGraphData object with its Graph property set to the diagramType
    /// </summary>
    /// <param name="diagramType">The type of diagram to create.</param>
    /// <param name="defaultFilter">The filter or root node to use for this diagram.  Leave null if you want to use the default (if you don't know just pass null)</param>
    /// <returns>The newly created UnityGraphData scriptable object wrapper with the internal graph as the diagramType specified.</returns>
    public override IGraphData CreateNewDiagram(Type diagramType, IDiagramFilter defaultFilter = null)
    {
        // Go ahead and select this project asset
        Selection.activeObject = this;
        // Attempt to create the asset with the asset manager
        var diagram = InvertGraphEditor.AssetManager.CreateAsset(typeof(UnityGraphData)) as UnityGraphData;
        if (diagram == null)
        {
            throw new Exception("Unity Graph Data couldn't be found.");
        }
        // Create the type of graph
        diagram.Graph = Activator.CreateInstance(diagramType) as IGraphData;
        if (diagram.Graph == null)
        {
            throw new Exception(string.Format("Graph of type {0} couldn't be created.", diagramType.Name));
        }
        // Create the default filter for this graph if specified
        if (defaultFilter != null && diagram != null)
        {
            // Set it as the root filter
            diagram.RootFilter = defaultFilter;

            var nodeItem = defaultFilter as IDiagramNode;
            if (nodeItem != null)
            {
                nodeItem.Graph = diagram;
            }
        }
        // Set the current version of the data to the current version of the grapheditor version
        diagram.Version = InvertGraphEditor.CURRENT_VERSION_NUMBER.ToString();
        // Add the graph to this list of graphs
        AddGraph(diagram);
        // Set the project for the diagram
        diagram.SetProject(this);
        // Mark the graph as dirty for unity to serialize them
        EditorUtility.SetDirty(diagram);
        EditorUtility.SetDirty(this);
        // Save everything
        AssetDatabase.SaveAssets();
        // Refresh the project
        Refresh();
        // Return the created UnityGraphData
        return diagram;
    }





    protected override void AddGraph(IGraphData graphData)
    {
        base.AddGraph(graphData);
        Diagrams.Add(graphData as ScriptableObject);
    }

    public override string Name
    {
        get
        {
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

    protected override List<OpenGraph> OpenTabs
    {
        get { return _currentTabs ?? (_currentTabs = new List<OpenGraph>()); }
        set { _currentTabs = value; }
    }

    [NonSerialized]
    private List<TextAsset> _textGraphs;

    private string _name;

    protected override string LastLoadedDiagram
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
            if (_currentGraph == null || object.ReferenceEquals(_currentGraph, null))
            {
                if (!String.IsNullOrEmpty(LastLoadedDiagram))
                {
                    CurrentGraph = Enumerable.FirstOrDefault<ScriptableObject>(Diagrams, p => p != null && !object.ReferenceEquals(p, null) && p.name == LastLoadedDiagram) as IGraphData;
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
            }

            LastLoadedDiagram = value.Name;
            //Debug.Log("SET PROJECT to " + this.name);
            //    value.SetProject(this);
            InvalidateCache();
            //foreach (var item in Graphs)
            //{
            //    item.SetProject(this);
            //}
            //foreach (var graph in PrecompiledGraphs)
            //{
            //    graph.SetProject(this);
            //}
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


            foreach (var n in PrecompiledGraphs)
            {
                //if (n.Project != (IProjectRepository)this)
                //{
                //    n.SetProject(this);
                //}
                //UnityEngine.Debug.Log("Loaded Project " + n.Identifier);
                yield return n;
            }


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
        _loadedTextGraphs = null;
        CurrentGraph = null;
        _nodeItems = null;

        foreach (var diagram in Diagrams)
        {
            (diagram as IGraphData).Prepare();


        }

    }
}