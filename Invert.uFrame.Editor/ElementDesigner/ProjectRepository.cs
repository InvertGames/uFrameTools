using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Invert.Common;
using Invert.Common.UI;
using Invert.uFrame;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.Refactoring;
using UnityEditor;
using UnityEngine;
using Object = System.Object;
[CustomEditor(typeof(ProjectRepository))]
public class ProjectRepositoryInspector : Editor
{
    private TypeMapping[] _generators;

    public ProjectRepository Target
    {
        get { return target as ProjectRepository; }
    }

    public TypeMapping[] CodeGenerators
    {
        get { return _generators ?? (_generators = uFrameEditor.Container.Mappings.Where(p=>p.From == typeof(DesignerGeneratorFactory)).ToArray()); }
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        foreach (var a in CodeGenerators)
        {
            GUIHelpers.DoTriggerButton(new UFStyle(a.To.Name, UBStyles.IncludeTriggerBackgroundStyle));

        }

    }
}
public class ProjectRepository : ScriptableObject, IProjectRepository
{
    public Vector2 GetItemLocation(IDiagramNode node)
    {
        //if (!CurrentDiagram.PositionData.HasPosition(CurrentFilter, node))
        //{
        //    foreach (var diagram in Diagrams)
        //    {
        //        if (diagram.PositionData.HasPosition(CurrentFilter, node))
        //        {
        //            return diagram.PositionData[CurrentFilter,node];
        //        }
        //    }
        //}
        return CurrentDiagram.PositionData[CurrentFilter, node];
    }

    public void SetItemLocation(IDiagramNode node, Vector2 position)
    {
        CurrentDiagram.PositionData[CurrentFilter, node] = position;
    }

    private Dictionary<string, string> _derivedTypes;

    public void MarkDirty(INodeRepository data)
    {
        EditorUtility.SetDirty(data as UnityEngine.Object);
    }

    protected string[] _diagramNames;

    [SerializeField]
    protected List<JsonElementDesignerData> _diagrams;

    [SerializeField]
    private string _outputDirectory;

    private JsonElementDesignerData _currentDiagram;

    public void RecacheAssets()
    {
        
    }

    public Dictionary<string, string> GetProjectDiagrams()
    {
        var items = new Dictionary<string, string>();
        foreach (var elementDesignerData in Diagrams.Where(p=>p.GetType() == RepositoryFor))
        {
            var asset = AssetDatabase.GetAssetPath(elementDesignerData as UnityEngine.Object);
            
            items.Add(elementDesignerData.Name, asset);
        }
        return items;
    }

    public IElementDesignerData CreateNewDiagram()
    {
        Selection.activeObject = this;
        var diagram = UFrameAssetManager.CreateAsset<JsonElementDesignerData>();
        Diagrams.Add(diagram);
        CurrentDiagram = diagram;
        Refresh();
        return diagram;
    }
    public virtual Type RepositoryFor
    {
        get { return typeof(JsonElementDesignerData); }
    }

    public string Name
    {
        get { return name; }
    }


    private IDiagramNode[] _nodeItems;
    [SerializeField]
    private GeneratorSettings _generatorSettings;

    public IEnumerable<IDiagramNode> NodeItems
    {
        get
        {
            return _nodeItems ?? (_nodeItems = AllNodeItems.ToArray());
        }
    }

    public IEnumerable<IDiagramNode> AllNodeItems
    {
        get
        {
            foreach (var item in Diagrams)
            {
                foreach (var node in item.NodeItems)
                {
                    yield return node;
                }
            }
        }
    }
    public ElementDiagramSettings Settings
    {
        get
        {
            return CurrentDiagram.Settings;
        }
    }

    public void AddNode(IDiagramNode data)
    {
        _nodeItems = null;
        CurrentDiagram.AddNode(data);

    }

    public void RemoveNode(IDiagramNode enumData)
    {
        _nodeItems = null;
        CurrentDiagram.RemoveNode(enumData);
    }

    public IDiagramFilter CurrentFilter
    {
        get
        {
            return CurrentDiagram.CurrentFilter;
        }
    }

    public FilterPositionData PositionData
    {
        get { return CurrentDiagram.PositionData; }
    }

