using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.MVVM;
using Invert.uFrame;
using Invert.uFrame.Editor;
using Invert.uFrame.Editor.ViewModels;
using UnityEngine;


    public class ComputedPropertyData : DiagramNode
    {
        private List<string> _dependantProperties = new List<string>();

        public override IEnumerable<IDiagramNodeItem> Items
        {
            get { yield break; }
        }

        public List<string> DependantProperties
        {
            get { return _dependantProperties; }
            set { _dependantProperties = value; }
        }

        public override string Label
        {
            get { return this.Name; }
        }

        //public override bool CanCreateLink(IGraphItem target)
        //{
        //    return false;
        //}

        //public override IEnumerable<IDiagramLink> GetLinks(IDiagramNode[] nodes)
        //{
        //    yield break;
        //}

        //public override void RemoveLink(IDiagramNode target)
        //{
            
        //}

        //public override void CreateLink(IDiagramNode container, IGraphItem target)
        //{
            
        //}

        public override IEnumerable<IDiagramNodeItem> ContainedItems
        {
            get
            {
                yield break;
            }
            set {  }
        }

        public string PropertyIdentifier { get; set; }
    }

    public class ComputedPropertyNodeViewModel : DiagramNodeViewModel<ComputedPropertyData>
    {
        public ComputedPropertyNodeViewModel(ComputedPropertyData graphItemObject, DiagramViewModel diagramViewModel) : base(graphItemObject, diagramViewModel)
        {
        }
    }

    public class ComputedPropertyDrawer : DiagramNodeDrawer<ComputedPropertyNodeViewModel>
    {
        public ComputedPropertyDrawer()
        {
        }

        public ComputedPropertyDrawer(ComputedPropertyNodeViewModel viewModel) : base(viewModel)
        {
        }
    }


    public class ComputedPropertyPlugin : DiagramPlugin
    {
        public override void Initialize(uFrameContainer container)
        {
            uFrameEditor.RegisterGraphItem<ComputedPropertyData,ComputedPropertyNodeViewModel,ComputedPropertyDrawer>();
            uFrameEditor.RegisterFilterNode<ElementData, ComputedPropertyData>();
            
            
        }
    }
