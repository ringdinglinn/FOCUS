using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using VehicleBehaviour;
using UnityEngine.Rendering.HighDefinition;
using FMODUnity;


public class SwitchingBehaviour : MonoBehaviourReferenced {
    private CarAI carAI;
    private CarVisuals carVisuals;
    private WheelVehicle wheelVehicle;
    private SwitchingManagement switchingManagement;
    private PathCreator myPath;
    private BeatDetector beatDetector;
    private CarManagement carManagement;

    private PathBehaviour pathBehaviour;

    public MeshRenderer meshRenderer;
    public GameObject morseSignalDisplay;
    public MeshRenderer[] morseSignalRenderers;

    public BoxCollider boxCollider;

    public GameObject camRotTarget;
    public GameObject camTranslateTarget;

    public GameObject armaturenbrett;
    public GameObject armaturenbrett2;

    public GameObject spotlights;
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

    bool flash = false;
    bool flashOn = false;

    public GameObject volumetrics;
    public MeshRenderer volumetricRenderer0;
    public MeshRenderer volumetricRenderer1;
    MeshFilter volumetricMesh0;
    MeshFilter volumetricMesh1;

    FMOD.Studio.EventInstance flashShort;
    FMOD.Studio.EventInstance flashLong;
    FMOD.Studio.EventInstance carExterior;

    public bool headlightTester;

    public MeshRenderer[] headlightCubes;
    public Material frontlightMat;
    public Material frontlightMatDull;

    private bool isMarkedCar;
    public bool IsMarkedCar {
        get { return isMarkedCar; }
        set { if (value != isMarkedCar) MarkedCarValueChanged(value);
            isMarkedCar = value; }
    }

    bool displayingSignal;
    public int variation;

    private void OnEnable() {
        CollectReferences();
        GenerateSignalPattern();
        referenceManagement.beatDetector.bdOnFourth.AddListener(HandleBeatFull);
        referenceManagement.beatDetector.bdOnEighth.AddListener(HandleBeatSubD);
        headlightDefaultIntensity = headlight1.intensity;
        headlightDefaultRange = headlight1.range;
        carManagement.AddSwitchingBehaviour(this);
        volumetricMesh0 = volumetricRenderer0.GetComponent<MeshFilter>();
        volumetricMesh1 = volumetricRenderer1.GetComponent<MeshFilter>();
        flashShort = RuntimeManager.CreateInstance(referenceManagement.flashShort);
        flashLong = RuntimeManager.CreateInstance(referenceManagement.flashLong);
        carVisuals = GetComponent<CarVisuals>();
        carExterior = RuntimeManager.CreateInstance(referenceManagement.carExterior);
        RuntimeManager.AttachInstanceToGameObject(carExterior, transform, GetComponent<Rigidbody>());
        carExterior.start();
        carExterior.setParameterByName("CarPitch", Random.Range(0f, 1f));

        for (int i = 0; i < 3; i++) {
            morseSignalRenderers[i].materials[0].SetFloat("_Index", i);
        }
    }

    private void Start() {
        SetVertexColor(volumetricMesh0.mesh);
        SetVertexColor(volumetricMesh1.mesh);
    }

