using System.Collections.Generic;
using UnityEngine;

public interface IGraphItem
{
    Rect Position { get; set; }
    string Label { get; }
    string Identifier { get; }


    //void CreateLink(IDiagramNode container, IGraphItem target);
    //bool CanCreateLink(IGraphItem target);
    //IEnumerable<IDiagramLink> GetLinks(IDiagramNode[] nodes);

    
}