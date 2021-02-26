using System.Collections.Generic;
using UnityEngine;

namespace GreasePencil
{
    public class NormalizeModifier : GreasePencilModifier, ISerializationCallbackReceiver
    {
        [SerializeField, Range(0.01f, 10f)] float _distance = 0.5f;
        [SerializeField] int _unifiedCount = 0;

        public override void ModifyStroke(Layer layer, List<Stroke> modifiedStrokes)
        {
            List<Point> points;
            foreach (var stroke in modifiedStrokes)
            {
                using (PooledList<Point>.Get(out points, stroke.Points))
                {
                    Normalize(points, _distance, _unifiedCount);
                    stroke.Points = points;
                }
            }
        }

        public static float GetLengths(List<Vector3> points, List<float> lengths, List<float> distances)
        {
            distances.Add(0f);

            float total = 0f;
            for (int i = 1; i < points.Count; ++i)
            {
                var d = Vector3.Distance(points[i], points[i - 1]);
                total += d;

                lengths.Add(d);
                distances.Add(total);
            }

            return total;
        }

        public static void Normalize(List<Point> points, float step, int unifiedCount)
        {
            step = Mathf.Max(0.0001f, step);
            unifiedCount = Mathf.Max(0, unifiedCount);

            List<Vector3> position;
            List<float> lengths;
            List<float> distances;
            List<Point> buffer;

            using (PooledList<Vector3>.Get(out position))
            using (PooledList<float>.Get(out lengths))
            using (PooledList<float>.Get(out distances))
            using (PooledList<Point>.Get(out buffer, points))
            {
                for (int i = 0; i < points.Count; ++i)
                    position.Add(points[i].position);

                float totalLength = GetLengths(position, lengths, distances);

                int count = Mathf.Min(4096, Mathf.RoundToInt(totalLength / step));

                if (unifiedCount > 2)
                    count = unifiedCount;

                step = totalLength / (count - 1);

                int currentIndex = 0;
                float currentDistance = 0f;
                float alpha;

                if ((totalLength <= step) && (points.Count > 2))
                {
                    points.RemoveRange(1, points.Count-2);
                    return;
                }

                points.Clear();

                for (int i = 0; i < count; ++i)
                {
                    currentDistance = step * i;

                    for (currentIndex = 0; currentIndex < distances.Count - 1; ++currentIndex)
                        if ((currentDistance >= distances[currentIndex]) && (currentDistance <= distances[currentIndex + 1]))
                            break;

                    if (i == (count - 1))
                        currentIndex = distances.Count - 2;

                    if (currentIndex + 1 < distances.Count)
                    {
                        alpha = Mathf.InverseLerp(distances[currentIndex], distances[currentIndex + 1], currentDistance);
                        points.Add(Point.LerpUnclamped(buffer[currentIndex], buffer[currentIndex + 1], alpha));
                    }
                }
            }
        }

        public void OnBeforeSerialize()
        {
            _unifiedCount = Mathf.Max(0, _unifiedCount);
        }

        public void OnAfterDeserialize()
        {
        }
    }
}