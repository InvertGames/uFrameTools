using System;
using UnityEngine;

namespace Invert.uFrame.Editor
{
    [Serializable]
    public class GeneratorSettings
    {
        [SerializeField]
        private bool _generateControllers = true;

        public bool GenerateControllers
        {
            get { return _generateControllers; }
            set { _generateControllers = value; }
        }
    }
}