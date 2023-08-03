using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using mattatz.Triangulation2DSystem;
using mattatz.Utils;
using mattatz.Triangulation2DSystem.Example;
using UnityEngine.UI;

public class ARManager : MonoBehaviour
{

    public static ARManager _instance { set; get; }

    [SerializeField] ARPlaneManager planeManager;
    [SerializeField] ARRaycastManager arRaycastManager;
    bool isPlaneDetected = false;
    bool trackPoints = false;
    bool createdPlane = false;
    [SerializeField] GameObject scanFloorUIGameobject;
    [SerializeField] GameObject pointIndicator;
    [SerializeField] Button addPointButton;
    [SerializeField] GameObject point;
    [SerializeField] Transform pointsParentTransform;

    Vector3 currentPoint;

    public List<Vector3> points = new List<Vector3>(); // List of points in the same plane
    public Material planeMaterial; // Material to assign to the plane
    private List<ARRaycastHit> hits = new List<ARRaycastHit>(); // Add this line to declare the 'hits' array.

    float timerToScan = 5;

    [SerializeField] TilesContainers tiles;


    public GameObject TilesCardAR;
    [SerializeField] Transform ItemsParent;
    [SerializeField] GameObject horScroll;


    [SerializeField] GameObject ResetButton;


    public static ARManager instance
    {
        get
        {
            // If the instance is null, try to find it in the scene
            if (_instance == null)
            {
                _instance = FindObjectOfType<ARManager>();

                // If it's still null, create a new GameObject and attach the script to it
                //if (_instance == null)
                //{
                //    GameObject singletonObject = new GameObject("AppManager");
                //    _instance = singletonObject.AddComponent<AppManager>();
                //}
            }

            return _instance;
        }

    }

    private void Awake()
    {
        planeMaterial.mainTexture = tiles.tileTextures[LoadingManager.instance.selectedTexIndex].texture;
        print("Assigned tex 1");


        SpawnTilesItemsUI();
        //List<Vector2> p = new List<Vector2>();
        //p.Clear();
        //for (int i = 0; i < points.Count; i++)
        //{
        //    p.Add(ConvertV3toV2(points[i]));
        //}
        //CreatePlane(p);

        print("Awake 1");
        Reset();


       

        


    }

