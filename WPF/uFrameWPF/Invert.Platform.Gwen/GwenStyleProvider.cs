using System.Collections.Generic;
using Gwen;
using Gwen.Skin.Texturing;
using Invert.Core.GraphDesigner;

namespace Invert.Platform.Gwen
{
    public class GwenStyleProvider : IStyleProvider
    {
        public Dictionary<InvertStyles, GwenStyle> Styles
        {
            get { return _styles; }
            set { _styles = value; }
        }


        public GwenStyleProvider()
        {


        }

        public global::Gwen.Skin.Base skin;
        private Dictionary<InvertStyles, GwenStyle> _styles;
        public Texture _Texture;

        public object GetImage(string name)
        {

            return null;
        }

        public object GetStyle(InvertStyles name)
        {
            if (skin == null)
                return null;
            var style = new GwenStyle();
            style.Font = skin.DefaultFont;
            // style.Font = new Font(skin.Renderer, "Motorwerk", 12);
            switch (name)
            {
                case InvertStyles.NodeBackground:
                    style.Bordered = new Bordered(_Texture, 84, 156, 194, 166, new Margin(16, 20, 16, 18))
                    {

                    };
                    return style;
                case InvertStyles.NodeHeader1:

                    style.Bordered = new Bordered(_Texture, 278, 156, 194, 54, new Margin(16, 11, 17, 18));
                    return style;
                case InvertStyles.NodeHeader2:
                    style.Bordered = new Bordered(_Texture, 278, 210, 194, 54, new Margin(16, 11, 17, 18));
                    return style;
                case InvertStyles.NodeHeader3:
                    style.Bordered = new Bordered(_Texture, 278, 264, 194, 54, new Margin(16, 11, 17, 18));
                    return style;
                case InvertStyles.NodeHeader4:
                    style.Bordered = new Bordered(_Texture, 278, 318, 194, 54, new Margin(16, 11, 17, 18));
                    return style;
                case InvertStyles.NodeHeader5:
                    style.Bordered = new Bordered(_Texture, 278, 372, 194, 54, new Margin(16, 11, 17, 18));
                    return style;
                case InvertStyles.NodeHeader6:
                    style.Bordered = new Bordered(_Texture, 278, 426, 194, 54, new Margin(16, 11, 17, 18));
                    return style;
                case InvertStyles.NodeHeader7:
                    style.Bordered = new Bordered(_Texture, 278, 480, 194, 54, new Margin(16, 11, 17, 18));
                    return style;
                case InvertStyles.NodeHeader8:
                    style.Bordered = new Bordered(_Texture, 278, 534, 194, 54, new Margin(16, 11, 17, 18));
                    return style;
                case InvertStyles.NodeHeader9:
                    style.Bordered = new Bordered(_Texture, 278, 588, 194, 54, new Margin(16, 11, 17, 18));
                    return style;
                case InvertStyles.NodeHeader10:
                    style.Bordered = new Bordered(_Texture, 278, 642, 194, 54, new Margin(16, 11, 17, 18));
                    return style;
                case InvertStyles.NodeHeader11:
                    style.Bordered = new Bordered(_Texture, 278, 696, 194, 54, new Margin(16, 11, 17, 18));
                    return style;
                case InvertStyles.NodeHeader12:
                    style.Bordered = new Bordered(_Texture, 278, 750, 194, 54, new Margin(16, 11, 17, 18));
                    return style;
                case InvertStyles.NodeHeader13:
                    style.Bordered = new Bordered(_Texture, 278, 804, 194, 54, new Margin(16, 11, 17, 18));
                    return style;
                case InvertStyles.BoxHighlighter1:
                    style.Bordered = new Bordered(_Texture, 476, 428, 87, 67, new Margin(20, 20, 20, 20));
                    return style;
                case InvertStyles.BoxHighlighter2:
                    style.Bordered = new Bordered(_Texture, 476, 497, 87, 67, new Margin(20, 20, 20, 20));
                    return style;
                case InvertStyles.BoxHighlighter3:
                    style.Bordered = new Bordered(_Texture, 476, 566, 87, 67, new Margin(20, 20, 20,20));
                    return style;
                case InvertStyles.BoxHighlighter4:
                    style.Bordered = new Bordered(_Texture, 476, 428, 87, 67, new Margin(20, 20, 20, 20));
                    return style;
                case InvertStyles.BoxHighlighter5:
                    style.Bordered = new Bordered(_Texture, 476, 635, 87, 67, new Margin(20, 20, 20, 20));
                    return style;
                case InvertStyles.BoxHighlighter6:
                    style.Bordered = new Bordered(_Texture, 476, 704, 87, 67, new Margin(20, 20, 20, 20));
                    return style;
                case InvertStyles.Tag1:
                    style.Bordered = new Bordered(_Texture, 229, 115, 33, 21, new Margin(9, 12, 9, 0));
                    return style;
                case InvertStyles.Toolbar:
                    style.Bordered = new Bordered(_Texture, 690, 497, 30, 24, new Margin(0, 0, 0, 0));
                    return style;
                case InvertStyles.ToolbarButton:
                    style.Bordered = new Bordered(_Texture, 722, 497, 56, 24, new Margin(0, 0, 0, 3));
                    return style;
                case InvertStyles.ToolbarButtonDown:
                    style.Bordered = new Bordered(_Texture, 722, 497, 56, 24, new Margin(0, 0, 0, 3));
                    return style;
                case InvertStyles.AddButtonStyle:
                    style.Bordered = new Bordered(_Texture, 64, 384, 16, 16, new Margin(0, 0, 0, 0));
                    return style;
                case InvertStyles.Item1:
                    style.Bordered = new Bordered(_Texture, 861, 175, 82, 25, new Margin(0, 0, 0, 0));
                    return style;
                case InvertStyles.Item2:
                    style.Bordered = new Bordered(_Texture, 861, 205, 82, 25, new Margin(0, 0, 0, 0));
                    return style;
                case InvertStyles.Item3:
                    style.Bordered = new Bordered(_Texture, 861, 235, 82, 25, new Margin(0, 0, 0, 0));
                    return style;
                case InvertStyles.Item4:
                    style.Bordered = new Bordered(_Texture, 861, 265, 82, 25, new Margin(0, 0, 0, 0));
                    return style;
                case InvertStyles.Item5:
                    style.Bordered = new Bordered(_Texture, 861, 295, 82, 25, new Margin(0, 0, 0, 0));
                    return style;
                    return style;
                case InvertStyles.Item6:
                    style.Bordered = new Bordered(_Texture, 722, 325, 56, 24, new Margin(0, 0, 0, 0));
                    return style;
                //case InvertStyles.Tag2:
                //    style.Bordered = new Bordered(_Texture, 229, 115, 33, 21, new Margin(3, 10, 8, 0));
                //    return style;
            }

            return null;
        }

        public Bordered GetBorderedImage(string texture)
        {
            if (texture == "DiagramArrowRight")
            {
                return new Bordered(_Texture, 205, 332, 16, 16, new Margin(0, 0, 0, 0));
            }
            else if (texture == "DiagramArrowRightEmpty")
            {
                return new Bordered(_Texture, 223, 332, 16, 16, new Margin(0, 0, 0, 0));
            }
            return new Bordered();

        }
    }
}