using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using mattatz.Triangulation2DSystem;
using mattatz.Utils;
using mattatz.Triangulation2DSystem.Example;

public class ARManager : MonoBehaviour
{
    [SerializeField] ARPlaneManager planeManager;
    bool isPlaneDetected = false;

    [SerializeField] GameObject scanFloorUIGameobject;

    public List<Vector2> points = new List<Vector2>(); // List of points in the same plane
    public Material planeMaterial; // Material to assign to the plane

    private void Awake()
    {
        CreatePlane();
    }

    void CreatePlane()
    {
        

        Polygon2D polygon = Polygon2D.Contour(points.ToArray());

        Debug.Log("Polygon Count : " + polygon.Vertices.Length);

        var vertices = polygon.Vertices;
        if (vertices.Length < 3) return; // error
        // construct Triangulation2D with Polygon2D and threshold angle (18f ~ 27f recommended)
        Triangulation2D triangulation = new Triangulation2D(polygon, 22.5f,22.5f);

        // build a mesh from triangles in a Triangulation2D instance
        Mesh mesh = triangulation.Build();
        //GetComponent<MeshFilter>().sharedMesh = mesh;
        Vector2[] uv = new Vector2[vertices.Length];
        for (int i = 0; i < uv.Length; i++)
        {
            uv[i] = vertices[i].Coordinate;
        }
        mesh.uv = uv;
        GameObject planeObject = new GameObject("Generated Plane");
        MeshFilter meshFilter = planeObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = planeObject.AddComponent<MeshRenderer>();

        // Set the mesh on the MeshFilter component
        meshFilter.mesh = mesh;

        // Assign a default material to the MeshRenderer component
        meshRenderer.material = planeMaterial;
    }

    
}