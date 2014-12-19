using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Json;
using Invert.uFrame.Editor;
using UnityEngine;

public class UnityGraphData<TData> : UnityGraphData, IGraphData, ISerializationCallbackReceiver, IItem
    where TData : InvertGraph, new()
{
    [SerializeField, HideInInspector]
    public string _jsonData;


    private TData Graph
    {
        get
        {
            return _graph ?? (_graph = new TData()
            {
                //Name = this.name,

            });
        }
        set { _graph = value; }
    }

    public UnityGraphData()
    {

    }

    [NonSerialized]
    private ICodePathStrategy _codePathStrategy;
    [NonSerialized]
    private TData _graph;

    public ICodePathStrategy CodePathStrategy
    {
        get
        {
            if (_codePathStrategy != null) return _codePathStrategy;

            _codePathStrategy =
                InvertGraphEditor.Container.Resolve<ICodePathStrategy>("Default");

            _codePathStrategy.Data = this;
            //var path = InvertGraphEditor.Platform.GetAssetPath(this);

            _codePathStrategy.AssetPath = Directory;

            return _codePathStrategy;
        }
        set { _codePathStrategy = value; }
    }

    public bool Errors
    {
        get { return Graph.Errors; }
        set { Graph.Errors = value; }
    }

    public Exception Error
    {
        get { return Graph.Error; }
        set { Graph.Error = value; }
    }

    public string Path
    {
        get
        {
            
            return InvertGraphEditor.Platform.GetAssetPath(this);
        }
        set { Graph.Path = value; }
    }

    public string Directory
    {
        get { return System.IO.Path.GetDirectoryName(Path); }

    }

    public IEnumerable<IGraphItem> AllGraphItems
    {
        get { return Graph.AllGraphItems; }
    }

    public IEnumerable<ConnectionData> Connections
    {
        get { return Graph.Connections; }
    }

    public FilterPositionData PositionData
    {
        set { Graph.PositionData = value; }
        get { return Graph.PositionData; }
    }

    public string Namespace { get; set; }

    public void RemoveItem(IDiagramNodeItem nodeItem)
    {
        Graph.RemoveItem(nodeItem);
    }

    public void AddItem(IDiagramNodeItem item)
    {
        Graph.AddItem(item);
    }

    public void Save()
    {
        //Graph.Save();
    }

    public void RecordUndo(INodeRepository data, string title)
    {
       
    }

    public void MarkDirty(INodeRepository data)
    {
        
    }

    public void SetItemLocation(IDiagramNode node, Vector2 position)
    {
        Graph.SetItemLocation(node,position);
    }

    public Vector2 GetItemLocation(IDiagramNode node)
    {
        return Graph.GetItemLocation(node);
    }

    public void HideNode(string identifier)
    {
        Graph.HideNode(identifier);
    }

    public IDiagramFilter CurrentFilter
    {
        get { return Graph.CurrentFilter; }
    }

    public string Identifier
    {
        set { Graph.Identifier = value; }
        get { return Graph.Identifier; }
    }

    public ElementDiagramSettings Settings
    {
        set { Graph.Settings = value; }
        get { return Graph.Settings; }
    }

    private string _name;
    public virtual string Name
    {
        get
        {
            if (!InvertApplication.IsMainThread)
            {
                return _name;
            }
            return (_name = Regex.Replace(this.name, "[^a-zA-Z0-9_.]+", "")); ;
        }
        set
        {
            this.name = value;
            _name = value;
        }
    }

    public string Version
    {
        set { Graph.Version = value; }
        get { return Graph.Version; }
    }

    public int RefactorCount
    {
        set { Graph.RefactorCount = value; }
        get { return Graph.RefactorCount; }
    }

    public virtual IEnumerable<IDiagramNode> NodeItems
    {
        get { return Graph.NodeItems; }
    }

    public FilterState FilterState
    {
        set { Graph.FilterState = value; }
        get { return Graph.FilterState; }
    }

    public virtual IDiagramFilter RootFilter
    {
        set { Graph.RootFilter = value; }
        get { return Graph.RootFilter; }
    }

    protected virtual IDiagramFilter CreateDefaultFilter()
    {
        return Graph.CreateDefaultFilter();
    }

    public void Initialize()
    {
        Graph.Initialize();
    }

    public void AddNode(IDiagramNode data)
    {
        Graph.AddNode(data);
    }

    public void RemoveNode(IDiagramNode enumData)
    {
        Graph.RemoveNode(enumData);
    }

    public void OnBeforeSerialize()
    {
        var backup = _jsonData;
        try
        {
            if (!Graph.Errors)
            {
                _jsonData = Graph.Serialize().ToString();
            }

        }
        catch (Exception ex)
        {
            _jsonData = backup;
            InvertApplication.LogException(ex);
        }

    }

    public void OnAfterDeserialize()
    {
        //Debug.Log("Deserialize");
        try
        {
            Graph.Deserialize(_jsonData);
            Graph.CodePathStrategy = this.CodePathStrategy;
            //Graph.Deserialize(_jsonData);

            Graph.CleanUpDuplicates();
            Graph.Errors = false;
        }
        catch (Exception ex)
        {
            InvertApplication.Log(_jsonData);
            InvertApplication.Log(this.Name + " has a problem.");

            InvertApplication.LogException(ex);
            Graph.Errors = true;
            Graph.Error = ex;
        }

    }

    public IProjectRepository Project
    {
        get { return Graph.Project; }
    }

    public void AddConnection(IGraphItem output, IGraphItem input)
    {
        Graph.AddConnection(output, input);
    }

    public void RemoveConnection(IGraphItem output, IGraphItem input)
    {
        Graph.RemoveConnection(output, input);
    }

    public void ClearOutput(IGraphItem output)
    {
        Graph.ClearOutput(output);
    }

    public void ClearInput(IGraphItem input)
    {
        Graph.ClearInput(input);
    }

    public string Title
    {
        get { return this.name; }
    }

    public string SearchTag
    {
        get { return this.name; }
    }



    public void SetProject(IProjectRepository project)
    {
        
        Graph.SetProject(project);
    }

    public void DeserializeFromJson(JSONNode graphData)
    {
        Graph.DeserializeFromJson(graphData);
    }
}