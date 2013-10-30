// Filename: GenerateMeshStatic.js
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

var meshDepth : float = 2;							// The depth the mesh will be in the z direction
var winding = ClockDirection.Anticlockwise;			// Points should be going anticlockwise. If not, set this to clockwise.
var closedLoop : boolean = true;					// Whether the last point should automatically join the first point
var autoAssignToFilter : boolean = true;			// Whether to assign the mesh to the filer
var autoAssignToCollider : boolean = true;			// Whether to assign the mesh to the collider
var shareThisMesh : boolean = true;					// If true, other objects will be allowed to use this mesh instead of their own		
var findSharedMesh : boolean = true;				// If true, this obejct will search for other meshes before generating its own
var gizmoOffset : float = 0;						// How much to draw the gizmo offset in the Z direction
var meshName : String = "";							// Name for the generated mesh. If blank, "GeneratedMesh" will be used.
var lineList : LineList[];							// Create an array of triangles for the user to fill in

protected var meshGen : Mesh;						// Create an empty mesh to be filled with the triangle coordinates
protected var meshList : GenerateMeshStatic[];		// A list of other instances of this script
protected var hasGeneratedMesh : boolean = false;	// Whether we have generate a mesh

function Start()
	{		
	// It will error if it is not kinematic
	rigidbody.isKinematic = true;
	
	// Whether we should generate a mesh, and what mesh we should share
	var generateOwnMesh = true;
	var meshToShare : Mesh;
	
	// Get a list of all other instances of this script, and loop through
	if (findSharedMesh)
		{
		meshList = FindObjectsOfType(GenerateMeshStatic);
		for (var i = 0; i < meshList.length;i++)
			{
			// Get the script component
			var iScript = meshList[i].GetComponent(GenerateMeshStatic) as GenerateMeshStatic;
			
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
	
	// Generate temporary lists to store the verticies, triangles and normals
	var tempVerts : Vector3[] = new Vector3[lineList.length*6];
	var tempTri = new int[lineList.length*6];

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
		tempVerts[i*6] = Vector3(lineList[i].x,lineList[i].y,meshDepthHalved);
		tempVerts[i*6+1] = Vector3(lineList[i].x,lineList[i].y,-meshDepthHalved);
		tempVerts[i*6+2] = Vector3(lineList[i2].x,lineList[i2].y,-meshDepthHalved);
		
		// second: near right, far right, far left
		tempVerts[i*6+3] = Vector3(lineList[i2].x,lineList[i2].y,-meshDepthHalved);
		tempVerts[i*6+4] = Vector3(lineList[i2].x,lineList[i2].y,meshDepthHalved);
		tempVerts[i*6+5] = Vector3(lineList[i].x,lineList[i].y,meshDepthHalved);
		
		// first: mark which points make up the triangle
		tempTri[i*6] = i*6;
		tempTri[i*6+1] = i*6+1;
		tempTri[i*6+2] = i*6+2;
		
		// second: mark which points make up the triangle
		tempTri[i*6+3] = i*6+3;
		tempTri[i*6+4] = i*6+4;
		tempTri[i*6+5] = i*6+5;
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
	
	for (var i = 0; i < loopLength; i++)
		{
		var i2 = i+1;
		if (i2 == lineList.length)
			i2 = 0;
		// Draw the shape, taking into account rotation and zoffset
		Gizmos.DrawLine(Vector3(xpos+lineList[i].x*xscale,ypos+lineList[i].y*yscale,zpos),Vector3(xpos+lineList[i2].x*xscale,ypos+lineList[i2].y*yscale,zpos));
		}
	}