    public string LastLoadedDiagram
    {
        get { return EditorPrefs.GetString("UF_LastLoadedDiagram" + this.name, string.Empty); }
        set { EditorPrefs.SetString("UF_LastLoadedDiagram" + this.name, value); }
    }
    public List<JsonElementDesignerData> Diagrams
    {
        get
        {
            if (_diagrams == null)
            {
               _diagrams = new List<JsonElementDesignerData>();
            }
            return _diagrams;
        }
        set { _diagrams = value; }
    }

    public GeneratorSettings GeneratorSettings
    {
        get { return _generatorSettings; }
        set { _generatorSettings = value; }
    }

    public JsonElementDesignerData CurrentDiagram
    {
        get
        {
            if (_currentDiagram == null)
            {
                if (!String.IsNullOrEmpty(LastLoadedDiagram))
                {
                    _currentDiagram = Diagrams.FirstOrDefault(p => p.name == LastLoadedDiagram);
                }
                if (_currentDiagram == null)
                {
                    _currentDiagram = Diagrams.FirstOrDefault();
                }
            }
            return _currentDiagram;
        }
        set { _currentDiagram = value; }
    }

    public string OutputDirectory
    {
        get { return _outputDirectory; }
        set { _outputDirectory = value; }
    }

    public IElementDesignerData LoadDiagram(string path)
    {
        var data = AssetDatabase.LoadAssetAtPath(path, RepositoryFor) as JsonElementDesignerData;
        if (data == null)
        {
            return null;
        }
        CurrentDiagram = data;
        return data;
    }

    public void SaveDiagram(INodeRepository data)
    {
        EditorUtility.SetDirty(data as UnityEngine.Object);
        AssetDatabase.SaveAssets();
    }

    public void RecordUndo(INodeRepository data, string title)
    {
        Undo.RecordObject(data as UnityEngine.Object, title);
    }

    public void FastUpdate()
    {

    }

    public void FastSave()
    {

    }

    public virtual void Refresh()
    {
        //var assets = uFrameEditor.GetAssets(RepositoryFor);
        //Diagrams = assets.OfType<JsonElementDesignerData>().ToList();

        foreach (var diagram in Diagrams)
        {
            diagram.Prepare();
        }
        
    }

    public void HideNode(string identifier)
    {
        CurrentDiagram.PositionData.Remove(CurrentDiagram.CurrentFilter, identifier);
    }
}

public interface INamespaceProvider
{
    string GetNamespace(IDiagramNode node);
}

[Serializable]
public class DefaultNamespaceProvider : INamespaceProvider
{
    [SerializeField]
    private string _rootNamespace;

    public string RootNamespace
    {
        get { return _rootNamespace; }
        set { _rootNamespace = value; }
    }

    public string GetNamespace(IDiagramNode node)
    {
        return RootNamespace;
    }
}

public interface ICodePathStrategy
{
    /// <summary>
    /// The root path to the diagram file
    /// </summary>
    string AssetPath { get; set; }

    /// <summary>
    /// Where behaviours are stored
    /// </summary>
    string BehavioursPath { get; }

    /// <summary>
    /// Where scenes are stored
    /// </summary>
    string ScenesPath { get; }

    IElementDesignerData Data { get; set; }

    /// <summary>
    /// The relative path to the controller designer file
    /// </summary>
    string GetControllersFileName(string name);

    /// <summary>
    /// The relative path to the views designer file
    /// </summary>
    string GetViewsFileName(string name);

    /// <summary>
    /// The relative path to the view-models designer file
    /// </summary>
    string GetViewModelsFileName(string name);

    string GetEditableViewFilename(ViewData nameAsView);
    string GetEditableViewComponentFilename(ViewComponentData name);
    string GetEditableSceneManagerFilename(SceneManagerData nameAsSceneManager);
    string GetEditableSceneManagerSettingsFilename(SceneManagerData nameAsSettings);
    string GetEditableControllerFilename(ElementData controllerName);
    string GetEditableViewModelFilename(ElementData nameAsViewModel);
    string GetEnumsFilename(EnumData name);

    void MoveTo(GeneratorSettings settings, ICodePathStrategy strategy, string name, ElementsDesigner designerWindow);
    string GetSceneManagersFilename(string name);
}

public class DefaultCodePathStrategy : ICodePathStrategy
{
    public IElementDesignerData Data { get; set; }

