using System;
using Invert.Common;
using UnityEditor;
using UnityEngine;

public abstract class BeizureLink : IDiagramLink
{
    private Vector3 _endPos;
    private bool _endRight;
    private Vector3 _startPos;
    private bool _startRight;

    public virtual NodeCurvePointStyle EndStyle
    {
        get { return NodeCurvePointStyle.Triangle; }
    }

    public abstract ISelectable Source { get; }

    public virtual NodeCurvePointStyle StartStyle
    {
        get { return NodeCurvePointStyle.Circle; }
    }

    public abstract ISelectable Target { get; }

    public virtual Rect StartRect
    {
        get
        {
            var source = Source as IDiagramNode;
            if (source != null)
            {
                return source.HeaderPosition;
            }
            return Source.Position;
        }
    }

    public virtual Rect EndRect
    {
        get
        {
            var target = Target as IDiagramNode;
            if (target != null)
            {
                return target.HeaderPosition;
            }
            return Target.Position;
        }
    }

    public virtual float Width
    {
        get { return 3; }
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

    public virtual void Draw(DiagramViewModel diagram)
    {
        try
        {
            DrawBeizure(StartRect, EndRect, GetColor(diagram), Width);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error couldn't find one of the rects on " + this.GetType().FullName);
            
        }
    }

    public virtual Vector3 LeftOffset
    {
        get { return Vector3.zero; }
    }

    public virtual Vector3 RightOffset
    {
        get { return Vector3.zero; }
    }
    public virtual void DrawBeizure(Rect s, Rect e, Color color, float width)
    {
        //var swap = start.center.x < end.center.x;
        var start = s.Scale(ElementDesignerStyles.Scale);
        var end = e.Scale(ElementDesignerStyles.Scale);
        var startPosLeft = new Vector3(start.x, (start.y + start.height / 2f) + 4, 0f) + LeftOffset;
        var startPosRight = new Vector3(start.x + start.width, (start.y + start.height / 2f) + 12, 0f) + RightOffset;

        var endPosLeft = new Vector3(end.x, (end.y + end.height / 2f) + 4, 0f) + LeftOffset;
        var endPosRight = new Vector3(end.x + end.width, (end.y + end.height / 2f) + 12, 0f) + RightOffset;

        int index1;
        int index2;
        FindClosestPoints(new[] { startPosLeft, startPosRight },
            new[] { endPosLeft, endPosRight }, out _startPos, out _endPos, out index1, out index2);

        //endPos = TestClosest(startPos, end);

        _startRight = index1 == 0 && index2 == 0 ? index1 == 1 : index2 == 1;
        _endRight = index1 == 1 && index2 == 1 ? index2 == 1 : index1 == 1;

        var startTan = _startPos + (_endRight ? Vector3.right : Vector3.left) * 35;

        var endTan = _endPos + (_startRight ? Vector3.right : Vector3.left) * 35;

        var shadowCol = new Color(0, 0, 0, 0.1f);

        if (DrawShadow)
        {
            for (int i = 0; i < 3; i++) // Draw a shadow
                Handles.DrawBezier(_startPos, _endPos, startTan, endTan, shadowCol, null, (i + 1)*5);
        }
        Handles.DrawBezier(_startPos, _endPos, startTan, endTan, color, null, width);

        _startPos.y -= 8;
        _startPos.x -= 8;

        _endPos.y -= 8;
        _endPos.x -= 8;
    }

    public virtual bool DrawShadow
    {
        get { return true; }
    }
    public abstract Color GetColor(DiagramViewModel diagram);

    public virtual void DrawPoints(DiagramViewModel diagram)
    {
        if (StartStyle == NodeCurvePointStyle.Circle)
        {
            GUI.DrawTexture(new Rect(_startPos.x, _startPos.y, 16f, 16f), _startRight ? ElementDesignerStyles.CircleRightTexture : ElementDesignerStyles.CircleLeftTexture);
        }
        else
        {
            GUI.DrawTexture(new Rect(_startPos.x, _startPos.y, 16f, 16f), _startRight ? ElementDesignerStyles.ArrowRightTexture : ElementDesignerStyles.ArrowLeftTexture);
        }

        if (EndStyle == NodeCurvePointStyle.Triangle)
        {
            GUI.DrawTexture(new Rect(_endPos.x, _endPos.y, 16f, 16f), _endRight ? ElementDesignerStyles.ArrowRightTexture : ElementDesignerStyles.ArrowLeftTexture);
        }
        else
        {
            GUI.DrawTexture(new Rect(_endPos.x, _endPos.y, 16f, 16f), _endRight ? ElementDesignerStyles.CircleLeftTexture : ElementDesignerStyles.CircleRightTexture);
        }
    }
}