using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Invert.uFrame.Editor
{
    public class UFrameSettings
    {
        [SerializeField]
        private Color? _associationLinkColor = Color.white;
        [SerializeField, HideInInspector]
        private Color? _definitionLinkColor = Color.cyan;
        [SerializeField]
        private Color? _inheritanceLinkColor = Color.green;

        [SerializeField]
        private Color? _subSystemLinkColor = Color.grey;

        private Color? _transitionLinkColor;
        private Color? _viewLinkColor;
        private Color? _gridLinesColor;
        private Color? _backgroundColor;
        private Color? _gridLinesColorSecondary;

        public void SetColorPref(string name, Color value)
        {
            EditorPrefs.SetFloat(name + "R", value.r);
            EditorPrefs.SetFloat(name + "G", value.g);
            EditorPrefs.SetFloat(name + "B", value.b);
            EditorPrefs.SetFloat(name + "A", value.a);
        }

        public Color GetColorPref(string name, Color def)
        {
            var r =EditorPrefs.GetFloat(name + "R", def.r);
            var g =EditorPrefs.GetFloat(name + "G", def.g);
            var b =EditorPrefs.GetFloat(name + "B", def.b);
            var a = EditorPrefs.GetFloat(name + "A", def.a);
            return new Color(r,g,b,a);
        }
        public virtual Color AssociationLinkColor
        {
            get
            {
                if (_associationLinkColor == null)
                {
                    return (_associationLinkColor = GetColorPref("AssociationLinkColor", Color.white)).Value;
                }
                return _associationLinkColor.Value;
            }
            set
            {
                _associationLinkColor = value;
                SetColorPref("AssociationLinkColor",value);
            }
        }

        public virtual Color DefinitionLinkColor
        {
            get
            {
                if (_definitionLinkColor == null)
                {
                    return (_definitionLinkColor = GetColorPref("_definitionLinkColor", Color.white)).Value;
                }
                return _definitionLinkColor.Value;
            }
            set
            {
                _definitionLinkColor = value;
                SetColorPref("_definitionLinkColor", value);
            }
        }

        public virtual Color InheritanceLinkColor
        {
            get
            {
                if (_inheritanceLinkColor == null)
                {
                    return (_inheritanceLinkColor = GetColorPref("_inheritanceLinkColor", Color.green)).Value;
                }
                return _inheritanceLinkColor.Value;
            }
            set
            {
                _inheritanceLinkColor = value;
                SetColorPref("_inheritanceLinkColor", value);
            }
        }

        //public Color SceneManagerLinkColor
        //{
        //    get
        //    {
        //        if (_sceneManagerLinkColor == null)
        //        {
        //            return (_sceneManagerLinkColor = GetColorPref("_sceneManagerLinkColor", Color.white)).Value;
        //        }
        //        return _sceneManagerLinkColor.Value;
        //    }
        //    set
        //    {
        //        _associationLinkColor = value;
        //        SetColorPref("AssociationLinkColor", value);
        //    }
        //}

        public virtual Color SubSystemLinkColor
        {
            get
            {
                if (_subSystemLinkColor == null)
                {
                    return (_subSystemLinkColor = GetColorPref("_subSystemLinkColor", Color.white)).Value;
                }
                return _subSystemLinkColor.Value;
            }
            set
            {
                _subSystemLinkColor = value;
                SetColorPref("_subSystemLinkColor", value);
            }
        }

        public virtual Color TransitionLinkColor
        {
            get
            {
                if (_transitionLinkColor == null)
                {
                    return (_transitionLinkColor = GetColorPref("_transitionLinkColor", Color.white)).Value;
                }
                return _transitionLinkColor.Value;
            }
            set
            {
                _transitionLinkColor = value;
                SetColorPref("_transitionLinkColor", value);
            }
        }

        public virtual Color ViewLinkColor
        {
            get
            {
                if (_viewLinkColor == null)
                {
                    return (_viewLinkColor = GetColorPref("_viewLinkColor", Color.white)).Value;
                }
                return _viewLinkColor.Value;
            }
            set
            {
                _viewLinkColor = value;
                SetColorPref("_viewLinkColor", value);
            }
        }

        public virtual Color GridLinesColor
        {
            get
            {
                if (_gridLinesColor == null)
                {
                    return (_gridLinesColor = GetColorPref("_gridLinesColor", new Color(0.1f, 0.1f, 0.1f))).Value;
                }
                return _gridLinesColor.Value;
            }
            set
            {
                _gridLinesColor = value;
                SetColorPref("_gridLinesColor", value);
            }
        }
        public virtual Color GridLinesColorSecondary
        {
            get
            {
                if (_gridLinesColorSecondary == null)
                {
                    return (_gridLinesColorSecondary = GetColorPref("_gridLinesColorSecondary", new Color(0.08f, 0.08f, 0.08f))).Value;
                }
                return _gridLinesColorSecondary.Value;
            }
            set
            {
                _gridLinesColorSecondary = value;
                SetColorPref("_gridLinesColorSecondary", value);
            }
        }
        public virtual Color BackgroundColor
        {
            get
            {
                if (_backgroundColor == null)
                {
                    return (_backgroundColor = GetColorPref("_backgroundColor", new Color(0.13f, 0.13f, 0.13f))).Value;
                }
                return _backgroundColor.Value;
            }
            set
            {
                _backgroundColor = value;
                SetColorPref("_backgroundColor", value);
            }
        }

     
    }
}
