using UnityEngine;

public static class RectExtensions
{
    public static Vector2 Snap(this Vector2 pos, float snapSize)
    {
        var x = Mathf.RoundToInt(pos.x / snapSize) * snapSize;
        var y = Mathf.RoundToInt(pos.y / snapSize) * snapSize;
        return new Vector2(x, y);
    }
    public static Rect Scale(this Rect r, float scale)
    {
        return new Rect(r.x * scale, r.y * scale, r.width * scale, r.height * scale);
    }
    public static Rect Normalize(this Rect r, float scale)
    {
        return new Rect(r.x / scale, r.y / scale, r.width / scale, r.height / scale);
    }
    public static RectOffset Scale(this RectOffset r, float scale)
    {
        return new RectOffset(Mathf.RoundToInt(r.left * scale), Mathf.RoundToInt(r.right * scale), Mathf.RoundToInt(r.top * scale), Mathf.RoundToInt(r.bottom * scale));
    }
    public static GUIStyle Scale(this GUIStyle style, float scale)
    {
        var s = new GUIStyle(style);
        s.fontSize = Mathf.RoundToInt(style.fontSize * scale);
        s.fixedHeight = Mathf.RoundToInt(style.fixedHeight * scale);
        s.fixedWidth = Mathf.RoundToInt(style.fixedWidth * scale);
        s.padding = s.padding.Scale(scale);
        s.margin = s.margin.Scale(scale);
        return s;
    }
}