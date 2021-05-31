using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeAudioScript : MonoBehaviourReferenced {
    List<Material> audioMats = new List<Material>();
    List<GameObject> trees = new List<GameObject>();
    List<MeshRenderer[]> mrList = new List<MeshRenderer[]>();

    private float beatCounterFull = 0;
    private float beatCounterSubD = 0;

    private bool animate = false;

    private void Start() {
        BeatDetector beatDetector = referenceManagement.beatDetector;
        beatDetector.bdOnEighth.AddListener(OnBeatSubD);
        audioMats = referenceManagement.treeAudioMats;
        trees = referenceManagement.trees;

        SetBasePos();
    }

    private void SetBasePos() {
        for (int i = 0; i < trees.Count; i++) {
            MeshRenderer[] mrs;
            mrs = trees[i].GetComponentsInChildren<MeshRenderer>();
            mrList.Add(mrs);
            for (int j = 0; j < mrs.Length; j++) {
                for (int k = 0; k < mrs[j].materials.Length; k++) {
                    mrs[j].materials[k].SetVector("_BasePos", trees[i].transform.position);
                }
            }
        }
    }

    private void OnBeatSubD() {

        if (animate) {
            beatCounterSubD++;
            for (int i = 0; i < mrList.Count; i++) {
                Debug.Log($"i = {i}");
                for (int j = 0; j < mrList[i].Length; j++) {
                    Debug.Log($"j = {j}");
                    for (int k = 0; k < mrList[i][j].materials.Length; k++) {
                        Debug.Log($"k = {k}");
                        mrList[i][j].materials[k].SetFloat("_BeatCounter", beatCounterSubD);
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Car")) {
            CarAI car = other.GetComponentInParent<CarAI>();
            if (!car.autopilotEnabled) {
                animate = true;
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Car")) {
            CarAI car = other.GetComponentInParent<CarAI>();
            if (!car.autopilotEnabled) {
                animate = false;
            }
        }
    }
}