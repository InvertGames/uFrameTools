using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Invert.Core.GraphDesigner;
using Rect = UnityEngine.Rect;

namespace DiagramDesigner.Platform
{
    public class DesignerStyles : IStyleProvider
    {
        private Dictionary<string, ImageSource> _cachedImages = new Dictionary<string, ImageSource>();
        private Dictionary<InvertStyles, Brush> _cachedBrushes = new Dictionary<InvertStyles, Brush>();
        private static ResourceDictionary _stylesDictionary;

        private static Dictionary<InvertStyles, GraphStyle> _graphStyles = new Dictionary<InvertStyles, GraphStyle>();

        public static Dictionary<InvertStyles, GraphStyle> GraphStyles
        {
            get { return _graphStyles; }
            set { _graphStyles = value; }
        }


        static DesignerStyles()
        {
            _stylesDictionary = new ResourceDictionary();
            _stylesDictionary.Source =
                new Uri("/Invert.GraphDesigner.WPF.Controls;component/Resources/InvertStyles.xaml",
                    UriKind.RelativeOrAbsolute);

            foreach (var item in Enum.GetNames(typeof (InvertStyles)))
            {
                var style = new GraphStyle();
                if (_stylesDictionary.Contains(item + "Background"))
                {
                    style.Background = _stylesDictionary[item + "Background"] as Brush;
                }
                if (_stylesDictionary.Contains(item + "Border"))
                {
                    style.Border = _stylesDictionary[item + "Border"] as Pen;
                }
                if (_stylesDictionary.Contains(item + "Foreground"))
                {
                    style.Foreground = _stylesDictionary[item + "Foreground"] as Brush;
                }
        
                _graphStyles.Add((InvertStyles)Enum.Parse(typeof(InvertStyles),item,true), style);
            }
            GraphStyles[InvertStyles.HeaderStyle].FontWeight = FontWeights.Bold;
            GridLine = _stylesDictionary["GridLine"] as Pen;
            GridLineSecondary = _stylesDictionary["GridLineSecondary"] as Pen;

            GraphStyles[InvertStyles.NodeBackground].CornerRadius = new CornerRadius(10f,10f,10f,10f);
            GraphStyles[InvertStyles.NodeBackground].Offset = new Rect(9,-1,-19,-9);
            GraphStyles[InvertStyles.NodeBackground].shadowSpread = 10;
            GraphStyles[InvertStyles.Tag1].CornerRadius = new CornerRadius(5, 5,0,0);
            GraphStyles[InvertStyles.Tag1].FontWeight = FontWeights.Normal;
            GraphStyles[InvertStyles.Tag1].FontSize = 10;
            foreach (var style in HeaderStyles)
            {
              
                GraphStyles[style].CornerRadius = new CornerRadius(10f,10f,10f,10f);
                GraphStyles[style].Border = GraphStyles[InvertStyles.NodeBackground].Border;
                GraphStyles[style].Offset = new Rect(9, -1, -19, -9);
            }
            GraphStyles[InvertStyles.ViewModelHeaderStyle].Offset =new Rect(9, -1, -19, -9);
            GraphStyles[InvertStyles.ViewModelHeaderStyle].FontSize = 14;
            GraphStyles[InvertStyles.ViewModelHeaderStyle].FontWeight = FontWeights.Bold;
            GraphStyles[InvertStyles.HeaderStyle].Offset =  new Rect(9, -1, -19, -9);
            foreach (var style in BoxHighlighters)
            {
                GraphStyles[style].CornerRadius = new CornerRadius(10f, 10f, 10f, 10f);
                GraphStyles[style].Offset = new Rect(9, -1, -19, -9);
            }
        }

        public static Pen GridLineSecondary { get; set; }

        public static Pen GridLine { get; set; }

        public static InvertStyles[] HeaderStyles = new[]
        {
            InvertStyles.NodeHeader1,
            InvertStyles.NodeHeader2,
            InvertStyles.NodeHeader3,
            InvertStyles.NodeHeader4,
            InvertStyles.NodeHeader5,
            InvertStyles.NodeHeader6,
            InvertStyles.NodeHeader7,
            InvertStyles.NodeHeader8,
            InvertStyles.NodeHeader9,
            InvertStyles.NodeHeader10,
            InvertStyles.NodeHeader11,
            InvertStyles.NodeHeader12,
            InvertStyles.NodeHeader13,

        };        
        public static InvertStyles[] BoxHighlighters = new[]
        {
            InvertStyles.BoxHighlighter1,
            InvertStyles.BoxHighlighter2,
            InvertStyles.BoxHighlighter3,
            InvertStyles.BoxHighlighter4,
            InvertStyles.BoxHighlighter5,
            InvertStyles.BoxHighlighter6,

        };

        public Brush CreateNodeBrush(string name)
        {

            var brush = new ImageBrush(GetImageInternal(name))
            {
                Viewport = new System.Windows.Rect(50, 50, 50, 50),
                Stretch = Stretch.UniformToFill,
                TileMode = TileMode.FlipXY
            };
            return brush;
        }
        public Brush GetBrush(InvertStyles name)
        {
            return (Brush)GraphStyles[name].Background;
        }
        public object GetStyle(InvertStyles name)
        {
            return GraphStyles[name];
        }

        public Dictionary<string, ImageSource> CachedImages
        {
            get { return _cachedImages; }
            set { _cachedImages = value; }
        }

        public Dictionary<InvertStyles, Brush> CachedBrushes
        {
            get { return _cachedBrushes; }
            set { _cachedBrushes = value; }
        }
        public ImageSource GetImageInternal(string name)
        {
            if (CachedImages.ContainsKey(name))
            {
                return CachedImages[name];

            }
            else
            {
                try
                {
                    if (_stylesDictionary.Contains(name))
                    {
                        var bmpImage = _stylesDictionary[name] as BitmapImage;
                        
                        CachedImages.Add(name, bmpImage);
                        return bmpImage;
                    }
                    else
                    {
                        CachedImages.Add(name, null);
                    }
                }
                catch (Exception e)
                {
                    CachedImages.Add(name, null);
                    return null;
                }
            }
            return null;
        }
        public object GetImage(string name)
        {
            return GetImageInternal(name);
        }
    }
}