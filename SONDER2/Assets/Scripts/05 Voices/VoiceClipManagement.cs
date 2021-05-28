using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceClipManagement : MonoBehaviourReferenced {

    private VoiceClipBehaviour voiceClipBehaviour;
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

    FMOD.Studio.EventInstance[] voicesOvers = new FMOD.Studio.EventInstance[17];

    private bool playVoiceOverAfterSwitch = true;
    private List<Coroutine> waitToPlayCoroutines = new List<Coroutine>();

    bool endSeqAlternating = false;

    private void OnEnable() {
        voiceClipBehaviour = referenceManagement.voiceClipBehaviour;
        switchingManagement = referenceManagement.switchingManagement;
        musicManagement = referenceManagement.musicManagement;
        levelManagement = referenceManagement.levelManagement;
        beatDetector = referenceManagement.beatDetector;

        switchingManagement.CarSwitchedEvent.AddListener(HandleCarSwitched);

        GetVoiceOvers();
    }

    private void GetVoiceOvers() {
        for (int i = 0; i < voicesOvers.Length; i++) {
            voicesOvers[i] = FMODUnity.RuntimeManager.CreateInstance(referenceManagement.voiceOver + $" {i+1}");
        }
    }

    private void OnDisable() {
        switchingManagement.CarSwitchedEvent.RemoveListener(HandleCarSwitched);
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

        if (levelManagement.levelNr == 6) endSeqAlternating = !endSeqAlternating;
        if (playVoiceOverAfterSwitch || (endSeqAlternating && levelManagement.levelNr == 6)) {
            waitToPlayCoroutines.Add(StartCoroutine(WaitToPlaySnippet()));
        }
    }

    IEnumerator WaitToPlaySnippet() {
        playVoiceOverAfterSwitch = false;
        float d = Random.Range(delay - delayRange * 0.5f, delay + delayRange * 0.5f);
        yield return new WaitForSeconds(levelManagement.levelNr >= 5 ? beatDetector.GetBarInterval() - volumeTime / 2 : d - volumeTime);
        targetVolume = 0.5f;
        yield return new WaitForSeconds(levelManagement.levelNr >= 5 ? volumeTime / 2 : volumeTime);

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


    bool IsPlaying(FMOD.Studio.EventInstance instance) {
        FMOD.Studio.PLAYBACK_STATE state;
        instance.getPlaybackState(out state);
        Debug.Log($"playback state = {state}");
        return state != FMOD.Studio.PLAYBACK_STATE.STOPPED;
    }

    public void SetPlayVoiceOverAfterSwitch(bool b) {
        playVoiceOverAfterSwitch = true;
    }
}