    public string AssetPath { get; set; }

    public virtual string BehavioursPath
    {
        get { return Path.Combine(AssetPath, "Behaviours"); }
    }
    public virtual string ScenesPath
    {
        get { return Path.Combine(AssetPath, "Scenes"); }
    }

    public virtual string GetControllersFileName(string name)
    {
        return name + "Controllers.designer.cs";
    }

    public virtual string GetViewsFileName(string name)
    {
        return name + "Views.designer.cs";
    }

    public virtual string GetViewModelsFileName(string name)
    {
        return name + ".designer.cs";
    }

    public virtual string GetEditableViewFilename(ViewData nameAsView)
    {
        return Path.Combine("Views", nameAsView.NameAsView + ".cs");
    }

    public virtual string GetEditableViewComponentFilename(ViewComponentData name)
    {
        return Path.Combine("ViewComponents", name.Name + ".cs");
    }

    public virtual string GetEditableSceneManagerFilename(SceneManagerData nameAsSceneManager)
    {
        return Path.Combine("SceneManagers", nameAsSceneManager.NameAsSceneManager + ".cs");
    }

    public virtual string GetEditableSceneManagerSettingsFilename(SceneManagerData nameAsSettings)
    {
        return Path.Combine("SceneManagers", nameAsSettings.NameAsSettings + ".cs");
    }

    public virtual string GetEditableControllerFilename(ElementData controllerName)
    {
        return Path.Combine("Controllers", controllerName.NameAsController + ".cs");
    }

    public virtual string GetEditableViewModelFilename(ElementData nameAsViewModel)
    {
        return Path.Combine("ViewModels", nameAsViewModel.NameAsViewModel + ".cs");
    }

    public virtual string GetEnumsFilename(EnumData name)
    {
        return GetViewModelsFileName(name.Data.Name);
    }

    public virtual void MoveTo(GeneratorSettings settings, ICodePathStrategy strategy, string name, ElementsDesigner designerWindow)
    {
        var sourceFiles = uFrameEditor.GetAllFileGenerators(settings,this).ToArray();
        strategy.Data = Data;
        strategy.AssetPath = AssetPath;
        var targetFiles = uFrameEditor.GetAllFileGenerators(settings, strategy).ToArray();

        if (sourceFiles.Length == targetFiles.Length)
        {
            // Attempt to move every file
            ProcessMove(strategy, name, sourceFiles, targetFiles);
        }
        else
        {
            // Attempt to move non designer files
           // var designerFiles = sourceFiles.Where(p => p.Filename.EndsWith("designer.cs"));
            sourceFiles = sourceFiles.Where(p => !p.Filename.EndsWith("designer.cs")).ToArray();
            targetFiles = targetFiles.Where(p => !p.Filename.EndsWith("designer.cs")).ToArray();
            if (sourceFiles.Length == targetFiles.Length)
            {
                ProcessMove(strategy,name,sourceFiles,targetFiles);
                //// Remove all designer files
                //foreach (var designerFile in designerFiles)
                //{
                //    File.Delete(System.IO.Path.Combine(AssetPath, designerFile.Filename));
                //}
                //var saveCommand = uFrameEditor.Container.Resolve<IToolbarCommand>("SaveCommand");
                //saveCommand.Execute();
            }
        }
        
    }

    public string GetSceneManagersFilename(string name)
    {
        return name + "SceneManagers.designer.cs";
    }

