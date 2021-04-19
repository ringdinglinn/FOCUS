using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManagement : MonoBehaviourReferenced {
    public Terrain terrain;
    public Texture2D tex;
    public List<TerrainCutter> allTerrainCutters;

    IEnumerator Start() {
        if (referenceManagement.cutTerrain) {
            foreach (TerrainCutter tc in allTerrainCutters) {
                tc.CutHoles();

                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}