using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BansheeGz.BGSpline.Curve;
using BansheeGz.BGSpline.Components;

public class TrackHelper : MonoBehaviour
{

    [SerializeField]
    private float minDistanceToShowHelper = 10;

    [SerializeField]
    private float forwardDistance = 10;

    [SerializeField]
    private Transform vehicle;

    [SerializeField]
    private BGCcMath curve;

    [SerializeField]
    private Transform trackHelper;

    [SerializeField]
    private Transform lookAtHelper;

    private void Start() {
        
    }

    private void Update()
    {
        Vector3 forwardVehiclePos = vehicle.position + (vehicle.forward * forwardDistance);
        Vector3 posHelper = curve.CalcPositionByClosestPoint(forwardVehiclePos);
        Vector3 posEffective = curve.CalcPositionByClosestPoint(vehicle.position );
        trackHelper.position = posHelper;

        float distance = Vector3.Distance(vehicle.position, posEffective);

        if(distance > minDistanceToShowHelper) {
            trackHelper.gameObject.SetActive(true);
            lookAtHelper.gameObject.SetActive(true);

            lookAtHelper.LookAt(posHelper);
        } else {
            trackHelper.gameObject.SetActive(false);
            lookAtHelper.gameObject.SetActive(false);
        }
    }
}
