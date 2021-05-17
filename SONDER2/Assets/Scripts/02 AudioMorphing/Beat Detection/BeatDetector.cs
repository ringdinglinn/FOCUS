using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatDetector : MonoBehaviourReferenced {
    private static BeatDetector BDInstance;
    private float bpm;
    private float halfInterval, halfTimer, fourthInterval, fourthTimer, eighthInterval, eighthTimer, barInterval, barTimer;
    public bool half, fourth, eighth, bar;
    public int halfCount, fourthCount, eighthCount, barCount;
    public float HalfInterval, FourthInterval, EighthInterval;

    public BDOnBeatEventHandler bdOnFourth;
    public BDOnBeatEventHandler bdOnEigth;
    public BDOnBeatEventHandler bdOnHalf;
    public BDOnBeatEventHandler bdOnBar;

    private float beatWindow = 0.2f;
    private bool withinBeatWindow;
    public bool WithinBeatWindow {
        get { return withinBeatWindow; }
        set {
            if (value != withinBeatWindow) WithinBeatWindowChanged(value);
            withinBeatWindow = value;
        }
    }

    protected override void Awake() {
        base.Awake();
        if (BDInstance != null && BDInstance != this) {
            Destroy(gameObject);
        }
        else {
            BDInstance = this;
            DontDestroyOnLoad(gameObject);
        }
        bpm = referenceManagement.GetBPM();
        FourthInterval = 60 / bpm;
        EighthInterval = FourthInterval / 2;
        HalfInterval = FourthInterval * 2f;
    }

    private void Update() {
        BeatDetection();
    }

    void BeatDetection() {
        // half
        half = false;
        halfInterval = 2f * (60 / bpm);
        halfTimer += Time.deltaTime;
        if (halfTimer >= halfInterval) {
            halfTimer -= halfInterval;
            halfCount++;
            bdOnHalf.Invoke();
        }

        // fourth
        fourth = false;
        fourthInterval = 60 / bpm;
        fourthTimer += Time.deltaTime;
        if (fourthTimer >= fourthInterval) {
            fourthTimer -= fourthInterval;
            fourthCount++;
            bdOnFourth.Invoke();
        }
        if (fourthTimer >= fourthInterval - beatWindow / 2) {
            WithinBeatWindow = true;
        }
        else if (fourthTimer <= beatWindow / 2) {
            WithinBeatWindow = true;
        }
        else {
            WithinBeatWindow = false;
        }

        // eighth
        eighth = false;
        eighthInterval = fourthInterval / 2;
        eighthTimer += Time.deltaTime;
        if (eighthTimer >= eighthInterval) {
            eighthTimer -= eighthInterval;
            eighth = true;
            eighthCount++;
            bdOnEigth.Invoke();
        }

        // bar
        bar = false;
        barInterval = fourthInterval * 4;
        barTimer += Time.deltaTime;
        if (barTimer >= barInterval) {
            barTimer -= barInterval;
            bar = true;
            barCount++;
            bdOnBar.Invoke();
        }
    }

    private void WithinBeatWindowChanged(bool value) {
    }

    public float GetTimeUntilBar() {
        return barInterval - barTimer;
    }

    public float GetBarInterval() {
        return barInterval;
    }

    [System.Serializable]
    public class BDOnBeatEventHandler : UnityEngine.Events.UnityEvent {

    }
}