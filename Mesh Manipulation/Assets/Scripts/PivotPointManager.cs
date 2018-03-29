using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PivotPointManager : MonoBehaviour {

    /// <summary>
	/// Gather references for the selected object and its components
	///  and update the pivot vector if the object has a Mesh.
	/// </summary>
	public static void centerPivotPointOfGameObject(GameObject gameObject)
    {
        Mesh gameObjectMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
        Vector3 centerPoint = findCenterPoint(gameObjectMesh.vertices);
        changePivotToCenter(gameObject, centerPoint, gameObjectMesh);
    }   

    private static Vector3 findCenterPoint(Vector3[] vertices) {
        Vector3 center = Vector3.zero;
        float count = 0;

        foreach (Vector3 vertex in vertices)
        {
            center += vertex;
            count++;
        }

        return -(center / count);
    }

    /// <summary>
	/// Moves the Object's Pivot into the Object's Center thus centering the Pivot!  \o/
	/// Few experiments shows this doesn't quite work on FBX's
	///  because it will move the Object into the Pivot instead.
	/// Either way, now we can rotate the object around its own center.
	/// </summary>
	private static void changePivotToCenter(GameObject gameObject, Vector3 centerPoint, Mesh gameObjectMesh/*, Vector3 worldRotation*/)
    {
        // Move object position by taking localScale into account
        gameObject.transform.position -= Quaternion.Euler(gameObject.transform.root.eulerAngles) * Vector3.Scale(centerPoint, gameObject.transform.localScale);

        // Iterate over all vertices and move them in the opposite direction of the object position movement
        Vector3[] verts = gameObjectMesh.vertices;
        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] += centerPoint;
        }
        gameObjectMesh.vertices = verts; //Assign the vertex array back to the mesh
        gameObjectMesh.RecalculateBounds(); //Recalculate bounds of the mesh, for the renderer's sake
        gameObjectMesh.RecalculateNormals();

        centerPoint = Vector3.zero;
    }  
}
