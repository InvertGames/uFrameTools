public class ElementsGraph : GraphData
{
    protected override IDiagramFilter CreateDefaultFilter()
    {
        return new SceneFlowFilter();
    }

}
public class JsonElementDesignerData : ElementsGraph
{

}