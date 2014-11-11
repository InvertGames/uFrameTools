using UnityEngine;

public interface ISelectable : IGraphItem
{
    bool IsSelected { get; set; }
}