using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CarVisuals : MonoBehaviourReferenced {
	public List<CarConfig> allCarConfigs = new List<CarConfig>();
	[HideInInspector]
	public List<WheelCollider> wheels = new List<WheelCollider>();

	VehicleBehaviour.WheelVehicle wv;
	SwitchingBehaviour switchingBehaviour;

	public enum car { car0, car1, car2, car3, car4, car5, car6, car7 };
	public car myCar;

	public void UpdateVisuals() {
		foreach (CarConfig cc in allCarConfigs) {
			cc.gameObject.SetActive(false);
        }
		allCarConfigs[(int)myCar].gameObject.SetActive(true);
		ConfigReferences();
	}

	public void SetCarVisuals(int i) {
		myCar = (car)i;
    }

	private void OnEnable() {
		switchingBehaviour = GetComponent<SwitchingBehaviour>();
		wv = GetComponent<VehicleBehaviour.WheelVehicle>();
        UpdateVisuals();
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
		switchingBehaviour.armaturenBrett = carConfig.armaturenbrett;
		wv.TurnWheel = new WheelCollider[] { carConfig.wheels[0], carConfig.wheels[1] };
		wv.DriveWheel = new WheelCollider[] { carConfig.wheels[0], carConfig.wheels[1] };
		wheels = carConfig.wheels;
	}
}