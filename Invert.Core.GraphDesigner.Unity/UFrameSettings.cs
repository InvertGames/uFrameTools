using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Invert.Core.GraphDesigner.Settings;
using UnityEditor;
using UnityEngine;

namespace Invert.uFrame.Editor
{


    public class UFrameSettings : IGraphEditorSettings
    {
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

        public virtual bool UseGrid
        {
            get { return Convert.ToBoolean(PlayerPrefs.GetInt("UseGrid", Convert.ToInt32(true))); }
            set
            {
                PlayerPrefs.SetInt("UseGrid",Convert.ToInt32(value));
            }
        }
        public virtual bool ShowHelp
        {
            get { return Convert.ToBoolean(PlayerPrefs.GetInt("ShowHelp", Convert.ToInt32(true))); }
            set
            {
                PlayerPrefs.SetInt("ShowHelp", Convert.ToInt32(value));
            }
        }

        public virtual bool ShowGraphDebug
        {
            get { return Convert.ToBoolean(PlayerPrefs.GetInt("ShowGraphDebug", Convert.ToInt32(false))); }
            set
            {
                PlayerPrefs.SetInt("ShowGraphDebug", Convert.ToInt32(value));
            }
        }
     
    }


}
