using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendShapesAudio : MonoBehaviourReferenced {

    SkinnedMeshRenderer skinnedMesh;
    BeatDetector bd;
   
    int currentShape = 0;

    public int nrOfShapes = 4;
    public float duration = 1f;

    int nrOfFrames = 2;

    int routineCounter = 0;
    int routinesEnded = 0;

    private void OnEnable() {
        bd = referenceManagement.beatDetector;
        bd.bdOnBeatFull.AddListener(OnBeatFull);
    }

    private void OnDisable() {
        bd.bdOnBeatFull.RemoveListener(OnBeatFull);
    }

    private void Start() {
        skinnedMesh = GetComponent<SkinnedMeshRenderer>();
    }

    private void OnBeatFull() {
        StartCoroutine(Morph());
    }

    IEnumerator Morph() {
        if (routineCounter > routinesEnded) {
            Debug.Log($"{routineCounter}, {routinesEnded}, dont start yet");
            yield break;
        }
        int myNr = ++routineCounter;
        for (float i = 0.0f; i <= 100f; i += 100f / nrOfFrames) {
            skinnedMesh.SetBlendShapeWeight(currentShape % nrOfShapes, 100 - i);
            yield return new WaitForSeconds(0.01f);
        }
        for (float i = 0.0f; i <= 100f; i += 100f / nrOfFrames) {
            skinnedMesh.SetBlendShapeWeight((currentShape + 1) % nrOfShapes, i);
            yield return new WaitForSeconds(0.01f);
        }

        currentShape++;
        routinesEnded = myNr;
        Debug.Log($"Ended Routine {myNr}, routine counter = {routineCounter}");
    }
}