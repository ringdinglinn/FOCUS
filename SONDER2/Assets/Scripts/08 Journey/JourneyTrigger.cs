using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JourneyTrigger : MonoBehaviourReferenced {
    JourneyManagement journeyManagement;
	public enum TriggerType { IntroLoop, Gate0, SlowDownGate, EndGate };
    public TriggerType type;

    private void Start() {
        journeyManagement = referenceManagement.journeyManagement;
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Car")) {
            if (type == TriggerType.SlowDownGate) {
                CarAI carAI = other.GetComponentInParent<CarAI>();
                carAI.SlowDown();
            } else if (type == TriggerType.EndGate) {
                StartCoroutine(EndScene());
            }
        }
    }

    IEnumerator EndScene() {
        referenceManagement.thankYouText.SetActive(true);
        yield return new WaitForSeconds(4f);
        SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);
    }
}