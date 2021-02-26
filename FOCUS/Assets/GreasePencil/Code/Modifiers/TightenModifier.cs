using System.Collections.Generic;
using UnityEngine;

namespace GreasePencil
{
    public class TightenModifier : GreasePencilModifier, IInlineModifier
    {
        [SerializeField, Range(0, 20)] int _iterations = 1;
        [SerializeField, Range(0, 0.6f)] float _smoothFactor = 0.5f;
        [SerializeField, Range(0, 180)] float _maxAngle = 30f;

        public override void ModifyStroke(Layer layer, List<Stroke> modifiedStrokes)
        {
            List<Point> points;
            foreach (var stroke in modifiedStrokes)
            {
                using (PooledList<Point>.Get(out points, stroke.Points))
                {
                    for (int i = 0; i < _iterations; ++i)
                    {
                        SmoothModifier.Smooth(points, _smoothFactor, _maxAngle);
                        SmoothModifier.Smooth(points, -_smoothFactor, _maxAngle);
                    }

                    stroke.Points = points;
                }
            }
        }
    }
}