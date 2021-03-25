using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatDetector : MonoBehaviourReferenced {
	private static BeatDetector BDInstance;
	private float bpm;
	private float beatInterval, beatTimer, beatIntervalSubD, beatTimerSubD, barInterval, barTimer;
    private float subD;
	public bool beatFull, beatSubD, bar;
	public int beatCountFull, beatCountSubD, barCount;

    public BDOnBeatEventHandler bdOnBeatFull;
    public BDOnBeatEventHandler bdOnBeatSubD;
    public BDOnBeatEventHandler bdOnBar;

    protected override void Awake() {
		base.Awake();
        if(BDInstance != null && BDInstance != this) {
			Destroy(gameObject);
        } else {
			BDInstance = this;
			DontDestroyOnLoad(gameObject);
        }
        bpm = referenceManagement.GetBPM();
        subD = referenceManagement.GetSubdivisions();
    }

    private void Update() {
        BeatDetection();
    }

    void BeatDetection() {
        // full beat
        beatFull = false;
        beatInterval = 60 / bpm;
        beatTimer += Time.deltaTime;
        if (beatTimer >= beatInterval) {
            beatTimer -= beatInterval;
            beatCountFull++;
            bdOnBeatFull.Invoke();
        }

        // subdivided beat
        beatSubD = false;
        beatIntervalSubD = beatInterval / subD; 
        beatTimerSubD += Time.deltaTime;
        if(beatTimerSubD >= beatIntervalSubD) {
            beatTimerSubD -= beatIntervalSubD;
            beatSubD = true;
            beatCountSubD++;
            bdOnBeatSubD.Invoke();
        }

        // bar
        bar = false;
        barInterval = beatInterval * 4;
        barTimer += Time.deltaTime;
        if (barTimer >= barInterval) {
            barTimer -= barInterval;
            bar = true;
            barCount++;
            bdOnBar.Invoke();
        }
    }
}

[System.Serializable]
public class BDOnBeatEventHandler : UnityEngine.Events.UnityEvent {

}