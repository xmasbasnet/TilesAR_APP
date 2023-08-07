using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using mattatz.Triangulation2DSystem;
using mattatz.Utils;
using mattatz.Triangulation2DSystem.Example;
using UnityEngine.UI;
using TMPro;


public class ARManager : MonoBehaviour
{

    public static ARManager _instance { set; get; }

    [SerializeField] ARPlaneManager planeManager;
    [SerializeField] ARRaycastManager arRaycastManager;
    [SerializeField] Camera arCamera;

    bool isPlaneDetected = false;
    //bool trackPoints = false;
    bool createdPlane = false;
    [SerializeField] GameObject scanFloorUIGameobject;
    [SerializeField] GameObject pointIndicator;
    [SerializeField] Button addPointButton;
    [SerializeField] GameObject point;
    [SerializeField] Transform pointsParentTransform;

    Vector3 currentPoint;

    public List<Vector3> points = new List<Vector3>(); // List of points in the same plane
    public Material planeMaterial; // Material to assign to the plane

    float timerToScan = 5;

    [SerializeField] TilesContainers tiles;


    public GameObject TilesCardAR;
    [SerializeField] Transform ItemsParent;
    [SerializeField] GameObject horScroll;


    [SerializeField] GameObject ResetButton;

    [SerializeField] GameObject ScanSelection;
    [SerializeField] TextMeshProUGUI scanTxt;

    bool isScanFloor = true;

    public static ARManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ARManager>();

            }

            return _instance;
        }

    }

    private void Awake()
    {
        
        planeMaterial.mainTexture = tiles.tileTextures[LoadingManager.instance.selectedTexIndex].texture;
        planeManager.enabled = false;
        ScanSelection.SetActive(true);
        scanFloorUIGameobject.SetActive(false);

        SpawnTilesItemsUI();
       
        Reset();

        

    }


    bool startAddingPoints = false;

    private void Update()
    {
        if (isPlaneDetected)
        {
            if (timerToScan > 0)
            {
                timerToScan -= Time.deltaTime;
            }
            else {
                if (!startAddingPoints)
                {
                    startAddingPoints = true;
                    StartAddingPoints();
                }
                
                
            }
        }
       

        RayToPlane();


    }

    bool startRaycast = false;
    void RayToPlane() {
        if (!startRaycast)
        {
            return;
        }
        //Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Ray ray = arCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if (arRaycastManager.Raycast(ray, hits, TrackableType.Planes))
        {
            // Get the hit position and move the object there.
            currentPoint = Vector3.Lerp(currentPoint, hits[0].pose.position, 0.5f);

            pointIndicator.transform.position = currentPoint;

        }
    }

    void StartAddingPoints() {
        startRaycast = true;
        addPointButton.gameObject.SetActive(true);
        pointIndicator.SetActive(true);
        pointIndicator.transform.rotation = Quaternion.Euler(isScanFloor? 90:0, 0, 0);
        DisablePlaneVisuals();
    }

    public void DisablePlaneVisuals()
    {
        foreach (var plane in planeManager.trackables)
        {
            plane.GetComponent<ARPlaneMeshVisualizer>().enabled = false;
        }
    }

    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (args.added.Count>0)
        {
            if (!isPlaneDetected)
            {
                isPlaneDetected = true;
                scanFloorUIGameobject.SetActive(false);
                
            }
           
        }

    }

    public void AddPoints() {
        points.Add(currentPoint);
        Instantiate(point, currentPoint, Quaternion.Euler(isScanFloor ? 90:0, 0, 0), pointsParentTransform);

        if (points.Count>3)
        {

            if (IsShapeClosed())
            {
                print("Points Complete, Shape is closed!");

                RemovePointDots();

                List<Vector2> p = new List<Vector2>();

                for (int i = 0; i < points.Count; i++)
                {
                    if (isScanFloor)
                    {
                        p.Add(ConvertV3toV2(points[i]));

                    }
                    else {
                        p.Add(new Vector2(points[i].x, points[i].y));
                    }
                }

                CreatePlane(p);
                return;
            }

        }


    }
    float closeShapeThreshold = 0.05f;
    private bool IsShapeClosed()
    {
        // Check if the number of points is sufficient to form a closed shape.
        if (points.Count < 3)
            return false;

        // Get the first and last point in the list.
        Vector2 firstPoint = points[0];
        Vector2 lastPoint = points[points.Count - 1];

        // Calculate the distance between the first and last point.
        float distance = Vector2.Distance(firstPoint, lastPoint);

        // Check if the distance is within the threshold to consider the shape as closed.
        return distance < closeShapeThreshold;
    }

    void RemovePointDots() {
        for (int i = 0; i < pointsParentTransform.childCount; i++)
        {
            Destroy(pointsParentTransform.GetChild(i).gameObject);
        }
    }


    public void Reset() {
        pointIndicator.SetActive(false);
        addPointButton.gameObject.SetActive(false);
        planeManager.planesChanged += OnPlanesChanged;
        points.Clear();
        RemovePointDots();
        
        timerToScan = 5;
        print("Untill Time is reset");

        isPlaneDetected = false;
        createdPlane = false;
        if (planeObject != null)
        {
            Destroy(planeObject);

        }
        print("Untill plane destroy is reset");

        ResetButton.SetActive(false);
        horScroll.SetActive(false);
    }

    #region Plane Mesh Creation
    GameObject planeObject;
    void CreatePlane(List<Vector2> points)
    {
        DisablePlaneVisuals();

        if (isScanFloor)
        {
            points = setPivotToFirst(points);

        }

        Polygon2D polygon = Polygon2D.Contour(points.ToArray());


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

        

        if (isScanFloor)
        {
            planeObject.transform.position = this.points[0];
            planeObject.transform.rotation = Quaternion.Euler(90, 0, 0);

        }

        foreach (ARPlane plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(false);
        }
        planeManager.enabled = false;
        createdPlane = true;
        EnableHorizontalTextureSelectionUI();


    }

    void EnableHorizontalTextureSelectionUI() {
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

    #endregion

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

    public void ScanWall() {
        planeManager.requestedDetectionMode = PlaneDetectionMode.Vertical;
        isScanFloor = false;

        scanTxt.text = "Scan Wall";
        startPlaneDetection();

    }

    public void ScanFloor() {
        planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
        isScanFloor = true;
        scanTxt.text = "Scan Floor";

        startPlaneDetection();
    }

    void startPlaneDetection() {
        planeManager.enabled = true;

        scanFloorUIGameobject.SetActive(true);
        ScanSelection.SetActive(false);

    }

}