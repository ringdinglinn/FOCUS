using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceClipTrigger : MonoBehaviourReferenced {
	bool triggered = false;
    VoiceClipManagement voiceClipManagement;

    private void Start() {
        voiceClipManagement = referenceManagement.voiceClipManagement;
    }

    private void OnTriggerEnter(Collider other) {
        if (!triggered && other.CompareTag("Camera")) {
            Debug.Log("Camera Enters Trigger");
            triggered = true;
            voiceClipManagement.SetPlayVoiceOverAfterSwitch(true);
        }
    }
}