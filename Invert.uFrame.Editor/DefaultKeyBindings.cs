using Invert.Core;
using Invert.Core.GraphDesigner;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using UnityEngine;

namespace Invert.uFrame.Editor
{
    public class DefaultKeyBindings : DiagramPlugin
    {
        public override void Initialize(uFrameContainer container)
        {
            InvertGraphEditor.RegisterKeyBinding(new AddNodeToGraph() {SelectedOption = new UFContextMenuItem() {Value=typeof(SceneManagerData) } },"Add Scene Manager", KeyCode.M,true,true);
            InvertGraphEditor.RegisterKeyBinding(new AddNodeToGraph() { SelectedOption = new UFContextMenuItem() { Value = typeof(SubSystemData) } }, "Add Sub System", KeyCode.U, true, true);
            InvertGraphEditor.RegisterKeyBinding(new AddNodeToGraph() { SelectedOption = new UFContextMenuItem() { Value = typeof(ElementData) } }, "Add Element", KeyCode.E, true);
            InvertGraphEditor.RegisterKeyBinding(new AddNodeToGraph() { SelectedOption = new UFContextMenuItem() { Value = typeof(EnumData) } }, "Add Enum", KeyCode.N, true, true);
            InvertGraphEditor.RegisterKeyBinding(new AddNodeToGraph() { SelectedOption = new UFContextMenuItem() { Value = typeof(ViewData) } }, "Add View", KeyCode.V, true, true);
            InvertGraphEditor.RegisterKeyBinding(new AddNodeToGraph() { SelectedOption = new UFContextMenuItem() { Value = typeof(ViewComponentData) } }, "Add View Component", KeyCode.W, true, true);
        }
    }
}