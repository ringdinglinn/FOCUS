using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using FMODUnity;
using FMOD.Studio;
using System;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;

public class SwitchingManagement : MonoBehaviourReferenced {

    public List<SwitchingBehaviour> allSwitchingBehaviours;
    private GearManagement gearManagement;
    private BeatDetector beatDetector;
    private SwitchingBehaviour activeCar;
    public SwitchingBehaviour ActiveCar {
        get { return activeCar; }
        set {
                if (activeCar != value) ActiveCarChanged(value);
                activeCar = value;
            }
    }
    private SwitchingBehaviour selectedCar;
    private SwitchingBehaviour markedCar;
    public SwitchingBehaviour MarkedCar {
        get {   return markedCar; }
        set {
                if (markedCar != value) MarkedCarChanged(value);
                markedCar = value;
        }
    }

    private Camera cam;
    private Camera1stPerson camController;
    private GameObject switchImgObj;
    private RectTransform switchImgTransform;
    private Image switchImg;
    private GameObject perceptionBorderObj;
    private RectTransform perceptionBorderTransform;
    public Volume postProcessVolume;
    public MotionBlur motionBlur;

    private float switchUIPadding = 10;

    private float perceptionW;
    private float perceptionH;
    private float perceptionR;
    private List<SwitchingBehaviour> SBsInFrame = new List<SwitchingBehaviour>();

    private bool selectCarNow = false;

    private bool hasMarkedCar = false;
    public bool HasMarkedCar {
        get { return hasMarkedCar; }
        set { if (value != hasMarkedCar) MarkedCarValueChanged(value);
            hasMarkedCar = value;
        }
    }
    private bool hasSelectedCar = false;
    private bool canSelectCar = true;

    bool carsCreated;

    public UnityEvent CarChangedEvent;
    public UnityEvent CarSwitchedEvent;

    bool flash = false;
    bool Flash {
        get { return flash; }
        set { if (flash != value) FlashValueChanged(value);
            flash = value;
        }
    }
    FlashType[] signalPattern;
    float[] flashRecordDurations = new float[]{ -1, -1, -1 };
    private float currentFlashDuration = 0;
    private bool measureFlash = false;
    private float currentFlashInterval = 0;
    private bool measureInterval = false;
    private float resetInterval = 1f;
    private bool identicalFlashes = false;
    Material morseSignalMat;
    List<Texture> morseSignalTex;

    public RectTransform debugImage1;
    public RectTransform debugImage2;
    public RectTransform debugImage3;

    private float currentSDSpeed = 0;
    public float signalDisplayAmplitude = 1f;

    EventInstance radioStatic;
    EventInstance switchEvt;
    EventInstance flashOn;
    EventInstance flashOff;
    EventInstance flashHum;
    EventInstance signalSuccess;
    EventInstance signalFailure;

    private Vector3 origScaleMorseDisplay;

    public bool inTunnel;

    Texture2D morseLongTex;
    Texture2D morseShortTex;

    int signalIndex = 0;

    float tolerance = 0.35f;


