using Invert.uFrame.Editor;
using UnityEngine;

public interface IDiagramNodeItem : ISelectable, IJsonObject,IItem
{
    string Name { get; set; }
    string Highlighter { get; }
    string FullLabel { get; }
    bool IsSelectable { get;}
    DiagramNode Node { get; set; }
    DataBag DataBag { get; set; }
        
    /// <summary>
    /// Is this node currently in edit mode/ rename mode.
    /// </summary>
    bool IsEditing { get; set; }

    //void Remove(IDiagramNode diagramNode);
    void Rename(IDiagramNode data, string name);
    void NodeRemoved(IDiagramNode item);
    void NodeItemRemoved(IDiagramNodeItem nodeItem);
}