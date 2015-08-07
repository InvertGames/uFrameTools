using System.Collections.Generic;
using System.Linq;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Data;
using Invert.IOC;
using UnityEditor;

public interface IDrawInspector
{
    void DrawInspector();
}


public class InspectorPlugin : DiagramPlugin, IDrawInspector, IDataRecordPropertyChanged, IDataRecordInserted, IDataRecordRemoved
{
    private bool _graphsOpen;

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
        if (Repository == null) return;
        if (WorkspaceService == null) return;
        if (WorkspaceService.CurrentWorkspace == null) return;
        if (Items == null) UpdateItems();
        foreach (var group in Items)
        {
            EditorPrefs.SetBool(group.Key, EditorGUILayout.Foldout(EditorPrefs.GetBool(group.Key),group.Key));
            if (EditorPrefs.GetBool(group.Key))
            {
                EditorGUI.indentLevel++;
                foreach (var node in group)
                {
                    
          
                }
                EditorGUI.indentLevel--;
            }
          
        }
    }

    public void UpdateItems()
    {
        if (WorkspaceService == null) return;
        
        Items =
            WorkspaceService.CurrentWorkspace.Graphs.SelectMany(p => p.NodeItems)
                .OfType<GenericNode>()
                .GroupBy(p => p.Config.Name)
                .OrderBy(p => p.Key).ToArray();

        
    }

    public IGrouping<string, GenericNode>[] Items { get; set; }

    public void PropertyChanged(IDataRecord record, string name, object previousValue, object nextValue)
    {
        UpdateItems();
        if (uFrameInspectorWindow.Instance != null)
            uFrameInspectorWindow.Instance.Repaint();
    }

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
}