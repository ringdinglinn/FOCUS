using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoBehaviourReferenced : MonoBehaviour {
    public ReferenceManagement referenceManagement;

    protected virtual void Awake() {
        referenceManagement = GameObject.Find("Reference Management").GetComponent<ReferenceManagement>();
    }
}

public class ReferenceManagement : MonoBehaviour
{
    [Header("Managers")]
    public InputManagement inputManagement;
    public SwitchingManagement switchingManagement;
    public PathManagement pathManagement;

    [Header("Player Items")]
    public Camera1stPerson cam;
    public SwitchingBehaviour initialCarSB;

    [Header("Audio")]
    public AudioProcessor audioProcessor;
    public BeatDetector beatDetector;

    [Header("Automation")]
    public GameObject streetlight;
    public GameObject carPrefab;

    [Header("UI")]
    public GameObject switchImgObj;

    [Header("Sounds")]
    public AudioSource switchSound;
    public AudioSource selectedSwitchCar;
    

    [SerializeField] private float BPM;
    [SerializeField] private float BeatSubdivisions;

    public float GetBPM() {
        return BPM;
    }

    public float GetSubdivisions() {
        return BeatSubdivisions;
    }
}
