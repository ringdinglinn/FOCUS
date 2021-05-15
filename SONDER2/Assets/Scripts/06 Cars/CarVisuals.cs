using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CarVisuals : MonoBehaviourReferenced {
	public List<CarConfig> allCarConfigs = new List<CarConfig>();
	[HideInInspector]
	public List<WheelCollider> wheels = new List<WheelCollider>();

	VehicleBehaviour.WheelVehicle wv;
	SwitchingBehaviour switchingBehaviour;
	DangleManagement dangleManagement;

	public enum car { car2, car3, car4, car5, car6, car7 };
	public car myCar;
	public int variation = 0;

	public int dangleNr;

	public void UpdateVisuals(bool active) {
		Debug.Log($"Update Visuals, {active}, {name}");
		foreach (CarConfig cc in allCarConfigs) {
			cc.gameObject.SetActive(false);
        }
		allCarConfigs[(int)myCar].gameObject.SetActive(true);
		if (variation == 0) {
			allCarConfigs[(int)myCar].armaturenbrett.SetActive(active);
			allCarConfigs[(int)myCar].armaturenbrett2.SetActive(false);
		} else if (variation == 1) {
			allCarConfigs[(int)myCar].armaturenbrett.SetActive(false);
			allCarConfigs[(int)myCar].armaturenbrett2.SetActive(active);
		}
		ConfigReferences();
	}

	public void SetCarVisuals(int i, int v) {
		myCar = (car)i;
		variation = v;
    }

	private void OnEnable() {
		switchingBehaviour = GetComponent<SwitchingBehaviour>();
		wv = GetComponent<VehicleBehaviour.WheelVehicle>();
		dangleManagement = referenceManagement.dangleManagement;
	}

	private void ConfigReferences() {
		CarConfig carConfig = allCarConfigs[(int)myCar];
		switchingBehaviour.meshRenderer = carConfig.meshRenderer;
		switchingBehaviour.boxCollider = carConfig.boxCollider;
		switchingBehaviour.camRotTarget = carConfig.camRotTarget;
		switchingBehaviour.camTranslateTarget = carConfig.camTranslateTarget;
		switchingBehaviour.spotlights = carConfig.spotlights;
		switchingBehaviour.headlight1 = carConfig.headlight1;
		switchingBehaviour.headlight2 = carConfig.headlight2;
		switchingBehaviour.volumetrics = carConfig.volumetrics;
		switchingBehaviour.volumetricRenderer0 = carConfig.volumetricRenderer0;
		switchingBehaviour.volumetricRenderer1 = carConfig.volumetricRenderer1;
		switchingBehaviour.headlightCubes = carConfig.headlightCubes.ToArray();
		switchingBehaviour.armaturenbrett = carConfig.armaturenbrett;
		switchingBehaviour.armaturenbrett2 = carConfig.armaturenbrett2;
		switchingBehaviour.variation = variation;
		wv.TurnWheel = new WheelCollider[] { carConfig.wheels[0], carConfig.wheels[1] };
		wv.DriveWheel = new WheelCollider[] { carConfig.wheels[0], carConfig.wheels[1] };
		wheels = carConfig.wheels;
	}

	public List<GameObject> GetDangleList() {
		return allCarConfigs[(int)myCar].dangles;
    }

	public void SetDangle(int index) {
		foreach (GameObject d in allCarConfigs[(int)myCar].dangles) {
			d.SetActive(false);
        }
		if (index != -1) {
			allCarConfigs[(int)myCar].dangles[index].SetActive(true);
			dangleManagement.SetDangleObj(allCarConfigs[(int)myCar].dangles[index]);
		} else {
			dangleManagement.SetDangleObj(null);
        }
		dangleNr = index;
	}
}