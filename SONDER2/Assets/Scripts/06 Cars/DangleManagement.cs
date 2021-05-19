using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DangleManagement : MonoBehaviourReferenced {
	GameObject dangle;
	Transform parentTransform;
	Rigidbody activeRB;
	InputManagement inputManagement;

	private float amplitudeX = 0;
	private float maxAmpX = 0.3f;
	private float frequencyX = 0.5f;
	private float maxFreqX = 15;

	private float amplitudeY = 0;
	private float maxAmpY = 0.6f;
	private float frequencyY = 0.3f;
	private float maxFreqY = 5;

	private float currentAmplitudeX;

	[Range(0, 1)]
	public float speed;

	private bool hasDangle;

	float angularSpeed;
	float angularOffset;
	float maxAngularOffset = 35;

	public List<Dangle> dangleData = new List<Dangle>();
	List<Dangle> possibleDangles = new List<Dangle>();
	List<int> intervalCounters = new List<int>();

	SwitchingManagement switchingManagement;
	CarVisuals activeCarVisuals;

	private bool turning;

	float rotX;
	float rotY;

    private void Start() {
		switchingManagement = referenceManagement.switchingManagement;
		switchingManagement.CarSwitchedEvent.AddListener(HandleCarSwitched);
		switchingManagement.CarChangedEvent.AddListener(HandleCarChanged);
		inputManagement = referenceManagement.inputManagement;
		foreach (Dangle d in dangleData) {
			intervalCounters.Add(0);
        }
	}

    private void Animate(float speed) {

		amplitudeX = Mathf.Lerp(0, maxAmpX, speed);
		amplitudeY = Mathf.Lerp(0, maxAmpY, speed);
		frequencyX = Mathf.Lerp(0, maxFreqX, speed);
		frequencyY = Mathf.Lerp(0, maxFreqY, speed);

		angularSpeed = inputManagement.GetInput(Inputs.turn);
		if (angularSpeed == 0) {
			turning = false;
        } else {
			turning = true;
			currentAmplitudeX = maxAmpX * 2;
		}
		currentAmplitudeX = Mathf.Lerp(currentAmplitudeX, amplitudeX, 0.02f);

		angularOffset = -1 * angularSpeed * maxAngularOffset;

		dangle.transform.localRotation = Quaternion.Euler(dangle.transform.rotation.x, dangle.transform.rotation.y, angularOffset);

		rotX = Mathf.Lerp(rotX, Mathf.Cos(Time.time * frequencyX) * Mathf.Lerp(currentAmplitudeX, 0, Mathf.Abs(angularSpeed)) * Mathf.Rad2Deg, 0.2f);
		rotY = Mathf.Lerp(rotY, Mathf.Cos(Time.time * frequencyY) * Mathf.Lerp(amplitudeY, 0, Mathf.Abs(angularSpeed)) * Mathf.Rad2Deg, 0.2f);

		dangle.transform.RotateAround(dangle.transform.position, activeCarVisuals.transform.forward, rotX);
		dangle.transform.RotateAround(dangle.transform.position, dangle.transform.up, rotY);
    }

    private void Update() {
        if (hasDangle)
            Animate(speed);
    }

	private void HandleCarSwitched() {
		PickDangleOnCarSwitched();
    }

	private void HandleCarChanged() {
		activeCarVisuals = switchingManagement.ActiveCar.GetCarVisuals();
		activeRB = activeCarVisuals.GetComponent<Rigidbody>();
	}

	private void PickDangleOnCarSwitched() {
		possibleDangles.Clear();
		for (int i = 0; i < dangleData.Count; i++) {
			Dangle d = dangleData[i];
			if (--intervalCounters[i] <= 0) {
				if (Random.Range(0f,1f) <= d.probability) {
					possibleDangles.Add(d);
                }
            }
        }
		if (possibleDangles.Count > 0) {
			int index = Random.Range(0, dangleData.Count);
            intervalCounters[index] = dangleData[index].fixedInterval;
			activeCarVisuals.SetDangle(index);
			hasDangle = true;
		} else {
			activeCarVisuals.SetDangle(-1);
			hasDangle = false;
			dangle = null;
        }
    }

	public void SetDangleObj(GameObject dangle) {
		if (dangle != null) {
			this.dangle = dangle;
			parentTransform = dangle.transform.parent;
			hasDangle = true;
		} else {
			hasDangle = false;
			this.dangle = null;
			parentTransform = null;
		}
	}

	[System.Serializable]
	public struct Dangle {
		public string name;
		public int fixedInterval;
		public float probability;
	}
}