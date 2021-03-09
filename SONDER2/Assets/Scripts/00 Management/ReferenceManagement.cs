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

    [Header("Automation")]
    public GameObject streetlight;

    [Header("UI")]
    public GameObject switchImgObj;

    [Header("Sounds")]
    public AudioSource switchSound;
    public AudioSource selectedSwitchCar;
    

    [SerializeField] private float BPM;

    public float GetBPM() {
        return BPM;
    }
}
