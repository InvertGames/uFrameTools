using System;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    [Serializable]
    public class GeneratorSettings
    {
        //[SerializeField]
        private bool _generateControllers = true;

        [SerializeField]
        private bool _generateComments = true;

        public bool GenerateControllers
        {
            get { return _generateControllers; }
            set { _generateControllers = value; }
        }

        public DefaultNamespaceProvider NamespaceProvider
        {
            get { return _namespaceProvider; }
            set { _namespaceProvider = value; }
        }

        public bool GenerateComments
        {
            get { return _generateComments; }
            set { _generateComments = value; }
        }

        [SerializeField]
        private DefaultNamespaceProvider _namespaceProvider;
    }
}