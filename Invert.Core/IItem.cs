namespace Invert.Core
{
    public interface IItem
    {
        string Title { get; }
        string Group { get; }
        string SearchTag { get; }
        string Description { get; set; }
    }

    public class DefaultItem : IItem
    {
        public DefaultItem(string title, string @group)
        {
            Title = title;
            Group = @group;
            SearchTag = Title + " " + group;
        }

        public string Title { get; set; }
        public string Group { get; set; }
        public string SearchTag { get; set; }
        public string Description { get; set; }
    }


}