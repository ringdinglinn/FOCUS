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
    private float dist;

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
    private bool successfulFlashes = false;
    Material morseSignalMat;
    List<Texture> morseSignalTex;

    public RectTransform debugImage1;
    public RectTransform debugImage2;
    public RectTransform debugImage3;

    private float currentSDSpeed = 0;
    public float signalDisplayAmplitude = 1f;

    EventInstance radioStatic;
    EventInstance switchDone;
    EventInstance switchStart;
    EventInstance flashOn;
    EventInstance flashOff;
    EventInstance flashHum;
    EventInstance flashHumMarkedCar;
    EventInstance signalSuccess;
    EventInstance signalFailure;
    EventInstance carInterior0;
    EventInstance carInterior1;
    EventInstance carInterior2;

    private Vector3 origScaleMorseDisplay;
    private Vector3[] origPosMorseDisplay = new Vector3[3];

    public bool inTunnel;

    Texture2D morseLongTex;
    Texture2D morseShortTex;

    int signalIndex = 0;

    float tolerance = 0.35f;

    float clarity = 0;
    float clarity2 = 0;

    float rndJitterRange = 0.1f;

    int carSoundIndex = 0;


    private void Start() {
        beatDetector = referenceManagement.beatDetector;
        beatDetector.bdOnEighth.AddListener(HandleSubDBeatDetected);
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
        switchDone = RuntimeManager.CreateInstance(referenceManagement.switchDone);
        switchStart = RuntimeManager.CreateInstance(referenceManagement.switchStart);
        postProcessVolume = referenceManagement.postProcess;
        postProcessVolume.profile.TryGet(out motionBlur);
        flashOn = RuntimeManager.CreateInstance(referenceManagement.flashOn);
        flashOff = RuntimeManager.CreateInstance(referenceManagement.flashOff);
        flashHum = RuntimeManager.CreateInstance(referenceManagement.flashHum);
        flashHumMarkedCar = RuntimeManager.CreateInstance(referenceManagement.flashHumMarkedCar);
        signalSuccess = RuntimeManager.CreateInstance(referenceManagement.signalSuccess);
        signalFailure = RuntimeManager.CreateInstance(referenceManagement.signalFailure);
        carInterior0 = RuntimeManager.CreateInstance(referenceManagement.carInterior0);
        carInterior1 = RuntimeManager.CreateInstance(referenceManagement.carInterior1);
        carInterior2 = RuntimeManager.CreateInstance(referenceManagement.carInterior2);
        flashHum.start();
        flashHumMarkedCar.start();
        carInterior0.start();
        flashHum.setParameterByName("HumStrength", 0f);
        morseLongTex = referenceManagement.morseSignalLong;
        morseShortTex = referenceManagement.morseSignalShort;
        clarity = activeCar.morseSignalRenderers[0].materials[0].GetFloat("Clarity");
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
        referenceManagement.cam.SwitchCar(newSB.camTranslateTarget.transform, newSB.camRotTarget.transform, !newSB.isSecondCar, newSB.transform);
        newSB.SwitchIntoCar(camController, false, activeCar.variation);
    }

    public void SetUpInitialCar(SwitchingBehaviour initSB) {
        referenceManagement.cam.SwitchCar(initSB.camTranslateTarget.transform, initSB.camRotTarget.transform, false, initSB.transform);
        initSB.isInitialCar = true;
        initSB.SwitchIntoCar(camController, true, UnityEngine.Random.Range(0, 2));
        origScaleMorseDisplay = initSB.morseSignalRenderers[0].transform.localScale;
        for (int i = 0; i < 3; i++) {
            origPosMorseDisplay[i] = initSB.morseSignalRenderers[i].transform.localPosition;
        }
    }

    private void Update() {
        Flash = referenceManagement.inputManagement.GetInputButton(Inputs.flash);

        if (carsCreated && !successfulFlashes) {
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

        for (int i = 0; i < activeCar.morseSignalRenderers.Length; i++) {
            clarity = activeCar.morseSignalRenderers[i].materials[0].GetFloat("Clarity");
            clarity += currentSDSpeed * Time.deltaTime;
            clarity = Mathf.Clamp(clarity, 0.05f, 2);
            activeCar.morseSignalRenderers[i].materials[0].SetFloat("Clarity", clarity);
            radioStatic.setParameterByName("SignalStrength", clarity);
        }

        MorseUIScaling();
        MorseUIColor();

        carInterior0.setParameterByName("Speed", Mathf.InverseLerp(0, 12, activeCar.GetCarAI().GetSpeed()));
    }

    private void SearchForCars() {
        SBsInFrame.Clear();
        SwitchingBehaviour sb = null;

        float minDist = Mathf.Infinity;

        if ((HasMarkedCar && !CheckIfVisible(MarkedCar)) || (!HasMarkedCar)) {
            for (int i = 0; i < allSwitchingBehaviours.Count; i++) {
                if (CheckIfVisible(allSwitchingBehaviours[i])) {
                    SBsInFrame.Add(allSwitchingBehaviours[i]);
                    //evaluate closest car
                    if (dist <= minDist) {
                        minDist = dist;
                        sb = allSwitchingBehaviours[i];
                    }
                }
            }

            HasMarkedCar = sb != null ? true : false;
            MarkedCar = sb;
        }

        if (inTunnel) {
            MarkedCar = null;
            HasMarkedCar = false;
        }
    }

    private bool CheckIfVisible(SwitchingBehaviour sb) {
        float w = cam.pixelWidth * perceptionW;
        float h = cam.pixelHeight * perceptionH;
        float originX = (cam.pixelWidth / 2) - w / 2;
        float originY = (cam.pixelHeight / 2) - h / 2;
        perceptionBorderTransform.anchoredPosition = new Vector2(cam.pixelWidth / 2, cam.pixelHeight / 2);
        perceptionBorderTransform.sizeDelta = new Vector2(cam.pixelWidth * perceptionW, cam.pixelHeight * perceptionH);

        dist = Mathf.Infinity;

        if (sb != activeCar && sb.gameObject.activeSelf && !sb.isInitialCar) {
            // check if other car is visible
            bool isVisible = sb.meshRenderer.IsVisibleFrom(cam);
            float[,] scrPos = CarScreenPos(sb);
            //check if other car is close enough
            bool inRange = scrPos[4, 0] <= perceptionR ? true : false;
            //check if points are in perceptionFrame
            bool inFrame = false;
            for (int j = 0; j < 4; j++) {
                if (scrPos[j, 0] > originX && scrPos[j, 0] < originX + w && scrPos[j, 1] > originY && scrPos[j, 1] < originY + h) {
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
                Vector3 pos = sb.transform.position;
                Vector3 offset = sb.transform.up * 1.2f;
                pos += offset;
                Vector3 dir = activeCar.transform.position - sb.transform.position;
                float mag = dir.magnitude;
                dir = dir / mag;
                if (Physics.Raycast(pos, dir, out hit, mag, layerMask)) {
                    occluded = true;
                    Debug.DrawRay(pos, dir * hit.distance, Color.red);
                }
                else {
                    occluded = false;
                }
            }
            if (isVisible && inRange && inFrame && !occluded) {
                dist = scrPos[4, 0];
                SBsInFrame.Add(sb);
            }
            return isVisible && inRange && inFrame && !occluded;
        } else {
            return false;
        }
    }

    private void MarkedCarValueChanged(bool b) {
        switchImgObj.SetActive(b);
    }

    private void MarkedCarChanged(SwitchingBehaviour sb) {
        if (markedCar != null && sb == null) {
            currentSDSpeed = signalDisplayAmplitude;
        } else if (sb != null) {
            currentSDSpeed = -1f * signalDisplayAmplitude;
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
        canSelectCar = false;
        Switch();
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
        switchStart.start();
        SwitchToCar(selectedCar);
        ActiveCar = selectedCar;
        DeselectCar();
        selectedCar = null;
        StartCoroutine(SelectCoolDown());
        CarChangedEvent.Invoke();
        CarSwitchedEvent.Invoke();
        float timeTilBar = referenceManagement.beatDetector.GetTimeUntilBar();
        camController.Loop(timeTilBar < 0.8f ? timeTilBar + referenceManagement.beatDetector.GetBarInterval() : timeTilBar);
        motionBlur.intensity.value = 10000f;
        DisplayMarkedCarSignalPattern();
        currentSDSpeed = signalDisplayAmplitude;

        // car sound
        carSoundIndex++;
        carSoundIndex %= 3;
        carInterior0.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        carInterior1.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        carInterior2.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public void SwitchDone() {
        Debug.Log("switch done");
        ActiveCar.SetActiveCarValues();
        switchDone.start();
        switchStart.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        motionBlur.intensity.value = 1f;

        if (carSoundIndex == 0) {
            carInterior0.start();
        } else if (carSoundIndex == 1) {
            carInterior1.start();
        } else {
            carInterior2.start();
        }
    }                

    private void FlashValueChanged(bool b) {
        ActiveCar.SetFlash(b);
        if (b) {
            successfulFlashes = false;
            gearManagement.PlayStopGearSound();
            measureFlash = true;
            measureInterval = false;
            StartCoroutine(MeasureFlashDuration());
            flashOn.start();
            if (!HasMarkedCar) flashHum.setParameterByName("HumStrength", 1f);
            else flashHumMarkedCar.setParameterByName("HumStrengthMarkedCar", 1f);
        } else {
            measureFlash = false;
            if (!successfulFlashes) {
                measureInterval = true;
                StartCoroutine(MeasureFlashInterval());
            }
            flashOff.start();
            flashHum.setParameterByName("HumStrength", 0f);
            flashHumMarkedCar.setParameterByName("HumStrengthMarkedCar", 0f);
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
        //if (currentFlashInterval >= resetInterval) ResetFlashRecordDurations();
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
                successfulFlashes = true;
                StartCoroutine(MorseSignalSuccessfullTimer(activeCar));
            }
            else {
                signalIndex++;
            }
            signalSuccess.start();
        } else {
            ResetFlashRecordDurations();
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
            activeCar.morseSignalRenderers[i].materials[0].SetTexture("BaseTexture", signalPattern[i] == FlashType.Long ? morseLongTex : morseShortTex);
            activeCar.morseSignalRenderers[i].transform.localPosition = new Vector3(-1,0,0) * units * 0.15f + new Vector3(offset, 0, 0);
            units += 5;
            units += signalPattern[i] == FlashType.Long ? 5 : 1.5f;
        }
        float centerOffset = units * 0.15f * 0.5f;
        for (int i = 0; i < 3; i++) {
            activeCar.morseSignalRenderers[i].transform.localPosition += new Vector3(centerOffset, 0, 0);
            origPosMorseDisplay[i] = activeCar.morseSignalRenderers[i].transform.localPosition;
        }
    }

    private void ResetFlashRecordDurations() {
        for (int i = 0; i < flashRecordDurations.Length; i++) {
            flashRecordDurations[i] = -1f;
        }
        signalIndex = 0;
        MorseUIProgress();
    }

    IEnumerator MorseSignalSuccessfullTimer(SwitchingBehaviour sb) {
        float secs = 1f;
        clarity2 = 0.05f;
        float speed = signalDisplayAmplitude * 6;
        float offset = 4;
        Vector3[] origPos = new Vector3[3];
        for (int i = 0; i < 3; i++) {
            origPos[i] = sb.morseSignalRenderers[i].transform.localPosition;
        }

        while (secs > 0) {
            yield return new WaitForEndOfFrame();
            secs -= Time.deltaTime;
            MorseSignalSuccessful(sb, speed, offset, origPos);
        }

        successfulFlashes = false;
        sb.morseSignalDisplay.SetActive(false);
        for (int i = 0; i < 3; i++) {
            MeshRenderer mr = sb.morseSignalRenderers[i];
            mr.transform.localPosition = new Vector3(origPos[i].x + (i - 1) * offset, mr.transform.localPosition.y, mr.transform.localPosition.z);
            mr.transform.localScale = origScaleMorseDisplay;
        }
    }

    private void MorseSignalSuccessful(SwitchingBehaviour sb, float speed, float offset, Vector3[] origPos) {
        clarity2 += speed * Time.deltaTime;
        //clarity2 = 2 - 2 * Mathf.Pow(clarity2 - 1, 2);
        clarity2 = Mathf.Clamp(clarity2, 0.05f, 2);
        Vector3 targetScale = origScaleMorseDisplay * 3;

        for (int i = 0; i < 3; i++) {
            MeshRenderer mr = sb.morseSignalRenderers[i];
            mr.transform.localScale = Vector3.Lerp(mr.transform.localScale, targetScale, 0.2f);
            mr.transform.localPosition = Vector3.Lerp(mr.transform.localPosition, new Vector3(origPos[i].x - (i-1) * offset, mr.transform.localPosition.y, mr.transform.localPosition.z), 0.2f);
            mr.materials[0].SetFloat("_AlphaFactor", 1);
            mr.materials[0].SetInt("_FlashOn", 1);
            mr.materials[0].SetFloat("Clarity", clarity2);
        }
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
                activeCar.morseSignalRenderers[i].transform.localScale = Vector3.Lerp(activeCar.morseSignalRenderers[i].transform.localScale, scale, 20 * Time.deltaTime);
            } else {
                activeCar.morseSignalRenderers[i].transform.localScale = Vector3.Lerp(activeCar.morseSignalRenderers[i].transform.localScale, scale2, 20 * Time.deltaTime);
            }
        }
    }

    private void MorseUIColor() {
        for (int i = 0; i < activeCar.morseSignalRenderers.Length; i++) {
            activeCar.morseSignalRenderers[i].materials[0].SetInt("_FlashOn", Flash && hasMarkedCar && i == signalIndex ? 1 : 0);
            if (Flash && i == signalIndex) {
                activeCar.morseSignalRenderers[i].transform.localPosition = origPosMorseDisplay[i] + new Vector3(UnityEngine.Random.Range(-rndJitterRange, rndJitterRange), UnityEngine.Random.Range(-rndJitterRange, rndJitterRange), UnityEngine.Random.Range(-rndJitterRange, rndJitterRange));
            } else {
                activeCar.morseSignalRenderers[i].transform.localPosition = origPosMorseDisplay[i];
            }
            if (i > signalIndex) {
                activeCar.morseSignalRenderers[i].materials[0].SetFloat("_AlphaFactor", Flash && hasMarkedCar ? 0.06f : 1);
            }
        }
    }

    private void MorseUIProgress() {
        for (int i = 0; i < 3; i++) {
            if (i < signalIndex) {
                activeCar.morseSignalRenderers[i].materials[0].SetFloat("_AlphaFactor", 0);
            }
            else {
                activeCar.morseSignalRenderers[i].materials[0].SetFloat("_AlphaFactor", 1);
            }

            activeCar.morseSignalRenderers[i].materials[0].SetInt("_FlashOn", 0);
        }
    }

    private void HandleFullBeatDetected() {
        //if (hasSelectedCar) Switch();
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
            //activeCar.morseSignalDisplay.SetActive(false);
        }
        sb.morseSignalDisplay.SetActive(true);
    }
}

public enum FlashType { None, Short, Long };
