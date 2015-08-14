using System.Collections.Generic;
using System.Linq;
using Invert.Common;
using Invert.Common.UI;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Core.GraphDesigner.Unity;
using Invert.Data;
using Invert.IOC;
using UnityEditor;
using UnityEngine;

public interface IDrawInspector
{
    void DrawInspector();
}

public interface IDrawExplorer
{
    void DrawExplorer();
}

public class InspectorPlugin : DiagramPlugin
    , IDrawInspector
    , IDrawExplorer
    , IDataRecordPropertyChanged
    , IDataRecordInserted
    , IDataRecordRemoved
    , IGraphSelectionEvents
    , IWorkspaceChanged
{
    private bool _graphsOpen;
    private static GUIStyle _item5;
    private static GUIStyle _item4;

    public override void Initialize(UFrameContainer container)
    {
        base.Initialize(container);

    }

    public override void Loaded(UFrameContainer container)
    {
        base.Loaded(container);
        Repository = container.Resolve<IRepository>();
        WorkspaceService = container.Resolve<WorkspaceService>();
    }

    public WorkspaceService WorkspaceService { get; set; }

    public IRepository Repository { get; set; }

    public void DrawInspector()
    {
      
    
        if (Fields == null) return;
        if (GUIHelpers.DoToolbarEx("Properties"))
        {
            foreach (var item in Fields)
            {
                var d = InvertGraphEditor.PlatformDrawer as UnityDrawer;
                d.DrawInspector(item);
            }
        }
     
    }

    public void UpdateItems()
    {
        if (WorkspaceService == null) return;
        
        Items =
            WorkspaceService.CurrentWorkspace.Graphs.SelectMany(p => p.NodeItems.OrderBy(x=>x.Name))
                .OfType<GenericNode>()
                .GroupBy(p => p.Graph.Name)
                .OrderBy(p => p.Key).ToArray();

        
    }

    public IGrouping<string, GenericNode>[] Items { get; set; }

    public void PropertyChanged(IDataRecord record, string name, object previousValue, object nextValue)
    {
     
        UpdateItems();
        if (uFrameInspectorWindow.Instance != null)
            uFrameInspectorWindow.Instance.Repaint();
    }
    public virtual IEnumerable<PropertyFieldViewModel> GetInspectorOptions(DiagramViewModel diagramViewModel)
    {
        var dataObject = this.Selected;
        if (dataObject == null) yield break;
        foreach (var item in dataObject.GetPropertiesWithAttribute<InspectorProperty>())
        {
            var property = item.Key;
            var attribute = item.Value;
            var fieldViewModel = new PropertyFieldViewModel()
            {
                Name = property.Name,
                

            };
            fieldViewModel.Getter = () => property.GetValue(dataObject, null);
            fieldViewModel.Setter = _ => property.SetValue(dataObject, _, null);
            fieldViewModel.InspectorType = attribute.InspectorType;
            fieldViewModel.Type = property.PropertyType;
            fieldViewModel.DiagramViewModel = diagramViewModel;
            fieldViewModel.CustomDrawerType = attribute.CustomDrawerType;
            fieldViewModel.CachedValue = fieldViewModel.Getter();
            yield return fieldViewModel;
        }
    }
    private void UpdateSelection(DiagramViewModel diagramViewModel)
    {
        
        Fields = GetInspectorOptions(diagramViewModel).ToArray();
        if (uFrameInspectorWindow.Instance != null)
            uFrameInspectorWindow.Instance.Repaint();
    }

    public PropertyFieldViewModel[] Fields { get; set; }

    public IDataRecord Selected { get; set; }

    public void RecordInserted(IDataRecord record)
    {
        InvertApplication.Log("Inserted");
        UpdateItems(); 
        if (uFrameInspectorWindow.Instance != null)
            uFrameInspectorWindow.Instance.Repaint();
    }

    public void RecordRemoved(IDataRecord record)
    {
        InvertApplication.Log("Removed");
        UpdateItems(); if (uFrameInspectorWindow.Instance != null)
            uFrameInspectorWindow.Instance.Repaint();
    }

    public void SelectionChanged(GraphItemViewModel selected)
    {
        Selected = selected.DataObject as IDataRecord;
        if (Selected != null)
        UpdateSelection(selected.DiagramViewModel);
    }

    public void DrawExplorer()
    {
        if (Repository == null) return;
        if (WorkspaceService == null) return;
        if (WorkspaceService.CurrentWorkspace == null) return;
        if (Items == null) UpdateItems();
        if (GUIHelpers.DoToolbarEx("Explorer"))
        {
            
            EditorGUI.indentLevel++;
            foreach (var group in Items)
            {
                //EditorPrefs.SetBool(group.Key, EditorGUILayout.Foldout(EditorPrefs.GetBool(group.Key), group.Key));
                if (GUIHelpers.DoToolbarEx(group.Key) ) //EditorPrefs.GetBool(group.Key))
                {
                    EditorGUI.indentLevel++;
                    foreach (var node in group)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(EditorGUI.indentLevel * 15f);
         
               

                        var selected = Selected != null && Selected.Identifier == node.Identifier;
                        if (GUILayout.Button(node.Name, selected ? Item5 : Item4))
                        {
                            Selected = node;
                            UpdateSelection(null);
                        } EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;
                }

            } EditorGUI.indentLevel--;
        }
    }
    public static GUIStyle Item4
    {
        get
        {
            if (_item4
                == null)
                _item4 = new GUIStyle
                {
                    normal = { background = ElementDesignerStyles.GetSkinTexture("Item4"), textColor = Color.white },
                    active = { background = ElementDesignerStyles.GetSkinTexture("EventButton"), textColor = Color.white },
                    stretchHeight = true,
                    stretchWidth = true,
                    fontSize = 12,
                    fixedHeight = 20f,
                    padding = new RectOffset(5, 0, 0, 0),
                    alignment = TextAnchor.MiddleLeft
                }.WithFont("Verdana", 12);

            return _item4;
        }
    }
    public static GUIStyle Item5
    {
        get
        {
            if (_item5
                == null)
                _item5= new GUIStyle
                {
                    normal = { background = ElementDesignerStyles.GetSkinTexture("Item1"), textColor = Color.white },
                    active = { background = ElementDesignerStyles.GetSkinTexture("EventButton"), textColor = Color.white },
                    stretchHeight = true,
                    stretchWidth = true,
                    padding = new RectOffset(5,0,0,0),
                    fontSize = 12,
                    fixedHeight = 20f,
                    alignment = TextAnchor.MiddleLeft
                }.WithFont("Verdana", 12);

            return _item5;
        }
    }
    public void WorkspaceChanged(Workspace workspace)
    {
        UpdateItems();
        Selected = null;
        UpdateSelection(null);
    }
}