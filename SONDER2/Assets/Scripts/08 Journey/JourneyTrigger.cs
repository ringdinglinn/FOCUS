using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JourneyTrigger : MonoBehaviourReferenced {
    JourneyManagement journeyManagement;
	public enum TriggerType { IntroLoop, Gate0, SlowDownGate, EndGate, JamStartDrivingGate, SlowDownActiveCar, StopActiveCar, FallTrigger};
    public TriggerType type;

    bool speedUpTrigger;

    private void Start() {
        journeyManagement = referenceManagement.journeyManagement;
    }

    private void OnTriggerEnter(Collider other) {
        if (type == TriggerType.SlowDownGate) {
            if (other.gameObject.CompareTag("Car")) {
                CarAI carAI = other.GetComponentInParent<CarAI>();
                carAI.SlowDown(1f);
            }
        }
        if (type == TriggerType.EndGate) {
            if (other.gameObject.CompareTag("Car")) {
                StartCoroutine(EndScene());

            }
        }
        if (type == TriggerType.SlowDownActiveCar) {
            if (other.gameObject.CompareTag("Car")) {
                CarAI carAI = other.GetComponentInParent<CarAI>();
                carAI.SlowDown(0f);
            }
        }
        if (type == TriggerType.StopActiveCar) {
            if (other.gameObject.CompareTag("Car")) {
                CarAI carAI = other.GetComponentInParent<CarAI>();
                carAI.StopCar();
            }
        }
        if (type == TriggerType.FallTrigger) {
            if (other.gameObject.CompareTag("Car")) {
                CarAI carAI = other.GetComponentInParent<CarAI>();
                if (carAI.autopilotEnabled) {
                    journeyManagement.StartCarsFalling(carAI);
                }
            }
        }
    }

    private void OnTriggerStay(Collider other) {
        if (type == TriggerType.JamStartDrivingGate) {
            if (other.gameObject.CompareTag("Car")) {
                CarAI carAI = other.GetComponentInParent<CarAI>();
                Debug.Log($"carAI = {carAI}");
                if (!carAI.autopilotEnabled && !speedUpTrigger) {
                    speedUpTrigger = true;
                    journeyManagement.SpeedUpAfterJam();
                }
            }
        }
    }

    IEnumerator EndScene() {
        referenceManagement.thankYouText.SetActive(true);
        yield return new WaitForSeconds(4f);
        SceneManager.LoadScene("MenuScene", LoadSceneMode.Single);
    }
}