using System.Collections.Generic;
using UnityEngine;

namespace GreasePencil
{
    public class SmoothModifier : GreasePencilModifier, IInlineModifier
    {
        [SerializeField, Range(0, 20)] int _iterations = 1;
        [SerializeField, Range(0, 1)] float _smoothFactor = 0.5f;
        [SerializeField, Range(0, 180)] float _maxAngle = 30f;

        public override void ModifyStroke(Layer layer, List<Stroke> modifiedStrokes)
        {
            List<Point> points;
            foreach (var stroke in modifiedStrokes)
            {
                using (PooledList<Point>.Get(out points, stroke.Points))
                {
                    for (int i = 0; i < _iterations; ++i)
                        Smooth(points, _smoothFactor, _maxAngle);
                    stroke.Points = points;
                }
            }
        }

        public static void Smooth(List<Point> _points, float factor, float maxAngle)
        {
            if (_points.Count < 3)
                return;

            List<Point> points;
            using (PooledList<Point>.Get(out points))
            {
                points.Add(_points[0]);
                Vector3 inVec, outVec;

                for (int i = 1; i < _points.Count - 1; ++i)
                {
                    inVec = (_points[i].position - _points[i - 1].position).normalized;
                    outVec = (_points[i + 1].position - _points[i].position).normalized;

                    if (Vector3.Angle(inVec, outVec) < maxAngle)
                        points.Add(Point.LerpUnclamped(_points[i], Point.LerpUnclamped(_points[i - 1], _points[i + 1], 0.5f), factor));
                    else
                        points.Add(_points[i]);
                }

                points.Add(_points[_points.Count - 1]);
                _points.Clear();
                _points.AddRange(points);
            }
        }

    }
}