using System;

namespace Invert.Core.GraphDesigner
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ShowInSettings : Attribute
    {
        public ShowInSettings(string @group)
        {
            Group = @group;
        }

        public string Group { get; set; }
    }
}