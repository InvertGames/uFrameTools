using System.Collections.Generic;
using System.Linq;
using Invert.Common;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEngine;

public class StateMachineStateNodeDrawer : DiagramNodeDrawer<StateMachineStateNodeViewModel>
{
    private NodeItemHeader _transitionsHeader;

    public StateMachineStateNodeDrawer()
    {
    }

    public NodeItemHeader TransitionsHeader
    {
        get
        {
            if (_transitionsHeader == null)
            {
                _transitionsHeader = Container.Resolve<NodeItemHeader>(null, false, ViewModel);
                _transitionsHeader.Label = "Transitions";
                _transitionsHeader.HeaderType = typeof(SceneTransitionItemViewModel);
                if (NodeViewModel.IsLocal)
                    _transitionsHeader.AddCommand = new SimpleEditorCommand<StateMachineStateNodeViewModel>((node) =>
                    {
                        node.AddTransition();
                        //var stateMachineNodeData = node.DiagramViewModel.Data.CurrentFilter as StateMachineNodeData;
                        //if (stateMachineNodeData == null) return;
                        //if (stateMachineNodeData.Element == null)
                        //{
                        //    EditorUtility.DisplayDialog("No Element",
                        //        "You must first wire and element to the {0} state machine.", stateMachineNodeData.Name);
                        //    return;
                        //}

                        //var triggerComputeds =
                        //  stateMachineNodeData.Element.Properties.Where(p => p.IsComputed && p.RelatedType == typeof(bool).Name).ToArray();

                        //ItemSelectionWindow.Init("Triggers", triggerComputeds, (item) =>
                        //{
                        //    //node.AddTransition(item);
                        //    node.AddTransition(item as ViewModelPropertyData);
                        //    //node.Add(item as ViewModelCommandData);
                        //});
                       
                    });
            }
            return _transitionsHeader;
        }
        set { _transitionsHeader = value; }
    }
    public StateMachineStateNodeDrawer(StateMachineStateNodeViewModel viewModel)
        : base(viewModel)
    {
    }

    protected override void GetContentDrawers(List<IDrawer> drawers)
    {
        base.GetContentDrawers(drawers);
        drawers.Insert(1, TransitionsHeader);
    }

    public override void Draw(float scale)
    {
        base.Draw(scale);
        if (NodeViewModel.IsCurrentState)
        {
          
                var adjustedBounds = new Rect(Bounds.x - 9, Bounds.y + 1, Bounds.width + 19, Bounds.height + 9);
                ElementDesignerStyles.DrawExpandableBox(adjustedBounds.Scale(Scale), ElementDesignerStyles.BoxHighlighter1, string.Empty, 20);
            
        }
    }
}