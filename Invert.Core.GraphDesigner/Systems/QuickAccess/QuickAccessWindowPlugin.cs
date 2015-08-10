using System;
using System.Collections.Generic;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Data;
using Invert.IOC;
using Invert.Windows;
using UnityEditor;
using UnityEngine;

public class QuickAccessWindowPlugin : DiagramPlugin, IQuickAccessEvents
{
    public override decimal LoadPriority
    {
        get { return 20; }
    }
    public override bool Required
    {
        get { return true; }
    }

    public override void Initialize(UFrameContainer container)
    {

        Container = container;
        


    }

    [MenuItem("uFrame/Quick Access #z")]
    public static void ShowQuickAccess()
    {
        InvertApplication.SignalEvent<IWindowsEvents>(_ => _.ShowWindow("QuickAccessWindowFactory", "Quick Access", null, new Vector2(Event.current.mousePosition.x, Event.current.mousePosition.y), new Vector2(150f, 250f)));
        
         
    }

    [MenuItem("uFrame/Quick Access #z",true)]
    public static bool ShowQuickAccessValidation()
    {
        return InvertGraphEditor.DesignerWindow != null && InvertGraphEditor.DesignerWindow.DiagramViewModel != null;
    }

    public void SelectionChanged(GraphItemViewModel selected)
    {
        Debug.Log("Item selected: "+selected.GetType().Name);
    }

    public DiagramViewModel CurrentDiagramViewModel
    {
        get { return InvertGraphEditor.DesignerWindow.DiagramViewModel; }
    }

    public IEnumerable<GraphItemViewModel> CurrentSelectedNodeItems
    {
        get { return InvertGraphEditor.DesignerWindow.DiagramViewModel.SelectedNodeItems; }
    }

    public IEnumerable<GraphItemViewModel> CurrentSelectedGraphItems
    {
        get { return InvertGraphEditor.DesignerWindow.DiagramViewModel.SelectedGraphItems; }
    }

    public UFrameContainer Container { get; set; }

    public void QuickAccessItemsEvents(QuickAccessContext context, List<IEnumerable<QuickAccessItem>> items)
    {
        
    }
}

public interface IQuickAccessEvents
{
    void QuickAccessItemsEvents(QuickAccessContext context, List<IEnumerable<QuickAccessItem>> items);
}

public interface IQuickAccessContext
{
    
}

public interface IInsertQuickAccessContext
{
    
}

public interface IConnectionQuickAccessContext
{
    
}
public class QuickAccessItem : IItem
{

    public Action<object> Action;

    public QuickAccessItem()
    {
    }

    public QuickAccessItem(string @group, string title, Action<object> action)
    {
        Group = @group;
        Title = title;
        Action = action;
    }

    public QuickAccessItem(string @group, string title, string searchTag, Action<object> action)
    {
        Group = @group;
        Title = title;
        SearchTag = searchTag;
        Action = action;
    }

    public QuickAccessItem(string title, Action<object> action)
    {
        Action = action;
        Title = title;
    }

    public string Title { get; set; }
    public string Group { get; set; }
    public string SearchTag { get; set; }
    public object Item { get; set; }
}

public class QuickAccessContext
{
    public MouseEvent MouseData;
    public Type ContextType;
    public object Data;
    
}
