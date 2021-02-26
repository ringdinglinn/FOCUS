using System.Collections.Generic;
using UnityEngine;

namespace GreasePencil
{
    [System.Serializable]
    public class Stroke
    {
        static class StrokePool
        {
            private static readonly ObjectPool<Stroke> _strokePool = new ObjectPool<Stroke>(null, null);
            public static Stroke Get() { return _strokePool.Get(); }
            public static void Release(Stroke toRelease) { _strokePool.Release(toRelease); }
            public static bool TestAllocations() { return _strokePool.countActive > 0; }
        }

        public static Stroke GetStroke()
        {
            return StrokePool.Get();
        }

        public static void ReleaseStroke(Stroke toRelease)
        {
            StrokePool.Release(toRelease);
        }

        public static void ReleaseStrokes(List<Stroke> toRelease)
        {
            toRelease.ForEach(s => StrokePool.Release(s));
            toRelease.Clear();
        }

        public static bool RemoveEmptyStrokes(Stroke s)
        {
            if (s.IsEmpty)
            {
                ReleaseStroke(s);
                return true;
            }
            return false;
        }

        public static Stroke New(Stroke stroke)
        {
            var newStroke = StrokePool.Get();

            newStroke.Validate(true);
            newStroke._points.AddRange(stroke.Points);

            newStroke._right = stroke._right;
            newStroke._up = stroke._up;
            newStroke._finalized = stroke._finalized;

            newStroke.SetColors(stroke._lineColor, stroke._fillColor);

            return newStroke;
        }

        public static Stroke New(Vector3 right, Vector3 up)
        {
            var newStroke = StrokePool.Get();

            newStroke.Validate(true);
            newStroke._right = right;
            newStroke._up = up;
            newStroke._finalized = false;

            newStroke.SetColors(Color.white, Color.white);

            return newStroke;
        }

        public static Stroke New(Point initialPosition, Vector3 right, Vector3 up, Color lineColor, Color fillColor)
        {
            var newStroke = StrokePool.Get();

            newStroke.Validate(true);
            newStroke._points.Add(initialPosition);
            newStroke._points.Add(initialPosition);

            newStroke._right = right;
            newStroke._up = up;
            newStroke._finalized = false;

            newStroke.SetColors (lineColor, fillColor);

            return newStroke;
        }

        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("points")]
        List<Point> _points;
        public List<Point> Points
        {
            get { return _points; }
            set
            {
                _points.Clear();
                _points.AddRange(value);
                Triangulate();
            }
        }

        [SerializeField]
        Color _lineColor = Color.white;
        public Color LineColor { get { return _lineColor; } }

        [SerializeField]
        Color _fillColor = Color.white;
        public Color FillColor { get { return _fillColor; } }

        List<int> _triangles;

        [SerializeField, HideInInspector]
        Vector3 _right;
        public Vector3 Right { get { return _right; } }

        [SerializeField, HideInInspector]
        Vector3 _up;
        public Vector3 Up { get { return _up; } }

        [SerializeField, HideInInspector]
        bool _finalized;
        public bool Finalized { get { return _finalized; } }

        void Validate(bool clear = false)
        {
            if (_points == null)
                _points = new List<Point>();

            if (clear) _points.Clear();

            if (_triangles == null)
                _triangles = new List<int>();

            if (clear) _triangles.Clear();
        }

        public void SetColors(Color lineColor, Color fillColor)
        {
            _lineColor = lineColor;
            _fillColor = fillColor;
        }

        public void Add(Point position, bool triangulate)
        {
            Validate();

            _points.Add(position);
            LastPoint = position;

            if (triangulate)
                Triangulate();
        }

        public void CloseStroke()
        {
            _finalized = true;
        }

        public void MarkPointForRemoval(int index)
        {
            var point = _points[index];
            point.markedForRemoval = true;
            _points[index] = point;
        }

        public Point LastPoint
        {
            get { return _points[_points.Count - 1]; }
            set { if (_points.Count > 0) _points[_points.Count - 1] = value; }
        }

        public Point FirstPoint
        {
            get { return _points[0]; }
        }

        public bool IsEmpty { get { return _points == null || _points.Count == 0; } }

        public float LastDistance()
        {
            return (_points.Count > 1) ? (_points[_points.Count - 2].position - LastPoint.position).magnitude : 0f;
        }

        public void Clear()
        {
            if (_points != null)
                _points.Clear();

            if (_triangles != null)
                _triangles.Clear();
        }

        public void ClearTriangles()
        {
            if (_triangles != null)
                _triangles.Clear();
        }

        public List<int> GetTriangles()
        {
            if ((_triangles == null) || (_triangles.Count == 0))
                Triangulate();

            return _triangles;
        }

        public void GetVertices(List<Vector3> points)
        {
            points.Clear();
            if (points.Capacity < _points.Count) points.Capacity = _points.Count;

            int count = _points.Count;

            for (int i=0; i<count; ++i)
                points.Add(_points[i].position);
        }

        public void Triangulate()
        {
            Validate();

            if (_points.Count < 3)
            {
                _triangles.Clear();
                return;
            }

            List<Vector2> pts;
            using (PooledList<Vector2>.Get(out pts))
            {
                var planeRight = new Plane(_right, Vector3.zero);
                var planeUp = new Plane(_up, Vector3.zero);

                var count = _points.Count;
                if (_points[count - 1].position == _points[count - 2].position)
                    --count;

                for (int pointIndex = 0; pointIndex < count; ++pointIndex)
                {
                    pts.Add(new Vector2(planeRight.GetDistanceToPoint(_points[pointIndex].position),
                        planeUp.GetDistanceToPoint(_points[pointIndex].position)));
                }

                _triangles.Clear();
                MeshFunctions.Triangulate(pts, _triangles);
            }
        }

        public void SetTriangles(List<int> tris)
        {
            Validate();

            _triangles.Clear();
            _triangles.AddRange(tris);
        }
    }
}