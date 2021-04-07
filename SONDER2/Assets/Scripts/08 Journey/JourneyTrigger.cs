using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JourneyTrigger : MonoBehaviourReferenced {
    JourneyManagement journeyManagement;
	public enum TriggerType { IntroLoop, Gate0 };
    public TriggerType type;

    private void Start() {
        journeyManagement = referenceManagement.journeyManagement;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Car")) {

        }
    }

}