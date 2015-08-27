namespace Invert.Core.GraphDesigner.Systems.GraphUI.api
{
    public class ActionItem
    {
        public ICommand Command { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
    }
}