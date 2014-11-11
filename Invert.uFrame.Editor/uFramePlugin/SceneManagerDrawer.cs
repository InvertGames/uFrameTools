using System.Collections.Generic;
using System.Linq;
using Invert.Common;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

public class SceneManagerDrawer : DiagramNodeDrawer<SceneManagerViewModel>
{
    private NodeItemHeader _transitionsHeader;


    public SceneManagerDrawer(SceneManagerViewModel viewModel)
        : base()
    {
        ViewModel = viewModel;
    }

    protected override GUIStyle HeaderStyle
    {
        get { return ElementDesignerStyles.NodeHeader6; }
    }

 
    public NodeItemHeader TransitionsHeader
    {
        get
        {
            if (_transitionsHeader != null) return _transitionsHeader;
            
                _transitionsHeader = new NodeItemHeader(ViewModel)
                {
                    Label = "Transitions",
                    HeaderType = typeof (SceneTransitionItemViewModel),
                  
                };

            if (NodeViewModel.IsLocal)
                _transitionsHeader.AddCommand = uFrameEditor.Container.Resolve<AddTransitionCommand>();

            return _transitionsHeader;
        }
        set { _transitionsHeader = value; }
    }

    protected override void GetContentDrawers(List<IDrawer> drawers)
    {
        //base.GetContentDrawers(drawers);
        drawers.Add( TransitionsHeader);
        foreach (var item in ViewModel.ContentItems.OfType<SceneTransitionItemViewModel>())
        {
            var drawer = InvertGraphEditor.CreateDrawer(item);
            if (drawer == null) Debug.Log(string.Format("Couldn't create drawer for {0} make sure it is registered.", item.GetType().Name));
            drawers.Add(drawer);
        }
    
    }

    public override GUIStyle ItemStyle
    {
        get { return ElementDesignerStyles.Item4; }
    }

}