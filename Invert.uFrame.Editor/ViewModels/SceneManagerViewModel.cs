namespace Invert.uFrame.Editor.ViewModels
{
    public class SceneManagerViewModel : DiagramNodeViewModel
    {
        public SceneManagerViewModel(SceneManagerData data) : base(data)
        {
        
        }
        public override bool AllowCollapsing
        {
            get { return true; }
        }

    }
}