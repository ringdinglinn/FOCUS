using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class PathBehaviour : MonoBehaviourReferenced {
	private PathCreator path;
    private PathManagement pathManagement;

    private int id;

    private void OnEnable() {
        path = GetComponent<PathCreator>();
        pathManagement = referenceManagement.pathManagement;
        pathManagement.AddToPaths(this);
    }

    public void SetID(int newID) {
        id = newID;
    }


    public PathCreator GetPath() {
        return path;
    }
}