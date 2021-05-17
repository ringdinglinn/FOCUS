using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;


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
    public JourneyManagement journeyManagement;
    public MusicManagement musicManagement;
    public DangleManagement dangleManagement;
    public LevelManagement levelManagement;

    [Header("Player Items")]
    public Camera1stPerson cam;
    public SwitchingBehaviour initialCarSB;

    [Header("Audio")]
    public AudioProcessor audioProcessor;
    public BeatDetector beatDetector;
    public List<Material> treeAudioMats;
    [SerializeField] private float BPM;
    [SerializeField] private float BeatSubdivisions;
    [SerializeField] private float BeatWindow;

    public float GetBPM() {
        return BPM;
    }

    public float GetSubdivisions() {
        return BeatSubdivisions;
    }

    public float GetBeatWindow() {
        return BeatWindow;
    }

    [Header("Levels")]
    public List<GameObject> levels;

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
    public GameObject beatIndicatorObj;
    public List<Sprite> gearSprites;
    public GameObject youDidItText;
    public GameObject thankYouText;
    public Canvas canvas;

    [Header("Sounds")]
    public AudioSource switchSound;
    public AudioSource selectedSwitchCar;
    public AudioSource voiceClips;
    public VoiceClipBehaviour voiceClipBehaviour;

    [FMODUnity.EventRef]
    public string track1;
    [FMODUnity.EventRef]
    public string gearShiftStart;
    [FMODUnity.EventRef]
    public string gearShiftStop;
    [FMODUnity.EventRef]
    public string headlightsFlash;
    [FMODUnity.EventRef]
    public string playerDrum;
    [FMODUnity.EventRef]
    public string radioStatic;
    [FMODUnity.EventRef]
    public string flashShort;
    [FMODUnity.EventRef]
    public string flashLong;
    [FMODUnity.EventRef]
    public string switchDone;
    [FMODUnity.EventRef]
    public string flashOn;
    [FMODUnity.EventRef]
    public string flashOff;
    [FMODUnity.EventRef]
    public string flashHum;
    [FMODUnity.EventRef]
    public string signalSuccess;
    [FMODUnity.EventRef]
    public string signalFailure;

    [Header("Switching")]
    [Range(0.0f, 1.0f)]
    public float switchViewWidth = 0.8f;
    [Range(0.0f, 1.0f)]
    public float switchViewHeight = 0.8f;
    public float switchViewRange = 200;

    [Header("Sprites")]
    public List<Texture> morseSingalTex;
    public Material morseSingalMat;
    public Texture2D morseSignalLong;
    public Texture2D morseSignalShort;

    [Header("Journey")]
    public GameObject alternatePath0;

    [Header("TerrainManipulation")]
    public TerrainManagement terrainManagement;
    public List<MeshFilter> entryFilters;
    public bool cutTerrain;

    [Header("Steering")]
    public SteerAssitMode steerAssitMode;
    public float turnAssistStrength = 5;

    [Header("Rendering")]
    public Volume postProcess;
}
