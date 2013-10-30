// Filename: GenerateMeshDynamic.js
// Author: Sam Hooke
// Date: 2010
// Originally written for Domino's Adventure

/*
General flow of 'Start()'
-Searchs for other objects with the same mesh name
-If one is found:
--We generate their mesh for them if it does not already exist
--We assign their generated mesh to us as well
-else
--Generate our own mesh

Generatel flow of 'GenerateMesh()'
-Create an empty mesh
-Loop through the 2D points list
--Generate a list of 3D points. each pair of 2D points corresponds to a pair of triangles
-Assign the list of 3D points to the empty mesh
*/

@script RequireComponent(MeshFilter)				// A MeshFilter is required
@script RequireComponent(MeshCollider)				// A MeshCollider is required
@script RequireComponent(Rigidbody)					// We have to have a rigidbody
//@script RequireComponent(ConfigurableJoint)			// We use a configurable joint to lock to the XY axis

var meshDepth : float = 2;							// The depth the mesh will be in the z direction
var winding = ClockDirection.Anticlockwise;			// Points should be going anticlockwise. If not, set this to clockwise.
var closedLoop : boolean = true;					// Whether the last point should automatically join the first point
var autoAssignToFilter : boolean = true;			// Whether to assign the mesh to the filer
var autoAssignToCollider : boolean = true;			// Whether to assign the mesh to the collider
var shareThisMesh : boolean = true;					// If true, other objects will be allowed to use this mesh instead of their own		
var findSharedMesh : boolean = true;				// If true, this obejct will search for other meshes before generating its own
//var lockToPlane : boolean = true;					// If true, it will use a configurable joint to lock to the XY plane
var gizmoOffset : float = 0;						// How much to draw the gizmo offset in the Z direction
var meshName : String = "";							// Name for the generated mesh. If blank, "GeneratedMesh" will be used.
var lineList : LineList[];							// Create an array of triangles for the user to fill in

protected var meshGen : Mesh;						// Create an empty mesh to be filled with the triangle coordinates
protected var meshList : GenerateMeshDynamic[];		// A list of other instances of this script
protected var hasGeneratedMesh : boolean = false;	// Whether we have generate a mesh

/*
function FixedUpdate()
	{
	if (lockToPlane)
		{
		var join : ConfigurableJoint = GetComponent(ConfigurableJoint);
		join.connectedBody = null;
		join.anchor = Vector3.zero;
		join.xMotion = ConfigurableJointMotion.Free;
		join.yMotion = ConfigurableJointMotion.Free;
		join.zMotion = ConfigurableJointMotion.Locked;
		join.angularXMotion = ConfigurableJointMotion.Locked;
		join.angularYMotion = ConfigurableJointMotion.Locked;
		join.angularZMotion = ConfigurableJointMotion.Free;
		}
	}
*/

function Start()
	{
		
	// We are making a dynamic mesh
	rigidbody.isKinematic = false;
	
	// Whether we should generate a mesh, and what mesh we should share
	var generateOwnMesh = true;
	var meshToShare : Mesh;
	
	// Get a list of all other instances of this script, and loop through
	if (findSharedMesh)
		{
		meshList = FindObjectsOfType(GenerateMeshDynamic);
		for (var i = 0; i < meshList.length;i++)
			{
			// Get the script component
			var iScript = meshList[i].GetComponent(GenerateMeshDynamic) as GenerateMeshDynamic;
			
			// Find mesh that is allowed to be shared that has the same name as ours
			if (iScript.IsMeshShared() && (iScript.GetMeshName() == GetMeshName()))
				{
				// If the one we found has not generated its mesh yet, generate it for them
				if (!iScript.HasGeneratedMesh())
					{
					iScript.GenerateMesh();
					iScript.AssignMesh(iScript.GetMesh());
					}
					
				// Then assign their mesh to us
				meshToShare = iScript.GetMesh();
				generateOwnMesh = false;
				break;
				}
			}
		}
	
	// Generate our own mesh if we should, else just use the one we are sharing
	if (generateOwnMesh)
		GenerateMesh();
	else
		meshGen = meshToShare;
	
	// Finally, assign this mesh to our collider and filter (if set so in the options)
	AssignMesh(meshGen);
	}
	
