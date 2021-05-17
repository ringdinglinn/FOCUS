using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using FMOD.Studio;

public class GearManagement : MonoBehaviourReferenced {

    private int goalGear = 0;
    private float currentGear = 1;
    public float CurrentGear {
        get { return currentGear; }
    }

    InputManagement inputManagement;
    CarManagement carManagement;
    GameObject gearTextGoalObj;
    TMP_Text gearTextGoal;
    GameObject gearTextCurrentObj;
    TMP_Text gearTextCurrent;
    BeatDetector beatDetector;

    private int barCounter = 0;
    private int gearInterval = 3;

    private bool clutchDown = false;

    private List<Sprite> gearSprites;
    private Image gearImage;

    bool mode1 = false;

    bool gearCoolDown = false;

    EventInstance startGearShift;
    EventInstance stopGearShift;

    private int queuedGearStopSounds = 0;
    private float queueSoundProbability = 1f;

    private void OnEnable() {
        referenceManagement.beatDetector.bdOnBar.AddListener(OnBar);
        referenceManagement.beatDetector.bdOnFourth.AddListener(OnBeat);
        referenceManagement.beatDetector.bdOnEigth.AddListener(OnSubBeat);
        inputManagement = referenceManagement.inputManagement;
        carManagement = referenceManagement.carManagement;
        beatDetector = referenceManagement.beatDetector;
        gearTextGoalObj = referenceManagement.gearTextGoalObj;
        gearTextGoal = gearTextGoalObj.GetComponent<TMP_Text>();
        gearTextCurrentObj = referenceManagement.gearTextCurrentObj;
        gearTextCurrent = gearTextCurrentObj.GetComponent<TMP_Text>();
        gearSprites = referenceManagement.gearSprites;
        gearImage = referenceManagement.gearImage;
        SetRandomGoalGear();
        clutchDown = false;

        startGearShift = FMODUnity.RuntimeManager.CreateInstance(referenceManagement.gearShiftStart);
        stopGearShift = FMODUnity.RuntimeManager.CreateInstance(referenceManagement.gearShiftStop);
    }

    private void Start() {
        SetCurrentGear(1);
    }

    private void OnDisable() {
        referenceManagement.beatDetector.bdOnBar.RemoveListener(OnBar);
        referenceManagement.beatDetector.bdOnFourth.RemoveListener(OnBeat);
        referenceManagement.beatDetector.bdOnEigth.RemoveListener(OnSubBeat);
    }

    private void OnBar() {
        barCounter++;
        if (barCounter >= gearInterval) {
            barCounter = 0;
            SetRandomGoalGear();
        }
    }

    private void OnBeat() {
    }

    private void OnSubBeat() {
        //PlayQueuedStopGearSound();
    }

    private void SetRandomGoalGear() {
        SetGoalGear(Random.Range(0, 6));
    }

    public void SetGoalGear(int i) {
        goalGear = i;
        gearTextGoal.text = (i+1).ToString();
    }

    public void SetCurrentGear(float i) {
        i = Mathf.Clamp(i,1,6);
        currentGear = i;
        DisplayGear();
        CheckIfFullGear();
    }

    private void InstantChangeGear(float a) {
        PlayStartGearSound();
        if (clutchDown && !gearCoolDown) {
            ChangeGear(a);
            StartCoroutine(GearCoolDown());
            PlayStopGearSound();
        }
    }

    private void ChangeGear(float a) {
        if (currentGear % 2f == 0 && a == 0.5f) return;
        if (currentGear % 2f == 1 && a == -0.5f) return;
        if ((currentGear % 2f != 1.5f && currentGear % 2 != 0.5f) && (a == 2 || a == -2f)) return;
        SetCurrentGear(currentGear + a);
        DisplayGear();
        //QueueStopGearSound();
    }

    private void DisplayGear() {
        for (int i = 0; i < 9; i++) {
            string name = gearSprites[i].name;
            var regex = new Regex("([-+]?[0-9]*\\.?[0-9]+)");

            if (float.Parse(regex.Match(name).ToString()) == currentGear) {
                gearImage.sprite = gearSprites[i];
                return;
            }
        }
    }

    private void CheckIfFullGear() {
        int n = (int)currentGear;
        if (n == currentGear) {
            carManagement.ActiveCarGearChanged(n);
        }
    }

    IEnumerator GearCoolDown() {
        gearCoolDown = true;
        yield return new WaitForSeconds(0.2f);
        gearCoolDown = false;
    }

    private void Update() {
        clutchDown = inputManagement.GetInputBool(Inputs.clutch);
        if (inputManagement.GetInputButton(Inputs.up)) InstantChangeGear(-0.5f);
        if (inputManagement.GetInputButton(Inputs.down)) InstantChangeGear(0.5f);
        if (inputManagement.GetInputButton(Inputs.left)) InstantChangeGear(-2f);
        if (inputManagement.GetInputButton(Inputs.right)) InstantChangeGear(2f);
    }

    private void PlayStartGearSound() {
        //startGearShift.start();
    }

    private void QueueStopGearSound() {
        if (Random.Range(0f,1f) <= queueSoundProbability) {
            queuedGearStopSounds++;
        }
    }

    private void PlayQueuedStopGearSound() {
        if (queuedGearStopSounds > 0) {
            stopGearShift.start();
            queuedGearStopSounds--;
        }
    }

    public void PlayStopGearSound() {
        //stopGearShift.start();
    }
}