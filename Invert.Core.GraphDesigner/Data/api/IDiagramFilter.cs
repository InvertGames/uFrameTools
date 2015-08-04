using Invert.Data;

namespace Invert.Core.GraphDesigner
{
    public interface IDiagramFilter : IDataRecord
    {
        //string Identifier { get; }
        bool ImportedOnly { get; }
        bool IsExplorerCollapsed { get; set; }

        string Name { get; set; }
        bool UseStraightLines { get; }

        //bool IsAllowed(object item, Type t);
        //bool IsItemAllowed(object item, Type t);
    }
}