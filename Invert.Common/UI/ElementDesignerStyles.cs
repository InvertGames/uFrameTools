using System.Reflection;
using Invert.Common.UI;
using UnityEditor;
using UnityEngine;

namespace Invert.Common
{
    public static class ElementDesignerStyles
    {
        private static GUIStyle _diagramBox1;
        private static GUIStyle _headerStyle;
        private static GUIStyle _background;
        private static GUIStyle _selectedItemStyle;
        private static GUIStyle _diagramBox2;
        private static Texture2D _arrowDownTexture;
        private static Texture2D _arrowLeftTexture;
        private static Texture2D _arrowUpTexture;
        private static Texture2D _arrowRightTexture;
        private static Texture2D _circleTexture;
        private static GUIStyle _viewModelHeaderStyle;
        private static GUIStyle _diagramBox3;
        private static GUIStyle _diagramBox4;
        private static GUIStyle _diagramBox5;
        private static GUIStyle _diagramBox6;
        private static GUIStyle _diagramBox7;
        private static GUIStyle _diagramBox8;
        private static GUIStyle _diagramBox9;
        private static GUIStyle _collapseArrowRight;
        private static GUIStyle _arrowDownButtonStyle;
        private static GUIStyle _boxHighlighter1;
        private static GUIStyle _boxHighlighter2;
        private static GUIStyle _boxHighlighter3;
        private static GUIStyle _boxHighlighter4;
        private static GUIStyle _itemStyle;
        private static GUIStyle _item6;
        private static GUIStyle _item1;
        private static GUIStyle _item2;
        private static GUIStyle _item3;
        private static GUIStyle _item4;
        private static GUIStyle _item5;
        private static GUIStyle _gridLines;
        private static GUIStyle _buttonStyle;
        private static GUIStyle _subHeaderStyle;
        private static GUIStyle _overlay;
        private static GUIStyle _titleBarStyle;
        private static GUIStyle _addButtonStyle;
        private static GUIStyle _clearItemStyle;
        private static GUIStyle _sceneViewBar;
        private static GUIStyle _eyeBall;
        private static GUIStyle _navigatePreviousStyle;
        private static GUIStyle _navigateNextStyle;
        private static GUIStyle _viewBarTitleStyle;
        private static GUIStyle _viewBarSubTitleStyle;

        public static string SkinName
        {
            get { return "images"; }
            //EditorGUIUtility.isProSkin ? "ProSkin" : "FreeSkin"; }
        }

        private static float _scale = 0.0f;
        private static GUIStyle _diagramTitle;
        private static GUIStyle _diagramBox10;
        private static GUIStyle _diagramBox11;
        private static GUIStyle _nodeBackground;
        private static GUIStyle _nodeHeader1;
        private static GUIStyle _nodeHeader2;
        private static GUIStyle _nodeHeader3;
        private static GUIStyle _nodeHeader4;
        private static GUIStyle _nodeHeader5;
        private static GUIStyle _nodeHeader6;
        private static GUIStyle _nodeHeader7;
        private static GUIStyle _nodeHeader8;
        private static GUIStyle _nodeHeader9;
        private static GUIStyle _nodeHeader10;
        private static GUIStyle _nodeHeader11;
        private static GUIStyle _nodeHeader12;
        private static GUIStyle _nodeHeader13;
        private static GUIStyle _nodeCollapse;
        private static GUIStyle _nodeExpand;
        private static Texture2D _circleRightTexture;
        private static Texture2D _circleLeftTexture;
        private static GUIStyle _boxHighlighter5;
        private static GUIStyle _tag1;
        private static GUIStyle _tag2;
        private static GUIStyle _viewModelHeaderEditingStyle;
        private static GUIStyle _itemTextEditingStyle;

