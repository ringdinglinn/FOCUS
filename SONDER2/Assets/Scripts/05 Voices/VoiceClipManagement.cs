using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceClipManagement : MonoBehaviourReferenced {

    private VoiceClipBehaviour voiceClipBehaviour;
    private SwitchingManagement switchingManagement;
    private MusicManagement musicManagement;

    float p = 1f;
    float delay = 7f;
    float delayRange = 2f;
    float volumeTime = 1f;

    float targetVolume = 1f;
    float currentVolume = 1f;

    bool playedAudio = false;

    private void OnEnable() {
        voiceClipBehaviour = referenceManagement.voiceClipBehaviour;
        switchingManagement = referenceManagement.switchingManagement;
        musicManagement = referenceManagement.musicManagement;

        switchingManagement.CarSwitchedEvent.AddListener(OnCarSwitched);
    }

    private void OnDisable() {
        switchingManagement.CarSwitchedEvent.RemoveListener(OnCarSwitched);
    }

    private void OnCarSwitched() {
        currentVolume = 1;
        targetVolume = 1;
        voiceClipBehaviour.Stop();
        StopCoroutine(WaitToPlaySnippet());
        if (Random.Range(0f, 1f) <= p) {
            StartCoroutine(WaitToPlaySnippet());
        }
    }

    IEnumerator WaitToPlaySnippet() {
        int index = Random.Range(0, 10);
        Debug.Log($"Wait to play snippet {index}");
        float d = Random.Range(delay - delayRange * 0.5f, delay + delayRange * 0.5f);
        yield return new WaitForSeconds(d - volumeTime);
        yield return new WaitForSeconds(d);
        targetVolume = 0.7f;
        PlaySnippet(index);
        playedAudio = true;
    }

    void SnippedDone() {
        Debug.Log("Snippet done");
        targetVolume = 1;
        playedAudio = false;
    }

    private void PlaySnippet(int i) {
        voiceClipBehaviour.ChangeClip(i);
        voiceClipBehaviour.Play();
    }

    private void Update() {
        currentVolume = Mathf.Lerp(currentVolume, targetVolume, 1 * Time.deltaTime);
        if (playedAudio) {
            if (!voiceClipBehaviour.IsPlaying()) {
                SnippedDone();
            }
        }

        musicManagement.SetVolume(currentVolume);
    }
}