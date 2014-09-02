using System;

namespace Invert.uFrame.Editor.ViewModels
{
    public class SceneManagerViewModel : DiagramNodeViewModel<SceneManagerData>
    {
        public SceneManagerViewModel(SceneManagerData data, DiagramViewModel diagramViewModel)
            : base(data,diagramViewModel)
        {
        
        }
        public override bool AllowCollapsing
        {
            get { return true; }
        }

        public Type CurrentType
        {
            get
            {
                return this.GraphItem.CurrentType;
            }
        }
    }
}