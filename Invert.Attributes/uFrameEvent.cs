using System;

namespace uFrame.Attributes
{
    public class uFrameEvent : Attribute
    {
        public string Title { get; set; }

        public uFrameEvent()
        {
        }

        public uFrameEvent(string title)
        {
            Title = title;
        }

    }

    public class uFrameCategory : Attribute
    {
        public string[] Title { get; set; }
        public uFrameCategory(params string[] title)
        {
            Title = title;
        }

    }
}