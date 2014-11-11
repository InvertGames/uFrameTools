using System;
using System.Collections.Generic;
using System.Linq;

namespace Invert.uFrame.Editor.ViewModels
{
    public class SceneManagerViewModel : DiagramNodeViewModel<SceneManagerData>
    {
        public SceneManagerViewModel(SceneManagerData data, DiagramViewModel diagramViewModel)
            : base(data, diagramViewModel)
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
        public override Invert.uFrame.Editor.ViewModels.ConnectorViewModel InputConnector
        {
            get
            {

                return base.InputConnector;
                return null;
            }
        }
        public override ConnectorViewModel OutputConnector
        {
            get { return null; }
        }
        //public override void GetConnectors(List<ConnectorViewModel> list)
        //{
        //    base.GetConnectors(list);
        //}
        public IEnumerable<RegisteredInstanceData> ImportedInstances
        {
            get { return GraphItem.SubSystem.AllInstances; }
        }
        public void AddCommandTransition(ViewModelCommandData item)
        {
            GraphItem.Project.AddItem(new SceneManagerTransition()
            {
                Node = GraphItem,
                CommandIdentifier = item.Identifier,
                Name = item.Name
            });
        }


    }

    //public class RegisteredInstanceViewModel : TypedItemViewModel
    //{
    //    public RegisteredInstanceViewModel(ITypeDiagramItem viewModelItem, DiagramNodeViewModel nodeViewModel) : base(viewModelItem, nodeViewModel)
    //    {
    //    }

    //    public override string TypeLabel
    //    {
    //        get { return  }
    //    }
    //}
}