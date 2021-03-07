using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PathCreation;

[CustomEditor(typeof(StartPosBehaviour))]
public class StartPosEditor : Editor {

    private Vector3 pos = Vector3.zero;

    private StartPosBehaviour startPosBehaviour;
    private PathCreator myPath;

    private PathBehaviour pathBehaviour;

    void OnEnable() {
        pathBehaviour = ((MonoBehaviour)target).gameObject.GetComponent<PathBehaviour>();
        myPath = pathBehaviour.GetPath();
    }

    private void OnSceneGUI() {
        Handles.color = Color.red;
        pos = Handles.FreeMoveHandle(pos, Quaternion.identity, HandleUtility.GetHandleSize(new Vector3(0,0,1)) * 0.2f, Vector3.zero, Handles.CircleHandleCap);
        pos = myPath.path.GetClosestPointOnPath(pos);
        pos = Handles.FreeMoveHandle(pos, Quaternion.identity, HandleUtility.GetHandleSize(new Vector3(0, 0, 1)) * 0.2f, Vector3.zero, Handles.CircleHandleCap);
        Handles.Label(pos, "Start Pos");
    }
}