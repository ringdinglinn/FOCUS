using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreetlampAudioTest : MonoBehaviourReferenced {

    private float p = 0.5f;

    [SerializeField] private float dist = 0.5f;

    private Light myLight;


    private void Start() {
        AudioProcessor processor = referenceManagement.audioProcessor;
        processor.onBeat.AddListener(OnBeatDetected);
        myLight = GetComponentInChildren<Light>();
    }

    void OnBeatDetected() {
        if (Random.Range(0f,1f) <= p) {
            Flicker();
        }
    }

    void Flicker() {
        myLight.enabled = false;
        StartCoroutine(SwitchBackOn());
    }

    IEnumerator SwitchBackOn() {
        float waitTime = Random.Range(0.2f, 0.5f);
        yield return new WaitForSeconds(waitTime);
        myLight.enabled = true;
    }
}