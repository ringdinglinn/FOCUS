using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

// This was created in response to this Unity3D forum post:
// http://forum.unity3d.com/threads/terrain-cutout.291836/
//
// BE SURE YOU BACK UP YOUR TERRAIN DATA FIRST! USE SOURCE CONTROL!
// I will not be responsible for damage to your terrainData.
//
// To use, place this script on the Terrain itself and make sure
// all "buildings" are parented to the terrain.
//
// There are MANY embedded assumptions here. Some of them are:
//	- All cut/raise objects should be parented below the terrain
//	- All objects both cut and raise (see boolean arguments).
//	- The only object-level scaling is done at the leaf objects of the tree

public class TerrainCutter : MonoBehaviourReferenced
{
	bool[,] holes;
	Terrain ter;
	TerrainData td;


	void ModifyTerrain( MeshFilter mf, bool raise, bool lower)
	{
		Bounds bounds = mf.mesh.bounds;

		Vector3 scaledSize = new Vector3 (
			bounds.size.x * mf.transform.localScale.x,
			bounds.size.y * mf.transform.localScale.y,
			bounds.size.z * mf.transform.localScale.z) * 0.5f;
		Quaternion rot = mf.transform.rotation;

		Vector3[] bottoms = new Vector3[4];
		Vector3[] tops = new Vector3[4];

		int n = 0;
		int m = 0;
		for (int i = -1; i <= 1; i += 2)
		{
			for (int j = -1; j <= 1; j += 2)
			{
				for (int k = -1; k <= 1; k += 2)
				{
					Vector3 pos = mf.transform.localPosition +
						bounds.center +
							rot * new Vector3(
								scaledSize.x * i,
								scaledSize.y * j,
								scaledSize.z * k);
					if (j == -1)
					{
						bottoms[n++] = pos;
                        //GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        //sphere.transform.position = pos;
                        //sphere.name = "BOTTOM";
                    }
					else {
						tops[m++] = pos;
						//GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
						//sphere.transform.position = pos;
						//sphere.name = "TOP";
					}

					// enable if you want to see the corners we calculated as spheres
				}
			}
		}

		// take the object's terrain-relative height and scale it by the terrain's size.
		float scaledUndersideHeight = bottoms[0].y / td.size.y;
		float scaledUppersideHeight = tops[0].y / td.size.y;

		float[,] heightField = td.GetHeights( 0, 0, td.heightmapResolution, td.heightmapResolution);

		// Now we traverse our bottom corners (in object bounds space) and set
		// the affected heightmaps to the new scaledUndersideHeight

		// This calculates number of steps and is naively overkill, as one only needs
		// to make sure that you don't skip any interim heightmap datapoints.
		int uSteps = (int)Vector3.Distance( bottoms[0], bottoms[1]);
		int vSteps = (int)Vector3.Distance( bottoms[0], bottoms[2]);

		// You need to hit points beyond the bounds or else your building
		// will be perched upon a too-narrow terrain pedestal, especially
		// as LOD kicks in at further viewing distances.
		const int extraCellsBeyond = 0;

		for (int v = -extraCellsBeyond; v <= vSteps + extraCellsBeyond; v++)
		{
			for (int u = -extraCellsBeyond; u <= uSteps + extraCellsBeyond; u++)
			{
				Vector3 position = bottoms[0] +
						((bottoms[1] - bottoms[0]) * u) / uSteps +
						((bottoms[2] - bottoms[0]) * v) / vSteps;

				position = new Vector3(
					(position.x * td.heightmapResolution) / td.size.x,
					0,		// do not care
					(position.z * td.heightmapResolution) / td.size.z);

				int terrainS = (int)position.x;
				int terrainR = (int)position.z;

				if (terrainR >= 0 && terrainR < heightField.GetLength(0)-1)
				{
					if (terrainS >= 0 && terrainS < heightField.GetLength(1)-1)
					{

						if ((heightField[terrainR,terrainS] < scaledUppersideHeight) && (heightField[terrainR, terrainS] > scaledUndersideHeight))
						{
							holes[terrainR, terrainS] = false;
						}
					}
				}
			}
		}
	}

	public void CutHoles ()
	{
		MeshFilter[] AllMeshFilters = referenceManagement.entryFilters.ToArray();

        ter = gameObject.GetComponent<Terrain>();
		td = ter.terrainData;
		holes = new bool[td.heightmapResolution - 1, td.heightmapResolution - 1];
		for (int i = 0; i < td.heightmapResolution - 1; i++) {
			for (int j = 0; j < td.heightmapResolution - 1; j++) {
				holes[i, j] = true;
			}
		}

		foreach (MeshFilter mf in AllMeshFilters) {
			mf.transform.SetParent(transform);
			ModifyTerrain(mf, true, true);
		}

		td.SetHoles(0, 0, holes);
	}

    private void OnEnable() {
        referenceManagement.terrainManagement.allTerrainCutters.Add(this);
    }
}
