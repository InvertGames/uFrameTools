using System;
using Invert.Data;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public interface IGraphItem : IItem, IDataRecord
    {
        [Obsolete]
        Rect Position { get; set; }
        string Label { get; }
        //string Identifier { get; set; }
        bool IsValid { get; }
        IGraphItem Copy();

    }

     
}