    private void Start() {
        beatDetector = referenceManagement.beatDetector;
        beatDetector.bdOnEigth.AddListener(HandleSubDBeatDetected);
        beatDetector.bdOnFourth.AddListener(HandleFullBeatDetected);
        cam = camController.GetComponent<Camera>();
        switchImgObj = referenceManagement.switchImgObj;
        switchImgTransform = switchImgObj.GetComponent<RectTransform>();
        switchImg = switchImgObj.GetComponent<Image>();
        perceptionBorderObj = referenceManagement.perceptionBorderObj;
        perceptionBorderTransform = perceptionBorderObj.GetComponent<RectTransform>();
        perceptionW = referenceManagement.switchViewWidth;
        perceptionH = referenceManagement.switchViewHeight;
        perceptionR = referenceManagement.switchViewRange;
        allSwitchingBehaviours = referenceManagement.carManagement.GetAllSwitchingBehaviours();
        gearManagement = referenceManagement.gearManagement;
        morseSignalMat = referenceManagement.morseSingalMat;
        morseSignalTex = referenceManagement.morseSingalTex;
        radioStatic = RuntimeManager.CreateInstance(referenceManagement.radioStatic);
        radioStatic.start();
        switchEvt = RuntimeManager.CreateInstance(referenceManagement.switchDone);
        postProcessVolume = referenceManagement.postProcess;
        postProcessVolume.profile.TryGet(out motionBlur);
        flashOn = RuntimeManager.CreateInstance(referenceManagement.flashOn);
        flashOff = RuntimeManager.CreateInstance(referenceManagement.flashOff);
        flashHum = RuntimeManager.CreateInstance(referenceManagement.flashHum);
        signalSuccess = RuntimeManager.CreateInstance(referenceManagement.signalSuccess);
        signalFailure = RuntimeManager.CreateInstance(referenceManagement.signalFailure);
        flashHum.start();
        flashHum.setParameterByName("HumStrength", 0f);
        morseLongTex = referenceManagement.morseSignalLong;
        morseShortTex = referenceManagement.morseSignalShort;
    }

    private void OnEnable() {
        referenceManagement.carManagement.cameraChanged.AddListener(OnCameraChanged);
        referenceManagement.carManagement.carsCreated.AddListener(OnCarsCreated);
        referenceManagement.carManagement.allSBChanged.AddListener(HandleAllSBChanged);
        camController = referenceManagement.cam;
    }

    private void OnDisable() {
        referenceManagement.carManagement.cameraChanged.RemoveListener(OnCameraChanged);
        referenceManagement.carManagement.carsCreated.RemoveListener(OnCarsCreated);
        referenceManagement.carManagement.allSBChanged.RemoveListener(HandleAllSBChanged);
    }

    private void HandleAllSBChanged() {
        allSwitchingBehaviours = referenceManagement.carManagement.GetAllSwitchingBehaviours();
    }

    private void OnCarsCreated() {
        carsCreated = true;
        allSwitchingBehaviours = referenceManagement.carManagement.GetAllSwitchingBehaviours();
    }

    private void OnCameraChanged() {
        cam = referenceManagement.cam.GetComponent<Camera>();
        camController = cam.GetComponent<Camera1stPerson>();
        referenceManagement.canvas.worldCamera = cam;
    }

    private void SwitchToCar(SwitchingBehaviour newSB) {
        activeCar.SwitchOutOfCar();
        referenceManagement.cam.SwitchCar(newSB.camTranslateTarget.transform, newSB.camRotTarget.transform, true, newSB.transform);
        newSB.SwitchIntoCar(camController, false, activeCar.variation);
    }

    public void SetUpInitialCar(SwitchingBehaviour initSB) {
        referenceManagement.cam.SwitchCar(initSB.camTranslateTarget.transform, initSB.camRotTarget.transform, false, initSB.transform);
        initSB.isInitialCar = true;
        initSB.SwitchIntoCar(camController, true, UnityEngine.Random.Range(0, 2));
        origScaleMorseDisplay = initSB.morseSingalRenderers[0].transform.localScale;
    }

    private void Update() {
        Flash = referenceManagement.inputManagement.GetInputButton(Inputs.flash);

        if (carsCreated) {
            SearchForCars();
        }

        if (HasMarkedCar && !hasSelectedCar) {
            HandleSwitchGUI(CarScreenPos(MarkedCar), Color.white);
        }

        if (!hasSelectedCar && canSelectCar) {
            if (identicalFlashes) {
                selectCarNow = true;
            }
        }

        if (selectCarNow) {
            SelectCar();
            selectCarNow = false;
        }

        identicalFlashes = false;

        for (int i = 0; i < activeCar.morseSingalRenderers.Length; i++) {
            float value = activeCar.morseSingalRenderers[i].materials[0].GetFloat("Clarity");
            value += currentSDSpeed * Time.deltaTime;
            value = Mathf.Clamp(value, 0.05f, 2);
            activeCar.morseSingalRenderers[i].materials[0].SetFloat("Clarity", value);
            radioStatic.setParameterByName("SignalStrength", value);
        }

        MorseUIScaling();
    }

