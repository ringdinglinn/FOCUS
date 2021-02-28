using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirchAudioTest : MonoBehaviourReferenced {

    private bool up = false;
    private float p = 1f;

    [SerializeField] private float dist = 0.5f;

    private Vector3 offset;



    private void Start() {
        AudioProcessor processor = FindObjectOfType<AudioProcessor>();
        processor.onBeat.AddListener(OnBeatDetected);
        offset = new Vector3(0, dist, 0);
        if (Random.Range(0f,1f) < 0.5f) up = true;
        ChangePos();
        RandomizePos();
    }

    void OnBeatDetected() {
        if (Random.Range(0f,1f) <= p) {
            up = !up;
            ChangePos();
        }
    }

    private void ChangePos() {
        if (up) {
            gameObject.transform.position += offset;
        } else {
            gameObject.transform.position -= offset;
        }
    }

    private void RandomizePos() {
        float yOffset = Random.Range(-0.5f, 0.5f);
        transform.position += new Vector3(0, yOffset, 0);
    }
}