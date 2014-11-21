using System.Collections.Generic;
using Invert.uFrame.Editor;
using UnityEngine;

public interface IGraphItem : IItem
{
    Rect Position { get; set; }
    string Label { get; }
    string Identifier { get; }
    bool IsValid { get; }
    IGraphItem Copy();
}