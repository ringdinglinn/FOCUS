using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceClipTrigger : MonoBehaviourReferenced {
	bool triggered = false;
    VoiceClipManagement voiceClipManagement;
    MusicManagement musicManagement;

    private void Start() {
        voiceClipManagement = referenceManagement.voiceClipManagement;
        musicManagement = referenceManagement.musicManagement;
    }

    private void OnTriggerEnter(Collider other) {
        if (!triggered && other.CompareTag("Camera")) {
            triggered = true;
            voiceClipManagement.SetPlayVoiceOverAfterSwitch(true);
            musicManagement.SetPlayVoiceOverAfterSwitch(true);
        }
    }
}