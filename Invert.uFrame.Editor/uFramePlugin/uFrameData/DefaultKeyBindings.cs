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
            InvertGraphEditor.RegisterKeyBinding(new RenameCommand(), "Rename", KeyCode.F2);
            InvertGraphEditor.RegisterKeyBinding(new SimpleEditorCommand<DiagramViewModel>((p) =>
            {
                p.DeselectAll();
            }), "End All Editing", KeyCode.Return);

            InvertGraphEditor.RegisterKeyBinding(new DeleteItemCommand(), "Delete Item", KeyCode.X,true);
            InvertGraphEditor.RegisterKeyBinding(new DeleteCommand(), "Delete", KeyCode.Delete);
            InvertGraphEditor.RegisterKeyBinding(new MoveUpCommand(), "Move Up", KeyCode.UpArrow);
            InvertGraphEditor.RegisterKeyBinding(new MoveDownCommand(), "Move Down", KeyCode.DownArrow);

            InvertGraphEditor.RegisterKeyBinding(new AddItemCommand2() {SelectedOption = new UFContextMenuItem() {Value=typeof(SceneManagerData) } },"Add Scene Manager", KeyCode.M,true,true);

            InvertGraphEditor.RegisterKeyBinding(new AddItemCommand2() { SelectedOption = new UFContextMenuItem() { Value = typeof(SubSystemData) } }, "Add Sub System", KeyCode.U, true, true);
            InvertGraphEditor.RegisterKeyBinding(new AddItemCommand2() { SelectedOption = new UFContextMenuItem() { Value = typeof(ElementData) } }, "Add Element", KeyCode.E, true);
            InvertGraphEditor.RegisterKeyBinding(new AddItemCommand2() { SelectedOption = new UFContextMenuItem() { Value = typeof(EnumData) } }, "Add Enum", KeyCode.N, true, true);
            InvertGraphEditor.RegisterKeyBinding(new AddItemCommand2() { SelectedOption = new UFContextMenuItem() { Value = typeof(ViewData) } }, "Add View", KeyCode.V, true, true);
            InvertGraphEditor.RegisterKeyBinding(new AddItemCommand2() { SelectedOption = new UFContextMenuItem() { Value = typeof(ViewComponentData) } }, "Add View Component", KeyCode.W, true, true);

            InvertGraphEditor.RegisterKeyBinding(new SimpleEditorCommand<DiagramViewModel>((p) =>
            {
                uFrameEditor.ShowHelp = !uFrameEditor.ShowHelp;
            }),"Show/Hide This Help", KeyCode.F1);

            InvertGraphEditor.RegisterKeyBinding(new SimpleEditorCommand<DiagramViewModel>((p) =>
            {
                var saveCommand = uFrameEditor.Container.Resolve<IToolbarCommand>("Save");
                InvertGraphEditor.ExecuteCommand(saveCommand);
            }), "Save & Compile", KeyCode.S, true, true);

          


        }
    }
}