function GenerateMesh()
	{
	// Create a new mesh
	meshGen = new Mesh();
	
	// Give it a name
	if (meshName != "")
		meshGen.name = meshName;
	else
		meshGen.name = "GeneratedMesh";
		
	//
	var vertsPerPoint = 12;
	
	// Generate temporary lists to store the verticies, triangles and normals
	var tempVerts : Vector3[] = new Vector3[lineList.length*vertsPerPoint];
	var tempTri = new int[lineList.length*vertsPerPoint];

	// Reverse the array if it is clockwise (should be anticlockwise)	
	if (winding == ClockDirection.Clockwise)
		lineList.Reverse(lineList);

	// Precalculate the half to save processing time later
	var meshDepthHalved = meshDepth/2;

	// Work out which points to loop through. If we are closed loop, we join the first with the last as well.
	var loopLength : float;
	if (closedLoop)
		loopLength = lineList.length;
	else
		loopLength = lineList.length-1;

	// Take the info from the GUI and store it in the appropriate arrays
	for (var i = 0; i < loopLength; i++)
		{
		// Work out what point we are joining to
		var i2 = i+1;
		if (i2 == lineList.length)
			i2 = 0;
			
		// first: far left, near left, near right
		tempVerts[i*vertsPerPoint] = Vector3(lineList[i].x,lineList[i].y,meshDepthHalved);
		tempVerts[i*vertsPerPoint+1] = Vector3(lineList[i].x,lineList[i].y,-meshDepthHalved);
		tempVerts[i*vertsPerPoint+2] = Vector3(lineList[i2].x,lineList[i2].y,-meshDepthHalved);
		
		// second: near right, far right, far left
		tempVerts[i*vertsPerPoint+3] = Vector3(lineList[i2].x,lineList[i2].y,-meshDepthHalved);
		tempVerts[i*vertsPerPoint+4] = Vector3(lineList[i2].x,lineList[i2].y,meshDepthHalved);
		tempVerts[i*vertsPerPoint+5] = Vector3(lineList[i].x,lineList[i].y,meshDepthHalved);
		
		// first: mark which points make up the triangle
		tempTri[i*vertsPerPoint] = i*vertsPerPoint;
		tempTri[i*vertsPerPoint+1] = i*vertsPerPoint+1;
		tempTri[i*vertsPerPoint+2] = i*vertsPerPoint+2;
		
		// second: mark which points make up the triangle
		tempTri[i*vertsPerPoint+3] = i*vertsPerPoint+3;
		tempTri[i*vertsPerPoint+4] = i*vertsPerPoint+4;
		tempTri[i*vertsPerPoint+5] = i*vertsPerPoint+5;
		

		// third: "top" (closest) face
		tempVerts[i*vertsPerPoint+6] = Vector3(0,0,meshDepthHalved);
		tempVerts[i*vertsPerPoint+7] = Vector3(lineList[i].x,lineList[i].y,meshDepthHalved);
		tempVerts[i*vertsPerPoint+8] = Vector3(lineList[i2].x,lineList[i2].y,meshDepthHalved);
		
		// fourth: "bottom" (furthest) face
		tempVerts[i*vertsPerPoint+9] = Vector3(0,0,-meshDepthHalved);
		tempVerts[i*vertsPerPoint+10] = Vector3(lineList[i2].x,lineList[i2].y,-meshDepthHalved);
		tempVerts[i*vertsPerPoint+11] = Vector3(lineList[i].x,lineList[i].y,-meshDepthHalved);
		
		// third: mark which points make up the triangle
		tempTri[i*vertsPerPoint+6] = i*vertsPerPoint+6;
		tempTri[i*vertsPerPoint+7] = i*vertsPerPoint+7;
		tempTri[i*vertsPerPoint+8] = i*vertsPerPoint+8;
		
		// fourth: mark which points make up the triangle
		tempTri[i*vertsPerPoint+9] = i*vertsPerPoint+9;
		tempTri[i*vertsPerPoint+10] = i*vertsPerPoint+10;
		tempTri[i*vertsPerPoint+11] = i*vertsPerPoint+11;
		}
		
	// Copy the arrays into the new mesh
	meshGen.vertices = tempVerts;
	meshGen.triangles = tempTri;
	
	// Recalculate stuff [not needed I don't think]
	/*
	meshGen.RecalculateNormals();
	meshGen.RecalculateBounds();
	meshGen.Optimize();
	*/
	
	hasGeneratedMesh = true;
	}

	
function AssignMesh(meshToAssign : Mesh)
	{
	// Assign the new mesh to the renderer and collider
	if (autoAssignToFilter)
		(GetComponent(MeshFilter) as MeshFilter).mesh = meshToAssign;
	if (autoAssignToCollider)
		(GetComponent(MeshCollider) as MeshCollider).sharedMesh = meshToAssign;
	}
	
function GetMesh()
	{
	return meshGen;
	}
	
function HasGeneratedMesh()
	{
	return hasGeneratedMesh;
	}
	
function GetMeshName()
	{
	return meshName;
	}
	
function IsMeshShared()
	{
	return shareThisMesh;
	}
	
// Make it draw the outline in the editor
function OnDrawGizmosSelected()
	{
	if (!Application.isPlaying)
		DrawGizmos();
	}
	
function DrawGizmos()
	{
	// Work out which points to loop through. If we are closed loop, we join the first with the last as well.
	var loopLength : float;
	if (closedLoop)
		loopLength = lineList.length;
	else
		loopLength = lineList.length-1;
	
	// Almost exactly the colour Unity uses for meshes
	Gizmos.color = Color(0.505882353,0.847058824,0.509803922);
	
	// Cache the scale and position
	var xscale = transform.lossyScale.x;
	var yscale = transform.lossyScale.y;
	var xpos = transform.position.x;
	var ypos = transform.position.y;
	var zpos = transform.position.z+gizmoOffset;
	var rot = transform.rotation.z*Mathf.Deg2Rad;
	
	for (var i = 0; i < loopLength; i++)
		{
		var i2 = i+1;
		if (i2 == lineList.length)
			i2 = 0;
		// Draw the shape, taking into account rotation and zoffset
		Gizmos.DrawLine(Vector3(xpos+lineList[i].x*xscale,ypos+lineList[i].y*yscale,zpos),Vector3(xpos+lineList[i2].x*xscale,ypos+lineList[i2].y*yscale,zpos));
		/*
		var x1 : float = lineList[i].x*xscale;
		var y1 : float = lineList[i].y*yscale;
		var x2 : float = lineList[i2].x*xscale;
		var y2 : float = lineList[i2].y*yscale;
		var r : float = 1;//Mathf.Rad2Deg;
		Gizmos.DrawLine(
		Vector3(xpos+(x1*Mathf.Cos(rot)*r-y1*Mathf.Sin(rot)*r),ypos+(x1*Mathf.Sin(rot)*r+y1*Mathf.Cos(rot)*r),zpos),
		Vector3(xpos+(x2*Mathf.Cos(rot)*r-y2*Mathf.Sin(rot)*r),ypos+(x2*Mathf.Sin(rot)*r+y2*Mathf.Cos(rot)*r),zpos)
		);
		*/
		}
	}