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
        get { return _generators ?? (_generators = uFrameEditor.Container.Mappings.Where(p => p.From == typeof(DesignerGeneratorFactory)).ToArray()); }
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Target.Diagrams.RemoveAll(p => p == null);

        serializedObject.Update();
        GUIHelpers.IsInsepctor = true;

        if (GUIHelpers.DoToolbarEx("Project Diagrams", ImportDiagram))
        {
            for (int index = 0; index < Target.Diagrams.Count; index++)
            {
                var diagram = Target.Diagrams[index];
                int index1 = index;
                GUIHelpers.DoTriggerButton(new UFStyle()
                {
                    Label = diagram.Name,
                    Enabled = true,
                    BackgroundStyle = UBStyles.EventButtonStyleSmall,
                    TextAnchor = TextAnchor.MiddleRight,
                    IconStyle = UBStyles.RemoveButtonStyle,
                    ShowArrow = true,
                    OnShowOptions = () =>
                    {
                        var items = Target.Diagrams.ToList();
                        items.RemoveAt(index1);

                        var so = new SerializedObject(target);
                        so.Update();
                        var property = serializedObject.FindProperty("_diagrams");
                        property.ClearArray();
                        property.arraySize = items.Count;
                        for (int i = 0; i < items.Count; i++)
                        {
                            var item = items[i];
                            property.GetArrayElementAtIndex(i).objectReferenceValue = item;
                        }
                        so.ApplyModifiedProperties();
                    }
                })
                ;
            }
        }

        if (GUIHelpers.DoToolbarEx("Settings"))
        {
            var settingsProperty = serializedObject.FindProperty("_generatorSettings");


            var property = settingsProperty.FindPropertyRelative("_namespaceProvider").FindPropertyRelative("_rootNamespace");
            EditorGUILayout.PropertyField(property);


            property = settingsProperty.FindPropertyRelative("_generateComments");
            var newValue = GUIHelpers.DoToggle("Generate Comments", property.boolValue);
            if (newValue != property.boolValue)
            {
                property.boolValue = newValue;

            }
        }
        if (GUIHelpers.DoToolbarEx("Code Generators"))
        {
            var codeGenerators = uFrameEditor.CodeGenerators;
            var groupList = new List<ShowInSettings>();

            foreach (var codeGenerator in codeGenerators)
            {
                var customAttribute = codeGenerator.GetCustomAttributes(typeof(ShowInSettings), true).OfType<ShowInSettings>().FirstOrDefault();
                if (customAttribute == null) continue;
                groupList.Add(customAttribute);

            }
            foreach (var item in groupList.Select(p => p.Group).Distinct())
            {
                var isOn = Target[item, true];
                var result = GUIHelpers.DoToggle(item, isOn);
                if (isOn != result)
                {
                    Target[item] = result;
                }
            }
        }
        serializedObject.ApplyModifiedProperties();
        GUIHelpers.IsInsepctor = false;
    }

    private void ImportDiagram()
    {
        var assets = uFrameEditor.GetAssets(typeof(GraphData)).OfType<GraphData>().ToArray();

        ItemSelectionWindow.Init("Select Diagram", assets, (item) =>
        {
            // Mimic the new list
            var list = Target.Diagrams.ToList();
            var selectedAsset = item as GraphData;
            list.Add(selectedAsset);

            var so = new SerializedObject(Target); so.Update();


            // Adjust the array
            var property = so.FindProperty("_diagrams");
            property.arraySize = list.Count;

            for (int index = 0; index < list.Count; index++)
            {
                var diagram = list[index];
                property.GetArrayElementAtIndex(index).objectReferenceValue = diagram;
            }

            so.ApplyModifiedProperties();
        });
    }
}

[Serializable]
public class ProjectRepository : ScriptableObject, IProjectRepository, ISerializationCallbackReceiver
{
    [SerializeField, HideInInspector]
    private string[] _settingsKeys;
    [SerializeField, HideInInspector]
    private bool[] _settingsValues;

    private Dictionary<string, bool> _settingsBag;
    public Dictionary<string, bool> SettingsBag
    {
        get { return _settingsBag ?? (_settingsBag = new Dictionary<string, bool>()); }
        set { _settingsBag = value; }
    }
    public bool this[string settingsKey, bool def = true]
    {
        get
        {
            if (SettingsBag.ContainsKey(settingsKey))
            {
                return SettingsBag[settingsKey];
            }
            return def;
        }
        set
        {
            if (SettingsBag.ContainsKey(settingsKey))
            {
                SettingsBag[settingsKey] = value;
            }
            else
            {
                SettingsBag.Add(settingsKey, value);
            }
        }
    }
    public void OnBeforeSerialize()
    {
        if (_settingsBag == null) return;
        _settingsKeys = _settingsBag.Keys.ToArray();
        _settingsValues = _settingsBag.Values.ToArray();
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
    }

