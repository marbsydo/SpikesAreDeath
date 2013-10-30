using UnityEngine;
using System.Collections;

public enum ClockDirection {Anticlockwise, Clockwise}

[System.Serializable]
public class MeshPoints {
	public string name = "";
	public float x = 0;
	public float y = 0;
}

[System.Serializable]
public class MeshData {
	public string name = "";
	public ClockDirection winding = ClockDirection.Anticlockwise;
	public float depth = 2.0f;
	public MeshPoints[] points;
}

public class GenerateMesh : MonoBehaviour {
	public bool shareThisMesh = true;
	public bool findSharedMesh = true;
	public bool assignToMeshCollider = true;
	public MeshData meshData;

	private Mesh generatedMesh;
	private bool hasGeneratedMesh = false;

	const int VERTS_PER_POINT = 12;

	void Awake() {
		bool generateOwnMesh = true;

		if (findSharedMesh) {
			GenerateMesh[] meshList = FindObjectsOfType(typeof(GenerateMesh)) as GenerateMesh[];
			for (int i = 0; i < meshList.Length; i++) {
				GenerateMesh gm = meshList[i].GetComponent<GenerateMesh>();

				// Find a matching mesh that is shared
				if (gm.IsMeshShared() && (gm.GetMeshName() == GetMeshName())) {

					// If they have not generated yet, make them generate
					if (!gm.HasGeneratedmesh()) {
						gm.Generate();
						gm.AssignMesh(gm.GetMesh());
					}

					generatedMesh = gm.GetMesh();
					generateOwnMesh = false;
					break;
				}
			}
		}

		if (generateOwnMesh) {
			Generate();
		}

		AssignMesh(generatedMesh);
	}

	void Generate() {
		generatedMesh = new Mesh();
		generatedMesh.name = meshData.name;

		// Create temporary lists to store new mesh data
		Vector3[] tempVerts = new Vector3[meshData.points.Length * VERTS_PER_POINT];
		int[] tempTris = new int[meshData.points.Length * VERTS_PER_POINT];

		// Reverse the array if it is clockwise
		if (meshData.winding == ClockDirection.Clockwise)
			System.Array.Reverse(meshData.points);

		float depthHalved = meshData.depth / 2.0f;

		for (int pointA = 0; pointA < meshData.points.Length; pointA++) {
			int pointB = pointA + 1;
			if (pointB == meshData.points.Length)
				pointB = 0;

			// first: far left, near left, near right
			tempVerts[pointA * VERTS_PER_POINT + 0] = new Vector3(meshData.points[pointA].x, meshData.points[pointA].y,  depthHalved);
			tempVerts[pointA * VERTS_PER_POINT + 1] = new Vector3(meshData.points[pointA].x, meshData.points[pointA].y, -depthHalved);
			tempVerts[pointA * VERTS_PER_POINT + 2] = new Vector3(meshData.points[pointB].x, meshData.points[pointB].y, -depthHalved);
			
			// second: near right, far right, far left
			tempVerts[pointA * VERTS_PER_POINT + 3] = new Vector3(meshData.points[pointB].x, meshData.points[pointB].y, -depthHalved);
			tempVerts[pointA * VERTS_PER_POINT + 4] = new Vector3(meshData.points[pointB].x, meshData.points[pointB].y,  depthHalved);
			tempVerts[pointA * VERTS_PER_POINT + 5] = new Vector3(meshData.points[pointA].x, meshData.points[pointA].y,  depthHalved);
			
			// first: mark which points make up the triangle
			tempTris[pointA * VERTS_PER_POINT + 0] = pointA * VERTS_PER_POINT + 0;
			tempTris[pointA * VERTS_PER_POINT + 1] = pointA * VERTS_PER_POINT + 1;
			tempTris[pointA * VERTS_PER_POINT + 2] = pointA * VERTS_PER_POINT + 2;
			
			// second: mark which points make up the triangle
			tempTris[pointA * VERTS_PER_POINT + 3] = pointA * VERTS_PER_POINT + 3;
			tempTris[pointA * VERTS_PER_POINT + 4] = pointA * VERTS_PER_POINT + 4;
			tempTris[pointA * VERTS_PER_POINT + 5] = pointA * VERTS_PER_POINT + 5;
			

			// third: "top" (closest) face
			tempVerts[pointA * VERTS_PER_POINT + 6] = new Vector3(0                        , 0                        , depthHalved);
			tempVerts[pointA * VERTS_PER_POINT + 7] = new Vector3(meshData.points[pointA].x, meshData.points[pointA].y, depthHalved);
			tempVerts[pointA * VERTS_PER_POINT + 8] = new Vector3(meshData.points[pointB].x, meshData.points[pointB].y, depthHalved);
			
			// fourth: "bottom" (furthest) face
			tempVerts[pointA * VERTS_PER_POINT +  9] = new Vector3(0                        , 0                        , -depthHalved);
			tempVerts[pointA * VERTS_PER_POINT + 10] = new Vector3(meshData.points[pointB].x, meshData.points[pointB].y, -depthHalved);
			tempVerts[pointA * VERTS_PER_POINT + 11] = new Vector3(meshData.points[pointA].x, meshData.points[pointA].y, -depthHalved);
			
			// third: mark which points make up the triangle
			tempTris[pointA * VERTS_PER_POINT + 6] = pointA * VERTS_PER_POINT + 6;
			tempTris[pointA * VERTS_PER_POINT + 7] = pointA * VERTS_PER_POINT + 7;
			tempTris[pointA * VERTS_PER_POINT + 8] = pointA * VERTS_PER_POINT + 8;
			
			// fourth: mark which points make up the triangle
			tempTris[pointA * VERTS_PER_POINT +  9] = pointA * VERTS_PER_POINT +  9;
			tempTris[pointA * VERTS_PER_POINT + 10] = pointA * VERTS_PER_POINT + 10;
			tempTris[pointA * VERTS_PER_POINT + 11] = pointA * VERTS_PER_POINT + 11;
		}

		// Copy the arrays into the new mesh
		generatedMesh.vertices = tempVerts;
		generatedMesh.triangles = tempTris;

		hasGeneratedMesh = true;
	}

