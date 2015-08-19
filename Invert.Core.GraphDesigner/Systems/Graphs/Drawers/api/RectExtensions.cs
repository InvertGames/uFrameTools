using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{
    public static class RectExtensions
    {
        public static Vector2 Snap(this Vector2 pos, float snapSize)
        {
            var x = Math.Round(pos.x/snapSize)*snapSize;
            var y = Math.Round(pos.y/snapSize)*snapSize;
            return new Vector2((float)x, (float)y);
        }

        public static Rect Scale(this Rect r, float scale)
        {
            return new Rect(r.x*scale, r.y*scale, r.width*scale, r.height*scale);
        }

        public static Rect Normalize(this Rect r, float scale)
        {
            return new Rect(r.x/scale, r.y/scale, r.width/scale, r.height/scale);
        }
#if UNITY_DLL
        public static RectOffset Scale(this RectOffset r, float scale)
        {
            return new RectOffset(Mathf.RoundToInt(r.left*scale), Mathf.RoundToInt(r.right*scale),
                Mathf.RoundToInt(r.top*scale), Mathf.RoundToInt(r.bottom*scale));
        }

        public static Rect Add(this Rect source, Rect add)
        {
            return new Rect(source.x + add.x, source.y + add.y, source.width + add.width, source.height + add.height);
        }
        public static GUIStyle Scale(this GUIStyle style, float scale)
        {
            var s = new GUIStyle(style);
            s.fontSize = Mathf.RoundToInt(style.fontSize*scale);
            s.fixedHeight = Mathf.RoundToInt(style.fixedHeight*scale);
            s.fixedWidth = Mathf.RoundToInt(style.fixedWidth*scale);
            s.padding = s.padding.Scale(scale);
            s.margin = s.margin.Scale(scale);
            return s;
        }
#endif
    }

    public static class RectLayoutExtensions
    {
        public static Rect Below(this Rect source, Rect belowSource)
        {
            return new Rect(source.x, belowSource.y + belowSource.height, source.width, source.height);
        }
   
        
        public static Rect RightOf(this Rect source, Rect target)
        {
            return new Rect(target.x + target.width, source.y, source.width, source.height);
        }

        public static Rect WithSize(this Rect source, float width, float height)
        {
            return new Rect(source.x, source.y, width, height);
        }

        public static Rect WithWidth(this Rect source, float width)
        {
            return new Rect(source.x, source.y, width, source.height);
        }

        public static Rect WithHeight(this Rect source, float height)
        {
            return new Rect(source.x, source.y, source.width, height);
        }

        public static Rect Pad(this Rect source, float left, float top, float right, float bottom)
        {
            return new Rect(source.x + left, source.y + top, source.width-right, source.height-bottom);
        }

        public static Rect PadSides(this Rect source, float padding)
        {
            return new Rect(source.x + padding, source.y + padding, source.width - padding*2, source.height - padding*2);
        }

        public static Rect AlignTopRight(this Rect source, Rect target)
        {
            return new Rect(target.xMax - source.width, target.y, source.width, source.height);
        }

        public static Rect AlignHorisonallyByCenter(this Rect source, Rect target)
        {

            var y = target.y + (target.height - source.height)/2;

            return new Rect(source.x, y, source.width, source.height);
        }

        public static Rect Translate(this Rect source, float x, float y)
        {
            return new Rect(source.x + x, source.y + y, source.width, source.height);
        }

        public static Rect WithOrigin(this Rect source, float x, float y)
        {
            return new Rect(x, y, source.width, source.height);
        }
   
        public static Rect Align(this Rect source, Rect target)
        {
            return new Rect(target.x, target.y, source.width, source.height);
        }

        public static Rect AlignAndScale(this Rect source, Rect target)
        {
            return new Rect(target.x, target.y, target.width, target.height);
        }

        public static Rect AlignHorisontally(this Rect source, Rect target)
        {
            return new Rect(source.x, target.y, source.width, source.height);
        }
        
        public static Rect AlignVertically(this Rect source, Rect target)
        {
            return new Rect(target.x, source.y, source.width, source.height);
        }


    }

}