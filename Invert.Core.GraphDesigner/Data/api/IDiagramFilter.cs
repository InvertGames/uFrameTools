namespace Invert.Core.GraphDesigner
{
    public interface IDiagramFilter
    {
        string Identifier { get; }
        bool ImportedOnly { get; }
        bool IsExplorerCollapsed { get; set; }

        FilterLocations Locations { get; set; }
        FilterCollapsedDictionary CollapsedValues { get; set; }
        string Name { get; set; }

        //bool IsAllowed(object item, Type t);
        //bool IsItemAllowed(object item, Type t);
    }
}