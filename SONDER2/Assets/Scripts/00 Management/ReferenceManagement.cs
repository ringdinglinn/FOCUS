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
    public CarManagement carManagement;

    [Header("Player Items")]
    public Camera1stPerson cam;
    public SwitchingBehaviour initialCarSB;

    [Header("Audio")]
    public AudioProcessor audioProcessor;
    public BeatDetector beatDetector;
    public List<Material> treeAudioMats;

    [Header("Prefabs")]
    public GameObject streetlight;
    public GameObject carPrefab;
    public GameObject camPrefab;

    [Header("UI")]
    public GameObject switchImgObj;
    public GameObject perceptionBorderObj;

    [Header("Sounds")]
    public AudioSource switchSound;
    public AudioSource selectedSwitchCar;

    [Range(0.0f, 1.0f)]
    public float switchViewWidth = 0.8f;
    [Range(0.0f, 1.0f)]
    public float switchViewHeight = 0.8f;
    public float switchViewRange = 200;
    
    [SerializeField] private float BPM;
    [SerializeField] private float BeatSubdivisions;

    public float GetBPM() {
        return BPM;
    }

    public float GetSubdivisions() {
        return BeatSubdivisions;
    }
}
