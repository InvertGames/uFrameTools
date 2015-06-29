using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Invert.Core.GraphDesigner
{
    public interface IAssetImported
    {
        void AssetImported(string filename);
    }
    public interface IAssetDeleted
    {
        void AssetDeleted(string filename);
    }
    public interface IAssetMoved
    {
        void AssetMoved(string from, string to);
    }
}
