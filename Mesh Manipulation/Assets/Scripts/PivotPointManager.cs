using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PivotPointManager : MonoBehaviour {

    //static GameObject selectedObject;
    //static Mesh selectedObjectMesh;
    //static Vector3 selectedObjectPivot;

    /// <summary>
	/// Gather references for the selected object and its components
	///  and update the pivot vector if the object has a Mesh.
	/// </summary>
	public static void centerPivotPointOfGameObject(GameObject gameObject)
    {
        Mesh gameObjectMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
        Vector3 currentPivotPoint = findCurrentPivot(gameObjectMesh.bounds);
        changePivotToCenter(gameObject, currentPivotPoint, gameObjectMesh);
    }

    /// <summary>
	/// Calculate the pivot position by comparing its bounds center offset with its extents.
	/// The bounds may come (for instance) from mesh, renderer or collider.
	/// </summary>
	private static Vector3 findCurrentPivot(Bounds bounds)
    {
        Vector3 offset = -1 * bounds.center;
        Vector3 extent = new Vector3(offset.x / bounds.extents.x, offset.y / bounds.extents.y, offset.z / bounds.extents.z);
        return Vector3.Scale(bounds.extents, extent);
    }

    /// <summary>
	/// Moves the Object's Pivot into the Object's Center thus centering the Pivot!  \o/
	/// Few experiments shows this doesn't quite work on FBX's
	///  because it will move the Object into the Pivot instead.
	/// Either way, now we can rotate the object around its own center.
	/// </summary>
	private static void changePivotToCenter(GameObject gameObject, Vector3 currentPivotPoint, Mesh gameObjectMesh)
    {
        // Move object position by taking localScale into account
        gameObject.transform.position -= Vector3.Scale(currentPivotPoint, gameObject.transform.localScale);

        // Iterate over all vertices and move them in the opposite direction of the object position movement
        Vector3[] verts = gameObjectMesh.vertices;
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] += currentPivotPoint;
        }
        gameObjectMesh.vertices = verts; //Assign the vertex array back to the mesh
        gameObjectMesh.RecalculateBounds(); //Recalculate bounds of the mesh, for the renderer's sake
    }

    /// <summary>
	/// The 'center' parameter of certain colliders need to be adjusted when the transform position is modified.
	/// </summary>
	/*private static void fixGameObjectCollider(GameObject gameObject, Vector3 currentPivotPoint)
    {
        Collider collider = gameObject.GetComponent<Collider>();

        if (collider)
        {
            if (collider is BoxCollider)
            {
                ((BoxCollider)collider).center += currentPivotPoint;
            }
            else if (collider is CapsuleCollider)
            {
                ((CapsuleCollider)collider).center += currentPivotPoint;
            }
            else if (collider is SphereCollider)
            {
                ((SphereCollider)collider).center += currentPivotPoint;
            }
        }

        //currentPivotPoint = Vector3.zero;
    }*/
}
