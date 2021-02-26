using System.Collections.Generic;
using UnityEngine;

namespace GreasePencil
{
    public class OptimizeModifier : GreasePencilModifier
    {
        [SerializeField, Range(0, 1)] float _epsilon = 1f;

        public override void ModifyStroke(Layer layer, List<Stroke> modifiedStrokes)
        {
            List<Point> points;
            using (PooledList<Point>.Get(out points))
            {
                foreach (var stroke in modifiedStrokes)
                {
                    points.Clear();
                    points.AddRange(stroke.Points);
                    Optimize(points, _epsilon);
                    stroke.Points = points;
                }
            }
        }

        public static void Optimize(List<Point> inPoints, float epsilon)
        {
            epsilon = Mathf.Max(0f, epsilon);

            List<int> keep;
            List<Point> buffer;

            using (PooledList<int>.Get(out keep))
            using (PooledList<Point>.Get(out buffer, inPoints))
            {
                LineUtility.Simplify(inPoints.ConvertAll(p => p.position), epsilon * epsilon, keep);
                keep.Sort();

                inPoints.Clear();

                foreach (var k in keep)
                    inPoints.Add(buffer[k]);
            }
        }
    }
}
