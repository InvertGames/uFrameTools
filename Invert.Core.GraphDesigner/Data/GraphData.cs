using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;
using UnityEngine;

public class UnityGraphData<TData> : ScriptableObject, IGraphData, ISerializationCallbackReceiver, IItem
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
                Name = this.name,

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
            _codePathStrategy.AssetPath = Path.GetDirectoryName(InvertGraphEditor.Platform.GetAssetPath(this));

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

    public void RemoveItem(IDiagramNodeItem nodeItem)
    {
        Graph.RemoveItem(nodeItem);
    }

    public void AddItem(IDiagramNodeItem item)
    {
        Graph.AddItem(item);
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

    public virtual string Name
    {
        get { return Regex.Replace(this.name, "[^a-zA-Z0-9_.]+", ""); ; }
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
            InvertApplication.Log(this.name + " has a problem.");

            InvertApplication.LogException(ex);
            Graph.Errors = true;
            Graph.Error = ex;
        }

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
        get { return Graph.Title; }
    }

    public string SearchTag
    {
        get { return Graph.SearchTag; }
    }



    public void SetProject(IProjectRepository project)
    {
        Graph.SetProject(project);
    }
}

public class GraphData : UnityGraphData<InvertGraph>
{
     
}

public class ConnectionData : IJsonObject
{

    private string _outputIdentifier;

    private string _inputIdentifier;

    public ConnectionData(string outputIdentifier, string inputIdentifier)
    {
        OutputIdentifier = outputIdentifier;
        InputIdentifier = inputIdentifier;
    }

    public ConnectionData()
    {
    }

    public string OutputIdentifier
    {
        get { return _outputIdentifier; }
        set { _outputIdentifier = value; }
    }

    public string InputIdentifier
    {
        get { return _inputIdentifier; }
        set { _inputIdentifier = value; }
    }

    public IGraphData Graph { get; set; }
    public IGraphItem Output { get; set; }
    public IGraphItem Input { get; set; }

    public void Serialize(JSONClass cls)
    {
        cls.Add("OutputIdentifier", OutputIdentifier ?? string.Empty);
        cls.Add("InputIdentifier", InputIdentifier ?? string.Empty);
    }

    public void Deserialize(JSONClass cls, INodeRepository repository)
    {
        if (cls["InputIdentifier"] != null)
        {
            InputIdentifier = cls["InputIdentifier"].Value;
        }
        if (cls["OutputIdentifier"] != null)
        {
            OutputIdentifier = cls["OutputIdentifier"].Value;
        }
    }
}