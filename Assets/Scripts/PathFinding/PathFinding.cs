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

//Todo: convert this class to a singleton, as there is no reason to have multiple instances of this
namespace MATCH
{
    namespace PathFinding
    {
        public class PathFinding : MonoBehaviour
        {
            private NavMeshPath Path;

            NavMeshData NavMesh;
            AsyncOperation Operation;
            NavMeshDataInstance Instance;
            List<NavMeshBuildSource> Sources = new List<NavMeshBuildSource>();

            //Transform InteractionSurfaceView;
            Assistances.InteractionSurface InteractionSurfaceController;

            Obstacles ObstaclesManager;

            private void Awake()
            {
                InteractionSurfaceController = null;
            }

            // Start is called before the first frame update
            IEnumerator Start()
            {
                // Get children and components
                /*InteractionSurfaceView = transform.Find("AssistanceInteractionSurface");
                InteractionSurfaceView.name = "PathInteractionSurface";
                InteractionSurfaceController = InteractionSurfaceView.GetComponent<Assistances.InteractionSurface>();
                
                InteractionSurfaceView.position = new Vector3(0.3f, -0.16f, -5f);
                InteractionSurfaceController.SetScaling(new Vector3(0.1f, 1f, 0.1f));
                InteractionSurfaceController.ShowInteractionSurfaceTable(true);
                InteractionSurfaceController.SetObjectResizable(true);
                InteractionSurfaceController.SetPreventResizeY(true);
                Utilities.WorldLockingToolsManager.Instance.RegisterObject(InteractionSurfaceView.name, InteractionSurfaceView, InteractionSurfaceController.GetInteractionSurface());*/
                ObstaclesManager = GetComponent<Obstacles>();
                InteractionSurfaceController = Assistances.Factory.Instance.CreateInteractionSurface("PathInteractionSurface", AdminMenu.Panels.Left, new Vector3(0.1f, 0.01f, 0.1f), new Vector3(0.3f, -0.16f, -5f), Utilities.Materials.Colors.CyanAdjustHSV, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);
                InteractionSurfaceController.SetPreventResizeY(true);

                // Set admin buttons
                AdminMenu.Instance.AddButton("Add obstacle", CallbackAddObstacle, AdminMenu.Panels.Left);
                InteractionSurfaceController.SetAdminButtons("path surface");
                InteractionSurfaceController.SetColor(Utilities.Materials.Colors.CyanGlowing);
                

                // Add component to the interaction surface to define walkable surface
                InteractionSurfaceController.GetInteractionSurface().gameObject.AddComponent<NavMeshSourceTag>();

                // Add callbacks
                ObstaclesManager.EventMoved += CallbackObstacleResizedOrMoved;
                ObstaclesManager.EventResized += CallbackObstacleResizedOrMoved;

                // Nav mesh computation
                while (true)
                {
                    UpdateNavMesh(true);
                    yield return Operation;
                }
            }

            void OnEnable()
            {
                /*InteractionSurfaceView = transform.Find("AssistanceInteractionSurface");
                InteractionSurfaceController = InteractionSurfaceView.GetComponent<Assistances.InteractionSurface>();*/

                // Construct and add navmesh
                NavMesh = new NavMeshData();
                Instance = UnityEngine.AI.NavMesh.AddNavMeshData(NavMesh);
                /*if (m_Tracked == null)
                    m_Tracked = transform;*/
                UpdateNavMesh(false);
            }

            void OnDisable()
            {
                // Unload navmesh and clear handle
                Instance.Remove();
            }

            void UpdateNavMesh(bool asyncUpdate = false)
            {
                NavMeshSourceTag.Collect(ref Sources);
                var defaultBuildSettings = UnityEngine.AI.NavMesh.GetSettingsByID(0);
                var bounds = QuantizedBounds();

                if (asyncUpdate)
                    Operation = NavMeshBuilder.UpdateNavMeshDataAsync(NavMesh, defaultBuildSettings, Sources, bounds);
                else
                    NavMeshBuilder.UpdateNavMeshData(NavMesh, defaultBuildSettings, Sources, bounds);
            }

            public Vector3[] ComputePath(Vector3 origin, Vector3 destination)
            {
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Number of registered meshes: " + UnityEngine.AI.NavMesh.GetSettingsCount());

                NavMeshPath path = new NavMeshPath();
                if (UnityEngine.AI.NavMesh.CalculatePath(origin, destination, UnityEngine.AI.NavMesh.AllAreas, path) == false)
                {
                    //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Cannot compute the path");
                }

                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Number of corner: " + path.corners.Length + " Start position: " + origin+ " Target position: " + destination);

                return path.corners;
            }

            public Vector3[] ComputePath(Transform origin, Transform destination)
            {
                return ComputePath(origin.position, destination.position);
            }

            Bounds QuantizedBounds()
            {
                Vector3 center = new Vector3(0, 0, 0);

                if (InteractionSurfaceController != null && InteractionSurfaceController.GetInteractionSurface() != null)
                {
                    center = InteractionSurfaceController.GetInteractionSurface().position; //new Vector3(1.0f, 0, 2.94f)
                                                                                              //MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Center: " + center);

                }
                else
                {
                    //MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Warning, "Interaction surface object not yet initialized - center initialized to (0,0,0)");
                }


                Vector3 size = new Vector3(20f, 20f, 20f);

                // Quantize the bounds to update only when theres a 10% change in size
                return new Bounds(center, size);
            }

            // Update is called once per frame
            void Update()
            {

            }

            void CallbackAddObstacle()
            {
                Vector3 position = new Vector3(Camera.main.transform.position.x + 0.5f, Camera.main.transform.position.y, Camera.main.transform.position.z);

                Vector3 scaling = new Vector3(0.1f, 0.1f, 0.1f);

                ObstaclesManager.AddObstacle("Obstacle " + (ObstaclesManager.GetObstacles().Count + 1).ToString(), scaling, position, Utilities.Materials.Colors.WhiteTransparent, true, false, true, transform);
            }

            void CallbackObstacleResizedOrMoved(System.Object sender, EventArgs e)
            {
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called");

                GameObject cube = (GameObject)sender;
                Transform interactionSurface = InteractionSurfaceController.GetInteractionSurface();

                float max = cube.transform.position.y + cube.transform.localScale.y / 2.0f;
                float newPosY = (max - interactionSurface.position.y) / 2.0f + interactionSurface.position.y;
                float newScalingY = (max - interactionSurface.position.y);

                cube.transform.localScale = new Vector3(cube.transform.localScale.x, newScalingY, cube.transform.localScale.z);
                cube.transform.position = new Vector3(cube.transform.position.x, newPosY, cube.transform.position.z);

            }
        }

    }
}

