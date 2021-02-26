using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GreasePencil
{
    public class DrawModePlanar : DrawModeImpl
    {
        public override int Order { get { return 100; } }
        public override string Name { get { return "Planar"; } }
        public override string Tooltip { get { return "Draws on a defined 'canvas' plane"; } }

        Vector3[] _planePoints = new Vector3[] { Vector3.zero, Vector3.up, Vector3.right };
        int[] _segments = new int[] { 0, 1, 1, 2, 2, 0 };
        int _currentPoint = 0;

        protected override void DrawSceneGUI()
        {
            var evt = Event.current;

            if (evt.control && !evt.shift)
            {
                if (evt.isMouse && (evt.button == 0))
                {
                    if ((evt.type == EventType.MouseDown) || (evt.type == EventType.MouseDrag))
                    {
                        RaycastHit hit;
                        if (IntersectMesh.Raycast(ScreenToGUIPoint(evt), out hit, EditorState.Current.ignoreObjects))
                            _planePoints[_currentPoint] = hit.point;
                    }

                    if ((evt.type == EventType.MouseUp))
                        _currentPoint = (_currentPoint + 1) % 3;
                }
            }

            if (evt.type == EventType.Repaint)
            {
                Handles.matrix = Matrix4x4.identity;
                Handles.DrawDottedLines(_planePoints, _segments, 5f);

                if (evt.control && !evt.shift)
                    DrawPoint(-1, _planePoints[_currentPoint], 0.05f);
            }
        }

        protected override bool DoHit(Event evt, Vector2 screenPosition, out GreasePencilHit worldPosition)
        {
            var plane = new Plane(_planePoints[0], _planePoints[1], _planePoints[2]);

            bool wasHit = false;
            if ((evt.type == EventType.MouseDown) || (evt.type == EventType.MouseUp))
                wasHit = GreasePencilEditor.PlaneCast(plane, screenPosition, EditorState.Current.camera, out worldPosition);
            else if (evt.type == EventType.MouseDrag)
                return GreasePencilEditor.PlaneCast(plane, screenPosition, EditorState.Current.camera, out worldPosition);
            else
                worldPosition = new GreasePencilHit();

            return wasHit;
        }

        public override void GetInfos(List<string> infos)
        {
            base.GetInfos(infos);

            infos.Add("<b>Ctrl + LMB</b> - Reposition Plane Point");
        }
    }
}