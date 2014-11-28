using System.Collections.Generic;
using System.Linq;
using Invert.Common;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class ClassNodeDrawer : DiagramNodeDrawer<ClassNodeViewModel>
    {
        protected override GUIStyle HeaderStyle
        {
            get { return ElementDesignerStyles.NodeHeader12; }
        }

        public ClassNodeDrawer(ClassNodeViewModel viewModelObject) : base(viewModelObject)
        {

        }

        protected override void GetContentDrawers(List<IDrawer> drawers)
        {
            //base.GetContentDrawers(drawers);

            drawers.Add(new SectionHeaderDrawer(new SectionHeaderViewModel()
            {
                Name = "Properties",
                AddCommand = new SimpleEditorCommand<ClassNodeViewModel>(n =>
                {
                    n.AddProperty();
                }),
            }));

            foreach (var item in ViewModel.ContentItems.Where(p => p.DataObject is ClassPropertyData))
            {
                var drawer = InvertGraphEditor.CreateDrawer(item);
                if (drawer == null) Debug.Log(string.Format("Couldn't create drawer for {0} make sure it is registered.", item.GetType().Name));
                drawers.Add(drawer);
            }
            drawers.Add(new SectionHeaderDrawer(new SectionHeaderViewModel()
            {
                Name = "Collections",
                AddCommand = new SimpleEditorCommand<ClassNodeViewModel>(n =>
                {
                    n.AddCollection();
                })
            }));
            foreach (var item in ViewModel.ContentItems.Where(p=>p.DataObject is ClassCollectionData))
            {
                var drawer = InvertGraphEditor.CreateDrawer(item);
                if (drawer == null) Debug.Log(string.Format("Couldn't create drawer for {0} make sure it is registered.", item.GetType().Name));
                drawers.Add(drawer);
            }
     
        }
    }
}