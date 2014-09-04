using System.IO;
using System.Security.Cryptography;
using Invert.uFrame.Editor.Refactoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[SerializeField]
public class ElementDesignerData : ScriptableObject,  IElementDesignerData
{
    public IDiagramFilter CurrentFilter
    {
        get
        {
            if (FilterState.FilterStack.Count < 1)
            {
                return SceneFlowFilter;
            }
            return FilterState.FilterStack.Peek();
        }
    }

    public IProjectRepository Repository { get; set; }

    private string _identifier;

    public List<string> ExternalReferences { get; set; }

    public string Identifier
    {
        get { return string.IsNullOrEmpty(_identifier) ? (_identifier = Guid.NewGuid().ToString()) : _identifier; }
        set { _identifier = value; }
    }

    public ElementDiagramSettings Settings
    {
        get { return _settings ?? (_settings = new ElementDiagramSettings()); }
        set { _settings = value; }
    }


    [SerializeField, HideInInspector]
    private SceneFlowFilter _sceneFlowFilter = new SceneFlowFilter();
    public SceneFlowFilter SceneFlowFilter
    {
        get
        {
            return _sceneFlowFilter;
        }
        set { _sceneFlowFilter = value; }
    }


    [SerializeField, HideInInspector]
    private List<EnumData> _enums = new List<EnumData>();

    //[NonSerialized]
    //private Stack<IDiagramFilter> _filterStack;

    //[SerializeField, HideInInspector]
    //private List<ImportedElementData> _importedElements = new List<ImportedElementData>();

   
    [NonSerialized]
    private List<IDiagramLink> _links = new List<IDiagramLink>();

    //[SerializeField, HideInInspector]
    //private List<string> _persistedFilterStack = new List<string>();

    //[SerializeField, HideInInspector]
    //private List<PluginData> _pluginItems = new List<PluginData>();

    [SerializeField, HideInInspector]
    private List<SceneManagerData> _SceneManagers = new List<SceneManagerData>();

 
    [SerializeField]
    private List<SubSystemData> _subSystems = new List<SubSystemData>();


    [SerializeField, HideInInspector]
    private string _version;

    [SerializeField, HideInInspector]
    private List<ViewComponentData> _viewComponents = new List<ViewComponentData>();

  

    [SerializeField, HideInInspector]//, HideInInspector]
    private List<ElementData> _viewModels = new List<ElementData>();

    [SerializeField, HideInInspector]
    private List<ViewData> _views = new List<ViewData>();

    [SerializeField]
    private ElementDiagramSettings _settings;
    [SerializeField]
    private FilterState _filterState = new FilterState();


    public IEnumerable<IDiagramNode> AllDiagramItems
    {
        get
        {
            return
                ViewModels.Cast<IDiagramNode>()
                    //.Concat(ImportedElements.Cast<IDiagramNode>())
                    .Concat(Enums.Cast<IDiagramNode>())
                    .Concat(Views.Cast<IDiagramNode>())
                    .Concat(SceneManagers.Cast<IDiagramNode>())
                    .Concat(SubSystems.Cast<IDiagramNode>())
                    .Concat(ViewComponents.Cast<IDiagramNode>()
                );
        }
    }

   

    public List<EnumData> Enums
    {
        get { return _enums; }
        set { _enums = value; }
    }




    public List<IDiagramLink> Links
    {
        get { return _links; }
        set { _links = value; }
    }

    public string Name
    {
        get
        {
            return Regex.Replace(name, "[^a-zA-Z0-9_.]+", "");
        }
    }

    public int RefactorCount { get; set; }

    public List<Refactorer> GetRefactorings()
    {
        return
            AllDiagramItems.OfType<IRefactorable>()
                .SelectMany(p => p.Refactorings)
                .Concat(AllDiagramItems.SelectMany(p => p.Items).OfType<IRefactorable>().SelectMany(p => p.Refactorings))
                .ToList();
    }

    public List<SceneManagerData> SceneManagers
    {
        get { return _SceneManagers; }
        set { _SceneManagers = value; }
    }

  
    public List<SubSystemData> SubSystems
    {
        get { return _subSystems; }
        set { _subSystems = value; }
    }

    public string Version
    {
        get { return _version; }
        set { _version = value; }
    }


    public List<ViewComponentData> ViewComponents
    {
        get { return _viewComponents; }
        set { _viewComponents = value; }
    }


    public List<ElementData> ViewModels
    {
        get { return _viewModels; }
        set { _viewModels = value; }
    }

    public List<ViewData> Views
    {
        get { return _views; }
        set { _views = value; }
    }

    public IEnumerable<IDiagramNode> NodeItems { get; set; }

    public FilterState FilterState
    {
        get { return _filterState ?? (_filterState = new FilterState()); }
        set { _filterState = value; }
    }

    public void Initialize()
    {
        //if (FilterState.FilterStack.Count < 1)
        //{
        //    FilterState.FilterStack.Push(SceneFlowFilter);
        //}
        FilterState.Reload(this);
    }

    public void AddNode(IDiagramNode data)
    {
        if (data != null)
        {
            data.IsCollapsed = true;
        }
        TryNode<ElementData>(data,n=>ViewModels.Add(n));
        TryNode<EnumData>(data,n=>Enums.Add(n));
        TryNode<SubSystemData>(data,n=>SubSystems.Add(n));
        TryNode<ViewData>(data,n=>Views.Add(n));
        TryNode<ViewComponentData>(data,n=>ViewComponents.Add(n));
        TryNode<SceneManagerData>(data,n=>SceneManagers.Add(n));
    }

    public void TryNode<TNodeData>(IDiagramNode node, Action<TNodeData> action) where TNodeData : class, IDiagramNode
    {
        var element = node as TNodeData;
        if (element != null)
        {
            action(element);
        }
        
    }
    public void RemoveNode(IDiagramNode data)
    {
        TryNode<ElementData>(data, n => ViewModels.Remove(n));
        TryNode<EnumData>(data, n => Enums.Remove(n));
        TryNode<SubSystemData>(data, n => SubSystems.Remove(n));
        TryNode<ViewData>(data, n => Views.Remove(n));
        TryNode<ViewComponentData>(data, n => ViewComponents.Remove(n));
        TryNode<SceneManagerData>(data, n => SceneManagers.Remove(n));
    }
}