    private void SearchForCars() {
        SBsInFrame.Clear();
        SwitchingBehaviour sb = null;
        // 1 Draw Perception Frame
        float w = cam.pixelWidth * perceptionW;
        float h = cam.pixelHeight * perceptionH;
        float originX = (cam.pixelWidth / 2) - w / 2;
        float originY = (cam.pixelHeight / 2) - h / 2;
        perceptionBorderTransform.anchoredPosition = new Vector2(cam.pixelWidth / 2, cam.pixelHeight / 2);
        perceptionBorderTransform.sizeDelta = new Vector2(cam.pixelWidth * perceptionW, cam.pixelHeight * perceptionH);

        float dist = Mathf.Infinity;
        for (int i = 0; i < allSwitchingBehaviours.Count; i++) {
            if (allSwitchingBehaviours[i] != activeCar && allSwitchingBehaviours[i].gameObject.activeSelf && !allSwitchingBehaviours[i].isInitialCar) {
                // check if other car is visible
                bool isVisible = allSwitchingBehaviours[i].meshRenderer.IsVisibleFrom(cam);
                float[,] scrPos = CarScreenPos(allSwitchingBehaviours[i]);
                //check if other car is close enough
                bool inRange = scrPos[4, 0] <= perceptionR ? true : false;
                //check if points are in perceptionFrame
                bool inFrame = false;
                for (int j = 0; j < 4; j++) {
                    if (scrPos[j,0] > originX && scrPos[j,0] < originX + w && scrPos[j,1] > originY && scrPos[j, 1] < originY + h) {
                        inFrame = true;
                    }
                }
                bool occluded = false;
                if (isVisible && inRange && inFrame) {
                    int layerMask0 = 1 << 9;
                    int layerMask1 = 1 << 11;
                    int layerMask = layerMask0 | layerMask1;
                    layerMask = ~layerMask;
                    RaycastHit hit;
                    // Does the ray intersect any objects excluding the player layer
                    Vector3 pos = allSwitchingBehaviours[i].transform.position;
                    Vector3 offset = allSwitchingBehaviours[i].transform.up * 1.2f;
                    pos += offset;
                    Vector3 dir = activeCar.transform.position - allSwitchingBehaviours[i].transform.position;
                    float mag = dir.magnitude;
                    dir = dir / mag;
                    if (Physics.Raycast(pos, dir, out hit, mag, layerMask)) {
                        occluded = true;
                        Debug.DrawRay(pos, dir * hit.distance, Color.red);
                    } else {
                        occluded = false;
                    }
                }
                if (isVisible && inRange && inFrame && !occluded) {
                    SBsInFrame.Add(allSwitchingBehaviours[i]);
                    //evaluate closest car
                    if (scrPos[4,0] <= dist) {
                        dist = scrPos[4, 0];
                        sb = allSwitchingBehaviours[i];
                    } 
                }
            }
        }
        HasMarkedCar = sb != null ? true : false;
        MarkedCar = sb;
        if (inTunnel) {
            MarkedCar = null;
            HasMarkedCar = false;
        }
    }

    private void MarkedCarValueChanged(bool b) {
        switchImgObj.SetActive(b);
    }

    private void MarkedCarChanged(SwitchingBehaviour sb) {
        if (markedCar != null && sb == null) {
            currentSDSpeed = signalDisplayAmplitude;
        } else if (markedCar == null && sb != null) {
            currentSDSpeed = -1f * signalDisplayAmplitude;
        } else if (markedCar != null && sb != null) {

        }
        if (markedCar != null) markedCar.IsMarkedCar = false;
        if (sb != null) {
            sb.IsMarkedCar = true;
            signalPattern = sb.GetSignal();
            sb.DisplaySignalPattern();
            DisplayMarkedCarSignalPattern(); // this car displays text
            ResetFlashRecordDurations();
        }
    }

