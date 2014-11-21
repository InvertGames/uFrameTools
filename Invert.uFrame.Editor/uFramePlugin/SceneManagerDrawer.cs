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
    private SectionHeaderDrawer _transitionsHeader;


    public SceneManagerDrawer(SceneManagerViewModel viewModel)
        : base()
    {
        ViewModel = viewModel;
    }

    protected override GUIStyle HeaderStyle
    {
        get { return ElementDesignerStyles.NodeHeader6; }
    }


    public override GUIStyle ItemStyle
    {
        get { return ElementDesignerStyles.Item4; }
    }

}