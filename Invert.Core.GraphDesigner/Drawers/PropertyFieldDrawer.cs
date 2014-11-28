using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Invert.Common;
using UnityEditor;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public class PropertyFieldDrawer : ItemDrawer<PropertyFieldViewModel>
    {
        public PropertyFieldDrawer(PropertyFieldViewModel viewModelObject) : base(viewModelObject)
        {
        }

        public IInspectorPropertyDrawer CustomDrawer { get; set; }
        public object CachedValue { get; set; }

        public override GUIStyle TextStyle
        {
            get { return EditorStyles.label; }
        }

        public override void Refresh(Vector2 position)
        {
            base.Refresh(position);
            if (ViewModel.CustomDrawerType != null)
            {
                CustomDrawer = (IInspectorPropertyDrawer) Activator.CreateInstance(ViewModel.CustomDrawerType);
                CustomDrawer.Refresh(position,ViewModel);
                return;
            }
            CachedValue = ViewModel.Getter();
            var bounds = new Rect(Bounds);
            bounds.width *= 2;
            if (ViewModel.Type == typeof(Vector2) || ViewModel.Type == typeof(Vector3) || ViewModel.Type == typeof(Quaternion))
            {
                bounds.height *= 2f;
            }
            Bounds = bounds;
        }

        public override void Draw(float scale)
        {
            //base.Draw(scale);
            GUILayout.BeginArea(Bounds.Scale(scale), ElementDesignerStyles.SelectedItemStyle);
            EditorGUIUtility.labelWidth = this.Bounds.width*0.55f;

            DrawInspector();
            GUILayout.EndArea();
        }

        public virtual void DrawInspector(bool refreshGraph = false, float scale = 1f)
        {
            EditorGUI.BeginChangeCheck();
            if (CustomDrawer != null)
            {
                CachedValue = CustomDrawer.Draw(scale,ViewModel);
            } else
            if (ViewModel.Type == typeof (string))
            {
                if (ViewModel.InspectorType == InspectorType.TextArea)
                {
                    EditorGUILayout.LabelField(ViewModel.Name);
                    CachedValue = EditorGUILayout.TextArea( (string)CachedValue,GUILayout.Height(50));
                }
                else if (ViewModel.InspectorType == InspectorType.TypeSelection)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(ViewModel.Name);

                    if (GUILayout.Button((string) CachedValue))
                    {
                        InvertGraphEditor.WindowManager.InitTypeListWindow(InvertApplication.GetDerivedTypes<System.Object>(true,true).Select(p=>new GraphTypeInfo()
                        {
                            Name = p.Name,
                            Group = p.Namespace,
                            Label = p.Name,
                            
                        }).ToArray()
                        , (type) =>
                        {
                            InvertGraphEditor.ExecuteCommand(d=>ViewModel.Setter(type.Name));
                        });
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    CachedValue = EditorGUILayout.TextField(ViewModel.Name, (string)CachedValue);
                }
                
            }
            else if (ViewModel.Type == typeof (int))
            {
                CachedValue = EditorGUILayout.IntField(ViewModel.Name, (int) CachedValue);
            }
            else if (ViewModel.Type == typeof (float))
            {
                CachedValue = EditorGUILayout.FloatField(ViewModel.Name, (float) CachedValue);
            }
            else if (ViewModel.Type == typeof (Vector2))
            {
                CachedValue = EditorGUILayout.Vector2Field(ViewModel.Name, (Vector3) CachedValue);
            }

            else if (ViewModel.Type == typeof (Vector3))
            {
                CachedValue = EditorGUILayout.Vector3Field(ViewModel.Name, (Vector3) CachedValue);

            }
            else if (ViewModel.Type == typeof(Color))
            {
                CachedValue = EditorGUILayout.ColorField(ViewModel.Name, (Color)CachedValue);

            }
            else if (ViewModel.Type == typeof (Vector4))
            {
                CachedValue = EditorGUILayout.Vector4Field(ViewModel.Name, (Vector4) CachedValue);
            }
            else if (ViewModel.Type == typeof (bool))
            {
                CachedValue = EditorGUILayout.Toggle(ViewModel.Name, (bool) CachedValue);
            }
            else if (typeof (Enum).IsAssignableFrom(ViewModel.Type))
            {
                CachedValue = EditorGUILayout.EnumPopup(ViewModel.Name, (Enum) CachedValue);
            }
            else if (ViewModel.Type == typeof (Type))
            {
                //InvertGraphEditor.WindowManager.InitTypeListWindow();
            }

            if (EditorGUI.EndChangeCheck())
            {

                ViewModel.Setter(CachedValue);
                if (refreshGraph)
                {

                    InvertGraphEditor.DesignerWindow.RefreshContent();
                }
            }
        }
    }

    public class SyntaxViewModel : GraphItemViewModel
    {
        private string _text;
        private LinkedList<LineViewModel> _lines;
        private int _endLine = Int32.MaxValue;

        public SyntaxViewModel(string text, string name, int startLine = 0)
        {
            StartLine = startLine;
            Text = text;
            Name = name;
          
        }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                if (value != null)
                {
                    Lines.Clear();
                    var lines = value.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.None);
                    for (int index = StartLine; index < Math.Min(EndLine, lines.Length); index++)
                    {
                        var line = lines[index];
                        var lineViewModel = ParseLine(line);
                        Lines.AddLast(lineViewModel);
                    }
                }
                
            }
        }

        private LineViewModel ParseLine(string line)
        {
            var lineViewModel = new LineViewModel(this);
            var matchList = Regex.Matches(line, RefactorContext.CSHARP_TOKENS + @"|.|\s+",RegexOptions.None);
            foreach (Match match in matchList)
            {
                var token = new TokenViewModel(lineViewModel, match.Value, Color.gray);
                GetColor(token);
                lineViewModel.Tokens.AddLast(token);
            }
            return lineViewModel;
        }

        public string[] Keywords =
        {
            "class",
            "int",
            "public",
            "void",
            "return",
            "virtual",
            "protected",
            "override",
            "var",
            "partial",
            "private",
            "using",
            "if",
            "foreach",
            "for",
            "string",
            "bool",
            "float",
            "decimal",
            "base",
            "get",
            "set"
        };
        public string[] Literals =
        {
            "{",
            "}",
            "(",
            ")",
          
        };

        private bool lastWasKeyword = false;
        private void GetColor(TokenViewModel value)
        {
       
            if (value.Text == "\"")
            {
                value.Color = Color.green;
                return;
            }
            if (Keywords.Contains(value.Text))
            {
                value.Color = Color.blue;
                value.Bold = true;
                lastWasKeyword = true;
                return;
            }
            if (lastWasKeyword && !value.Text.Any(char.IsWhiteSpace))
            {
                value.Color = Color.grey;
                lastWasKeyword = false;
                return;
            }
            if (Literals.Contains(value.Text))
            {
                value.Color = Color.gray;
                value.Bold = true;

            }
            value.Color = Color.white;
        }

        public LinkedList<LineViewModel> Lines
        {
            get { return _lines ?? (_lines = new LinkedList<LineViewModel>()); }
            set { _lines = value; }
        }

        public override Vector2 Position { get; set; }
        public override string Name { get; set; }

        public int StartLine { get; set; }

        public int EndLine
        {
            get { return _endLine; }
            set { _endLine = value; }
        }
    }

    public class LineViewModel : GraphItemViewModel
    {
        private readonly SyntaxViewModel _syntaxViewModel;
        private LinkedList<TokenViewModel> _tokens;

        public LineViewModel(SyntaxViewModel syntaxViewModel)
        {
            _syntaxViewModel = syntaxViewModel;
        }

        public LinkedList<TokenViewModel> Tokens
        {
            get { return _tokens ?? (_tokens = new LinkedList<TokenViewModel>()); }
            set { _tokens = value; }
        }

        public SyntaxViewModel SyntaxViewModel
        {
            get { return _syntaxViewModel; }
        }

        public override Vector2 Position { get; set; }
        public override string Name { get; set; }
    }

    public class TokenViewModel : GraphItemViewModel
    {
        private LineViewModel _lineViewModel;

        public TokenViewModel(LineViewModel lineViewModel, string text, Color color)
        {
            _lineViewModel = lineViewModel;
            Text = text;
            Color = color;
        }

        public TokenViewModel(LineViewModel lineViewModel)
        {
            LineViewModel = lineViewModel;
        }

        public string Text { get; set; }
        public Color Color { get; set; }
        public bool Bold { get; set; }
        public override Vector2 Position { get; set; }
        public override string Name { get; set; }

        public LineViewModel LineViewModel
        {
            get { return _lineViewModel; }
            set { _lineViewModel = value; }
        }

        public Vector2 TextSize { get; set; }
    }
    public class SyntaxDrawer : Drawer<SyntaxViewModel>
    {
        private GUIStyle guiStyle;

        public SyntaxDrawer(SyntaxViewModel viewModelObject) : base(viewModelObject)
        {
        }

        public override void Refresh(Vector2 position)
        {
            base.Refresh(position);
            guiStyle = new GUIStyle(EditorStyles.label);
            guiStyle.padding = new RectOffset(0,0,0,0);
            guiStyle.margin = new RectOffset(0,0,0,0);
            var height = 0f;
            var y = position.y;
            var maxLineWidth = 0f;
            var maxWidth = 0f;
            foreach (var line in ViewModel.Lines)
            {
                var x = position.x;
                var maxHeight = 0f;
                foreach (var token in line.Tokens)
                {
                    if (token.Bold)
                    {
                        guiStyle.fontStyle = FontStyle.Bold;
                    }
                    else
                    {
                        guiStyle.fontStyle = FontStyle.Normal;
                    }
                    if (token.Text.All(char.IsWhiteSpace))
                    {
                        token.TextSize = guiStyle.CalcSize(new GUIContent("f"));
                    }
                    else
                    {
                        token.TextSize = guiStyle.CalcSize(new GUIContent(token.Text));
                    }
                    
                    token.Bounds = new Rect(x, y, token.TextSize.x,  token.TextSize.y);
                    x += token.TextSize.x;
                    maxWidth = Math.Max(token.TextSize.x, maxWidth);
                    maxHeight = Math.Max(token.TextSize.y, maxHeight);
                }
                line.Bounds = new Rect(x, y, line.Tokens.Sum(p => p.TextSize.x), line.Tokens.Sum(p => p.TextSize.y));

                y += maxHeight;
                height += maxHeight;
                maxLineWidth = Math.Max(maxLineWidth, line.Bounds.width);
            }
            var newBounds = new Rect(Bounds);
            newBounds.x = position.x;
            newBounds.y = position.y;
            newBounds.height = height;
            newBounds.width = maxLineWidth;
            Bounds = newBounds;
            
        }

        public override void Draw(float scale)
        {
            base.Draw(scale);
            GUI.Box(Bounds.Scale(scale),string.Empty,EditorStyles.textArea);
            foreach (var line in ViewModel.Lines)
            {

                foreach (var token in line.Tokens)
                {
                    guiStyle.normal.textColor = token.Color;
                    if (token.Bold)
                    {
                        guiStyle.fontStyle = FontStyle.Bold;
                    }
                    else
                    {
                        guiStyle.fontStyle = FontStyle.Normal;
                    }
                    GUI.Label(token.Bounds,token.Text,guiStyle);
                }
            }
        }
    }

    public interface IInspectorPropertyDrawer
    {
        void Refresh(Vector2 position, PropertyFieldViewModel viewModel);
        object Draw(float scale, PropertyFieldViewModel viewModel);
    }
}