using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PathCreation;

[CustomEditor(typeof(StartPosBehaviour))]
public class StartPosEditor : Editor {

    //private Vector3 pos = Vector3.zero;

    private StartPosBehaviour startPosBehaviour;
    private PathCreator myPath;

    private PathBehaviour pathBehaviour;

    private List<Vector3> startPosList = new List<Vector3>();
    private Camera sceneCam;

    private float lineWidth = 2;
    private float lineHeight = 30;
    private float flagWidth = 15;
    private float flagHeight = 10;

    void OnEnable() {
        startPosBehaviour = (StartPosBehaviour)target;
        startPosList = startPosBehaviour.GetList();
        myPath = startPosBehaviour.myPath;
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        startPosBehaviour = (StartPosBehaviour)target;

        for (int i = 0; i < startPosList.Count; i++) {
            if (GUILayout.Button($"Delete Start Position {i}")) {
                Undo.RecordObject(startPosBehaviour, "Delete Start Pos");
                startPosBehaviour.DeleteStartPos(i);
                if (myPath != null) {
                    Draw();
                }
            }
        }

        if (GUILayout.Button("Add Start Position", GUILayout.Height(80))) {
            Undo.RecordObject(startPosBehaviour, "Add Start Pos");
            startPosBehaviour.AddStartPos();
            if (myPath != null) {
                Draw();
            }
        }
        
    }

    private void OnSceneGUI() {
        if (myPath != null) {
            Draw();
        }
        else {
            Debug.LogError("No Path Assigned To StartPosBehaviour");
        }
    }

    private void Draw() {
        for (int i = 0; i < startPosList.Count; i++) {
            Vector3 pos = startPosList[i];
            Vector3 initPos = pos;
            Handles.color = Color.red;
            pos = Handles.FreeMoveHandle(pos, Quaternion.Euler(0, 0, 0), HandleUtility.GetHandleSize(new Vector3(0, 0, 0)) * 0.2f, Vector3.zero, Handles.CircleHandleCap);
            pos = myPath.path.GetClosestPointOnPath(pos);
            Handles.Label(pos, $"Start Pos {i}");

            //line
            sceneCam = SceneView.lastActiveSceneView.camera;
            Vector3 screenPos = sceneCam.WorldToScreenPoint(pos);
            Rect rect1 = new Rect(screenPos.x / 2 - lineWidth / 2, (sceneCam.pixelHeight - screenPos.y) / 2 - lineHeight, lineWidth, lineHeight);
            Rect rect2 = new Rect(screenPos.x / 2 + lineWidth - 1, (sceneCam.pixelHeight - screenPos.y) / 2 - lineHeight, flagWidth, flagHeight);
            Handles.BeginGUI();
            Handles.color = Color.white;
            Handles.DrawSolidRectangleWithOutline(rect1, Color.white, Color.white);
            Handles.DrawSolidRectangleWithOutline(rect2, Color.red, Color.white);
            Handles.EndGUI();
            if (initPos != pos) startPosBehaviour.MoveStartPos(i, pos);
        }
    }
}