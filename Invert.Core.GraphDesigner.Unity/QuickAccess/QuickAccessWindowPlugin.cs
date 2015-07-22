using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.Core.GraphDesigner.Unity;
using Invert.IOC;
using Invert.Windows;
using Mono.CSharp;
using UnityEditor;

public class QuickAccessWindowPlugin : DiagramPlugin, IQuickAccessEvents
{
    public override decimal LoadPriority
    {
        get { return 20; }
    }

    public override void Initialize(UFrameContainer container)
    {

        Container = container;
        container.RegisterWindow<QuickAccessWindowViewModel>("QuickAccessWindowFactory")
            .HasPanel<QuickAccessWindowSearchPanel, QuickAccessWindowViewModel>()
            .WithDefaultInstance(_=>new QuickAccessWindowViewModel(new QuickAccessContext()
            {
                CurrentDiagramViewModel = CurrentDiagramViewModel,
                CurrentProject = CurrentProjectRepository,
                CurrentSelectGraphItems = CurrentSelectedGraphItems,
                CurrentSelectedNodeItems = CurrentSelectedNodeItems,
                SelectedNodeItem = CurrentDiagramViewModel.SelectedNode,
                SelectedGraphItem = CurrentDiagramViewModel.SelectedGraphItem
            }));

        ListenFor<IQuickAccessEvents>();
    }

    [MenuItem("uFrame/Quick Access #z")]
    public static void ShowQuickAccess()
    {
        var window = WindowsPlugin.GetWindowFor("QuickAccessWindowFactory");
            window.titleContent = new GUIContent("Quick Access");
            window.ShowAsDropDown(new Rect(0,0,200,200),new Vector2(400,400) );
            window.Repaint();
            window.Focus();
    }

    [MenuItem("uFrame/Quick Access #z",true)]
    public static bool ShowQuickAccessValidation()
    {
        return ElementsDesigner.Instance != null && ElementsDesigner.Instance.DiagramViewModel != null;
    }

    public void SelectionChanged(GraphItemViewModel selected)
    {
        Debug.Log("Item selected: "+selected.GetType().Name);
    }

    public INodeRepository CurrentProjectRepository
    {
        get { return CurrentDiagramViewModel.CurrentRepository; }
    }

    public DiagramViewModel CurrentDiagramViewModel
    {
        get { return ElementsDesigner.Instance.DiagramViewModel; }
    }

    public IEnumerable<GraphItemViewModel> CurrentSelectedNodeItems
    {
        get { return ElementsDesigner.Instance.DiagramViewModel.SelectedNodeItems; }
    }

    public IEnumerable<GraphItemViewModel> CurrentSelectedGraphItems
    {
        get { return ElementsDesigner.Instance.DiagramViewModel.SelectedGraphItems; }
    }

    public UFrameContainer Container { get; set; }

    public void QuickAccessItemsEvents(QuickAccessContext context, List<IEnumerable<QuickAccessItem>> items)
    {
        var selectedNode = context.SelectedNodeItem;
        if (selectedNode != null)
        {
            var actions = Container.ResolveAll<IDiagramNodeCommand>()
                .OfType<EditorCommand>()
                .Where(t => t.For == selectedNode.DataObject.GetType())
                .Select(t =>
                {
                    return new QuickAccessItem()
                    {
                        Title = t.Title,
                        Action = () => InvertGraphEditor.ExecuteCommand(t)
                    };
                });

            items.Add(actions);
        }

        var addCmd = Container.ResolveAll<IDiagramContextCommand>().FirstOrDefault(s => s.GetType() == typeof(AddNodeToGraph)) as AddNodeToGraph;
        var options = addCmd.GetOptions(CurrentDiagramViewModel);

        var addItems = options.Select(o =>
        {
            return new QuickAccessItem()
            {
                Title = o.Name,
                Action = () =>
                {
                    addCmd.SelectedOption = o;
                    InvertGraphEditor.ExecuteCommand(addCmd);
                }
            };
        });

        items.Add(addItems);





    }
}

public interface IQuickAccessEvents
{
    void QuickAccessItemsEvents(QuickAccessContext context, List<IEnumerable<QuickAccessItem>> items);
}

public struct QuickAccessItem
{
    public string Title;
    public Action Action;
}

public class QuickAccessContext
{
    public INodeRepository CurrentProject;
    public DiagramViewModel CurrentDiagramViewModel;
    public IEnumerable<GraphItemViewModel> CurrentSelectGraphItems;
    public IEnumerable<GraphItemViewModel> CurrentSelectedNodeItems;
    public GraphItemViewModel SelectedGraphItem;
    public DiagramNodeViewModel SelectedNodeItem;
}
