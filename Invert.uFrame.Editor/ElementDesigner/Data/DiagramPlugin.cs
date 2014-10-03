using Invert.uFrame;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ElementDesigner.Commands;
using Invert.uFrame.Editor.ViewModels;
using UnityEditor;
using UnityEngine;

namespace Invert.uFrame.Editor
{
    public abstract class DiagramPlugin : IDiagramPlugin
    {
        public virtual string PackageName
        {
            get { return string.Empty; }
        }

        public string Title
        {
            get { return this.GetType().Name; }
        }

        public virtual bool EnabledByDefault
        {
            get { return true; }
        }
        public virtual bool Enabled
        {
            get { return EditorPrefs.GetBool("UFRAME_PLUGIN_" + this.GetType().Name, EnabledByDefault); }
            set { EditorPrefs.SetBool("UFRAME_PLUGIN_" + this.GetType().Name, value); }
        }

        public virtual decimal LoadPriority { get { return 1; } }
        public abstract void Initialize(uFrameContainer container);
    }

    public class DefaultKeyBindings : DiagramPlugin
    {
        public override void Initialize(uFrameContainer container)
        {
            uFrameEditor.RegisterKeyBinding(new RenameCommand(), "Rename", KeyCode.F2);
            uFrameEditor.RegisterKeyBinding(new SimpleEditorCommand<DiagramViewModel>((p) =>
            {
               p.DeselectAll();
            }), "End All Editing", KeyCode.Return);

            uFrameEditor.RegisterKeyBinding(new DeleteItemCommand(), "Delete Item", KeyCode.X,true);
            uFrameEditor.RegisterKeyBinding(new DeleteCommand(), "Delete", KeyCode.Delete);
            uFrameEditor.RegisterKeyBinding(new MoveUpCommand(), "Move Up", KeyCode.UpArrow);
            uFrameEditor.RegisterKeyBinding(new MoveDownCommand(), "Move Down", KeyCode.DownArrow);

            uFrameEditor.RegisterKeyBinding(new AddItemCommand2() {SelectedOption = new UFContextMenuItem() {Value=typeof(SceneManagerData) } },"Add Scene Manager", KeyCode.M,true,true);

            uFrameEditor.RegisterKeyBinding(new AddItemCommand2() { SelectedOption = new UFContextMenuItem() { Value = typeof(SubSystemData) } }, "Add Sub System", KeyCode.U, true, true);
            uFrameEditor.RegisterKeyBinding(new AddItemCommand2() { SelectedOption = new UFContextMenuItem() { Value = typeof(ElementData) } }, "Add Element", KeyCode.E, true);
            uFrameEditor.RegisterKeyBinding(new AddItemCommand2() { SelectedOption = new UFContextMenuItem() { Value = typeof(EnumData) } }, "Add Enum", KeyCode.N, true, true);
            uFrameEditor.RegisterKeyBinding(new AddItemCommand2() { SelectedOption = new UFContextMenuItem() { Value = typeof(ViewData) } }, "Add View", KeyCode.V, true, true);
            uFrameEditor.RegisterKeyBinding(new AddItemCommand2() { SelectedOption = new UFContextMenuItem() { Value = typeof(ViewComponentData) } }, "Add View Component", KeyCode.W, true, true);

            uFrameEditor.RegisterKeyBinding(new SimpleEditorCommand<DiagramViewModel>((p) =>
            {
                uFrameEditor.ShowHelp = !uFrameEditor.ShowHelp;
            }),"Show/Hide This Help", KeyCode.F1);

            uFrameEditor.RegisterKeyBinding(new SaveCommand(), "Save & Compile", KeyCode.S,true,true);

          


        }
    }
}