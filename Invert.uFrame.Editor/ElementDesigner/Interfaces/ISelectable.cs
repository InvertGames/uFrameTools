using UnityEngine;

public interface ISelectable : IGraphItem
{
    bool IsSelected { get; set; }
    void RemoveLink(IDiagramNode target);
   // Vector2[] ConnectionPoints { get; set; }
}