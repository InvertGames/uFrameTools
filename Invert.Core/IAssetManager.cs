using System;
using System.Collections.Generic;
using Invert.uFrame.Editor;

namespace Invert.Core
{
    public interface IAssetManager
    {
        object CreateAsset(Type type);
        object LoadAssetAtPath(string path, Type repositoryFor);

        IEnumerable<object> GetAssets(Type type);
    }

    public interface IWindowManager
    {
        void InitTypeListWindow(GraphTypeInfo[] typesInfoList, Action<GraphTypeInfo> action);

        void InitItemWindow<TItem>(IEnumerable<TItem> items, Action<TItem> action)
            where TItem : IItem;
    }
}