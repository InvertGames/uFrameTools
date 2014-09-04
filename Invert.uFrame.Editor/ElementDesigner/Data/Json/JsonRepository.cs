using System;
using System.Collections.Generic;
using System.Linq;
using Invert.uFrame.Editor;
using Microsoft.SqlServer.Server;
using UnityEditor;
using UnityEngine;

public class JsonRepository : DefaultElementsRepository
{
    public override Type RepositoryFor
    {
        get { return typeof (JsonElementDesignerData); }
    }

    public override void CreateNewDiagram()
    {
        _diagrams.Add(UFrameAssetManager.CreateAsset<JsonElementDesignerData>());
    }


    protected override void Refresh()
    {
        Diagrams = GetAssets().OfType<JsonElementDesignerData>().ToList();

        foreach (var diagram in Diagrams)
        {
            diagram.Prepare();
        }
#if DEBUG
        Debug.Log(string.Join(Environment.NewLine, Diagrams.Select(p => p.Name + ":" + p.Identifier).ToArray()));
#endif
        DiagramNames = Diagrams.Select(p => p.Name).ToArray();
    }

    public override IEnumerable<IDiagramNode> AllNodes
    {
        get
        {
            foreach (var item in Diagrams)
            {
                foreach (var node in item.LocalNodes)
                {
                    yield return node;
                }
            }
        }
    }

}