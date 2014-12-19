using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Invert.Core.Data
{
    public struct Vector4
    {
        public Vector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public float x;
        public float y;
        public float z;
        public float w;
    }

    public struct Vector3
    {
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float x;
        public float y;
        public float z;
    }

    public struct Vector2
    {
        public Vector2(float x, float y) : this()
        {
            this.x = x;
            this.y = y;
        }

        public float x { get; set; }
        public float y { get; set; }
 
    }

    public struct Rect
    {
        private float _x;
        private float _y;
        private float _w;
        private float _h;

        public float x
        {
            get { return this._x; }
            set { this._x = value; }
        }

        public float y
        {
            get { return this._y; }
            set { this._y = value; }
        }

        public Vector2 position
        {
            get { return new Vector2(this._x, this._y); }
            set
            {
                this._x = value.x;
                this._y = value.y;
            }
        }

        public Vector2 center
        {
            get { return new Vector2(this.x + this._w/2f, this.y + this._h/2f); }
            set
            {
                this._x = value.x - this._w/2f;
                this._y = value.y - this._h/2f;
            }
        }

        public float width
        {
            get { return this._w; }
            set { this._w = value; }
        }

        public float height
        {
            get { return this._h; }
            set { this._h = value; }
        }

        public Vector2 size
        {
            get { return new Vector2(this._w, this._h); }
            set
            {
                this._w = value.x;
                this._h = value.y;
            }
        }
    }

    public struct Color
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public static Color red
        {
            get
            {
                return new Color(1f, 0.0f, 0.0f, 1f);
            }
        }

        public static Color green
        {
            get
            {
                return new Color(0.0f, 1f, 0.0f, 1f);
            }
        }

        public static Color blue
        {
            get
            {
                return new Color(0.0f, 0.0f, 1f, 1f);
            }
        }

        public static Color white
        {
            get
            {
                return new Color(1f, 1f, 1f, 1f);
            }
        }

        public static Color black
        {
            get
            {
                return new Color(0.0f, 0.0f, 0.0f, 1f);
            }
        }

        public static Color yellow
        {
            get
            {
                return new Color(1f, 0.9215686f, 0.01568628f, 1f);
            }
        }

        public static Color cyan
        {
            get
            {
                return new Color(0.0f, 1f, 1f, 1f);
            }
        }

        public static Color magenta
        {
            get
            {
                return new Color(1f, 0.0f, 1f, 1f);
            }
        }

        public static Color gray
        {
            get
            {
                return new Color(0.5f, 0.5f, 0.5f, 1f);
            }
        }

        public static Color grey
        {
            get
            {
                return new Color(0.5f, 0.5f, 0.5f, 1f);
            }
        }

        public static Color clear
        {
            get
            {
                return new Color(0.0f, 0.0f, 0.0f, 0.0f);
            }
        }


        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this.r;
                    case 1:
                        return this.g;
                    case 2:
                        return this.b;
                    case 3:
                        return this.a;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3 index!");
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        this.r = value;
                        break;
                    case 1:
                        this.g = value;
                        break;
                    case 2:
                        this.b = value;
                        break;
                    case 3:
                        this.a = value;
                        break;
                    default:
                        throw new IndexOutOfRangeException("Invalid Vector3 index!");
                }
            }
        }

        public Color(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public Color(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = 1f;
        }

        public static implicit operator Vector4(Color c)
        {
            return new Vector4(c.r, c.g, c.b, c.a);
        }

        public static implicit operator Color(Vector4 v)
        {
            return new Color(v.x, v.y, v.z, v.w);
        }

        public static Color operator +(Color a, Color b)
        {
            return new Color(a.r + b.r, a.g + b.g, a.b + b.b, a.a + b.a);
        }

        public static Color operator -(Color a, Color b)
        {
            return new Color(a.r - b.r, a.g - b.g, a.b - b.b, a.a - b.a);
        }

        public static Color operator *(Color a, Color b)
        {
            return new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
        }

        public static Color operator *(Color a, float b)
        {
            return new Color(a.r * b, a.g * b, a.b * b, a.a * b);
        }

        public static Color operator *(float b, Color a)
        {
            return new Color(a.r * b, a.g * b, a.b * b, a.a * b);
        }

        public static Color operator /(Color a, float b)
        {
            return new Color(a.r / b, a.g / b, a.b / b, a.a / b);
        }




        public string ToString(string format)
        {
            string fmt = "RGBA({0}, {1}, {2}, {3})";
            object[] objArray = new object[4];
            int index1 = 0;
            string str1 = this.r.ToString(format);
            objArray[index1] = (object)str1;
            int index2 = 1;
            string str2 = this.g.ToString(format);
            objArray[index2] = (object)str2;
            int index3 = 2;
            string str3 = this.b.ToString(format);
            objArray[index3] = (object)str3;
            int index4 = 3;
            string str4 = this.a.ToString(format);
            objArray[index4] = (object)str4;
            return string.Format(fmt, objArray);
        }


        public override bool Equals(object other)
        {
            if (!(other is Color))
                return false;
            Color color = (Color)other;
            if (this.r.Equals(color.r) && this.g.Equals(color.g) && this.b.Equals(color.b))
                return this.a.Equals(color.a);
            else
                return false;
        }


    }

    public class SerializeField : Attribute
    {
        
    }

    public class HideInInspector : Attribute
    {
        
    }
}