    public bool GetSetting(string key, bool def = true)
    {
        return this[key, def];
    }

    public bool SetSetting(string key, bool value)
    {
        return this[key] = value;
    }

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
        return CurrentGraph.PositionData[CurrentFilter, node];
    }

    public void SetItemLocation(IDiagramNode node, Vector2 position)
    {
        CurrentGraph.PositionData[CurrentFilter, node] = position;
    }

    private Dictionary<string, string> _derivedTypes;

    public void MarkDirty(INodeRepository data)
    {
        if (data != null)
            EditorUtility.SetDirty(data as UnityEngine.Object);
    }

    protected string[] _diagramNames;

    [SerializeField, HideInInspector]
    protected List<GraphData> _diagrams;


    private GraphData _currentGraph;

    public void RecacheAssets()
    {

    }

    public Dictionary<string, string> GetProjectDiagrams()
    {
        var items = new Dictionary<string, string>();
        foreach (var elementDesignerData in Diagrams.Where(p => p.GetType() == RepositoryFor))
        {
            var asset = AssetDatabase.GetAssetPath(elementDesignerData as UnityEngine.Object);

            items.Add(elementDesignerData.Name, asset);
        }
        return items;
    }

    public IGraphData CreateNewDiagram(Type diagramType = null, IDiagramFilter defaultFilter = null)
    {
        Selection.activeObject = this;
        var t = diagramType ?? typeof(ElementsGraph);
        var diagram = UFrameAssetManager.CreateAsset(t) as GraphData;
        if (defaultFilter != null && diagram != null)
        {
            diagram.RootFilter = defaultFilter;
        }
        diagram.Version = uFrameVersionProcessor.CURRENT_VERSION_NUMBER.ToString();
        Diagrams.Add(diagram);
        Refresh();
        return diagram;
    }
    public virtual Type RepositoryFor
    {
        get { return typeof(ElementsGraph); }
    }

    public string Name
    {
        get { return name; }
    }


    private IDiagramNode[] _nodeItems;
    [SerializeField, HideInInspector]
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
            return CurrentGraph.Settings;
        }
    }

    public void AddNode(IDiagramNode data)
    {
        _nodeItems = null;
        CurrentGraph.AddNode(data);

    }

    public void RemoveNode(IDiagramNode enumData)
    {
        _nodeItems = null;
        foreach (var item in enumData.ContainedItems.ToArray())
        {
            RemoveItem(item);
        }
        CurrentGraph.RemoveNode(enumData);
        foreach (var item in NodeItems.ToArray())
        {
            item.NodeRemoved(enumData);
        }
    }

    public void RemoveItem(IDiagramNodeItem nodeItem)
    {
        nodeItem.Node.RemoveItem(nodeItem);

        foreach (var node in NodeItems.ToArray())
        {
            node.NodeItemRemoved(nodeItem);
            foreach (var item in node.ContainedItems.ToArray())
            {
                item.NodeItemRemoved(nodeItem);
            }

        }
    }
    public IDiagramFilter CurrentFilter
    {
        get
        {
            return CurrentGraph.CurrentFilter;
        }
    }

    public FilterPositionData PositionData
    {
        get { return CurrentGraph.PositionData; }
    }

    public string LastLoadedDiagram
    {
        get { return EditorPrefs.GetString("UF_LastLoadedDiagram" + this.name, string.Empty); }
        set { EditorPrefs.SetString("UF_LastLoadedDiagram" + this.name, value); }
    }
    public List<GraphData> Diagrams
    {
        get
        {
            if (_diagrams == null)
            {
                _diagrams = new List<GraphData>();
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

    public GraphData CurrentGraph
    {
        get
        {
            if (_currentGraph == null)
            {
                if (!String.IsNullOrEmpty(LastLoadedDiagram))
                {
                    _currentGraph = Diagrams.FirstOrDefault(p => p.name == LastLoadedDiagram);
                }
                if (_currentGraph == null)
                {
                    _currentGraph = Diagrams.FirstOrDefault();
                }
            }
            return _currentGraph;
        }
        set
        {
            _currentGraph = value;
            if (value != null)
                LastLoadedDiagram = value.name;
        }
    }


    public IGraphData LoadDiagram(string path)
    {
        var data = AssetDatabase.LoadAssetAtPath(path, RepositoryFor) as ElementsGraph;
        if (data == null)
        {
            return null;
        }
        CurrentGraph = data;
        return data;
    }

    public void SaveDiagram(INodeRepository data)
    {
        if (data != null)
        {
            EditorUtility.SetDirty(data as UnityEngine.Object);
        }
        AssetDatabase.SaveAssets();
    }

    public void RecordUndo(INodeRepository data, string title)
    {
        if (data != null)
            Undo.RecordObject(data as UnityEngine.Object, title);
    }


    public virtual void Refresh()
    {
        //var assets = uFrameEditor.GetAssets(typeof(GraphData));
        //Diagrams = assets.OfType<GraphData>().ToList();

        CurrentGraph = null;
        _nodeItems = null;

        foreach (var diagram in Diagrams)
        {
            diagram.Prepare();
        }

    }

    public void HideNode(string identifier)
    {
        CurrentGraph.PositionData.Remove(CurrentGraph.CurrentFilter, identifier);
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

    IGraphData Data { get; set; }


    string GetDesignerFilePath(string postFix);

    string GetEditableViewFilename(ViewData nameAsView);
    string GetEditableViewComponentFilename(ViewComponentData name);
    string GetEditableSceneManagerFilename(SceneManagerData nameAsSceneManager);
    string GetEditableSceneManagerSettingsFilename(SceneManagerData nameAsSettings);
    string GetEditableControllerFilename(ElementData controllerName);
    string GetEditableViewModelFilename(ElementData nameAsViewModel);
    string GetEnumsFilename(EnumData name);

    void MoveTo(GeneratorSettings settings, ICodePathStrategy strategy, string name, ElementsDesigner designerWindow);
}

public class DefaultCodePathStrategy : ICodePathStrategy
{
    public IGraphData Data { get; set; }

    public string AssetPath { get; set; }

    public virtual string BehavioursPath
    {
        get { return Path.Combine(AssetPath, "Behaviours"); }
    }

    public virtual string ScenesPath
    {
        get { return Path.Combine(AssetPath, "Scenes"); }
    }

    public string GetDesignerFilePath(string postFix)
    {
        return Path.Combine("_DesignerFiles", Data.Name + postFix + ".designer.cs");
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
        return GetDesignerFilePath(string.Empty);
    }

    public virtual void MoveTo(GeneratorSettings settings, ICodePathStrategy strategy, string name, ElementsDesigner designerWindow)
    {
        var sourceFiles = uFrameEditor.GetAllFileGenerators(settings).ToArray();
        strategy.Data = Data;
        strategy.AssetPath = AssetPath;
        var targetFiles = uFrameEditor.GetAllFileGenerators(settings).ToArray();

        if (sourceFiles.Length == targetFiles.Length)
        {
            // Attempt to move every file
            ProcessMove(strategy, name, sourceFiles, targetFiles);
        }
        else
        {
            // Attempt to move non designer files
            // var designerFiles = sourceFiles.Where(p => p.Filename.EndsWith("designer.cs"));
            sourceFiles = sourceFiles.Where(p => !p.SystemPath.EndsWith("designer.cs")).ToArray();
            targetFiles = targetFiles.Where(p => !p.SystemPath.EndsWith("designer.cs")).ToArray();
            if (sourceFiles.Length == targetFiles.Length)
            {
                ProcessMove(strategy, name, sourceFiles, targetFiles);
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

            var sourceFileInfo = new FileInfo(System.IO.Path.Combine(AssetPath, sourceFile.SystemPath));
            var targetFileInfo = new FileInfo(System.IO.Path.Combine(AssetPath, targetFile.SystemPath));
            if (sourceFileInfo.FullName == targetFileInfo.FullName) continue;
            if (!sourceFileInfo.Exists) continue;
            EnsurePath(sourceFileInfo);
            if (targetFileInfo.Exists) continue;
            EnsurePath(targetFileInfo);

            var sourceAsset = "Assets" + sourceFileInfo.FullName.Replace("\\", "/").Replace(Application.dataPath, "").Replace("\\", "/");
            var targetAsset = "Assets" + targetFileInfo.FullName.Replace("\\", "/").Replace(Application.dataPath, "").Replace("\\", "/");
            uFrameEditor.Log(string.Format("Moving file {0} to {1}", sourceAsset, targetAsset));
            AssetDatabase.MoveAsset(sourceAsset, targetAsset);
        }

        Data.Settings.CodePathStrategyName = name;
        Data.CodePathStrategy = null;
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
            if (node != diagramFilter && node.Project.PositionData.HasPosition(diagramFilter, node))
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
        return Path.Combine(GetSubSystemPath(nameAsView), base.GetEditableViewFilename(nameAsView));
    }

    public override string GetEditableViewModelFilename(ElementData nameAsViewModel)
    {
        return Path.Combine(GetSubSystemPath(nameAsViewModel), base.GetEditableViewModelFilename(nameAsViewModel));
    }

    public override string GetEditableViewComponentFilename(ViewComponentData name)
    {
        return Path.Combine(GetSubSystemPath(name), base.GetEditableViewComponentFilename(name));
    }

}