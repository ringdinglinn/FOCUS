using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeAudioScript : MonoBehaviourReferenced {
    List<Material> audioMats = new List<Material>();
    List<GameObject> trees = new List<GameObject>();
    List<MeshRenderer[]> mrList = new List<MeshRenderer[]>();

    private float beatCounterFull = 0;
    private float beatCounterSubD = 0;

    private void Start() {
        BeatDetector beatDetector = referenceManagement.beatDetector;
        beatDetector.bdOnFourth.AddListener(OnBeatFull);
        beatDetector.bdOnEigth.AddListener(OnBeatSubD);
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
                mrs[j].materials[0].SetVector("_BasePos", trees[i].transform.position);
            }
        }
    }

    private void OnBeatFull() {

    }

    private void OnBeatSubD() {
        beatCounterSubD++;
        //for (int i = 0; i < audioMats.Count; i++) {
        //    audioMats[i].SetFloat("BeatCounter", beatCounterSubD);
        //}

        for (int i = 0; i < mrList.Count; i++) {
            for (int j = 0; j < mrList[i].Length; j++) {
                mrList[i][j].materials[0].SetFloat("BeatCounter", beatCounterSubD);
            }
        }
    }
}