    private void OnDisable() {
        referenceManagement.beatDetector.bdOnFourth.RemoveListener(HandleBeatFull);
        referenceManagement.beatDetector.bdOnEighth.RemoveListener(HandleBeatSubD);
        carManagement.RemoveSwitchingBehaviour(this);
        carExterior.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public void CollectReferences() {
        wheelVehicle = GetComponent<WheelVehicle>();
        wheelVehicle.IsPlayer = false;
        carAI = GetComponent<CarAI>();
        carManagement = referenceManagement.carManagement;
        switchingManagement = referenceManagement.switchingManagement;
        id = switchingManagement.allSwitchingBehaviours.Count;
        beatDetector = referenceManagement.beatDetector;
    }

    public void SwitchIntoCar(Camera1stPerson cam, bool isInitialCar, int variation) {
        if (variation == this.variation) {
            carVisuals.SetCarVisuals((int)carVisuals.myCar, variation == 0 ? 1 : 0);
        }
        carVisuals.UpdateVisuals(true);
        carAI.SwitchOffAutopilot();
        carAI.cam = cam;
        switchingManagement.ActiveCar = this;
        wheelVehicle.IsPlayer = true;
        if (isInitialCar && carManagement.HasManualInitialCar()) wheelVehicle.IsPlayer = false;

        if (isInitialCar) SetActiveCarValues();

    }

    public void SwitchOutOfCar() {
        if (!(carManagement.HasManualInitialCar() && isInitialCar)) {
            carAI.SwitchOnAutopilot();
        }
        carAI.cam = null;
        wheelVehicle.IsPlayer = false;

        SetInactiveCarValues();
    }

    public void SetActiveCarValues() {
        volumetrics.SetActive(false);
        spotlights.SetActive(true);
        SetHeadlightsActiveCar();
        GameObject obj = variation == 0 ? armaturenbrett : armaturenbrett2;
        obj.SetActive(true);
        meshRenderer.gameObject.SetActive(false);

        //carExterior.setParameterByName("IsActiveCar", 0);
        carExterior.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

        foreach (MeshRenderer mr in headlightCubes) mr.material = frontlightMatDull;
    }

    public void SetInactiveCarValues() {
        volumetrics.SetActive(true);
        spotlights.SetActive(false);
        SetHeadlightsInactiveCar();
        GenerateSignalPattern();
        armaturenbrett.SetActive(false);
        armaturenbrett2.SetActive(false);
        meshRenderer.gameObject.SetActive(true);

        //carExterior.setParameterByName("IsActiveCar", 1);
        if (isInitialCar) carExterior.start();

        foreach (MeshRenderer mr in headlightCubes) mr.material = frontlightMat;
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
        if (!displayingSignal) StartCoroutine(Signal());
    }

    IEnumerator Signal() {
        displayingSignal = true;
        yield return new WaitForSeconds(beatDetector.GetTimeUntilBar());
        for (int i = 0; i < signalPattern.Length; i++) {
            flashOn = true;
            volumetricRenderer0.material.SetInt("_FlashOn", flashOn ? 1 : 0);
            volumetricRenderer1.material.SetInt("_FlashOn", flashOn ? 1 : 0);

            float time = beatDetector.EighthInterval;

            if (signalPattern[i] == FlashType.Long) {
                time = beatDetector.FourthInterval;
                flashLong.start();
            } else {
                flashShort.start();
            }
            yield return new WaitForSeconds(time);

            flashOn = false;
            volumetricRenderer0.material.SetInt("_FlashOn", flashOn ? 1 : 0);
            volumetricRenderer1.material.SetInt("_FlashOn", flashOn ? 1 : 0);

            float deltaTime = beatDetector.HalfInterval - time;

            yield return new WaitForSeconds(deltaTime);
        }
        // interval
        if (isMarkedCar) yield return new WaitForSeconds(beatDetector.GetTimeUntilBar());

        displayingSignal = false;

        // restart
        if (isMarkedCar) StartCoroutine(Signal());
    }

    public void SetFlash(bool b) {
        flash = b;
        if (flash) {
            headlight1.intensity = headlightDefaultIntensity * 40f;
            headlight2.intensity = headlightDefaultIntensity * 40f;
            headlight1.range = headlightDefaultRange * 4f;
            headlight2.range = headlightDefaultRange * 4f;
        } else {
            headlight1.intensity = headlightDefaultIntensity;
            headlight2.intensity = headlightDefaultIntensity;
            headlight1.range = headlightDefaultRange;
            headlight2.range = headlightDefaultRange;
        }
    }

    IEnumerator ResetBeatValue() {
        yield return new WaitForEndOfFrame();
    }

    private void HandleBeatFull() {
        StartCoroutine(ResetBeatValue());
    }

    private void HandleBeatSubD() {
        StartCoroutine(ResetBeatValue());
    }

    public FlashType[] GetSignal() {
        return signalPattern;
    }

    public CarAI GetCarAI() {
        return carAI;
    }

    private void Update() {
        float angle = CameraAngle();
        volumetricRenderer0.material.SetFloat("_CameraAngle", angle);
        volumetricRenderer1.material.SetFloat("_CameraAngle", angle);
    }

    float offset = 70;

    private float CameraAngle() {
        float angle;
        Vector3 camPos = referenceManagement.cam.transform.position;
        Vector3 camDir = camPos - (transform.position + transform.forward * offset);
        camDir.Normalize();
        angle = Vector3.Dot(transform.forward, camDir);
        angle *= 0.2f;
        return angle;
    }

    private void SetVertexColor(Mesh mesh) {
        if (mesh.colors.Length != mesh.vertices.Length) {
            Color[] colors = new Color[mesh.vertices.Length];
            for (int i = 0; i < mesh.vertices.Length; i++) {
                colors[i] = transform.localPosition.y < transform.TransformPoint(mesh.vertices[i]).y ? Color.black : Color.white;
            }
            mesh.colors = colors;
        } else {
            for (int i = 0; i < mesh.vertices.Length; i++) {
                mesh.colors[i] = transform.localPosition.y < transform.TransformPoint(mesh.vertices[i]).y ? Color.black : Color.white;
            }
        }
    }

    void MarkedCarValueChanged(bool b) {
        if (!b) {
            StopSignal();
        }
    }

    void StopSignal() {
        StopCoroutine(Signal());
        flashShort.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        flashLong.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        volumetricRenderer0.material.SetInt("_FlashOn", 0);
        volumetricRenderer1.material.SetInt("_FlashOn", 0);
    }

    public CarVisuals GetCarVisuals() {
        return carVisuals;
    }
}