using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using Microsoft.MixedReality.Toolkit.Experimental.SpatialAwareness;
using System.Reflection;
using System;

public class MouseUtilitiesPathFinding : MonoBehaviour
{
    /*public Transform m_origin;
    public Transform m_target;*/
    private NavMeshPath m_path;
    //private float m_elapsed = 0.0f;


    NavMeshData m_NavMesh;
    AsyncOperation m_Operation;
    NavMeshDataInstance m_Instance;
    List<NavMeshBuildSource> m_Sources = new List<NavMeshBuildSource>();

    /*Transform m_cube1;
    Transform m_cube2;
    Transform m_obstacle1;
    Transform m_obstacle2;*/

    Transform m_interactionSurfaceView;
    MouseInteractionSurface m_interactionSurfaceController;

    MouseUtilitiesPathFindingObstacles m_obstaclesManager;

    //Transform m_plane;

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    /*void Start()
    {
        m_path = new NavMeshPath();
        m_elapsed = 0.0f;

        //NavMeshBuilder.CollectSources()
    }*/
    IEnumerator Start()
    {
        // Get children and components
        //m_cube1 = transform.Find("Cube1");
        //m_cube2 = transform.Find("Cube2");
        m_interactionSurfaceView = transform.Find("AssistanceInteractionSurface");
        m_interactionSurfaceController = m_interactionSurfaceView.GetComponent<MouseInteractionSurface>();
        //m_obstacle1 = transform.Find("Obstacles").Find("Obstacle1");
        //m_obstacle2 = transform.Find("Obstacles").Find("Obstacle2");
        m_obstaclesManager = GetComponent<MouseUtilitiesPathFindingObstacles>();

        // Set admin buttons
        //MouseUtilitiesAdminMenu.Instance.addButton("Compute path", callbackFindPath);
        //MouseUtilitiesAdminMenu.Instance.addButton("Bring cube 1", callbackBringCube1);
        //MouseUtilitiesAdminMenu.Instance.addButton("Bring cube 2", callbackBringCube2);
        /*MouseUtilitiesAdminMenu.Instance.addButton("Bring obstacle 1", callbackBringObstacle1);
        MouseUtilitiesAdminMenu.Instance.addButton("Bring obstacle 2", callbackBringObstacle2);
        MouseUtilitiesAdminMenu.Instance.addSwitchButton("Hide obstacle 1", callbackHideShowObstacle1);
        MouseUtilitiesAdminMenu.Instance.addSwitchButton("Hide obstacle 2", callbackHideShowObstacle2);*/
        MouseUtilitiesAdminMenu.Instance.addButton("Add obstacle", callbackAddObstacle, MouseUtilitiesAdminMenu.Panels.Obstacles);
        m_interactionSurfaceController.setAdminButtons("path surface");
        m_interactionSurfaceController.setColor("Mouse_Cyan_Glowing");
        /*m_interactionSurfaceController.getInteractionSurface().localPosition*/ m_interactionSurfaceView.position = new Vector3(0.3f,-0.16f,-5f);
        m_interactionSurfaceController.setScaling(new Vector3(0.1f, 1f, 0.1f));
        m_interactionSurfaceController.showInteractionSurfaceTable(true);
        m_interactionSurfaceController.setObjectResizable(true);
        m_interactionSurfaceController.setPreventResizeY(true);

        // Add component to the interaction surface to define walkable surface
        m_interactionSurfaceController.getInteractionSurface().gameObject.AddComponent<NavMeshSourceTag>();

        

        // Nav mesh computation
        while (true)
        {
            UpdateNavMesh(true);
            yield return m_Operation;
        }
    }

    void OnEnable()
    {
        m_interactionSurfaceView = transform.Find("AssistanceInteractionSurface");
        m_interactionSurfaceController = m_interactionSurfaceView.GetComponent<MouseInteractionSurface>();

        // Construct and add navmesh
        m_NavMesh = new NavMeshData();
        m_Instance = NavMesh.AddNavMeshData(m_NavMesh);
        /*if (m_Tracked == null)
            m_Tracked = transform;*/
        UpdateNavMesh(false);
    }

    void OnDisable()
    {
        // Unload navmesh and clear handle
        m_Instance.Remove();
    }

    void UpdateNavMesh(bool asyncUpdate = false)
    {
        NavMeshSourceTag.Collect(ref m_Sources);
        var defaultBuildSettings = NavMesh.GetSettingsByID(0);
        var bounds = QuantizedBounds();

        if (asyncUpdate)
            m_Operation = NavMeshBuilder.UpdateNavMeshDataAsync(m_NavMesh, defaultBuildSettings, m_Sources, bounds);
        else
            NavMeshBuilder.UpdateNavMeshData(m_NavMesh, defaultBuildSettings, m_Sources, bounds);
    }