    private void HandleSwitchGUI(float [,] scrPos1, Color col) {
        //get x1, y1, x2, y2
        float[] scrPos = new float[] { scrPos1[0, 0], scrPos1[0, 1], scrPos1[3, 0], scrPos1[3, 1] };
        float x = scrPos[0];
        float y = scrPos[1];
        float w = scrPos[2] - x;
        float h = scrPos[3] - y;

        float padding = switchUIPadding;

        switchImg.color = col;
        switchImgTransform.anchoredPosition = new Vector2(x - padding, y - padding);
        switchImgTransform.sizeDelta = new Vector2(w + 2 * padding, h + 2 * padding);
    }

    private float[,] CarScreenPos(SwitchingBehaviour sb) {
        Vector3[] boundingVerts;
        boundingVerts = sb.boxCollider.GetVerticesOfBoxCollider();

        float sumZ = 0;
        for (int i = 0; i < 8; i++) {
            boundingVerts[i] = cam.WorldToScreenPoint(boundingVerts[i]);
            sumZ += boundingVerts[i].z;
        }

        float x1 = Mathf.Infinity;
        float y1 = Mathf.Infinity;
        float x2 = 0;
        float y2 = 0;
        for (int i = 0; i < 8; i++) {
            y1 = boundingVerts[i].y < y1 ? boundingVerts[i].y : y1;
            x1 = boundingVerts[i].x < x1 ? boundingVerts[i].x : x1;
            x2 = boundingVerts[i].x > x2 ? boundingVerts[i].x : x2;
            y2 = boundingVerts[i].y > y2 ? boundingVerts[i].y : y2;
        }

        float averageZ = sumZ / 8;

        return new float[,] { { x1, y1 }, { x1, y2 }, { x2, y1 }, { x2, y2 } , { averageZ, 0 } };
    }

    private void SelectCar() {
        ResetFlashRecordDurations();
        hasSelectedCar = true;
        selectedCar = MarkedCar;
        referenceManagement.selectedSwitchCar.Play();
        canSelectCar = false;
    }

    private void DeselectCar() {
        selectedCar = null;
        hasSelectedCar = false;
    }

    private IEnumerator SelectCoolDown() {
        while (camController.GetLooping()) {
            yield return new WaitForEndOfFrame();
        }
        canSelectCar = true;
    }

    private void Switch() {
        SwitchToCar(selectedCar);
        ActiveCar = selectedCar;
        DeselectCar();
        //referenceManagement.switchSound.Play();
        selectedCar = null;
        StartCoroutine(SelectCoolDown());
        CarChangedEvent.Invoke();
        CarSwitchedEvent.Invoke();
        float timeTilBar = referenceManagement.beatDetector.GetTimeUntilBar();
        camController.Loop(timeTilBar < 0.8f ? timeTilBar + referenceManagement.beatDetector.GetBarInterval() : timeTilBar);
        motionBlur.intensity.value = 10000f;
    }

    public void SwitchDone() {
        ActiveCar.SetActiveCarValues();
        switchEvt.start();
        motionBlur.intensity.value = 1f;
    }

    private void FlashValueChanged(bool b) {
        ActiveCar.SetFlash(b);
        if (b) {
            gearManagement.PlayStopGearSound();
            measureFlash = true;
            measureInterval = false;
            StartCoroutine(MeasureFlashDuration());
            flashOn.start();
            flashHum.setParameterByName("HumStrength", 1f);
        }
        else {
            measureFlash = false;
            measureInterval = true;
            StartCoroutine(MeasureFlashInterval());
            flashOff.start();
            flashHum.setParameterByName("HumStrength", 0f);
        }

        for (int i = 0; i < activeCar.morseSingalRenderers.Length; i++) {
            if (i == signalIndex) {
                Debug.Log($"flashSignal Index = {signalIndex}");
                activeCar.morseSingalRenderers[i].materials[0].SetInt("_FlashOn", b && hasMarkedCar ? 1 : 0);
            } else if (i > signalIndex) {
                activeCar.morseSingalRenderers[i].materials[0].SetFloat("_AlphaFactor", b && hasMarkedCar ? 0.06f : 1);
            }
        }
    }

