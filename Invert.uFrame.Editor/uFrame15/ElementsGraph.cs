
    using Invert.Core.GraphDesigner;
    using UnityEngine;
    public class ElementsGraph : UnityGraphData
    {

        public override IDiagramFilter CreateDefaultFilter()
        {
            return new SceneFlowFilter();
        }

    }