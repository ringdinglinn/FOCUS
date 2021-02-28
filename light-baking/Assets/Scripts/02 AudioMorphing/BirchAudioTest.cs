using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirchAudioTest : MonoBehaviourReferenced {

    private bool up = false;
    private float p = 1f;
    private Vector3 dist = new Vector3(0,0.5f,0);


    private void Start() {
        AudioProcessor processor = FindObjectOfType<AudioProcessor>();
        processor.onBeat.AddListener(OnBeatDetected);
        Debug.Log(processor);
        if (Random.Range(0f,1f) < 0.5f) up = true;
        ChangePos();
        RandomizePos();
    }

    void OnBeatDetected() {
        Debug.Log("BEAT!!!!");
        if (Random.Range(0f,1f) <= p) {
            up = !up;
            ChangePos();
        }
    }

    private void ChangePos() {
        if (up) {
            gameObject.transform.position += dist;
        } else {
            gameObject.transform.position -= dist;
        }
    }

    private void RandomizePos() {
        float yOffset = Random.Range(-0.5f, 0.5f);
        transform.position += new Vector3(0, yOffset, 0);
    }
}