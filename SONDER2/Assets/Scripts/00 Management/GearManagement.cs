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

    private void OnEnable() {
        referenceManagement.beatDetector.bdOnBar.AddListener(OnBar);
        inputManagement = referenceManagement.inputManagement;
        carManagement = referenceManagement.carManagement;
        gearTextGoalObj = referenceManagement.gearTextGoalObj;
        gearTextGoal = gearTextGoalObj.GetComponent<TMP_Text>();
        gearTextCurrentObj = referenceManagement.gearTextCurrentObj;
        gearTextCurrent = gearTextCurrentObj.GetComponent<TMP_Text>();
        gearSprites = referenceManagement.gearSprites;
        gearImage = referenceManagement.gearImage;
        StartConfig();
        SetCurrentGear(0);
        SetRandomGoalGear();
        clutchDown = false;

        startGearShift = FMODUnity.RuntimeManager.CreateInstance(referenceManagement.gearShiftStart);
        stopGearShift = FMODUnity.RuntimeManager.CreateInstance(referenceManagement.gearShiftStop);
    }

    private void OnDisable() {
        referenceManagement.beatDetector.bdOnBar.RemoveListener(OnBar);
    }

    private void OnBar() {
        barCounter++;
        if (barCounter >= gearInterval) {
            barCounter = 0;
            SetRandomGoalGear();
        }
    }

    private void SetRandomGoalGear() {
        SetGoalGear(Random.Range(0, 6));
    }

    public void SetGoalGear(int i) {
        goalGear = i;
        gearTextGoal.text = (i+1).ToString();
    }

    public void SetCurrentGear(int i) {
        currentGear = i;
        DisplayGear();
    }

    private void ChangeGear(float a) {
        if (clutchDown && !gearCoolDown) {
            if (currentGear % 2f == 0 && a == 0.5f) return;
            if (currentGear % 2f == 1 && a == -0.5f) return;
            if ((currentGear % 2f != 1.5f && currentGear % 2 != 0.5f) && (a == 2 || a == -2f)) return;
            currentGear += a;
            currentGear = Mathf.Clamp(currentGear, 1f, 6f);
            StartCoroutine(GearCoolDown());
            DisplayGear();
            CheckIfFullGear();
        }
    }

    private void DisplayGear() {
        for (int i = 0; i < 9; i++) {
            string name = gearSprites[i].name;
            var regex = new Regex("([-+]?[0-9]*\\.?[0-9]+)");

            if (float.Parse(regex.Match(name).ToString()) == currentGear) {
                Debug.Log("selected sprite");
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
        if (inputManagement.GetInputButton(Inputs.up)) ChangeGear(-0.5f);
        if (inputManagement.GetInputButton(Inputs.down)) ChangeGear(0.5f);
        if (inputManagement.GetInputButton(Inputs.left)) ChangeGear(-2f);
        if (inputManagement.GetInputButton(Inputs.right)) ChangeGear(2f);
    }
}