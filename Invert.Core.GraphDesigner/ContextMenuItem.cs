using System.Reflection;

namespace Invert.Core.GraphDesigner
{
    public class ContextMenuItem
    {
        public MethodInfo ActionMethod { get; set; }

        public DiagramContextMenuAttribute Attribute { get; set; }

        public MethodInfo IsCheckedMethod { get; set; }
    }
}