using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeAudioScript : MonoBehaviourReferenced {
    List<Material> audioMats = new List<Material>();

    private float beatCounterFull = 0;
    private float beatCounterSubD = 0;

    private void Start() {
        BeatDetector beatDetector = referenceManagement.beatDetector;
        beatDetector.bdOnBeatFull.AddListener(OnBeatFull);
        beatDetector.bdOnBeatSubD.AddListener(OnBeatSubD);
        audioMats = referenceManagement.treeAudioMats;
    }

    private void OnBeatFull() {

    }

    private void OnBeatSubD() {
        beatCounterSubD++;
        for (int i = 0; i < audioMats.Count; i++) {
            audioMats[i].SetFloat("BeatCounter", beatCounterSubD);
        }
    }
}