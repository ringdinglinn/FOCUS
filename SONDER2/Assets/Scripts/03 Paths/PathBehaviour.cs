using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class PathBehaviour : MonoBehaviourReferenced {
	private PathCreator path;
    private PathManagement pathManagement;

    private int id;

    private GameObject startTrigger;
    private GameObject endTrigger;

    [SerializeField] float speedLimit;

    private void OnEnable() {
        path = GetComponent<PathCreator>();
        pathManagement = referenceManagement.pathManagement;
        pathManagement.AddToPaths(this);
    }

    private void Start() {
        startTrigger = GetChildWithName(gameObject, "StartTrigger");
        endTrigger = GetChildWithName(gameObject, "EndTrigger");
        SetUpTriggers();
    }

    public void SetID(int newID) {
        id = newID;
    }

    public PathCreator GetPath() {
        return path;
    }

    private void SetUpTriggers() {
        PlaceTrigger(startTrigger, path.path.GetPointAtDistance(0), path.path.GetDirectionAtDistance(0));
        PlaceTrigger(endTrigger, path.path.GetPointAtDistance(path.path.length - 0.1f), path.path.GetDirectionAtDistance(path.path.length - 0.1f));
    }

    private void PlaceTrigger(GameObject obj, Vector3 pos, Vector3 dir) {
        obj.transform.position = pos;
        obj.transform.rotation = Quaternion.LookRotation(dir);
    }

    GameObject GetChildWithName(GameObject obj, string name) {
        Transform trans = obj.transform;
        Transform childTrans = trans.Find(name);
        if (childTrans != null) {
            return childTrans.gameObject;
        }
        else {
            return null;
        }
    }

    public float GetSpeedLimit() {
        return speedLimit;
    }
}