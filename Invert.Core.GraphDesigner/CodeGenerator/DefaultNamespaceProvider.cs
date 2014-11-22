using System;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    [Serializable]
    public class DefaultNamespaceProvider : INamespaceProvider
    {
        [SerializeField]
        private string _rootNamespace;

        public string RootNamespace
        {
            get { return _rootNamespace; }
            set { _rootNamespace = value; }
        }

        public string GetNamespace(IDiagramNode node)
        {
            return RootNamespace;
        }
    }
}