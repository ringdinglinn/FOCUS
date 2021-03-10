using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PathCreation;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(StartPosBehaviour))]
public class StartPosEditor : Editor {

    SerializedProperty startPosListSO;
    SerializedProperty initialPosIndexSO;

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
        startPosListSO = serializedObject.FindProperty("startPosList");
        initialPosIndexSO = serializedObject.FindProperty("initialPosIndex");
    }

    private static GUILayoutOption miniButtonWidth = GUILayout.Width(30f);

    public override void OnInspectorGUI() {

        serializedObject.Update();
        EditorGUILayout.PropertyField(initialPosIndexSO);

        for (int i = 0; i < startPosList.Count; i++) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(startPosListSO.GetArrayElementAtIndex(i));
            if (GUILayout.Button($"⊗", miniButtonWidth)) {
                Undo.RecordObject(startPosBehaviour, "Delete Start Pos");
                startPosBehaviour.DeleteStartPos(i);
                serializedObject.ApplyModifiedProperties();
                if (myPath != null) {
                    Draw();
                }
            }
            if (GUILayout.Button($"★", miniButtonWidth)) {
                Undo.RecordObject(startPosBehaviour, "Mark As Initial Point");
                startPosBehaviour.MarkAsInitialPos(i);
                serializedObject.ApplyModifiedProperties();
                if (myPath != null) {
                    Draw();
                }
            }
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Start Position", GUILayout.Height(80))) {
            Undo.RecordObject(startPosBehaviour, "Add Start Pos");
            startPosBehaviour.AddStartPos();
            if (myPath != null) {
                Draw();
            }
        }

        if (GUI.changed) {
            EditorUtility.SetDirty(startPosBehaviour);
            EditorSceneManager.MarkSceneDirty(startPosBehaviour.gameObject.scene);
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
            Color flagCol = i == startPosBehaviour.GetInitialPosIndex() ? Color.blue : Color.red;
            Handles.color = flagCol;
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
            Handles.DrawSolidRectangleWithOutline(rect2, flagCol, Color.white);
            Handles.EndGUI();
            if (initPos != pos) {
                Undo.RecordObject(startPosBehaviour, "Move Start Pos");
                startPosBehaviour.MoveStartPos(i, pos);
            }
        }
    }
}