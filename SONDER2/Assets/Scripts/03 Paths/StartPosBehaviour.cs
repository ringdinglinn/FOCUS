using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PathCreation;

[System.Serializable]
public class StartPosBehaviour : MonoBehaviour {
    public PathCreator myPath;

    [SerializeField]
    private List<Vector3> startPosList;

    [SerializeField]
    private List<Vector3> streetlightMarkers;

    [SerializeField]
    private int initialPosIndex = -1;

    public int pathID;

    public StartPosBehaviour() {
        startPosList = new List<Vector3>();
        streetlightMarkers = new List<Vector3>();
    }


    public void MoveStartPos(int index, Vector3 pos) {
        startPosList[index] = pos;
    }

    public void MoveStreetlightMarker(int index, Vector3 pos) {
        streetlightMarkers[index] = pos;
    }

    public List<Vector3> GetStartPosList() {
        return startPosList;
    }

    public List<Vector3> GetStreetlightMarkerList() {
        return streetlightMarkers;
    }

    public void AddStartPos() {
        startPosList.Add(myPath.path.GetPointAtDistance(0));
    }

    public void AddStreetlightMarker() {
        streetlightMarkers.Add(myPath.path.GetPointAtDistance(0));
    }

    public void DeleteStartPos(int i) {
        startPosList.RemoveAt(i);
        if (i == initialPosIndex) {
            initialPosIndex = -1;
        }
    }

    public void DeleteStreetlightMarker(int i) {
        streetlightMarkers.RemoveAt(i);
    }

    public void MarkAsInitialPos(int i) {
        initialPosIndex = i;
    }

    public void ResetInitialPos() {
        initialPosIndex = -1;
    }

    public int GetInitialPosIndex() {
        return initialPosIndex;
    }
}

