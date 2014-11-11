namespace Invert.uFrame.Editor.ViewModels
{
    public class SceneTransitionItemViewModel : ItemViewModel<SceneManagerTransition>
    {

        public SceneTransitionItemViewModel(SceneManagerTransition data, DiagramNodeViewModel nodeViewModel)
            : base(nodeViewModel)
        {
            DataObject = data;
        }

        public override ConnectorViewModel InputConnector
        {
            get { return null; }
        }
    }
}