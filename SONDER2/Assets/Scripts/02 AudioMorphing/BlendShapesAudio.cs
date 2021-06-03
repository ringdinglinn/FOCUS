using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendShapesAudio : MonoBehaviourReferenced {

    SkinnedMeshRenderer skinnedMesh;
    BeatDetector bd;

    int currentShape = 0;

    public int nrOfShapes = 4;
    public float duration = 1f;

    public int nrOfFrames = 2;

    int routineCounter = 0;
    int routinesEnded = 0;

    public bool moveForward;
    public float moveDist;

    public enum rate { bar, half, fourth };
    public rate myRate;

    Vector3 startPosition;

    private void OnEnable() {
        bd = referenceManagement.beatDetector;
        bd.bdOnFourth.AddListener(HandleFourth);
        bd.bdOnBar.AddListener(HandleBar);
        bd.bdOnHalf.AddListener(HandleHalf);
    }

    private void OnDisable() {
        bd.bdOnFourth.RemoveListener(HandleFourth);
        bd.bdOnBar.RemoveListener(HandleBar);
        bd.bdOnHalf.RemoveListener(HandleHalf);
    }

    private void Start() {
        skinnedMesh = GetComponent<SkinnedMeshRenderer>();
        startPosition = transform.position;
    }

    private void HandleFourth() {
        if (myRate == rate.fourth) {
            if (moveForward) {
                Animate();
            } else {
                StartCoroutine(Morph());
            }
        }
    }

    private void HandleHalf() {
        if (myRate == rate.half) {
            if (moveForward) {
                Animate();
            } else {
                StartCoroutine(Morph());
            }
        }
    }

    private void HandleBar() {
        if (myRate == rate.bar) {
            if (moveForward) {
                Animate();
            } else {
                StartCoroutine(Morph());
            }
        }
    }

    IEnumerator Morph() {
        if (routineCounter > routinesEnded) {
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
    }

    void Animate() {

        if (moveForward) {
            transform.position = transform.position + transform.right * -1f * moveDist;
        }

        skinnedMesh.SetBlendShapeWeight(currentShape % nrOfShapes, 0);
        skinnedMesh.SetBlendShapeWeight((currentShape + 1) % nrOfShapes, 100);

        if ((currentShape + 1) % nrOfShapes == 0) {
            transform.position = startPosition;
        }

        currentShape++;

    }
}