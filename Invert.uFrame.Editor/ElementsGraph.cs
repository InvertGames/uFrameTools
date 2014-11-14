
    using UnityEngine;
    public class ElementsGraph : GraphData
    {
        protected override IDiagramFilter CreateDefaultFilter()
        {
            return new SceneFlowFilter();
        }

    }