    /*void callbackFindPath()
    {
        MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Number of registered meshes: " + NavMesh.GetSettingsCount());

        Vector3 target = new Vector3(m_target.position.x, m_target.position.y, m_target.position.z);

        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(m_origin.position, target, NavMesh.AllAreas, path) == false)
        {
            MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Warning, "Cannot compute the path");
        }
        MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Number of corner: " + path.corners.Length + " Start position: " + m_origin.position + " Target position: " + target);

        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = path.corners.Length;

        for (int i = 0; i < path.corners.Length; i++)
        {
            Vector3 corner = path.corners[i];

            lineRenderer.SetPosition(i, corner);

            MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Corner start: " + corner);
        }
    }*/

    public Vector3[] computePath(Transform origin, Transform destination)
    {
        MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Number of registered meshes: " + NavMesh.GetSettingsCount());

        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(origin.position, destination.position, NavMesh.AllAreas, path) == false)
        {
            MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Warning, "Cannot compute the path");
        }

        MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Number of corner: " + path.corners.Length + " Start position: " + origin.position + " Target position: " + destination.position);

        return path.corners;
    }

    /*static Vector3 Quantize(Vector3 v, Vector3 quant)
    {
        float x = quant.x * Mathf.Floor(v.x / quant.x);
        float y = quant.y * Mathf.Floor(v.y / quant.y);
        float z = quant.z * Mathf.Floor(v.z / quant.z);
        return new Vector3(x, y, z);
    }*/

    Bounds QuantizedBounds()
    {
        Vector3 center = new Vector3(0, 0, 0);

        if (m_interactionSurfaceController.getInteractionSurface() != null)
        {
            center = m_interactionSurfaceController.getInteractionSurface().position; //new Vector3(1.0f, 0, 2.94f)
            //MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Center: " + center);
            
        }
        else
        {
            //MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Warning, "Interaction surface object not yet initialized - center initialized to (0,0,0)");
        }

        
        Vector3 size = new Vector3(20f, 20f, 20f);

        // Quantize the bounds to update only when theres a 10% change in size
        //var center = m_Tracked ? m_Tracked.position : transform.position;
        return new Bounds(center, size);
    }

    // Update is called once per frame
    void Update()
    {
        /*m_elapsed += Time.deltaTime;
        if(m_elapsed > 1.0f)
        {
            m_elapsed -= 1.0f;
            NavMesh.CalculatePath(m_origin.position, m_target.position, NavMesh.AllAreas, m_path);

            MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Update navmesh called. Number of points in the path: " + m_path.corners.Length);
        }
        for (int i = 0; i < m_path.corners.Length - 1; i ++)
        {
            MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Line drew between : " + m_path.corners[i] + " and " + m_path.corners[i + 1]);
            Debug.DrawLine(m_path.corners[i], m_path.corners[i + 1], Color.red);
        }*/
    }

    /*void callbackBringCube1()
    {
        m_cube1.transform.position = new Vector3(Camera.main.transform.position.x + 0.5f, Camera.main.transform.position.y, Camera.main.transform.position.z);
        m_cube1.transform.LookAt(Camera.main.transform);
        m_cube1.transform.Rotate(new Vector3(0, 1, 0), 180);
    }

    void callbackBringCube2()
    {
        m_cube2.transform.position = new Vector3(Camera.main.transform.position.x + 0.5f, Camera.main.transform.position.y, Camera.main.transform.position.z);
        m_cube2.transform.LookAt(Camera.main.transform);
        m_cube2.transform.Rotate(new Vector3(0, 1, 0), 180);
    }*/

    /*void callbackBringObstacle1()
    {
        MouseUtilities.bringObject(m_obstacle1);
    }

    void callbackBringObstacle2()
    {
        MouseUtilities.bringObject(m_obstacle2);
    }

    void callbackHideShowObstacle1()
    {
        MouseUtilities.showInteractionSurface(m_obstacle1, !m_obstacle1.GetComponent<Renderer>().enabled);
    }

    void callbackHideShowObstacle2()
    {
        MouseUtilities.showInteractionSurface(m_obstacle2, !m_obstacle2.GetComponent<Renderer>().enabled);
    }*/

    void callbackAddObstacle()
    {
        m_obstaclesManager.addCube("Obstacle " + (m_obstaclesManager.getCubes().Count + 1).ToString(), new Vector3(0.1f, 0.1f, 0.1f), new Vector3(Camera.main.transform.position.x + 0.5f, Camera.main.transform.position.y, Camera.main.transform.position.z), "Mouse_White_Transparent", true, false, transform);
    }
}