using System.Collections.Generic;
using PathCreation.Utility;
using UnityEngine;

namespace PathCreation.Examples {
    public class RoadMeshCreator : PathSceneTool {
        [Header ("Road settings")]
        public float roadWidth = .4f;
        [Range (0, 5f)]
        public float thickness = .15f;
        [Range(0, 3)]
        public float barrierHeight = 0.8f;
        [Range(0, 1)]
        public float barrierThickness = 0.3f;
        public bool flattenSurface;

        [Header ("Material settings")]
        public Material roadMaterial;
        public Material undersideMaterial;
        public float textureTiling = 1;

        [SerializeField]
        GameObject meshHolder;
        const string meshHolderName = "Road Mesh Holder";

        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        Mesh mesh;
        MeshCollider coll;

        private void Awake() {
            meshHolder = transform.Find(meshHolderName).gameObject != null ? transform.Find(meshHolderName).gameObject: null;
        }

        public void PathAdjusted() {
            PathUpdated();
        }

        protected override void PathUpdated () {
            if (pathCreator != null) {
                AssignMeshComponents ();
                AssignMaterials ();
                CreateRoadMesh ();
                CreateCollider();
            }
        }

        void CreateCollider() {
            if (meshHolder.GetComponent<MeshCollider>() == null) {
                coll = meshHolder.AddComponent<MeshCollider>();
            } else if (coll == null) {
                coll = meshHolder.GetComponent<MeshCollider>();
            }
            coll.sharedMesh = null;
            coll.sharedMesh = mesh;
        }

