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

    float nOffset = 0;
    float animTime = 0.2f;

    private void Start() {
        referenceManagement.beatDetector.bdOnBar.AddListener(HandleBar);
        referenceManagement.beatDetector.bdOnHalf.AddListener(HandleHalf);
        referenceManagement.beatDetector.bdOnFourth.AddListener(HandleFourth);
        meshRenderer = GetComponent<MeshRenderer>();

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
        currentBeat++;
        currentBeat %= totalBeats;
        //SetValues(currentBeat);
        StartCoroutine(Animate());

    }

    void SetValues(int i) {
        if (i != totalBeats - 1) {
            noiseOffset[i] += 10;
            persistance[i] = 0.28f;
            maxHeight[i] = 3f;
        }
    }

    //private void Update() {
    //    for (int i = 0; i < 3; i++) {
    //        meshRenderer.material.SetFloat($"_Persistance{i}", -1f * Mathf.Pow(persistance[i] - 1f, 2) + 1);
    //        meshRenderer.material.SetFloat($"_NoiseOffset{i}", noiseOffset[i]);
    //        meshRenderer.material.SetFloat($"_MaxHeight{i}", maxHeight[i]);

    //        persistance[i] -= deltaPersistance * Time.deltaTime;
    //        persistance[i] = Mathf.Clamp(persistance[i], 0, 1);

    //        maxHeight[i] -= deltaMaxHeight * Time.deltaTime;
    //        maxHeight[i] = Mathf.Clamp(maxHeight[i], 0, 3.3f);
    //    }
    //}

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