﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreetlampAudioTest : MonoBehaviourReferenced {

    private float p = 0.5f;

    [SerializeField] private float dist = 0.5f;

    private Light myLight;

    private bool onSubBeat;


    private void Start() {
        BeatDetector beatDetector = referenceManagement.beatDetector;
        beatDetector.bdOnBeatFull.AddListener(OnFullBeatDetected);
        beatDetector.bdOnBeatSubD.AddListener(OnSubDBeatDetected);
        myLight = GetComponentInChildren<Light>();
        //onSubBeat = Random.Range(0f, 1f) <= 0.66f ? true : false;
    }

    void OnFullBeatDetected() {
        if (Random.Range(0f, 1f) <= p) {
            Flicker();
        }

        //if (!onSubBeat) {
        //    Flicker();
        //}
    }

    void OnSubDBeatDetected() {
        //if (Random.Range(0f, 1f) <= p) {
        //    Flicker();
        //}

        //if (onSubBeat) {
        //    Flicker();
        //}
    }

    void Flicker() {
        myLight.enabled = false;
        StartCoroutine(SwitchBackOn());
    }

    IEnumerator SwitchBackOn() {
        float waitTime = Random.Range(0.1f, 0.2f);
        yield return new WaitForSeconds(waitTime);
        myLight.enabled = true;
    }
}