using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class CarManagement : MonoBehaviourReferenced {

    PathManagement pathManagement;
    List<PathBehaviour> pathBehaviours;
    List<SwitchingBehaviour> allSwitchingBehaviours = new List<SwitchingBehaviour>();

    private GameObject carPrefab;

    private void OnEnable() {
        pathManagement = referenceManagement.pathManagement;
        pathBehaviours = pathManagement.GetAllPaths();
        carPrefab = referenceManagement.carPrefab;
    }

    private void Start() {
        InstantiateCars();
    }

    private void InstantiateCars() {
        bool setInitial = false;
        for (int i = 0; i < pathBehaviours.Count; i++) {
            List<Vector3> startPosList = pathBehaviours[i].GetStartPosBehaviour().GetList();
            int initPosIndex = pathBehaviours[i].GetStartPosBehaviour().GetInitialPosIndex();
            PathCreator path = pathBehaviours[i].GetPath();
            for (int j = 0; j < startPosList.Count; j++) {
                Vector3 pos = startPosList[j];
                Vector3 dir = path.path.GetDirectionAtDistance(path.path.GetClosestDistanceAlongPath(pos));
                GameObject car = Instantiate(carPrefab, startPosList[j], Quaternion.LookRotation(dir));
                CarAI carAI = car.GetComponent<CarAI>();
                SwitchingBehaviour sb = car.GetComponent<SwitchingBehaviour>();
                allSwitchingBehaviours.Add(sb);
                carAI.SetPathID(i);
                carAI.CreateStartConfig(pos, dir);
                if (!setInitial && j == initPosIndex) {
                    setInitial = true;
                    referenceManagement.switchingManagement.SetUpInitialCar(sb);
                }
            }
        }
    }

    public List<SwitchingBehaviour> getAllSwitchingBehaviours() {
        return allSwitchingBehaviours;
    }
}