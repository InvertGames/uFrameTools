using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Invert.uFrame;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.Refactoring;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

public class ProjectRepository : ScriptableObject, IProjectRepository
{

    private Dictionary<string, string> _derivedTypes;

    public void MarkDirty(INodeRepository data)
    {
        EditorUtility.SetDirty(data as UnityEngine.Object);
    }

    protected string[] _diagramNames;
    
    private uFrameProject[] _projects;

    [SerializeField]
    protected List<JsonElementDesignerData> _diagrams;
    [SerializeField]
    private string _outputDirectory;

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

    public void CreateNewDiagram()
    {
        _diagrams.Add(UFrameAssetManager.CreateAsset<JsonElementDesignerData>());
    }
    public virtual Type RepositoryFor
    {
        get { return typeof(JsonElementDesignerData); }
    }

    public string Name
    {
        get { return name; }
    }

    public IEnumerable<IDiagramNode> NodeItems
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
        CurrentDiagram.AddNode(data);
    }

    public void RemoveNode(IDiagramNode enumData)
    {
        CurrentDiagram.RemoveNode(enumData);
    }

    public IDiagramFilter CurrentFilter
    {
        get
        {
            return CurrentDiagram.CurrentFilter;
        }
    }

    public List<JsonElementDesignerData> Diagrams
    {
        get
        {
            if (_diagrams == null)
            {
                Refresh();
            }
            return _diagrams;
        }
        set { _diagrams = value; }
    }

    public INodeRepository CurrentDiagram
    {
        get; set;
    }

    public virtual string[] DiagramNames
    {
        get
        {
            if (_diagramNames == null)
            {
                Refresh();
            }
            return _diagramNames;
        }
        set { _diagramNames = value; }
    }

    public string OutputDirectory
    {
        get { return _outputDirectory; }
        set { _outputDirectory = value; }
    }

    public IElementDesignerData LoadDiagram(string path)
    {
        var data = AssetDatabase.LoadAssetAtPath(path, RepositoryFor) as IElementDesignerData;
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
        var assets = uFrameEditor.GetAssets(RepositoryFor);
        Diagrams = assets.OfType<JsonElementDesignerData>().ToList();

        foreach (var diagram in Diagrams)
        {
            diagram.Prepare();
        }
        DiagramNames = Diagrams.Select(p => p.Name).ToArray();
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
    
    void MoveTo(ICodePathStrategy strategy,string name,ElementsDesigner designerWindow);
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

    public virtual void MoveTo(ICodePathStrategy strategy, string name, ElementsDesigner designerWindow)
    {
        var sourceFiles = uFrameEditor.GetAllFileGenerators(this).ToArray();
        strategy.Data = Data;
        strategy.AssetPath = AssetPath;
        var targetFiles = uFrameEditor.GetAllFileGenerators(strategy).ToArray();

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
            if (node != diagramFilter &&  diagramFilter.Locations.Keys.Contains(node.Identifier))
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