    private void Update()
    {
        if (timerToScan>0)
        {
            timerToScan -= Time.deltaTime;
        }

        if (!createdPlane)
        {
            Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Ray ray = Camera.main.ScreenPointToRay(screenCenter);
            if (arRaycastManager.Raycast(ray, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
            {
                // Get the hit position and move the object there.

                currentPoint = hits[0].pose.position;


                if (Vector3.Distance(currentPoint, points[0]) < 0.06f)
                {
                    //addPointButton.interactable = false;
                    currentPoint = points[0];
                    //print("Too Close!");

                }

                pointIndicator.transform.position = currentPoint + pointIndicator.transform.up * 0.01f;

            }
        }
    }

    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        foreach (var addedPlane in args.added)
        {
            if (timerToScan <= 0)
            {
                if (scanFloorUIGameobject.activeInHierarchy)
                {
                    scanFloorUIGameobject.SetActive(false);
                    if (!isPlaneDetected)
                    {
                        //planeManager.SetTrackablesActive(false);
                        isPlaneDetected = true;
                        trackPoints = true;
                        addPointButton.gameObject.SetActive(true);


                    }
                }


            }
            if (trackPoints)
            {
                foreach (ARPlane plane in planeManager.trackables)
                {
                    plane.GetComponent<ARPlaneMeshVisualizer>().enabled = false;
                }
            }

        }

        foreach (var updatedPlane in args.updated)
        {
            if (timerToScan<=0)
            {
                if (scanFloorUIGameobject.activeInHierarchy)
                {
                    scanFloorUIGameobject.SetActive(false);
                    if (!isPlaneDetected)
                    {
                        //planeManager.SetTrackablesActive(false);
                        isPlaneDetected = true;
                        trackPoints = true;
                        addPointButton.gameObject.SetActive(true);
                        

                    }
                }

                
            }
            if (trackPoints)
            {
                foreach (ARPlane plane in planeManager.trackables)
                {
                    plane.GetComponent<ARPlaneMeshVisualizer>().enabled = false;
                }
            }
            

        }

        

        //foreach (var removedPlane in args.removed)
        //{
        //    // Plane removed, do something when a detected plane is removed (e.g., clean up instantiated objects).
        //}
    }

    public void AddPoints() {
        if (points.Count>3)
        {
            if (Vector3.Distance(points[0],currentPoint)<0.05 )
            {
                for (int i = 0; i < pointsParentTransform.childCount; i++)
                {
                    Destroy(pointsParentTransform.GetChild(i).gameObject);
                }
                print("Points Complete");
                trackPoints = false;

                List<Vector2> p = new List<Vector2>();

                for (int i = 0; i < points.Count; i++)
                {
                    p.Add(ConvertV3toV2(points[i]));
                }

                CreatePlane(p);
                return;
            }
        }

        points.Add(currentPoint);
        Instantiate(point, currentPoint, (Quaternion.Euler(90,0,0)) , pointsParentTransform);


    }


    public void Reset() {
        pointIndicator.SetActive(true);
        addPointButton.gameObject.SetActive(false);
        planeManager.planesChanged += OnPlanesChanged;
        points.Clear();
        for (int i = 0; i < pointsParentTransform.childCount; i++)
        {
            Destroy(pointsParentTransform.GetChild(0).gameObject);
        }
        foreach (ARPlane plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(false);
        }
        timerToScan = 5;
        print("Untill Time is reset");

        isPlaneDetected = false;
        trackPoints = false;
        createdPlane = false;
        if (planeObject != null)
        {
            Destroy(planeObject);

        }
        print("Untill plane destroy is reset");

        ResetButton.SetActive(false);
        horScroll.SetActive(false);
    }
    GameObject planeObject;
    void CreatePlane(List<Vector2> points)
    {

        points = setPivotToFirst(points);

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
        planeObject = new GameObject("Generated Plane");
        MeshFilter meshFilter = planeObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = planeObject.AddComponent<MeshRenderer>();

        // Set the mesh on the MeshFilter component
        meshFilter.mesh = mesh;

        // Assign a default material to the MeshRenderer component
        meshRenderer.material = planeMaterial;

        planeObject.transform.position = this.points[0];

        planeObject.transform.rotation = Quaternion.Euler(90, 0, 0);

        foreach (ARPlane plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(false);
        }
        planeManager.enabled = false;
        createdPlane = true;
        EnableTextSelect();


    }

    void EnableTextSelect() {
        pointIndicator.SetActive(false);

        horScroll.SetActive(true);
        addPointButton.gameObject.SetActive(false);
        ResetButton.SetActive(true);
    }

    Vector2 ConvertV3toV2(Vector3 vector3) {
        return new Vector2(vector3.x, vector3.z);
    }

    public List<Vector2> setPivotToFirst(List<Vector2> points)
    {
        if (points == null || points.Count == 0)
        {
            Debug.LogError("List of points is null or empty!");
            return null;
        }

        Vector2 firstPoint = points[0];
        List<Vector2> offsetPoints = new List<Vector2>();

        foreach (Vector2 point in points)
        {
            Vector2 offsetPoint = point - firstPoint;
            offsetPoints.Add(offsetPoint);
        }

        return offsetPoints;
    }

    void SpawnTilesItemsUI()
    {
        for (int i = 0; i < tiles.tileTextures.Length; i++)
        {
            GameObject go = Instantiate(TilesCardAR, ItemsParent);
            go.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = tiles.tileTextures[i];
            go.transform.GetComponent<ButtonItem>().SetButton(i,false);
        }
    }

    ButtonItem buttonItem;
    public void PressedItemButton(int i, ButtonItem _buttonItem)
    {
        
        print("Pressed " + i);
        LoadingManager.instance.selectedTexIndex = i;

        planeMaterial.mainTexture = tiles.tileTextures[LoadingManager.instance.selectedTexIndex].texture;

        if (buttonItem != null)
        {
            buttonItem.DisableSelectedUI();

        }
        buttonItem = _buttonItem;
        buttonItem.EnableSelectedUI();

        planeObject.GetComponent<MeshRenderer>().material = planeMaterial;

    }

    public void RestartLevel() {
        LoadingManager.instance.LoadSceneAsync(1);
    }

}