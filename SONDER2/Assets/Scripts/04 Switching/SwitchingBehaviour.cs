using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using VehicleBehaviour;
using UnityEngine.Rendering.HighDefinition;
using TMPro;


public class SwitchingBehaviour : MonoBehaviourReferenced {
    private CarAI carAI;
    private WheelVehicle wheelVehicle;
    private SwitchingManagement switchingManagement;
    private PathCreator myPath;
    private GearManagement gearManagement;

    private PathBehaviour pathBehaviour;

    public MeshRenderer meshRenderer;
    public Material visibleMat;
    public Material invisibleMat;
    public Material windowMat;

    public BoxCollider boxCollider;

    public GameObject camRotTarget;
    public GameObject camTranslateTarget;

    public HDAdditionalLightData headlight1;
    public HDAdditionalLightData headlight2;
    private float activeCarVolumetric = 0.5f;
    private float inactiveCarVolumetric = 3.5f;

    public int id;
    public bool isInitialCar = false;

    public TMP_Text gearText;

    public List<bool> signalPattern = new List<bool>(); // list where long signal is true and short signal is false
    private int signalPatternMin = 3;
    private int signalPatternMax = 3;
    private float longSignalP = 0.5f;
    private float headlightDefaultIntensity;
    private float headlightDefaultRange;

    bool beatFull;
    bool beatSubD;

    bool flash = false;
    bool signalCoolDown = false;

    private void OnEnable() {
        CollectReferences();
        GenerateSignalPattern();
        referenceManagement.beatDetector.bdOnBeatFull.AddListener(HandleBeatFull);
        referenceManagement.beatDetector.bdOnBeatSubD.AddListener(HandleBeatSubD);
        headlightDefaultIntensity = headlight1.intensity;
        headlightDefaultRange = headlight1.range;
    }

    private void OnDisable() {
        referenceManagement.beatDetector.bdOnBeatFull.RemoveListener(HandleBeatFull);
        referenceManagement.beatDetector.bdOnBeatSubD.RemoveListener(HandleBeatSubD);
    }

    public void CollectReferences() {
        wheelVehicle = GetComponent<WheelVehicle>();
        wheelVehicle.IsPlayer = isInitialCar;
        carAI = GetComponent<CarAI>();
        switchingManagement = referenceManagement.switchingManagement;
        id = switchingManagement.allSwitchingBehaviours.Count;
        gearManagement = referenceManagement.gearManagement;
    }

    public void SwitchIntoCar(Camera1stPerson cam) {
        carAI.SwitchOffAutopilot();
        ChangeColorToInvisible();
        carAI.cam = cam;
        switchingManagement.activeCar = this;
        wheelVehicle.IsPlayer = true;
        gearText.gameObject.SetActive(switchingManagement.HasMarkedCar);

        SetHeadlightsActiveCar();
    }

    public void SwitchOutOfCar() {
        carAI.SwitchOnAutopilot();
        carAI.cam = null;
        wheelVehicle.IsPlayer = false;
        gearText.gameObject.SetActive(false);
        int newGear = Random.Range(1, 6);
        if (newGear >= gearManagement.CurrentGear) newGear++;
        carAI.SetGear(newGear);

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

    public void ChangeColorToVisible() {
        Material[] mats = new Material[2];
        mats[0] = windowMat;
        mats[1] = visibleMat;
        meshRenderer.materials = mats;
    }

    public void ChangeColorToInvisible() {
        Material[] mats = new Material[2];
        mats[0] = windowMat;
        mats[1] = invisibleMat;
        meshRenderer.materials = mats;
    }

    public int GetGear() {
        return carAI.CurrentGear;
    }

    private void GenerateSignalPattern() {
        signalPattern.Clear();
        int length = Random.Range(signalPatternMin, signalPatternMax + 1);
        for (int i = 0; i < length; i++) {
            bool hit = Random.Range(0.0f, 1.0f) <= longSignalP ? true : false;
            signalPattern.Add(hit);
        }
    }

    public void DisplaySignalPattern() {
        if (!signalCoolDown) {
            StartCoroutine(Signal());
            StartCoroutine(DisplaySignalCoolDown());
        }
    }

    IEnumerator Signal() {
        for (int i = 0; i < signalPattern.Count; i++) {
            if (signalPattern[i]) {
                while (!beatFull) {
                    yield return new WaitForEndOfFrame();
                }
                StartCoroutine(FlashHeadlight());
            } else {
                while (!beatSubD) {
                    yield return new WaitForEndOfFrame();
                }
                StartCoroutine(FlashHeadlight());
            }
            beatFull = false;
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
            headlight1.intensity = headlightDefaultIntensity * 4f;
            headlight2.intensity = headlightDefaultIntensity * 4f;
            headlight1.range = headlightDefaultRange * 4f;
            headlight2.range = headlightDefaultRange * 4f;
        } else {
            headlight1.intensity = headlightDefaultIntensity;
            headlight2.intensity = headlightDefaultIntensity;
            headlight1.range = headlightDefaultRange;
            headlight2.range = headlightDefaultRange;
        }
    }

    IEnumerator FlashHeadlight() {
        headlight1.intensity = headlightDefaultIntensity * 4f;
        headlight2.intensity = headlightDefaultIntensity * 4f;
        headlight1.range = headlightDefaultRange * 4f;
        headlight2.range = headlightDefaultRange * 4f;
        float time = Random.Range(0.1f, 0.2f);
        yield return new WaitForSeconds(time);
        headlight1.intensity = headlightDefaultIntensity;
        headlight2.intensity = headlightDefaultIntensity;
        headlight1.range = headlightDefaultRange;
        headlight2.range = headlightDefaultRange;
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
}