	void AssignMesh(Mesh meshToAssign) {
		if (assignToMeshCollider) {
			MeshCollider meshCollider = GetComponent<MeshCollider>();
			if (meshCollider != null)
				meshCollider.sharedMesh = meshToAssign;
			else {
				Debug.LogWarning("Tried to assign a mesh to a MeshCollider that does not exist");
			}
		}
	}

	Mesh GetMesh() {
		return generatedMesh;
	}

	bool HasGeneratedmesh() {
		return hasGeneratedMesh;
	}

	string GetMeshName() {
		return meshData.name;
	}

	bool IsMeshShared() {
		return shareThisMesh;
	}

	// Make it draw the outline in the editor
	void OnDrawGizmosSelected()
		{
		if (!Application.isPlaying)
			DrawGizmos();
		}

	void DrawGizmos() {
		// Almost exactly the colour Unity uses for meshes
		Gizmos.color = new Color(0.505882353f,0.847058824f,0.509803922f);

		float xscale = transform.lossyScale.x;
		float yscale = transform.lossyScale.y;
		float xpos = transform.position.x;
		float ypos = transform.position.y;
		float zpos = transform.position.z;
		//float rot = transform.rotation.z * Mathf.Deg2Rad;

		for (int pointA = 0; pointA < meshData.points.Length; pointA++) {
			int pointB = pointA + 1;
			if (pointB == meshData.points.Length)
				pointB = 0;

			Gizmos.DrawLine(new Vector3(xpos + meshData.points[pointA].x * xscale, ypos + meshData.points[pointA].y * yscale, zpos), new Vector3(xpos + meshData.points[pointB].x * xscale, ypos + meshData.points[pointB].y * yscale, zpos));
		}
	}
}