    IEnumerator MeasureFlashDuration() {
        currentFlashDuration = 0;
        while (measureFlash) {
            yield return new WaitForEndOfFrame();
            currentFlashDuration += Time.deltaTime;
        }
        RecordFlashes();
    }

    IEnumerator MeasureFlashInterval() {
        currentFlashInterval = 0;
        while (measureInterval) {
            yield return new WaitForEndOfFrame();
            currentFlashInterval += Time.deltaTime;
        }
        if (currentFlashInterval >= resetInterval) ResetFlashRecordDurations();
    }

    private void RecordFlashes() {
        for (int i = 0; i < flashRecordDurations.Length - 1; i++) {
            flashRecordDurations[i] = flashRecordDurations[i + 1];
        }
        flashRecordDurations[flashRecordDurations.Length - 1] = currentFlashDuration;

        if (markedCar != null) {
            //EvaluateFlashRecordRelative();
            EvaluateFlashRecordFixed();
            DisplayMarkedCarSignalPattern();
            MorseUIProgress();
        }
    }

    private void EvaluateFlashRecordFixed() {
        float duration = flashRecordDurations[flashRecordDurations.Length - 1];
        float targetDuration = signalPattern[signalIndex] == FlashType.Long ? beatDetector.FourthInterval : beatDetector.EighthInterval;

        if (duration > targetDuration - targetDuration * tolerance && duration < targetDuration + targetDuration * tolerance) {
            if (signalIndex == 2) {
                signalIndex = 0;
                identicalFlashes = true;
            } else {
                signalIndex++;
            }
            signalSuccess.start();
        } else {
            signalFailure.start();
        }
    }

    private void EvaluateFlashRecordRelative() {
        if (flashRecordDurations[0] == -1) return;

        bool correct = true;
        float baseLength;

        if (signalPattern[0] == FlashType.Short) {
            baseLength = flashRecordDurations[0];
        } else {
            baseLength = flashRecordDurations[0] / 2f;
        }

        for (int i = 1; i < flashRecordDurations.Length; i++) {
            float factor = signalPattern[i] == FlashType.Short ? 1 : 2.5f;
            if (!(flashRecordDurations[i] <= factor * baseLength * 1.5f && flashRecordDurations[i] >= factor * baseLength * 0.5f)) {
                correct = false;
            }
        }
        if (correct && HasMarkedCar) {
            identicalFlashes = true;
        }
    }

    private void UpdateDebugUI() {
        if (flashRecordDurations[0] == -1) {
            debugImage1.sizeDelta = new Vector2(debugImage1.sizeDelta.x, 100);
            debugImage1.GetComponent<Image>().color = Color.red;
        } else {
            debugImage1.sizeDelta = new Vector2(debugImage1.sizeDelta.x, 100 * flashRecordDurations[0]);
            debugImage1.GetComponent<Image>().color = Color.white;
        }

        if (flashRecordDurations[1] == -1) {
            debugImage2.sizeDelta = new Vector2(debugImage2.sizeDelta.x, 100);
            debugImage2.GetComponent<Image>().color = Color.red;
        } else {
            debugImage2.sizeDelta = new Vector2(debugImage2.sizeDelta.x, 100 * flashRecordDurations[1]);
            debugImage2.GetComponent<Image>().color = Color.white;
        }

        if (flashRecordDurations[2] == -1) {
            debugImage3.sizeDelta = new Vector2(debugImage3.sizeDelta.x, 100);
            debugImage3.GetComponent<Image>().color = Color.red;
        } else {
            debugImage3.sizeDelta = new Vector2(debugImage3.sizeDelta.x, 100 * flashRecordDurations[2]);
            debugImage3.GetComponent<Image>().color = Color.white;
        }
    }

