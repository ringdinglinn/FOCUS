using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceClipManagement : MonoBehaviourReferenced {

    private SwitchingManagement switchingManagement;
    private MusicManagement musicManagement;
    private LevelManagement levelManagement;
    private BeatDetector beatDetector;

    float p = 1f;
    float delay = 10f;
    float delayRange = 2f;
    float volumeTime = 0.5f;

    float targetVolume = 1f;
    float currentVolume = 1f;

    bool playedAudio = false;

    int voiceOverIndex = -1;

    FMOD.Studio.EventInstance[] voicesOvers = new FMOD.Studio.EventInstance[15];

    private bool playVoiceOverAfterSwitch = false;
    private List<Coroutine> waitToPlayCoroutines = new List<Coroutine>();

    bool endSeqAlternating = true;

    public bool inOutro;

    private void OnEnable() {
        switchingManagement = referenceManagement.switchingManagement;
        musicManagement = referenceManagement.musicManagement;
        levelManagement = referenceManagement.levelManagement;
        beatDetector = referenceManagement.beatDetector;

        switchingManagement.CarSwitchedEvent.AddListener(HandleCarSwitched);
        switchingManagement.CarSwitchDoneEvent.AddListener(HandleCarSwitchDone);

        GetVoiceOvers();
    }

    private void GetVoiceOvers() {
        for (int i = 0; i < voicesOvers.Length; i++) {
            voicesOvers[i] = FMODUnity.RuntimeManager.CreateInstance(referenceManagement.voiceOver + $" {i+1}");
        }
    }

    private void OnDisable() {
        switchingManagement.CarSwitchedEvent.RemoveListener(HandleCarSwitched);
        switchingManagement.CarSwitchDoneEvent.RemoveListener(HandleCarSwitchDone);
    }

    private void HandleCarSwitched() {
        currentVolume = 1;
        targetVolume = 1;
        StopCoroutine(WaitToPlaySnippet());
        foreach (Coroutine coroutine in waitToPlayCoroutines) {
            StopCoroutine(coroutine);
        }
        waitToPlayCoroutines.Clear();
        TerminateSnippet();
    }

    private void HandleCarSwitchDone() {
        if (switchingManagement.ActiveCar.GetCarAI().PathID < 20) {
            if (playVoiceOverAfterSwitch) {
                waitToPlayCoroutines.Add(StartCoroutine(WaitToPlaySnippet()));
            }
            else if (endSeqAlternating && levelManagement.levelNr == 6) {
                waitToPlayCoroutines.Add(StartCoroutine(WaitToPlaySnippet()));
            }
            if (levelManagement.levelNr == 6) endSeqAlternating = !endSeqAlternating;
        }
    }

    IEnumerator WaitToPlaySnippet() {
        Debug.Log("Wait to play snippet");
        if (voiceOverIndex < voicesOvers.Length && !inOutro) {
            playVoiceOverAfterSwitch = false;
            targetVolume = 0.6f;
            yield return new WaitForSeconds(levelManagement.levelNr >= 5 ? volumeTime / 2 : volumeTime);
            PlaySnippet();
        } else {
            yield return null;
        }

    }

    IEnumerator WaitToPlaySnippetAfterTrack() {
        while (musicManagement.Track1IsPlaying()) {
            yield return new WaitForEndOfFrame();
        }
        PlaySnippet();
    }

    void PlaySnippet() {
        voiceOverIndex++;
        voicesOvers[voiceOverIndex].start();
        playedAudio = true;
    }

    void TerminateSnippet() {
        if (voiceOverIndex >= 0)
        voicesOvers[voiceOverIndex].stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    void SnippedDone() {
        targetVolume = 1;
        playedAudio = false;
    }

    private void Update() {
        currentVolume = Mathf.Lerp(currentVolume, targetVolume, 1 * Time.deltaTime);
        if (playedAudio) {
            if (!IsPlaying(voicesOvers[voiceOverIndex])) {
                SnippedDone();
            }
        }
        musicManagement.SetVolume(currentVolume);
    }


    public static bool IsPlaying(FMOD.Studio.EventInstance instance) {
        FMOD.Studio.PLAYBACK_STATE state;
        instance.getPlaybackState(out state);
        return state != FMOD.Studio.PLAYBACK_STATE.STOPPED;
    }

    public void SetPlayVoiceOverAfterSwitch(bool b) {
        playVoiceOverAfterSwitch = b;
    }

    public bool GetPlayVoiceOverAfterSwitch() {
        return playVoiceOverAfterSwitch;
    }

    public void StartOutro() {
        Debug.Log("Start OUTRO");
        inOutro = true;
        TerminateSnippet();
    }
}