using System.Collections.Generic;
using UnityEngine;

namespace GreasePencil
{
    public class ConnectModifier : GreasePencilModifier, ISerializationCallbackReceiver
    {
        [SerializeField] int _start = 0;
        [SerializeField] int _step = 1;
        [SerializeField] int _end = 0;

        public override void ModifyStroke(Layer layer, List<Stroke> modifiedStrokes)
        {
            if (modifiedStrokes.Count > 0)
                Connect(modifiedStrokes);
        }

        public void Connect(List<Stroke> strokes)
        {
            var newStrokes = new List<Stroke>();

            Validate();

            var depth = _start;
            var calculate = true;

            List<Point> points;

            using (PooledList<Point>.Get(out points))
            {
                while (calculate)
                {
                    points.Clear();

                    for (int i = 0; i < strokes.Count; ++i)
                    {
                        if ((depth + _end) >= strokes[i].Points.Count)
                        {
                            calculate = false;
                            break;
                        }

                        if (strokes.Count <= i)
                            Debug.LogFormat("Over Stroke {0} {1}", i, strokes.Count);

                        else if (strokes[i].Points.Count <= depth)
                            Debug.LogFormat("Over Points {0} {1}", depth, strokes[i].Points.Count);

                        points.Add(new Point(strokes[i].Points[depth]));
                    }

                    var stroke = Stroke.New(strokes[0].Right, strokes[0].Up);
                    stroke.SetColors(strokes[0].LineColor, strokes[0].FillColor);
                    stroke.Points = points;
                    stroke.CloseStroke();
                    newStrokes.Add(stroke);

                    depth += _step;
                }
            }

            strokes.AddRange(newStrokes);
        }

        public void OnBeforeSerialize()
        {
            Validate();
        }

        private void Validate()
        {
            _start = Mathf.Max(0, _start);
            _step = Mathf.Max(1, _step);
            _end = Mathf.Max(0, _end);
        }

        public void OnAfterDeserialize()
        {
        }
    }
}