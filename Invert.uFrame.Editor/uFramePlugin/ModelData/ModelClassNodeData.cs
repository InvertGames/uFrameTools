using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.Core.GraphDesigner;

public class ModelClassNodeData : ClassNodeData
{
    public string NameAsAssetClass
    {
        get
        {
            return string.Format("{0}Asset", Name);
        }
    }

    public string NameAsInterface
    {
        get
        {
            return string.Format("I{0}", this.Name);
        }
    }
}