    private void DisplayMarkedCarSignalPattern() {
        // HERE LIETH THE BEST PIECE OF CODE I HAVE WRITTEN. RIP.

        //string digits = "";
        //for (int i = 0; i < 3; i++) {
        //    string digit = signalPattern[i] == FlashType.Short ? "0" : "1";
        //    digits += digit;
        //}
        //int index = Convert.ToInt32(digits, 2);

        //activeCar.morseSingalRenderer.materials[0].SetTexture("BaseTexture", morseSignalTex[index]);

        float units = 0;
        float offset = signalPattern[0] == FlashType.Long ? 5 : 1.5f;
        offset *= 0.15f;
        for (int i = 0; i < 3; i++) {
            units += signalPattern[i] == FlashType.Long ? 5 : 1.5f;
            activeCar.morseSingalRenderers[i].materials[0].SetTexture("BaseTexture", signalPattern[i] == FlashType.Long ? morseLongTex : morseShortTex);
            activeCar.morseSingalRenderers[i].transform.localPosition = new Vector3(-1,0,0) * units * 0.15f + new Vector3(offset, 0, 0);
            units += 5;
            units += signalPattern[i] == FlashType.Long ? 5 : 1.5f;
        }
        float centerOffset = units * 0.15f * 0.5f;
        for (int i = 0; i < 3; i++) {
            activeCar.morseSingalRenderers[i].transform.localPosition += new Vector3(centerOffset, 0, 0);
        }
    }

    private void ResetFlashRecordDurations() {
        for (int i = 0; i < flashRecordDurations.Length; i++) {
            flashRecordDurations[i] = -1f;
        }
        signalIndex = 0;
        MorseUIProgress();
    }

    private void MorseUIScaling() {
        Vector3 scale;
        Vector3 scale2;
        if (measureFlash && hasMarkedCar) {
            scale = origScaleMorseDisplay * 2f;
            scale2 = origScaleMorseDisplay * 0.7f;
        } else {
            scale = origScaleMorseDisplay;
            scale2 = origScaleMorseDisplay;
        }

        for (int i = 0; i < 3; i++) {
            if (i == signalIndex) {
                activeCar.morseSingalRenderers[i].transform.localScale = Vector3.Lerp(activeCar.morseSingalRenderers[i].transform.localScale, scale, 20 * Time.deltaTime);
            } else {
                activeCar.morseSingalRenderers[i].transform.localScale = Vector3.Lerp(activeCar.morseSingalRenderers[i].transform.localScale, scale2, 20 * Time.deltaTime);
            }
        }
    }

    private void MorseUIProgress() {
        for (int i = 0; i < 3; i++) {
            if (i < signalIndex) {
                activeCar.morseSingalRenderers[i].materials[0].SetFloat("_AlphaFactor", 0);
            }
            else {
                activeCar.morseSingalRenderers[i].materials[0].SetFloat("_AlphaFactor", 1);
            }
            activeCar.morseSingalRenderers[i].materials[0].SetInt("_FlashOn", 0);
        }
    }

    private void HandleFullBeatDetected() {
        if (hasSelectedCar) Switch();
    }

    private void HandleSubDBeatDetected() {
        //if (hasSelectedCar) Switch();
    }

    private void HandleBarDetected() {
        //Debug.Log("BAR!");
    }

    public void TimelineBarDetected() {
        //Debug.Log("TIMELINE BAR!");
    }

    public void SetActiveCar(SwitchingBehaviour sb) {
        ActiveCar = sb;
        CarChangedEvent.Invoke();
    }

    private void ActiveCarChanged(SwitchingBehaviour sb) {
        if (activeCar != null) {
            activeCar.morseSignalDisplay.SetActive(false);
        }
        sb.morseSignalDisplay.SetActive(true);
    }
}

public enum FlashType { None, Short, Long };
