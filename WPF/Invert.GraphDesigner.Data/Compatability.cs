using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace UnityEngine
{
        public struct Rect
        {
            private float m_XMin;
            private float m_YMin;
            private float m_Width;
            private float m_Height;

            public float x
            {
                get
                {
                    return this.m_XMin;
                }
                set
                {
                    this.m_XMin = value;
                }
            }

            public float y
            {
                get
                {
                    return this.m_YMin;
                }
                set
                {
                    this.m_YMin = value;
                }
            }

            public Vector2 position
            {
                get
                {
                    return new Vector2(this.m_XMin, this.m_YMin);
                }
                set
                {
                    this.m_XMin = value.x;
                    this.m_YMin = value.y;
                }
            }

            public Vector2 center
            {
                get
                {
                    return new Vector2(this.x + this.m_Width / 2f, this.y + this.m_Height / 2f);
                }
                set
                {
                    this.m_XMin = value.x - this.m_Width / 2f;
                    this.m_YMin = value.y - this.m_Height / 2f;
                }
            }

            public Vector2 min
            {
                get
                {
                    return new Vector2(this.xMin, this.yMin);
                }
                set
                {
                    this.xMin = value.x;
                    this.yMin = value.y;
                }
            }

            public Vector2 max
            {
                get
                {
                    return new Vector2(this.xMax, this.yMax);
                }
                set
                {
                    this.xMax = value.x;
                    this.yMax = value.y;
                }
            }

            public float width
            {
                get
                {
                    return this.m_Width;
                }
                set
                {
                    this.m_Width = value;
                }
            }

            public float height
            {
                get
                {
                    return this.m_Height;
                }
                set
                {
                    this.m_Height = value;
                }
            }

            public Vector2 size
            {
                get
                {
                    return new Vector2(this.m_Width, this.m_Height);
                }
                set
                {
                    this.m_Width = value.x;
                    this.m_Height = value.y;
                }
            }

            [Obsolete("use xMin")]
            public float left
            {
                get
                {
                    return this.m_XMin;
                }
            }

            [Obsolete("use xMax")]
            public float right
            {
                get
                {
                    return this.m_XMin + this.m_Width;
                }
            }

            [Obsolete("use yMin")]
            public float top
            {
                get
                {
                    return this.m_YMin;
                }
            }

            [Obsolete("use yMax")]
            public float bottom
            {
                get
                {
                    return this.m_YMin + this.m_Height;
                }
            }

            public float xMin
            {
                get
                {
                    return this.m_XMin;
                }
                set
                {
                    float xMax = this.xMax;
                    this.m_XMin = value;
                    this.m_Width = xMax - this.m_XMin;
                }
            }

            public float yMin
            {
                get
                {
                    return this.m_YMin;
                }
                set
                {
                    float yMax = this.yMax;
                    this.m_YMin = value;
                    this.m_Height = yMax - this.m_YMin;
                }
            }

            public float xMax
            {
                get
                {
                    return this.m_Width + this.m_XMin;
                }
                set
                {
                    this.m_Width = value - this.m_XMin;
                }
            }

            public float yMax
            {
                get
                {
                    return this.m_Height + this.m_YMin;
                }
                set
                {
                    this.m_Height = value - this.m_YMin;
                }
            }

            public Rect(float left, float top, float width, float height)
            {
                this.m_XMin = left;
                this.m_YMin = top;
                this.m_Width = width;
                this.m_Height = height;
            }

            public Rect(Rect source)
            {
                this.m_XMin = source.m_XMin;
                this.m_YMin = source.m_YMin;
                this.m_Width = source.m_Width;
                this.m_Height = source.m_Height;
            }

            public static bool operator !=(Rect lhs, Rect rhs)
            {
                if ((double)lhs.x == (double)rhs.x && (double)lhs.y == (double)rhs.y && (double)lhs.width == (double)rhs.width)
                    return (double)lhs.height != (double)rhs.height;
                else
                    return true;
            }

            public static bool operator ==(Rect lhs, Rect rhs)
            {
                if ((double)lhs.x == (double)rhs.x && (double)lhs.y == (double)rhs.y && (double)lhs.width == (double)rhs.width)
                    return (double)lhs.height == (double)rhs.height;
                else
                    return false;
            }

            public static Rect MinMaxRect(float left, float top, float right, float bottom)
            {
                return new Rect(left, top, right - left, bottom - top);
            }

            public void Set(float left, float top, float width, float height)
            {
                this.m_XMin = left;
                this.m_YMin = top;
                this.m_Width = width;
                this.m_Height = height;
            }

     

            public string ToString(string format)
            {
                string fmt = "(x:{0}, y:{1}, width:{2}, height:{3})";
                object[] objArray = new object[4];
                int index1 = 0;
                string str1 = this.x.ToString(format);
                objArray[index1] = (object)str1;
                int index2 = 1;
                string str2 = this.y.ToString(format);
                objArray[index2] = (object)str2;
                int index3 = 2;
                string str3 = this.width.ToString(format);
                objArray[index3] = (object)str3;
                int index4 = 3;
                string str4 = this.height.ToString(format);
                objArray[index4] = (object)str4;
                return string.Format(fmt, objArray);
            }

            public bool Contains(Vector2 point)
            {
                if ((double)point.x >= (double)this.xMin && (double)point.x < (double)this.xMax && (double)point.y >= (double)this.yMin)
                    return (double)point.y < (double)this.yMax;
                else
                    return false;
            }

            public bool Contains(Vector3 point)
            {
                if ((double)point.x >= (double)this.xMin && (double)point.x < (double)this.xMax && (double)point.y >= (double)this.yMin)
                    return (double)point.y < (double)this.yMax;
                else
                    return false;
            }

            public bool Contains(Vector3 point, bool allowInverse)
            {
                if (!allowInverse)
                    return this.Contains(point);
                bool flag = false;
                if ((double)this.width < 0.0 && (double)point.x <= (double)this.xMin && (double)point.x > (double)this.xMax || (double)this.width >= 0.0 && (double)point.x >= (double)this.xMin && (double)point.x < (double)this.xMax)
                    flag = true;
                return flag && ((double)this.height < 0.0 && (double)point.y <= (double)this.yMin && (double)point.y > (double)this.yMax || (double)this.height >= 0.0 && (double)point.y >= (double)this.yMin && (double)point.y < (double)this.yMax);
            }

            private static Rect OrderMinMax(Rect rect)
            {
                if ((double)rect.xMin > (double)rect.xMax)
                {
                    float xMin = rect.xMin;
                    rect.xMin = rect.xMax;
                    rect.xMax = xMin;
                }
                if ((double)rect.yMin > (double)rect.yMax)
                {
                    float yMin = rect.yMin;
                    rect.yMin = rect.yMax;
                    rect.yMax = yMin;
                }
                return rect;
            }

            public bool Overlaps(Rect other)
            {
                if ((double)other.xMax > (double)this.xMin && (double)other.xMin < (double)this.xMax && (double)other.yMax > (double)this.yMin)
                    return (double)other.yMin < (double)this.yMax;
                else
                    return false;
            }

            public bool Overlaps(Rect other, bool allowInverse)
            {
                Rect rect = this;
                if (allowInverse)
                {
                    rect = Rect.OrderMinMax(rect);
                    other = Rect.OrderMinMax(other);
                }
                return rect.Overlaps(other);
            }

            //public static Vector2 NormalizedToPoint(Rect rectangle, Vector2 normalizedRectCoordinates)
            //{
            //    return new Vector2(Mathf.Lerp(rectangle.x, rectangle.xMax, normalizedRectCoordinates.x), Mathf.Lerp(rectangle.y, rectangle.yMax, normalizedRectCoordinates.y));
            //}

            //public static Vector2 PointToNormalized(Rect rectangle, Vector2 point)
            //{
            //    return new Vector2(Mathf.InverseLerp(rectangle.x, rectangle.xMax, point.x), Mathf.InverseLerp(rectangle.y, rectangle.yMax, point.y));
            //}

            public override int GetHashCode()
            {
                return this.x.GetHashCode() ^ this.width.GetHashCode() << 2 ^ this.y.GetHashCode() >> 2 ^ this.height.GetHashCode() >> 1;
            }

            public override bool Equals(object other)
            {
                if (!(other is Rect))
                    return false;
                Rect rect = (Rect)other;
                if (this.x.Equals(rect.x) && this.y.Equals(rect.y) && this.width.Equals(rect.width))
                    return this.height.Equals(rect.height);
                else
                    return false;
            }
        }


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
        public static Vector3 zero = new Vector3(0f,0f,0f);
    }

    public struct Vector2
    {
        public static Vector2 zero = new Vector2(0f,0f);
        public static Vector2 right = new Vector2(1f,0f);

        public Vector2(float x, float y) : this()
        {
            this.x = x;
            this.y = y;
        }

        public float x { get; set; }
        public float y { get; set; }
        public static implicit operator Vector2(Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static implicit operator Vector3(Vector2 v)
        {
            return new Vector3(v.x, v.y, 0.0f);
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x + b.x, a.y + b.y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x - b.x, a.y - b.y);
        }

        public static Vector2 operator -(Vector2 a)
        {
            return new Vector2(-a.x, -a.y);
        }

        public static Vector2 operator *(Vector2 a, float d)
        {
            return new Vector2(a.x * d, a.y * d);
        }

        public static Vector2 operator *(float d, Vector2 a)
        {
            return new Vector2(a.x * d, a.y * d);
        }

        public static Vector2 operator /(Vector2 a, float d)
        {
            return new Vector2(a.x / d, a.y / d);
        }

        public static bool operator ==(Vector2 lhs, Vector2 rhs)
        {
            return (double)Vector2.SqrMagnitude(lhs - rhs) < 0.0 / 1.0;
        }

        public static bool operator !=(Vector2 lhs, Vector2 rhs)
        {
            return (double)Vector2.SqrMagnitude(lhs - rhs) >= 0.0 / 1.0;
        }
        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode() << 2;
        }
        public static float SqrMagnitude(Vector2 a)
        {
            return (float)((double)a.x * (double)a.x + (double)a.y * (double)a.y);
        }

        public float SqrMagnitude()
        {
            return (float)((double)this.x * (double)this.x + (double)this.y * (double)this.y);
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector2))
                return false;
            Vector2 vector2 = (Vector2)other;
            if (this.x.Equals(vector2.x))
                return this.y.Equals(vector2.y);
            else
                return false;
        }


        public float sqrMagnitude
        {
            get
            {
                return (float)((double)this.x * (double)this.x + (double)this.y * (double)this.y);
            }
        }

 
    }

    public static class Mathf
    {
        public static int RoundToInt(float val)
        {
            return (int)Math.Round(val, 0);
        }

        public static float Min(float f, float f1)
        {
            if (f < f1)
            {
                return f;
            }
            return f1;
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

public class ModelCollection<T> : ObservableCollection<T>
{
    
}

