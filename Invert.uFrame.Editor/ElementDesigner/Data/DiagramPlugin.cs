using Invert.uFrame;
using Invert.uFrame.Editor.ElementDesigner;
using Invert.uFrame.Editor.ElementDesigner.Commands;
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

            uFrameEditor.RegisterKeyBinding(new DeleteItemCommand(), "Delete Item", KeyCode.X,true);
            uFrameEditor.RegisterKeyBinding(new DeleteCommand(), "Delete", KeyCode.Delete);
            uFrameEditor.RegisterKeyBinding(new MoveUpCommand(), "Move Up", KeyCode.UpArrow);
            uFrameEditor.RegisterKeyBinding(new MoveDownCommand(), "Move Down", KeyCode.DownArrow);

            uFrameEditor.RegisterKeyBinding(new AddNewSceneManagerCommand(), "Add Scene Manager", KeyCode.M,true,true);
            uFrameEditor.RegisterKeyBinding(new AddNewSubSystemCommand(), "Add Sub System", KeyCode.S,true,true);
            uFrameEditor.RegisterKeyBinding(new AddNewElementCommand(), "Add Element", KeyCode.E,true);
            uFrameEditor.RegisterKeyBinding(new AddNewEnumCommand(), "Add Enum", KeyCode.N,true,true);
            uFrameEditor.RegisterKeyBinding(new AddNewViewCommand(), "Add View", KeyCode.V,true,true);
            uFrameEditor.RegisterKeyBinding(new AddNewViewComponentCommand(), "Add View Component", KeyCode.W,true,true);

            uFrameEditor.RegisterKeyBinding(new SimpleEditorCommand<ElementsDiagram>((p) =>
            {
                uFrameEditor.ShowHelp = !uFrameEditor.ShowHelp;
            }),"Show/Hide This Help", KeyCode.F1);

            uFrameEditor.RegisterKeyBinding(new SaveCommand(), "Save & Compile", KeyCode.S,true,true);

            //  container.RegisterInstance<IKeyBinding>(
            //new KeyBinding<IEditorCommand>("Save & Compile", KeyCode.S, true, true, false), "SaveCommand");
 

            //container.RegisterInstance<IKeyBinding>(
            //    new KeyBinding<IDiagramNodeItemCommand>("Move Item Up", KeyCode.UpArrow, true), "MoveItemUp");
            //container.RegisterInstance<IKeyBinding>
            //    (new KeyBinding<IDiagramNodeItemCommand>("Move Item Down", KeyCode.DownArrow, true), "MoveItemDown");

            //container.RegisterInstance<IKeyBinding>(
            //    new KeyBinding<IDiagramContextCommand>("Add New Element", KeyCode.E, true, true), "AddNewElementCommand");
            //container.RegisterInstance<IKeyBinding>(
            //    new KeyBinding<IDiagramContextCommand>("Add New Sub System", KeyCode.S, true, true), "AddNewSubSystemCommand");
            //container.RegisterInstance<IKeyBinding>
            //    (new KeyBinding<IDiagramContextCommand>("Add New Scene Manager", KeyCode.C, true, true), "AddNewSceneManagerCommand");
            //container.RegisterInstance<IKeyBinding>(
            //    new KeyBinding<IDiagramContextCommand>("Add New View", KeyCode.V, true, true), "AddNewViewCommand");
            //container.RegisterInstance<IKeyBinding>(
            //    new KeyBinding<IDiagramContextCommand>("Add New Enum", KeyCode.N, true, true), "AddNewEnumCommand");
            //container.RegisterInstance<IKeyBinding>(
            //    new KeyBinding<IDiagramContextCommand>("Add New View Component", KeyCode.W, true, true), "AddNewViewComponentCommand");

            //container.RegisterInstance<IKeyBinding>(
            //    new KeyBinding<IEditorCommand>("Show/Hide This", KeyCode.F1, true, true, false), "ShowHideHelp");

            //container.RegisterInstance<IKeyBinding>(
            //new KeyBinding<IEditorCommand>("Save & Compile", KeyCode.S, true, true, false), "SaveCommand");


        }
    }
}