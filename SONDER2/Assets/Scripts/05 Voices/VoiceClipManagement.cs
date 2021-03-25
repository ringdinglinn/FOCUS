using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceClipManagement : MonoBehaviourReferenced {

    private VoiceClipBehaviour voiceClipBehaviour;
    private SwitchingManagement switchingManagement;

    private void OnEnable() {
        voiceClipBehaviour = referenceManagement.voiceClipBehaviour;
        switchingManagement = referenceManagement.switchingManagement;

        switchingManagement.CarSwitchedEvent.AddListener(OnCarSwitchted);
    }

    private void OnCarSwitchted() {
        PlaySnippet(Random.Range(0, 10));
    }

    private void PlaySnippet(int i) {
        voiceClipBehaviour.ChangeClip(i);
        voiceClipBehaviour.Play();
    }
}