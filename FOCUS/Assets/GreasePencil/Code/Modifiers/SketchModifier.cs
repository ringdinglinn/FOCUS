using System.Collections.Generic;
using UnityEngine;

namespace GreasePencil
{
    public class SketchModifier : GreasePencilModifier
    {
        [SerializeField, Range(0, 10)] float _scatter = 0.5f;
        [SerializeField, Range(0, 1)] float _overlapMin = 0.3f;
        [SerializeField, Range(0, 1)] float _overlapMax = 0.3f;

        [Header("Construction Lines")]
        [SerializeField, Range(0, 1)] float _constructionChance = 0.01f;
        [SerializeField] float _constructionScale = 3f;
        [SerializeField] int _seed = 12345;

        public override void ModifyStroke(Layer layer, List<Stroke> modifiedStrokes)
        {
            List<Point> points;
            List<Stroke> strokes;

            using (PooledList<Point>.Get(out points))
            using (PooledList<Stroke>.Get(out strokes, modifiedStrokes))
            {
                modifiedStrokes.Clear();
                foreach (var stroke in strokes)
                    Sketch(layer, stroke, modifiedStrokes, this);

                Stroke.ReleaseStrokes(strokes);
            }
        }

        private void Sketch(Layer layer, Stroke stroke, List<Stroke> resultStrokes, SketchModifier modifier)
        {
            List<Point> buffer;
            List<Point> points;

            float scatter = (_scatter / 10f);
            scatter = (scatter * scatter) * 10f;

            using (PooledList<Point>.Get(out buffer, stroke.Points))
            using (PooledList<Point>.Get(out points))
            {
                var state = Random.state;
                Random.InitState(_seed);

                for (int i = 0; i < buffer.Count - 1; ++i)
                {
                    var p1 = buffer[i];
                    var p2 = buffer[i + 1];

                    var vec = (p2.position - p1.position).normalized;

                    var random = Random.onUnitSphere * scatter;

                    var plane1 = new Plane(vec, p1.position);
                    var plane2 = new Plane(vec, p2.position);

                    var constructionChance = modifier._constructionChance  * modifier._constructionChance;
                    constructionChance = (Random.value < constructionChance) ? modifier._constructionScale : 1f;

                    p1.position = plane1.ClosestPointOnPlane(p1.position + random) - vec * Random.Range(modifier._overlapMin, modifier._overlapMax) * constructionChance;
                    p2.position = plane2.ClosestPointOnPlane(p2.position + random) + vec * Random.Range(modifier._overlapMin, modifier._overlapMax) * constructionChance;

                    points.Clear();
                    points.Add(p1);
                    points.Add(p2);

                    var newStroke = Stroke.New(stroke.Right, stroke.Up);
                    newStroke.SetColors(stroke.LineColor, stroke.FillColor);
                    newStroke.Points = points;
                    newStroke.CloseStroke();
                    resultStrokes.Add(newStroke);
                }

                Random.state = state;
            }
        }
    }
}