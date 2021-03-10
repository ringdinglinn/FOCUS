using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatDetector : MonoBehaviourReferenced {
	private static BeatDetector BDInstance;
	private float bpm;
	private float beatInterval, beatTimer, beatIntervalSubD, beatTimerSubD;
    private float subD;
	public bool beatFull, beatSubD;
	public int beatCountFull, beatCountSubD;

    public BDOnBeatEventHandler bdOnBeatFull;
    public BDOnBeatEventHandler bdOnBeatSubD;

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
            Debug.Log("Full");
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
            Debug.Log("SubD");
        }
    }
}

[System.Serializable]
public class BDOnBeatEventHandler : UnityEngine.Events.UnityEvent {

}