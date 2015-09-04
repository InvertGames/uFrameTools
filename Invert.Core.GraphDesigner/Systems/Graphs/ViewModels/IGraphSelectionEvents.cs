namespace Invert.Core.GraphDesigner
{
    public interface IGraphSelectionEvents
    {
        void SelectionChanged(GraphItemViewModel selected);
    }

    public interface INothingSelectedEvent
    {
        void NothingSelected();
    }
}