        void CreateRoadMesh () {
            Vector3[] verts = new Vector3[path.NumPoints * 20];
            Vector2[] uvs = new Vector2[verts.Length];
            Vector3[] normals = new Vector3[verts.Length];

            int numTris = 2 * (path.NumPoints - 1) + ((path.isClosedLoop) ? 2 : 0);
            int[] roadTriangles = new int[numTris * 3];
            int[] underRoadTriangles = new int[numTris * 3];
            int[] sideOfRoadTriangles = new int[numTris * 2 * 3];
            int[] barrierTriangles = new int[numTris * 8 * 3];

            int vertIndex = 0;
            int triIndex = 0;
            int barrierTriIndex = 0;

            // Vertices for the top of the road are layed out:
            // 0  1
            // 8  9
            // and so on... So the triangle map 0,8,1 for example, defines a triangle from top left to bottom left to bottom right.
            int[] triangleMap = { 0, 20, 1, 1, 20, 21 };
            int[] sidesTriangleMap = { 10, 12, 32, 10, 32, 30, 11, 33, 13, 11, 31, 33 };
            int[] barrierTriangleMap = { 14, 38, 18, 14, 34, 38, 2, 22, 4, 4, 22, 24, 8, 26, 6, 8, 28, 26, 16, 36, 10, 10, 36, 30, 15, 19, 39, 15, 39, 35, 7, 27, 29, 7, 29, 9, 11, 37, 17, 11, 31, 37, 3, 5, 25, 3, 25, 23 };

            bool usePathNormals = !(path.space == PathSpace.xyz && flattenSurface);

            for (int i = 0; i < path.NumPoints; i++) {
                Vector3 localUp = (usePathNormals) ? Vector3.Cross(path.GetTangent(i), path.GetNormal(i)) : path.up;
                Vector3 localRight = (usePathNormals) ? path.GetNormal(i) : Vector3.Cross(localUp, path.GetTangent(i));

                // Find position to left and right of current path vertex
                Vector3 vertSideA = path.GetPoint(i) - localRight * Mathf.Abs(roadWidth);
                Vector3 vertSideB = path.GetPoint(i) + localRight * Mathf.Abs(roadWidth);

                // Add top of road vertices
                verts[vertIndex + 0] = vertSideA;
                verts[vertIndex + 1] = vertSideB;
                // Add bottom of road vertices
                verts[vertIndex + 2] = vertSideA - localUp * thickness;
                verts[vertIndex + 3] = vertSideB - localUp * thickness;
                // Add bottom barrier verts
                verts[vertIndex + 4] = vertSideA - localRight * barrierThickness - localUp * thickness;
                verts[vertIndex + 5] = vertSideB + localRight * barrierThickness - localUp * thickness;
                // Add top barrier verts inner side
                verts[vertIndex + 6] = vertSideA + localUp * barrierHeight;
                verts[vertIndex + 7] = vertSideB + localUp * barrierHeight;
                // Add top barrier verts outer side
                verts[vertIndex + 8] = vertSideA - localRight * barrierThickness + localUp * barrierHeight;
                verts[vertIndex + 9] = vertSideB + localRight * barrierThickness + localUp * barrierHeight;

                // Duplicate vertices to get flat shading for sides of road
                verts[vertIndex + 10] = verts[vertIndex + 0];
                verts[vertIndex + 11] = verts[vertIndex + 1];
                verts[vertIndex + 12] = verts[vertIndex + 2];
                verts[vertIndex + 13] = verts[vertIndex + 3];
                verts[vertIndex + 14] = verts[vertIndex + 4];
                verts[vertIndex + 15] = verts[vertIndex + 5];
                verts[vertIndex + 16] = verts[vertIndex + 6];
                verts[vertIndex + 17] = verts[vertIndex + 7];
                verts[vertIndex + 18] = verts[vertIndex + 8];
                verts[vertIndex + 19] = verts[vertIndex + 9];

                // Set uv on y axis to path time (0 at start of path, up to 1 at end of path)
                uvs[vertIndex + 0] = new Vector2 (0, path.times[i]);
                uvs[vertIndex + 1] = new Vector2 (1, path.times[i]);

                // Top of road normals
                normals[vertIndex + 0] = localUp;
                normals[vertIndex + 1] = localUp;
                // Bottom of road normals
                normals[vertIndex + 2] = -localUp;
                normals[vertIndex + 3] = -localUp;
                // Bottom barrier normals
                normals[vertIndex + 4] = -localUp;
                normals[vertIndex + 5] = -localUp;
                // Top inner barrier normals
                normals[vertIndex + 6] = localUp;
                normals[vertIndex + 7] = localUp;
                // Top outer barrier normals
                normals[vertIndex + 8] = localUp;
                normals[vertIndex + 9] = localUp;

                // inner top side normals -> top of road
                normals[vertIndex + 10] = localRight;
                normals[vertIndex + 11] = -localRight;
                // inner bottom side normals -> bottom of road
                normals[vertIndex + 12] = -localRight;
                normals[vertIndex + 13] = localRight;
                // outer bottom side normals -> bottom barrier normals
                normals[vertIndex + 14] = -localRight;
                normals[vertIndex + 15] = localRight;
                // inner top barrier side normals -> top inner barrier normals
                normals[vertIndex + 16] = localRight;
                normals[vertIndex + 17] = -localRight;
                // outer top barrier side normals -> top outer barrier normals
                normals[vertIndex + 18] = -localRight;
                normals[vertIndex + 19] = localRight;

                // Set triangle indices
                if (i < path.NumPoints - 1 || path.isClosedLoop) {
                    for (int j = 0; j < triangleMap.Length; j++) {
                        roadTriangles[triIndex + j] = (vertIndex + triangleMap[j]) % verts.Length;
                        // reverse triangle map for under road so that triangles wind the other way and are visible from underneath
                        underRoadTriangles[triIndex + j] = (vertIndex + triangleMap[triangleMap.Length - 1 - j] + 2) % verts.Length;
                    }
                    for (int j = 0; j < sidesTriangleMap.Length; j++) {
                        sideOfRoadTriangles[triIndex * 2 + j] = (vertIndex + sidesTriangleMap[j]) % verts.Length;
                    }
                    for (int j = 0; j < barrierTriangleMap.Length; j++) {
                        barrierTriangles[barrierTriIndex + j] = (vertIndex + barrierTriangleMap[j]) % verts.Length;
                    }

                }

                vertIndex += 20;
                triIndex += 6;
                barrierTriIndex += barrierTriangleMap.Length;
            }

            mesh.Clear ();
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.normals = normals;
            mesh.subMeshCount = 4;
            mesh.SetTriangles(roadTriangles, 0);
            mesh.SetTriangles(underRoadTriangles, 1);
            mesh.SetTriangles(sideOfRoadTriangles, 2);
            mesh.SetTriangles(barrierTriangles, 3);
            mesh.RecalculateBounds ();
        }

        // Add MeshRenderer and MeshFilter components to this gameobject if not already attached
        void AssignMeshComponents () {
            if (transform.Find(meshHolderName) == null) {
                meshHolder = new GameObject(meshHolderName);
                meshHolder.transform.parent = gameObject.transform;
            } else {
                meshHolder = transform.Find(meshHolderName).gameObject;
            }

            meshHolder.transform.rotation = Quaternion.identity;
            meshHolder.transform.position = Vector3.zero;
            meshHolder.transform.localScale = Vector3.one;

            // Ensure mesh renderer and filter components are assigned
            if (!meshHolder.gameObject.GetComponent<MeshFilter> ()) {
                meshHolder.gameObject.AddComponent<MeshFilter> ();
            }
            if (!meshHolder.GetComponent<MeshRenderer> ()) {
                meshHolder.gameObject.AddComponent<MeshRenderer> ();
            }

            meshRenderer = meshHolder.GetComponent<MeshRenderer> ();
            meshFilter = meshHolder.GetComponent<MeshFilter> ();
            if (mesh == null) {
                mesh = new Mesh ();
            }
            meshFilter.sharedMesh = mesh;
        }

        void AssignMaterials () {
            if (roadMaterial != null && undersideMaterial != null) {
                meshRenderer.sharedMaterials = new Material[] { roadMaterial, undersideMaterial, undersideMaterial, undersideMaterial };
                meshRenderer.sharedMaterials[0].mainTextureScale = new Vector3 (1, textureTiling);
            }
        }

    }
}