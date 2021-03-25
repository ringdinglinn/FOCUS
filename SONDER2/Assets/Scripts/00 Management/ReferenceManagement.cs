using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public VoiceClipManagement voiceClipManagement;
    public GearManagement gearManagement;

    [Header("Player Items")]
    public Camera1stPerson cam;
    public SwitchingBehaviour initialCarSB;

    [Header("Audio")]
    public AudioProcessor audioProcessor;
    public BeatDetector beatDetector;
    public List<Material> treeAudioMats;
    [SerializeField] private float BPM;
    [SerializeField] private float BeatSubdivisions;

    public float GetBPM() {
        return BPM;
    }

    public float GetSubdivisions() {
        return BeatSubdivisions;
    }

    [Header("Prefabs")]
    public GameObject streetlight;
    public GameObject carPrefab;
    public GameObject camPrefab;

    [Header("UI")]
    public GameObject switchImgObj;
    public GameObject perceptionBorderObj;
    public GameObject gearTextGoalObj;
    public GameObject gearTextCurrentObj;
    public Image gearImage;
    public List<Sprite> gearSprites;

    [Header("Sounds")]
    public AudioSource switchSound;
    public AudioSource selectedSwitchCar;
    public AudioSource voiceClips;
    public VoiceClipBehaviour voiceClipBehaviour;

    [FMODUnity.EventRef]
    public string gearShiftStart;
    public string gearShiftStop;

    [Header("Switching")]
    [Range(0.0f, 1.0f)]
    public float switchViewWidth = 0.8f;
    [Range(0.0f, 1.0f)]
    public float switchViewHeight = 0.8f;
    public float switchViewRange = 200;
}