    protected virtual void ProcessMove(ICodePathStrategy strategy, string name, CodeFileGenerator[] sourceFiles,
        CodeFileGenerator[] targetFiles)
    {
        for (int index = 0; index < sourceFiles.Length; index++)
        {
            var sourceFile = sourceFiles[index];
            var targetFile = targetFiles[index];

            var sourceFileInfo = new FileInfo(System.IO.Path.Combine(AssetPath, sourceFile.Filename));
            var targetFileInfo = new FileInfo(System.IO.Path.Combine(AssetPath, targetFile.Filename));
            if (sourceFileInfo.FullName == targetFileInfo.FullName) continue;
            if (!sourceFileInfo.Exists) continue;
            EnsurePath(sourceFileInfo);
            if (targetFileInfo.Exists) continue;
            EnsurePath(targetFileInfo);
            
            var sourceAsset = "Assets" + sourceFileInfo.FullName.Replace("\\", "/").Replace(Application.dataPath, "").Replace("\\", "/");
            var targetAsset = "Assets" + targetFileInfo.FullName.Replace("\\", "/").Replace(Application.dataPath, "").Replace("\\", "/");
            uFrameEditor.Log(string.Format("Moving file {0} to {1}",sourceAsset, targetAsset));
            AssetDatabase.MoveAsset(sourceAsset, targetAsset);
        }
 
        Data.Settings.CodePathStrategy = strategy;
        Data.Settings.CodePathStrategyName = name;

        EditorUtility.SetDirty(Data as UnityEngine.Object);
        AssetDatabase.SaveAssets();
        EditorApplication.SaveAssets();
        AssetDatabase.Refresh();
        ////Clean up old directories
        //foreach (var sourceFile in sourceFiles)
        //{
        //    var sourceFileInfo = new FileInfo(System.IO.Path.Combine(AssetPath, sourceFile.Filename));
        //    if (sourceFileInfo.Directory != null)
        //    {
        //        if (!sourceFileInfo.Directory.Exists) continue;

        //        var directories = sourceFileInfo.Directory.GetDirectories("*", SearchOption.AllDirectories);
        //        foreach (var directory in directories)
        //        {
        //            if (directory.GetFiles("*").Count(x => x.Extension != ".meta" && x.Extension != "meta") == 0)
        //            {
        //                directory.Delete(true);
        //                Debug.Log("Removed Directory " + directory.FullName);
        //            }
        //        }
        //    }
        //}
        //AssetDatabase.Refresh();
    }

    protected void EnsurePath(FileInfo fileInfo)
    {
        
// Get the path to the directory
        var directory = System.IO.Path.GetDirectoryName(fileInfo.FullName);
        // Create it if it doesn't exist
        if (directory != null && !Directory.Exists(directory))
        {
            
            Directory.CreateDirectory(directory);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }
    }
}

public class SubSystemPathStrategy : DefaultCodePathStrategy
{
    public IDiagramFilter FindFilter(IDiagramNode node)
    {
        var allFilters = Data.GetFilters();
        foreach (var diagramFilter in allFilters)
        {
            if (node != diagramFilter &&  node.Data.PositionData.HasPosition(diagramFilter,node))
            {
                return diagramFilter;
            }
        }
        return null;
    }

    public SubSystemData FindSubsystem(IDiagramNode node)
    {
        var filter = FindFilter(node);
        if (filter == node) return null;
        if (filter == null) return null;

        while (!(filter is SubSystemData))
        {
            // Convert to node
            var filterNode = filter as IDiagramNode;
            
            // If its not a node at this point it must be hidden
            if (filterNode == null) return null;
            // Try again with the new filternode
            filter = FindFilter(filterNode);
            // if its null return
            if (filter == null)
            {
                return null;
            }
        }
        return filter as SubSystemData;
    }

    public string GetSubSystemPath(IDiagramNode node)
    {
        var subsystem = FindSubsystem(node);
        if (subsystem == null) return string.Empty;
        return subsystem.Name;
    }

    public override string GetEditableControllerFilename(ElementData controllerName)
    {
        return Path.Combine(GetSubSystemPath(controllerName), base.GetEditableControllerFilename(controllerName));
    }

    //public override string GetEditableSceneManagerFilename(SceneManagerData nameAsSceneManager)
    //{
    //    return Path.Combine(GetSubSystemPath(nameAsSceneManager),base.GetEditableSceneManagerFilename(nameAsSceneManager);
    //}

    public override string GetEditableViewFilename(ViewData nameAsView)
    {
        return Path.Combine(GetSubSystemPath(nameAsView),base.GetEditableViewFilename(nameAsView));
    }

    public override string GetEditableViewModelFilename(ElementData nameAsViewModel)
    {
        return Path.Combine(GetSubSystemPath(nameAsViewModel),base.GetEditableViewModelFilename(nameAsViewModel));
    }

    public override string GetEditableViewComponentFilename(ViewComponentData name)
    {
        return Path.Combine(GetSubSystemPath(name),base.GetEditableViewComponentFilename(name));
    }
    
}