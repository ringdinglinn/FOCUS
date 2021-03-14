using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class CarManagement : MonoBehaviourReferenced {

    PathManagement pathManagement;
    List<PathBehaviour> pathBehaviours;
    List<SwitchingBehaviour> allSwitchingBehaviours = new List<SwitchingBehaviour>();

    List<CarAI> carClones = new List<CarAI>();

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

                // find start position
                Vector3 pos = startPosList[j];
                Vector3 dir = path.path.GetDirectionAtDistance(path.path.GetClosestDistanceAlongPath(pos));

                // instantiate and references
                GameObject car = Instantiate(carPrefab, startPosList[j], Quaternion.LookRotation(dir));
                CarAI carAI = car.GetComponent<CarAI>();
                SwitchingBehaviour sb = car.GetComponent<SwitchingBehaviour>();
                allSwitchingBehaviours.Add(sb);

                // set path id
                carAI.SetPathID(i);

                // set and save start position
                carAI.CreateStartConfig(pos, dir);
                carAI.SetToStartConfig();

                // initial car
                if (!setInitial && j == initPosIndex) {
                    setInitial = true;
                    referenceManagement.switchingManagement.SetUpInitialCar(sb);
                }
            }
        }
    }

    public CarAI CreateCarClone(int pathID) {
        GameObject cloneObj = Instantiate(carPrefab, Vector3.zero, Quaternion.identity);
        CarAI carAI = cloneObj.GetComponent<CarAI>();
        carAI.SetPathID(pathID);
        return carAI;
    }

    public List<SwitchingBehaviour> GetAllSwitchingBehaviours() {
        return allSwitchingBehaviours;
    }
}