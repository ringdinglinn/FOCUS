using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainAudio : MonoBehaviourReferenced {

    MeshRenderer meshRenderer;
    float noiseOffset0;
    float[] noiseOffset = new float[3];
    float[] persistance = new float[3];
    float[] maxHeight = new float[3];
    float persistance0;
    float persistance1;
    float persistance2;
    float deltaPersistance = 0.2f;
    float maxHeight0;
    float maxHeight1;
    float maxHeight2;
    float deltaMaxHeight = 0.02f;

    int currentBeat;
    int totalBeats = 4;

    public AnimationCurve animCurve;

    LevelManagement levelManagement;

    float nOffset = 0;
    float animTime = 0.2f;

    private void Start() {
        referenceManagement.beatDetector.bdOnBar.AddListener(HandleBar);
        referenceManagement.beatDetector.bdOnHalf.AddListener(HandleHalf);
        referenceManagement.beatDetector.bdOnFourth.AddListener(HandleFourth);
        meshRenderer = GetComponent<MeshRenderer>();
        levelManagement = referenceManagement.levelManagement;

        for (int i = 0; i < 3; i++) {
            noiseOffset[i] = i * 2.5f;
        }

        nOffset = meshRenderer.material.GetFloat($"_NoiseOffset0");
    }

    private void HandleBar() {

    }

    private void HandleHalf() {

    }

    private void HandleFourth() {
        if (levelManagement.levelNr == 3) {
            currentBeat++;
            currentBeat %= totalBeats;
            StartCoroutine(Animate());
        }
    }

    IEnumerator Animate() {
        float time = animTime;
        int frame = 0;
        while (time > 0) {
            yield return new WaitForEndOfFrame();
            nOffset += animCurve.Evaluate(Mathf.InverseLerp(0, animTime, frame * Time.deltaTime));
            meshRenderer.material.SetFloat($"_NoiseOffset0", nOffset);
            frame++;
            time -= Time.deltaTime;
        }
    }
}