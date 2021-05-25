﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreetlampAudioTest : MonoBehaviourReferenced {

    private float p = 0.5f;

    [SerializeField] private float dist = 0.5f;
    [SerializeField] private GameObject cone;

    private Light myLight;

    private bool onSubBeat;

    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Material matOn;
    [SerializeField] Material matOff;


    private void Start() {
        BeatDetector beatDetector = referenceManagement.beatDetector;
        beatDetector.bdOnFourth.AddListener(OnFullBeatDetected);
        beatDetector.bdOnEighth.AddListener(OnSubDBeatDetected);
        myLight = GetComponentInChildren<Light>();
    }

    void OnFullBeatDetected() {
        if (Random.Range(0f, 1f) <= p) {
            Flicker();
        }
    }

    void OnSubDBeatDetected() {
    }

    void Flicker() {
        //myLight.enabled = false;
        cone.SetActive(true);
        meshRenderer.materials[1] = matOff;
        StartCoroutine(SwitchBackOn());
    }

    IEnumerator SwitchBackOn() {
        float waitTime = Random.Range(0.1f, 0.2f);
        yield return new WaitForSeconds(waitTime);
        //myLight.enabled = true;
        cone.SetActive(false);
        meshRenderer.materials[1] = matOn;
    }
}