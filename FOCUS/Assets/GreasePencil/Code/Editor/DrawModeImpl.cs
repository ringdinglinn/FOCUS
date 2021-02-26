using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GreasePencil
{
    public abstract class DrawModeImpl : System.IComparable<DrawModeImpl>
    {
        public abstract int Order { get; }
        public abstract string Name { get; }
        public abstract string Tooltip { get; }

        public virtual void OnSceneGUI(Event evt)
        {
            Vector3 screenPosition = ScreenToGUIPoint(evt);

            var wasHit = false;
            var worldHit = new GreasePencilHit();

            if (EditorWindow.mouseOverWindow is SceneView)
            {
                if (evt.shift)
                {
                    EditorState.Current.erase = true;
                    EditorState.Current.softErase = !evt.control;
                }
                else
                {
                    EditorState.Current.erase = false;
                    EditorState.Current.softErase = false;
                }
            }
            else
            {
                EditorState.Current.erase = false;
                EditorState.Current.softErase = false;
            }

            if ((evt.type != EventType.Layout) && (evt.type != EventType.Repaint))
            {
                if (EditorState.Current.erase)  // we're going to delete
                    wasHit = GreasePencilEditor.PlaneCast(EditorState.Current.camera.transform.TransformPoint(Vector3.forward), screenPosition, EditorState.Current.camera, out worldHit);
                    //wasHit = GreasePencilEditor.PlaneCast(EditorState.Current.Instance.transform.position, screenPosition, EditorState.Current.camera, out worldHit);
                else            // we're going to draw
                    wasHit = DoHit(evt, screenPosition, out worldHit);
            }

            DrawSceneGUI();

            if (wasHit)
            {
                if ((evt.button == 0) && !EditorState.Current.erase && (!evt.shift) && (!evt.control))
                {
                    if (evt.type == EventType.MouseDown)
                        DoMouseDown(evt, worldHit);
                    else if (!EditorState.Current.Instance.StrokeFinalized())
                    {
                        if (evt.type == EventType.MouseDrag)
                            DoMouseDrag(evt, worldHit);
                        else if (evt.type == EventType.MouseUp)
                            DoMouseUp(evt, worldHit);
                    }
                }
                else if (EditorState.Current.erase)
                {
                    if (evt.button == 0)
                        DoErase(evt, screenPosition, worldHit);
                }
            }

            if (EditorState.Current.erase)
                ShowErase();
        }

        protected static Vector3 ScreenToGUIPoint(Event evt)
        {
            Vector3 screenPosition = evt.mousePosition;
            screenPosition.y = Screen.height - (screenPosition.y + 38f);
            return screenPosition;
        }

        protected abstract bool DoHit(Event evt, Vector2 screenPosition, out GreasePencilHit worldHit);
        protected virtual void DrawSceneGUI() { }

        protected virtual void DoMouseDown(Event evt, GreasePencilHit worldHit)
        {
            Undo.RecordObject(EditorState.Current.Instance, "Add Stroke");
            EditorState.Current.Instance.StartStroke(new Point(worldHit),
                HandleUtility.GetHandleSize(EditorState.Current.Instance.transform.position),
                EditorState.Current.camera.transform,
                EditorState.Current.lineColor,
                EditorState.Current.fillColor);

            evt.Use();
        }

        protected virtual void DoMouseDrag(Event evt, GreasePencilHit worldHit)
        {
            EditorState.Current.Instance.UpdateStroke(new Point(worldHit));
            evt.Use();
        }

        protected virtual void DoMouseUp(Event evt, GreasePencilHit worldHit)
        {
            EditorState.Current.Instance.EndStroke(new Point(worldHit));
            evt.Use();
        }

        protected virtual void DoErase(Event evt, Vector3 screenPosition, GreasePencilHit worldHit)
        {
            if ((evt.type == EventType.MouseDrag) || (evt.type == EventType.MouseDown))
            {
                if (!EditorState.Current.erase)
                {
                    EditorState.Current.erase = true;
                }

                EditorGUIUtility.AddCursorRect(new Rect(0f, 0f, Screen.width, Screen.height), MouseCursor.Arrow);
                EditorGUIUtility.SetWantsMouseJumping(0);

                // erase

                if (EditorState.Current.softErase)
                {
                    Undo.RegisterCompleteObjectUndo(EditorState.Current.Instance, "Erase Stroke");
                    List<int> points;
                    using (PooledList<int>.Get(out points))
                    {
                        GreasePencilEditor.GetErasePoints(screenPosition,
                            (index, position, size) => { points.Add(index); },
                            s =>
                            {
                                if (points.Count > 0)
                                {
                                    EditorState.Current.Instance.ActiveLayer.Remove(s, points);
                                    points.Clear();
                                }
                            });
                    }
                }
                else
                {
                    Undo.RegisterCompleteObjectUndo(EditorState.Current.Instance, "Erase Stroke");
                    List<int> points;
                    using (PooledList<int>.Get(out points))
                    {
                        GreasePencilEditor.GetErasePoints(screenPosition,
                        (index, position, size) => { points.Add(index); },
                        s =>
                        {
                            if (points.Count > 0)
                            {
                                EditorState.Current.Instance.ActiveLayer.Erase(s, points);
                                points.Clear();
                            }
                        });
                    }
                }

                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                //EditorState.Current.instance.UpdateStroke(new Point(worldHit));
                evt.Use();
            }
            else if ((evt.type == EventType.MouseUp) || (evt.type == EventType.MouseMove)
                || ((evt.type == EventType.KeyUp) && ((evt.keyCode == KeyCode.LeftShift) || (evt.keyCode == KeyCode.RightShift))))
            {
                if (EditorState.Current.softErase)
                    EditorState.Current.softErase = false;
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
        }

        private void ShowErase()
        {
            var evt = Event.current;

            if (evt.type == EventType.Repaint)
            {
                var screenPosition = ScreenToGUIPoint(evt);

                // show all points
                Handles.color = Color.yellow;

                Stroke stroke;

                var tm = EditorState.Current.Instance.transform.localToWorldMatrix;
                Vector3 worldPosition, worldNormal;

                for (int s = 0; s < EditorState.Current.Instance.ActiveLayer.Strokes.Count; ++s)
                {
                    stroke = EditorState.Current.Instance.ActiveLayer.Strokes[s];

                    for (int i = 0; i < stroke.Points.Count; ++i)
                    {
                        worldPosition = tm.MultiplyPoint(stroke.Points[i].position);
                        worldNormal = tm.MultiplyVector(stroke.Points[i].normal).normalized;

                        var p = EditorState.Current.camera.WorldToScreenPoint(worldPosition) - screenPosition;
                        p.z = 0f;
                        DrawPoint(i, worldPosition, 0.02f);
                        //Handles.DrawLine(worldPosition, worldPosition + worldNormal * 0.5f);
                    }
                }

                Handles.color = Color.red;
                GreasePencilEditor.GetErasePoints(screenPosition, DrawPoint, null);

                Handles.BeginGUI();
                Handles.matrix = Matrix4x4.identity;

                if (EditorState.Current.erase)
                    Handles.color = EditorState.Current.softErase ? Color.yellow : Color.blue;
                else
                    Handles.color = EditorState.Current.softErase ? Color.red : Color.black;

                Handles.DrawWireDisc(evt.mousePosition, Vector3.forward, EditorState.EraserSize);
                Handles.color = new Color(EditorState.Current.softErase ? 1f : 0f, 0f, 0f, 0.125f);
                Handles.DrawSolidDisc(evt.mousePosition, Vector3.forward, EditorState.EraserSize);
                Handles.EndGUI();
            }
        }

        protected void DrawPoint(int index, Vector3 position, float size = 0.02f)
        {
            Event evt = Event.current;
            Handles.DotHandleCap(-1, position, Quaternion.identity, HandleUtility.GetHandleSize(position) * size, evt.type);
        }

        public int CompareTo(DrawModeImpl other)
        {
            return Order.CompareTo(other.Order);
        }

        public virtual void GetInfos(List<string> infos)
        {
            infos.Add("<b>LMB</b> - Draw");
            infos.Add("<b>Shift + LMB</b> - Remove Points");
            infos.Add("<b>Ctrl + Shift + LMB</b> - Clip Points");
        }
    }
}
