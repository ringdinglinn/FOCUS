using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManagement : MonoBehaviourReferenced {

    PathManagement pathManagement;
    List<PathBehaviour> allPathBehaviours;
    List<GameObject> levels;
    CarManagement carManagement;

    bool inEndTunnel = false;

    public int levelNr = 0;

    private void Start() {
        pathManagement = referenceManagement.pathManagement;
        allPathBehaviours = pathManagement.GetAllPaths();
        carManagement = referenceManagement.carManagement;
        levels = referenceManagement.levels;
        ChangeLevel(true);
    }

    public void EnteredEndTunnel(int startPathID, int endPathID) {
        foreach(GameObject level in levels) {
            level.SetActive(true);
        }
        //carManagement.ClearSBs();
        //carManagement.ClearCarAIs();

        foreach (PathBehaviour pb in allPathBehaviours) {
            if (pb.id != startPathID && pb.id != endPathID) pb.gameObject.SetActive(false);
        }
        inEndTunnel = true;
    }

    public void CheckIfNextLevel() {
        if (inEndTunnel) {

            levelNr++;

            foreach (PathBehaviour pb in allPathBehaviours) {
                pb.gameObject.SetActive(true);
            }
            ChangeLevel(false);

            inEndTunnel = false;
        }
    }

    private void ChangeLevel(bool start) {
        if (!start) {
            //carManagement.ClearSBs();
            //carManagement.ClearCarAIs();
        }

        for (int i = 0; i < levels.Count; i++) {
            if (i != levelNr)
            levels[i].SetActive(false);
        }
        levels[levelNr].SetActive(true);
    }
}