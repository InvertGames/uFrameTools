using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        public override object TextStyle
        {
            get { return CachedStyles.DefaultLabel; }
        }
        private string _left;
        private string _right;
        private Vector2 _leftSize;
        private Vector2 _rightSize;
        //public override void Refresh(IPlatformDrawer platform, Vector2 position, bool hardRefresh = true)
        //{
        //    base.Refresh(platform, position, hardRefresh);
        //    if (hardRefresh)
        //    {
              
        //    }


        //    Bounds = new Rect(position.x + 10, position.y, _leftSize.x + 5 + _rightSize.x + 40, 18);
        //}
        public override void Refresh(IPlatformDrawer platform, Vector2 position, bool hardRefresh = true)
        {
            base.Refresh(platform, position,hardRefresh);
            if (hardRefresh)
            {
                CachedValue = ViewModel.Getter();
            }

            if (ViewModel.CustomDrawerType != null && hardRefresh)
            {
                CustomDrawer = (IInspectorPropertyDrawer) Activator.CreateInstance(ViewModel.CustomDrawerType);
            }

            
            if (CustomDrawer != null)
            {
                CustomDrawer.Refresh(platform, position, this);

            }
            else
            {
                var bounds = new Rect(Bounds);
                bounds.width *= 2;
                if (ViewModel.Type == typeof(Vector2) || ViewModel.Type == typeof(Vector3))// || ViewModel.Type == typeof(Quaternion))
                {
                    bounds.height *= 2f;
                }
                bounds.x += 3;
                Bounds = bounds;

            }

            
   
        }

        public override void Draw(IPlatformDrawer platform, float scale)
        {
            if (CustomDrawer != null)
            {
                CustomDrawer.Draw(platform, scale, this);
            }
            else
            {
                platform.DrawPropertyField(this, scale);
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

    public interface IInspectorPropertyDrawer
    {
        void Refresh(IPlatformDrawer platform, Vector2 position, PropertyFieldDrawer viewModel);
        void Draw(IPlatformDrawer platform, float scale, PropertyFieldDrawer viewModel);
    }
}