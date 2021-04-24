using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using VehicleBehaviour;
using UnityEngine.Rendering.HighDefinition;
using TMPro;
using UnityEngine.UI;


public class SwitchingBehaviour : MonoBehaviourReferenced {
    private CarAI carAI;
    private WheelVehicle wheelVehicle;
    private SwitchingManagement switchingManagement;
    private PathCreator myPath;
    private BeatDetector beatDetector;
    private CarManagement carManagement;

    private PathBehaviour pathBehaviour;

    public MeshRenderer meshRenderer;
    public GameObject morseSignalDisplay;
    public MeshRenderer morseSingalRenderer;

    public BoxCollider boxCollider;

    public GameObject camRotTarget;
    public GameObject camTranslateTarget;

    public HDAdditionalLightData headlight1;
    public HDAdditionalLightData headlight2;
    private float activeCarVolumetric = 0.5f;
    private float inactiveCarVolumetric = 3.5f;

    public int id;
    public bool isInitialCar = false;

    FlashType[] signalPattern = new FlashType[3];
    private float longSignalP = 0.5f;
    private float headlightDefaultIntensity;
    private float headlightDefaultRange;

    bool beatFull;
    bool beatSubD;

    bool flash = false;
    bool flashOn = false;
    bool signalCoolDown = false;

    float beatInterval;
    float beatIntervalSubD;

    public GameObject volumetrics;

    private void OnEnable() {
        CollectReferences();
        GenerateSignalPattern();
        referenceManagement.beatDetector.bdOnBeatFull.AddListener(HandleBeatFull);
        referenceManagement.beatDetector.bdOnBeatSubD.AddListener(HandleBeatSubD);
        headlightDefaultIntensity = headlight1.intensity;
        headlightDefaultRange = headlight1.range;
        carManagement.AddSwitchingBehaviour(this);
    }

    private void OnDisable() {
        referenceManagement.beatDetector.bdOnBeatFull.RemoveListener(HandleBeatFull);
        referenceManagement.beatDetector.bdOnBeatSubD.RemoveListener(HandleBeatSubD);
    }

    public void CollectReferences() {
        wheelVehicle = GetComponent<WheelVehicle>();
        wheelVehicle.IsPlayer = false;
        carAI = GetComponent<CarAI>();
        carManagement = referenceManagement.carManagement;
        switchingManagement = referenceManagement.switchingManagement;
        id = switchingManagement.allSwitchingBehaviours.Count;
        beatDetector = referenceManagement.beatDetector;
        beatInterval = beatDetector.BeatInterval;
        beatIntervalSubD = beatDetector.BeatIntervalSubD;
    }

    public void SwitchIntoCar(Camera1stPerson cam) {
        carAI.SwitchOffAutopilot();
        carAI.cam = cam;
        switchingManagement.ActiveCar = this;
        wheelVehicle.IsPlayer = true;
        if (isInitialCar && carManagement.HasManualInitialCar()) wheelVehicle.IsPlayer = false;

        SetHeadlightsActiveCar();
    }

    public void SwitchOutOfCar() {
        if (!(carManagement.HasManualInitialCar() && isInitialCar)) {
            carAI.SwitchOnAutopilot();
        }
        carAI.cam = null;
        wheelVehicle.IsPlayer = false;

        SetHeadlightsInactiveCar();
        GenerateSignalPattern();
    }

    public void SetHeadlightsActiveCar() {
        headlight1.volumetricDimmer = activeCarVolumetric;
        headlight2.volumetricDimmer = activeCarVolumetric;
    }

    public void SetHeadlightsInactiveCar() {
        headlight1.volumetricDimmer = inactiveCarVolumetric;
        headlight2.volumetricDimmer = inactiveCarVolumetric;
    }

    public int GetGear() {
        return carAI.CurrentGear;
    }

    private void GenerateSignalPattern() {
        for (int i = 0; i < signalPattern.Length; i++) {
            FlashType flash = Random.Range(0.0f, 1.0f) <= longSignalP ? FlashType.Long : FlashType.Short;
            signalPattern[i] = flash;
        }
    }

    public void DisplaySignalPattern() {
        if (!signalCoolDown) {
            StartCoroutine(Signal());
            StartCoroutine(DisplaySignalCoolDown());
        }
    }

    IEnumerator Signal() {
        for (int i = 0; i < signalPattern.Length; i++) {
            while (!beatSubD || flashOn) {
                yield return new WaitForEndOfFrame();
            }
            StartCoroutine(FlashHeadlight(signalPattern[i]));
            beatSubD = false;
        }
    }

    IEnumerator DisplaySignalCoolDown() {
        signalCoolDown = true;
        yield return new WaitForSeconds(8f);
        signalCoolDown = false;
    }

    public void SetFlash(bool b) {
        flash = b;
        if (flash) {
            headlight1.intensity = headlightDefaultIntensity * 20f;
            headlight2.intensity = headlightDefaultIntensity * 20f;
            headlight1.range = headlightDefaultRange * 1f;
            headlight2.range = headlightDefaultRange * 1f;
        } else {
            headlight1.intensity = headlightDefaultIntensity;
            headlight2.intensity = headlightDefaultIntensity;
            headlight1.range = headlightDefaultRange;
            headlight2.range = headlightDefaultRange;
        }
    }

    IEnumerator FlashHeadlight(FlashType flashType) {
        flashOn = true;
        volumetrics.SetActive(true);

        headlight1.intensity = headlightDefaultIntensity * 4f;
        headlight2.intensity = headlightDefaultIntensity * 4f;
        headlight1.range = headlightDefaultRange * 4f;
        headlight2.range = headlightDefaultRange * 4f;

        float time = beatIntervalSubD;
        if (flashType == FlashType.Long) {
            time = beatInterval;
        }
        yield return new WaitForSeconds(time);

        headlight1.intensity = headlightDefaultIntensity;
        headlight2.intensity = headlightDefaultIntensity;
        headlight1.range = headlightDefaultRange;
        headlight2.range = headlightDefaultRange;

        beatSubD = false;
        flashOn = false;
        volumetrics.SetActive(false);

    }

    IEnumerator ResetBeatValue() {
        yield return new WaitForEndOfFrame();
        beatFull = false;
        beatSubD = false;
    }

    private void HandleBeatFull() {
        beatFull = true;
        StartCoroutine(ResetBeatValue());
    }

    private void HandleBeatSubD() {
        beatSubD = true;
        StartCoroutine(ResetBeatValue());
    }

    public FlashType[] GetSignal() {
        return signalPattern;
    }

    public CarAI GetCarAI() {
        return carAI;
    }
}