using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PathCreation;

public class StartPosBehaviour : MonoBehaviour {

    public PathCreator myPath;

    private List<Vector3> startPosList = new List<Vector3>();

    public void MoveStartPos(int index, Vector3 pos) {
        startPosList[index] = pos;
    }

    public List<Vector3> GetList() {
        return startPosList;
    }

    public void AddStartPos() {
        startPosList.Add(myPath.path.GetPointAtDistance(0));
    }

    public void DeleteStartPos(int i) {
        startPosList.RemoveAt(i);
    }
}