        public static float Scale
        {
            get
            {
                if (_scale == 0.0f)
                {
                    return _scale = EditorPrefs.GetFloat("ElementDesigner_Scale", 1f);
                }
                return _scale;
            }
            set
            {
                EditorPrefs.SetFloat("ElementDesigner_Scale", value);
                _scale = value;
                foreach (var field in typeof(ElementDesignerStyles).GetFields(BindingFlags.Static | BindingFlags.NonPublic))
                {
                    if (field.FieldType == typeof(GUIStyle))
                        field.SetValue(null, null);
                }


            }
        }
        public static GUIStyle Tag1
        {
            get
            {
                if (_tag1 == null)
                    _tag1 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("Tag1"), textColor = Color.white },
                        padding = new RectOffset(8, 3, 3, 3),
                        fixedHeight = 19f * Scale,
                        stretchWidth = true,
                        fontSize = Mathf.RoundToInt(12 * Scale),
                        fontStyle = FontStyle.Bold
                        
                    };
                return _tag1;
            }
        }
        public static GUIStyle Tag2
        {
            get
            {
                if (_tag2 == null)
                    _tag2 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("Tag1"), textColor = Color.white },
                        padding = new RectOffset(7, 7, 5, 3),
                        border = new RectOffset(10,10,10,0),
                        fixedHeight = 19f * Scale,
                        stretchWidth = true,
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = Mathf.RoundToInt(12 * Scale),
                        fontStyle = FontStyle.Bold

                    };
                return _tag2;
            }
        }
        public static GUIStyle AddButtonStyle
        {
            get
            {
                if (_addButtonStyle == null)
                    _addButtonStyle = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("Plus"), textColor = Color.white },
                        padding = new RectOffset(10, 10, 10, 10),
                        fixedHeight = 16f * Scale,
                        fixedWidth = 16f * Scale
                    };
                return _addButtonStyle;
            }
        }
        public static GUIStyle ViewBarTitleStyle
        {
            get
            {
                if (_viewBarTitleStyle == null)
                    _viewBarTitleStyle = new GUIStyle
                    {
                        normal = { textColor = new Color(0.95f, 0.95f, 9.95f) },
                        alignment = TextAnchor.MiddleRight,
                        fontSize = 16,
                        fontStyle = FontStyle.Bold
                        //fontStyle = FontStyle.Bold
                    };
                return _viewBarTitleStyle;
            }
        }
        public static GUIStyle ViewBarSubTitleStyle
        {
            get
            {
                if (_viewBarSubTitleStyle == null)
                    _viewBarSubTitleStyle = new GUIStyle
                    {
                        normal = { textColor = new Color(0.7f, 0.7f, 0.7f) },
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 10,
                        //                    fontStyle = FontStyle.Bold
                        //fontStyle = FontStyle.Bold
                    };
                return _viewBarSubTitleStyle;
            }
        }
        public static GUIStyle TitleBarStyle
        {
            get
            {
                if (_titleBarStyle == null)
                    _titleBarStyle = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("Background"), textColor = new Color(0.7f, 0.7f, 0.7f) },
                        padding = new RectOffset(2, 2, 2, 2),
                        margin = new RectOffset(0, 0, 0, 0),
                        fixedHeight = 45f,
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 14,
                        fontStyle = FontStyle.Bold
                        //fontStyle = FontStyle.Bold
                    };
                return _titleBarStyle;
            }
        }


        public static Texture2D GetSkinTexture(string name)
        {
            return Resources.Load<Texture2D>(string.Format("{0}/{1}", SkinName, name));
        }
        public static GUIStyle SubHeaderStyle
        {
            get
            {
                if (_subHeaderStyle == null)
                    _subHeaderStyle = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("EventButton"), textColor = EditorGUIUtility.isProSkin ? new Color(0.6f, 0.6f, 0.6f) : Color.black },
                        padding = new RectOffset(2, 2, 2, 2),
                        margin = new RectOffset(0, 0, 0, 0),
                        fixedHeight = 18f,
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 9,
                        //fontStyle = FontStyle.Bold
                    };
                return _subHeaderStyle;
            }
        }
        public static GUIStyle NodeCollapse
        {
            get
            {
                if (_nodeCollapse == null)
                    _nodeCollapse = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("NodeCollapse"), textColor = Color.white },

                      
                    };
                return _nodeCollapse;
            }
        }
        public static GUIStyle NodeExpand
        {
            get
            {
                if (_nodeExpand== null)
                    _nodeExpand = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("NodeExpand"), textColor = Color.white },
                      
                   
                    };
                return _nodeExpand;
            }
        }
        public static GUIStyle CollapseRight
        {
            get
            {
                if (_collapseArrowRight == null)
                    _collapseArrowRight = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("CollapseRight"), textColor = Color.white },
                        margin = new RectOffset(35, 0, 10, 0),
                        padding = new RectOffset(10, 0, 0, 0),
                        fixedHeight = 16f * Scale,
                        fixedWidth = 16f * Scale
                    };
                return _collapseArrowRight;
            }
        }
        public static GUIStyle CollapseDown
        {
            get
            {
                if (_arrowDownButtonStyle == null)
                    _arrowDownButtonStyle = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("CollapseDown"), textColor = Color.white },
                        margin = new RectOffset(35, 0, 8, 0),
                        padding = new RectOffset(25, 0, 0, 0),
                        fixedHeight = 16f * Scale,
                        fixedWidth = 16f * Scale
                    };
                return _arrowDownButtonStyle;
            }
        }
        public static GUIStyle SceneViewBar
        {
            get
            {
                //if (_diagramBox1 == null)
                _sceneViewBar = new GUIStyle
                {
                    normal = { background = GetSkinTexture("SceneViewBar"), textColor = new Color(0.3f, 0.3f, 0.3f) },

                    stretchHeight = true,
                    stretchWidth = true,
                    // border = new RectOffset(44, 50, 20, 34),
                    padding = new RectOffset(7, 7, 7, 7)
                };

                return _sceneViewBar;
            }
        }
        public static GUIStyle NodeHeader1
        {
            get
            {
                return _nodeHeader1 ?? (_nodeHeader1 = CreateHeader("Header1", new Color(0.8f, 0.8f, 0.8f)));
            }
        }
        public static GUIStyle NodeHeader2
        {
            get
            {
                return _nodeHeader2 ?? (_nodeHeader2 = CreateHeader("Header2", new Color(0.82f, 0.82f, 0.82f)));
            }
        }
        public static GUIStyle NodeHeader3
        {
            get
            {
                return _nodeHeader3 ?? (_nodeHeader3 = CreateHeader("Header3", new Color(0.23f, 0.23f, 0.23f)));
            }
        }
        public static GUIStyle NodeHeader4
        {
            get
            {
                return _nodeHeader4 ?? (_nodeHeader4 = CreateHeader("Header4", new Color(0.82f, 0.82f, 0.82f)));
            }
        }
        public static GUIStyle NodeHeader5
        {
            get
            {
                return _nodeHeader5 ?? (_nodeHeader5 = CreateHeader("Header5", new Color(0.82f, 0.82f, 0.82f)));
            }
        }
        public static GUIStyle NodeHeader6
        {
            get
            {
                return _nodeHeader6 ?? (_nodeHeader6 = CreateHeader("Header6", new Color(0.82f, 0.82f, 0.82f)));
            }
        }
        public static GUIStyle NodeHeader7
        {
            get
            {
                return _nodeHeader7 ?? (_nodeHeader7 = CreateHeader("Header7", new Color(0.82f, 0.82f, 0.82f)));
            }
        }
        public static GUIStyle NodeHeader8
        {
            get
            {
                return _nodeHeader8 ?? (_nodeHeader8 = CreateHeader("Header8", new Color(0.82f, 0.82f, 0.82f)));
            }
        }
        public static GUIStyle NodeHeader9
        {
            get
            {
                return _nodeHeader9 ?? (_nodeHeader9 = CreateHeader("Header9", new Color(0.82f, 0.82f, 0.82f)));
            }
        }
        public static GUIStyle NodeHeader10
        {
            get
            {
                return _nodeHeader10 ?? (_nodeHeader10 = CreateHeader("Header10", new Color(0.82f, 0.82f, 0.82f)));
            }
        }
        public static GUIStyle NodeHeader11
        {
            get
            {
                return _nodeHeader11 ?? (_nodeHeader11 = CreateHeader("Header11", new Color(0.82f, 0.82f, 0.82f)));
            }
        }
        public static GUIStyle NodeHeader12
        {
            get
            {
                return _nodeHeader12 ?? (_nodeHeader12 = CreateHeader("Header12", new Color(0.82f, 0.82f, 0.82f)));
            }
        }
        public static GUIStyle NodeHeader13
        {
            get
            {
                return _nodeHeader13 ?? (_nodeHeader13 = CreateHeader("Header13", new Color(0.1f, 0.1f, 0.1f)));
            }
        }
        public static GUIStyle CreateHeader(string texture,Color color)
        {
           return new GUIStyle
            {
                normal = { background = GetSkinTexture(texture), textColor = color },

                stretchHeight = true,
                stretchWidth = true,
               // fixedHeight = 36,
                // border = new RectOffset(44, 50, 20, 34),
                //padding = new RectOffset(7, 7, 7, 7)
            };
        }
        public static GUIStyle NodeBackground
        {
            get
            {
                //if (_diagramBox1 == null)
                _nodeBackground = new GUIStyle
                {
                    normal = { background = GetSkinTexture("Box12"), textColor = new Color(0.82f, 0.82f, 0.82f) },

                    stretchHeight = true,
                    stretchWidth = true,
                    // border = new RectOffset(44, 50, 20, 34),
                    padding = new RectOffset(7, 7, 7, 7)
                };

                return _nodeBackground;
            }
        }
        public static GUIStyle DiagramBox1
        {
            get
            {
                //if (_diagramBox1 == null)
                _diagramBox1 = new GUIStyle
                {
                    normal = { background = GetSkinTexture("Box1"), textColor = new Color(0.3f, 0.3f, 0.3f) },

                    stretchHeight = true,
                    stretchWidth = true,
                    // border = new RectOffset(44, 50, 20, 34),
                    padding = new RectOffset(7, 7, 7, 7)
                };

                return _diagramBox1;
            }
        }
        public static GUIStyle DiagramBox2
        {
            get
            {
                if (_diagramBox2 == null)
                    _diagramBox2 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("Box2"), textColor = Color.white },

                        stretchHeight = true,
                        stretchWidth = true,
                        border = new RectOffset(171, 180, 120, 135),
                        padding = new RectOffset(7, 7, 7, 7)
                    };

                return _diagramBox2;
            }
        }
        public static GUIStyle DiagramBox3
        {
            get
            {
                if (_diagramBox3 == null)
                    _diagramBox3 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("Box3"), textColor = new Color(0.2f, 0.2f, 0.2f) },

                        stretchHeight = true,
                        stretchWidth = true,
                        border = new RectOffset(171, 180, 120, 135),
                        padding = new RectOffset(7, 7, 7, 7)
                    };

                return _diagramBox3;
            }
        }
        public static GUIStyle DiagramBox4
        {
            get
            {
                if (_diagramBox4 == null)
                    _diagramBox4 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("Box4"), textColor = new Color(0.22f, 0.22f, 0.22f) },
                        stretchHeight = true,
                        stretchWidth = true,
                        border = new RectOffset(171, 180, 120, 135),
                        padding = new RectOffset(7, 7, 7, 7)
                    };

                return _diagramBox4;
            }
        }
        public static GUIStyle DiagramBox5
        {
            get
            {
                if (_diagramBox5 == null)
                    _diagramBox5 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("Box5"), textColor = Color.white },
                        stretchHeight = true,
                        stretchWidth = true,
                        border = new RectOffset(171, 180, 120, 135),
                        padding = new RectOffset(7, 7, 7, 7)
                    };

                return _diagramBox5;
            }
        }
        public static GUIStyle DiagramBox6
        {
            get
            {
                if (_diagramBox6 == null)
                    _diagramBox6 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("Box6"), textColor = Color.white },
                        stretchHeight = true,
                        stretchWidth = true,
                        border = new RectOffset(171, 180, 120, 135),
                        padding = new RectOffset(7, 7, 7, 7)
                    };

                return _diagramBox6;
            }
        }
        public static GUIStyle DiagramBox7
        {
            get
            {
                if (_diagramBox7 == null)
                    _diagramBox7 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("Box7"), textColor = Color.white },
                        stretchHeight = true,
                        stretchWidth = true,
                        border = new RectOffset(171, 180, 120, 135),
                        padding = new RectOffset(7, 7, 7, 7)
                    };

                return _diagramBox7;
            }
        }
        public static GUIStyle DiagramBox8
        {
            get
            {
                if (_diagramBox8 == null)
                    _diagramBox8 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("Box8"), textColor = new Color(0.9f, 0.9f, 0.9f) },
                        stretchHeight = true,
                        stretchWidth = true,
                        border = new RectOffset(171, 180, 120, 135),
                        padding = new RectOffset(7, 7, 7, 7)
                    };

                return _diagramBox8;
            }
        }
        public static GUIStyle DiagramBox9
        {
            get
            {
                if (_diagramBox9 == null)
                    _diagramBox9 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("Box9"), textColor = Color.white },
                        stretchHeight = true,
                        stretchWidth = true,
                        border = new RectOffset(171, 180, 120, 135),
                        padding = new RectOffset(7, 7, 7, 7)
                    };

                return _diagramBox9;
            }
        }
        public static GUIStyle DiagramBox10
        {
            get
            {
                if (_diagramBox10 == null)
                    _diagramBox10 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("Box10"), textColor = Color.white },
                        stretchHeight = true,
                        stretchWidth = true,
                        border = new RectOffset(171, 180, 120, 135),
                        padding = new RectOffset(7, 7, 7, 7)
                    };

                return _diagramBox10;
            }
        }
        public static GUIStyle DiagramBox11
        {
            get
            {
                if (_diagramBox11 == null)
                    _diagramBox11 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("Box11"), textColor = Color.white },
                        stretchHeight = true,
                        stretchWidth = true,
                        border = new RectOffset(171, 180, 120, 135),
                        padding = new RectOffset(7, 7, 7, 7)
                    };

                return _diagramBox11;
            }
        }
        public static GUIStyle Item1
        {
            get
            {
                if (_item1 == null)
                    _item1 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("Item1"), textColor = Color.white },
                        stretchHeight = true,
                        stretchWidth = true,
                        fixedHeight = 18f * Scale,
                        fontSize = Mathf.RoundToInt(9f * Scale),
                        alignment = TextAnchor.MiddleCenter
                    };

                return _item1;
            }
        }
        public static GUIStyle Item2
        {
            get
            {
                if (_item2 == null)
                    _item2 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("Item2"), textColor = Color.white },
                        stretchHeight = true,
                        stretchWidth = true,
                        //fixedHeight = 18f * Scale,
                        fontSize = Mathf.RoundToInt(9f * Scale),
                        alignment = TextAnchor.MiddleCenter
                    };

                return _item2;
            }
        }
        public static GUIStyle Item3
        {
            get
            {
                if (_item3 == null)
                    _item3 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("Item3"), textColor = Color.white },
                        stretchHeight = true,
                        stretchWidth = true,
                        //fixedHeight = 18f * Scale,
                        fontSize = Mathf.RoundToInt(9f * Scale),
                        alignment = TextAnchor.MiddleCenter
                    };

                return _item3;
            }
        }
        public static GUIStyle Overlay
        {
            get
            {
                if (_overlay == null)
                    _overlay = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("Item1"), textColor = Color.white },
                        stretchHeight = true,
                        stretchWidth = true,
                        fontSize = Mathf.RoundToInt(16f * Scale),
                        alignment = TextAnchor.MiddleCenter
                    };

                return _overlay;
            }
        }
        public static GUIStyle ClearItemStyle
        {
            get
            {
                if (_clearItemStyle == null)
                    _clearItemStyle = new GUIStyle
                    {
                        normal = { textColor = new Color(0.6f,0.6f,0.8f) },
                        stretchHeight = true,
                        stretchWidth = true,
                        padding = new RectOffset(5,0,0,0),
                        fontSize = Mathf.RoundToInt(8f * Scale),
                        fixedHeight = 18f * Scale,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleLeft
                    };

                return _clearItemStyle;
            }
        }
        public static GUIStyle Item4
        {
            get
            {
                if (_item4 == null)
                    _item4 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("Item4"), textColor = Color.white },
                        stretchHeight = true,
                        stretchWidth = true,
                        fontSize = Mathf.RoundToInt(9f * Scale),
                        fixedHeight = 18f * Scale,
                        alignment = TextAnchor.MiddleCenter
                    };

                return _item4;
            }
        }
        public static GUIStyle Item5
        {
            get
            {
                if (_item5 == null)
                    _item5 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("Item5"), textColor = new Color(0.8f, 0.8f, 0.8f) },
                        stretchHeight = true,
                        fontSize = Mathf.RoundToInt(9 * Scale),
                        alignment = TextAnchor.MiddleCenter,
                        fixedHeight = 18f * Scale,
                    };

                return _item5;
            }
        }
        public static GUIStyle Item6
        {
            get
            {
                if (_item6 == null)
                    _item6 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("Item6"), textColor = Color.white },
                        stretchHeight = true,
                        fixedHeight = 18f * Scale,
                        fontSize = Mathf.RoundToInt(9 * Scale),
                        alignment = TextAnchor.MiddleCenter
                    };

                return _item6;
            }
        }
        public static GUIStyle BoxHighlighter2
        {
            get
            {
                if (_boxHighlighter1 == null)
                    _boxHighlighter1 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("BoxHighlighter2"), textColor = Color.white },
                        stretchHeight = true,
                        stretchWidth = true,
                        border = new RectOffset(44, 44, 35, 35),
                        padding = new RectOffset(10, 10, 7, 7),
                        margin = new RectOffset(7, 7, 7, 7)
                    };

                return _boxHighlighter1;
            }
        }
        public static GUIStyle BoxHighlighter1
        {
            get
            {
                if (_boxHighlighter2 == null)
                    _boxHighlighter2 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("BoxHighlighter1"), textColor = Color.white },
                        stretchHeight = true,
                        stretchWidth = true,
                        border = new RectOffset(171, 180, 120, 135),
                        padding = new RectOffset(7, 7, 7, 7)
                    };

                return _boxHighlighter2;
            }
        }
        public static GUIStyle BoxHighlighter3
        {
            get
            {
                if (_boxHighlighter3 == null)
                    _boxHighlighter3 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("BoxHighlighter3"), textColor = Color.white },
                        stretchHeight = true,
                        stretchWidth = true,
                        border = new RectOffset(44, 44, 35, 35),
                        padding = new RectOffset(7, 7, 7, 7)
                    };

                return _boxHighlighter3;
            }
        }
        public static GUIStyle BoxHighlighter4
        {
            get
            {
                if (_boxHighlighter4 == null)
                    _boxHighlighter4 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("BoxHighlighter4"), textColor = Color.white },
                        stretchHeight = true,
                        stretchWidth = true,
                        border = new RectOffset(171, 180, 120, 135),
                        padding = new RectOffset(7, 7, 7, 7)
                    };

                return _boxHighlighter4;
            }
        }
        public static GUIStyle BoxHighlighter5
        {
            get
            {
                if (_boxHighlighter5 == null)
                    _boxHighlighter5 = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("BoxHighlighter5"), textColor = Color.white },
                        stretchHeight = true,
                        stretchWidth = true,
                        border = new RectOffset(171, 180, 120, 135),
                        padding = new RectOffset(7, 7, 7, 7)
                    };

                return _boxHighlighter5;
            }
        }
        public static GUIStyle Background
        {
            get
            {
                if (_background == null)
                    _background = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("Background"), textColor = Color.white },
                        stretchHeight = true,
                        stretchWidth = true,
                        border = new RectOffset(171, 180, 120, 135),
                        padding = new RectOffset(7, 7, 7, 7)
                    };

                return _background;
            }
        }
        public static GUIStyle GridLines
        {
            get
            {
                if (_gridLines == null)
                    _gridLines = new GUIStyle
                    {
                        normal = { background = GetSkinTexture("GridLines"), textColor = Color.white },
                        stretchHeight = false,
                        stretchWidth = false,

                        //border = new RectOffset(171, 180, 120, 135),
                        //padding = new RectOffset(7, 7, 7, 7)
                    };

                return _gridLines;
            }
        }
        public static GUIStyle DiagramTitle
        {
            get
            {
                if (_diagramTitle == null)
                {
                    _diagramTitle = new GUIStyle()
                    {
                        normal = { background = GetSkinTexture("SelectedNodeItem"), textColor = new Color(0.45f, 0.45f, 0.45f) },
                        padding = new RectOffset(4, 4, 4, 4),
                        stretchWidth = true,
                        stretchHeight = true,

                        fontSize = 18,// Mathf.RoundToInt(10f * Scale),
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleLeft
                    };
                }
                return _diagramTitle;
            }
        }
        public static GUIStyle HeaderStyle
        {
            get
            {
                if (_headerStyle == null)
                {
                    _headerStyle = new GUIStyle()
                    {
                        normal = { textColor = new Color(0.35f, 0.35f, 0.35f) },
                        padding = new RectOffset(4, 4, 4, 4),
                        fontSize = Mathf.RoundToInt(11f * Scale),
                        alignment = TextAnchor.MiddleLeft
                    };
                }
                return _headerStyle;
            }
        }
        public static GUIStyle SelectedItemStyle
        {
            get
            {
                if (_selectedItemStyle == null)
                {
                    _selectedItemStyle = new GUIStyle()
                    {
                        normal = {  textColor = new Color(0.7f,0.7f,0.7f) },
                        active = {  textColor = new Color(0.7f, 0.7f, 0.7f) },
                        stretchHeight = true,
                        padding = new RectOffset(0,5,0,0),
                        fixedHeight = Mathf.RoundToInt(18 * Scale),
                        fontSize = Mathf.RoundToInt(10 * Scale),
                        alignment = TextAnchor.MiddleRight
                    };
                }
                return _selectedItemStyle;
            }
        }
        public static GUIStyle ViewModelHeaderStyle
        {
            get
            {
                if (_viewModelHeaderStyle == null)
                {
                    _viewModelHeaderStyle = new GUIStyle(GUIStyle.none)
                    {
                        normal = { textColor = Color.white },
                        //padding = new RectOffset(0, 12, 10, 4),
                        fixedHeight = 25 * Scale,
                        fontSize = Mathf.RoundToInt(12 * Scale),

                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleLeft
                    };
                }
                return _viewModelHeaderStyle;
            }
        }   
        public static GUIStyle ViewModelHeaderEditingStyle
        {
            get
            {
                if (_viewModelHeaderEditingStyle == null)
                {
                    _viewModelHeaderEditingStyle = new GUIStyle(EditorStyles.textField);
                    _viewModelHeaderEditingStyle.normal.background = null;
                    _viewModelHeaderEditingStyle.active.background = null;
                    _viewModelHeaderEditingStyle.hover.background = null;
                    _viewModelHeaderEditingStyle.focused.background = null;
                    _viewModelHeaderEditingStyle.alignment = TextAnchor.MiddleCenter;
                    _viewModelHeaderEditingStyle.fontStyle = FontStyle.Bold;
                    //_viewModelHeaderEditingStyle.fontSize = Mathf.RoundToInt(12*Scale);
                }
                return _viewModelHeaderEditingStyle;
            }
        }
        public static GUIStyle ItemTextEditingStyle
        {
            get
            {
                if (_itemTextEditingStyle == null)
                {
                    _itemTextEditingStyle = new GUIStyle(EditorStyles.miniTextField);
                    _itemTextEditingStyle.normal.background = null;
                    _itemTextEditingStyle.active.background = null;
                    _itemTextEditingStyle.hover.background = null;
                    _itemTextEditingStyle.focused.background = null;
                    _itemTextEditingStyle.alignment = TextAnchor.MiddleCenter;
                    _itemTextEditingStyle.fontStyle = FontStyle.Bold;
                    _itemTextEditingStyle.padding = new RectOffset(0,0,0,0);
                    //_viewModelHeaderEditingStyle.fontSize = Mathf.RoundToInt(12*Scale);
                }
                return _itemTextEditingStyle;
            }
        }
        public static Texture2D ArrowLeftTexture
        {
            get
            {
                return _arrowLeftTexture ?? (_arrowLeftTexture = GetSkinTexture("DiagramArrowLeft"));
            }
        }
        public static Texture2D ArrowRightTexture
        {
            get
            {
                return _arrowRightTexture ?? (_arrowRightTexture = GetSkinTexture("DiagramArrowRight"));
            }
        }
        public static Texture2D ArrowUpTexture
        {
            get
            {
                return _arrowUpTexture ?? (_arrowUpTexture = GetSkinTexture("DiagramArrowUp"));
            }
        }
        public static Texture2D ArrowDownTexture
        {
            get
            {
                return _arrowDownTexture ?? (_arrowDownTexture = GetSkinTexture("DiagramArrowDown"));
            }
        }
        public static Texture2D CircleRightTexture
        {
            get
            {
                return _circleRightTexture ?? (_circleRightTexture = GetSkinTexture("DiagramCircleRight"));
            }
        }
        public static Texture2D CircleLeftTexture
        {
            get
            {
                return _circleLeftTexture ?? (_circleLeftTexture = GetSkinTexture("DiagramCircleLeft"));
            }
        }
        public static GUIStyle ItemStyle
        {
            get
            {
                return Item5;
                //if (_itemStyle == null)
                //    _itemStyle = new GUIStyle
                //    {
                //        normal = { background = GetSkinTexture("Item5"), textColor = new Color(0.8f, 0.8f, 0.8f) },
                //        padding = new RectOffset(2, 6, 2, 2),
                //        margin = new RectOffset(0, 0, 0, 0),
                //        fixedHeight = 18f,

                //        alignment = TextAnchor.MiddleLeft,
                //        fontSize = 9,
                //        //fontStyle = FontStyle.Bold
                //    };
                //return _itemStyle;
            }
        }
        public static GUIStyle EyeBall
        {
            get
            {
                if (_eyeBall == null)
                {
                    var textColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f) : Color.black;
                    _eyeBall = new GUIStyle
                    {
                        normal =
                        {
                            background = GetSkinTexture("EyeBall"),
                            textColor = textColor
                        },
                        focused =
                        {
                            background = GetSkinTexture("EyeBall"),
                            textColor = textColor
                        },
                        active = { background = GetSkinTexture("EyeBall2"), textColor = textColor },
                        hover = { background = GetSkinTexture("EyeBall"), textColor = textColor },
                        onHover =
                        {
                            background = GetSkinTexture("EyeBall"),
                            textColor = textColor
                        },
                        onFocused = { background = GetSkinTexture("EyeBall2"), textColor = textColor },
                        onNormal = { background = GetSkinTexture("EyeBall2"), textColor = textColor },
                        onActive = { background = GetSkinTexture("EyeBall2"), textColor = textColor },
                        fixedHeight = 48,
                        fixedWidth = 36,
                        alignment = TextAnchor.MiddleCenter,
                    };
                }
                return _eyeBall;
            }
        }
        public static GUIStyle NavigatePreviousStyle
        {
            get
            {
                if (_navigatePreviousStyle == null)
                {
                    var textColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f) : Color.black;
                    _navigatePreviousStyle = new GUIStyle
                    {
                        normal =
                        {
                            background = GetSkinTexture("NavigatePrevious"),
                            textColor = textColor
                        },
                        focused =
                        {
                            background = GetSkinTexture("NavigatePrevious2"),
                            textColor = textColor
                        },
                        active = { background = GetSkinTexture("NavigatePrevious"), textColor = textColor },
                        hover = { background = GetSkinTexture("NavigatePrevious2"), textColor = textColor },
                        onHover =
                        {
                            background = GetSkinTexture("NavigatePrevious"),
                            textColor = textColor
                        },
                        onFocused = { background = GetSkinTexture("NavigatePrevious2"), textColor = textColor },
                        onNormal = { background = GetSkinTexture("NavigatePrevious"), textColor = textColor },
                        onActive = { background = GetSkinTexture("NavigatePrevious"), textColor = textColor },
                        fixedHeight = 48,
                        fixedWidth = 36,
                        alignment = TextAnchor.MiddleCenter,
                    };
                }
                return _navigatePreviousStyle;
            }
        }
        public static GUIStyle NavigateNextStyle
        {
            get
            {
                if (_navigateNextStyle == null)
                {
                    var textColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f) : Color.black;
                    _navigateNextStyle = new GUIStyle
                    {
                        normal =
                        {
                            background = GetSkinTexture("NavigateNext"),
                            textColor = textColor
                        },
                        focused =
                        {
                            background = GetSkinTexture("NavigateNext2"),
                            textColor = textColor
                        },
                        active = { background = GetSkinTexture("NavigateNext"), textColor = textColor },
                        hover = { background = GetSkinTexture("NavigateNext2"), textColor = textColor },
                        onHover =
                        {
                            background = GetSkinTexture("NavigateNext"),
                            textColor = textColor
                        },
                        onFocused = { background = GetSkinTexture("NavigateNext2"), textColor = textColor },
                        onNormal = { background = GetSkinTexture("NavigateNext"), textColor = textColor },
                        onActive = { background = GetSkinTexture("NavigateNext2"), textColor = textColor },
                        fixedHeight = 48,
                        fixedWidth = 36,
                        alignment = TextAnchor.MiddleCenter,
                    };
                }
                return _navigateNextStyle;
            }
        }
        public static GUIStyle ButtonStyle
        {
            get
            {
                if (_buttonStyle == null)
                {
                    var textColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f) : Color.black;
                    _buttonStyle = new GUIStyle
                    {
                        normal =
                        {
                            background = GetSkinTexture("CommandBar"),
                            textColor = textColor
                        },
                        focused =
                        {
                            background = GetSkinTexture("CommandBar"),
                            textColor = textColor
                        },
                        active = { background = GetSkinTexture("EventButton"), textColor = textColor },
                        hover = { background = GetSkinTexture("CommandBar"), textColor = textColor },
                        onHover =
                        {
                            background = GetSkinTexture("CommandBar"),
                            textColor = textColor
                        },
                        onFocused = { background = GetSkinTexture("EventButton"), textColor = textColor },
                        onNormal = { background = GetSkinTexture("EventButton"), textColor = textColor },
                        onActive = { background = GetSkinTexture("EventButton"), textColor = textColor },
                        fixedHeight = 25,
                        alignment = TextAnchor.MiddleCenter,
                    };
                }
                return _buttonStyle;
            }
        }


        //public static void DrawNodeCurve(Rect start, Rect end, Color color, float width, NodeCurvePointStyle startStyle = NodeCurvePointStyle.Circle, NodeCurvePointStyle endStyle = NodeCurvePointStyle.Triangle)
        //{
        //    //var swap = start.center.x < end.center.x;
        //    var startPosLeft = new Vector3(start.x, (start.y + start.height / 2f) - 4, 0f);
        //    var startPosRight = new Vector3(start.x + start.width, (start.y + start.height / 2f) - 4, 0f);

        //    var endPosLeft = new Vector3(end.x, (end.y + end.height / 2f) + 4, 0f);
        //    var endPosRight = new Vector3(end.x + end.width, (end.y + end.height / 2f) + 4, 0f);

        //    Vector3 startPos;
        //    Vector3 endPos;
        //    int index1;
        //    int index2;
        //    FindClosestPoints(new[] { startPosLeft, startPosRight },
        //        new[] { endPosLeft, endPosRight }, out startPos, out endPos, out index1, out index2);

        //    //endPos = TestClosest(startPos, end);

        //    bool startRight = index1 == 0 && index2 == 0 ? index1 == 1 : index2 == 1;
        //    bool endRight = index1 == 1 && index2 == 1 ? index2 == 1 : index1 == 1;

        //    var startTan = startPos + (endRight ? Vector3.right : Vector3.left) * 50;

        //    var endTan = endPos + (startRight ? Vector3.right : Vector3.left) * 50;

        //    var shadowCol = new Color(0, 0, 0, 0.1f);

        //    for (int i = 0; i < 3; i++) // Draw a shadow

        //        Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);

        //    Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, width * Scale);

        //    startPos.y -= 8 * Scale;
        //    startPos.x -= 8 * Scale;
        //    if (startStyle == NodeCurvePointStyle.Circle)
        //    {

        //        GUI.DrawTexture(new Rect(startPos.x, startPos.y, 16f * Scale, 16f * Scale), CircleTexture);
        //    }
        //    else
        //    {
        //        GUI.DrawTexture(new Rect(startPos.x, startPos.y, 16f * Scale, 16f * Scale), startRight ? ArrowRightTexture : ArrowLeftTexture);
        //    }

        //    endPos.y -= 8 * Scale;
        //    endPos.x -= 8 * Scale;
        //    if (endStyle == NodeCurvePointStyle.Triangle)
        //    {
        //        GUI.DrawTexture(new Rect(endPos.x, endPos.y, 16f * Scale, 16f * Scale), startRight ? ArrowLeftTexture : ArrowRightTexture, ScaleMode.ScaleToFit);
        //    }
        //    else
        //    {
        //        GUI.DrawTexture(new Rect(endPos.x, endPos.y, 16f * Scale, 16f * Scale), CircleTexture, ScaleMode.ScaleToFit);
        //    }



        //}
        public static Vector3 TestClosest(Vector2 a, Rect b)
        {
            var result = new Vector2(a.x, a.y);
            result.y = Mathf.Clamp(a.y, b.y, b.y + b.height);
            result.x = Mathf.Clamp(a.x, b.x, b.x + b.width);


            return result;
        }//testclosest

        public static void DrawExpandableBox(Rect rect, GUIStyle style, string text,float offset = 12)
        {
            style.border = new RectOffset(
                Mathf.RoundToInt(offset),
                Mathf.RoundToInt(offset),
                Mathf.RoundToInt(offset),
                Mathf.RoundToInt(offset));

            GUI.Box(rect, text, style);

        }
        public static void DrawExpandableBox(Rect rect, GUIStyle style, string text, RectOffset offset)
        {
            style.border = offset;

            GUI.Box(rect, text, style);

        }

        public static void FindClosestPoints(Vector3[] a, Vector3[] b,
            out Vector3 pointA, out Vector3 pointB, out int indexA, out int indexB)
        {
            var distance = float.MaxValue;
            pointA = a[0];
            pointB = b[0];
            var i1 = 0;
            var i2 = 0;
            for (var i = 0; i < a.Length; i++)
            {
                var pA = a[i];
                for (var z = 0; z < b.Length; z++)
                {
                    var pB = b[z];

                    var newDistance = (pA - pB).sqrMagnitude;
                    if (newDistance < distance)
                    {
                        distance = newDistance;
                        pointA = pA;
                        pointB = pB;
                        i1 = i;
                        i2 = z;
                    }
                }
            }
            indexA = i1;
            indexB = i2;


        }

        public static GUIStyle GetHighlighter(string highlighter)
        {
            if (string.IsNullOrEmpty(highlighter)) return BoxHighlighter1;
            switch (highlighter.ToUpper())
            {
                case "YIELD":
                    return Item3;
                case "COMPUTED":
                    return Item2;
                default:
                    return BoxHighlighter1;
            }
        }

        public static void DoTilebar(string label)
        {
            var rect = GUIHelpers.GetRect(TitleBarStyle);
            DrawExpandableBox(rect, TitleBarStyle, label);
            //GUI.Box(rect, label, TitleBarStyle);
        }
    }
}