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

public class MouseUtilitiesSpatialAwareness : MonoBehaviour
{
    public SurfaceMeshesToPlanes ScenePlanes;

    DateTime Time;
    DateTime TimeOneMinuteTrigger;

    // Start is called before the first frame update
    void Start()
    {
        //m_scenePlanes = new SurfaceMeshesToPlanes();
        if (SurfaceMeshesToPlanes.CanCreatePlanes)
        {
            MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Things are ok for the system to create planes");
        }
        else
        {
            MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Warning, "Components are missing to create planes - see the documentation of SurfaceMeshesToPlanes.CanCreatePlanes for more information on the required packages");
        }

        

        TimeOneMinuteTrigger = DateTime.Now;
    }

    // Update is called once per frame
    void Update()
    {
        /*TimeSpan elapsed = DateTime.Now.Subtract(m_timeOneMinuteTrigger);
        if (elapsed.Seconds >= 10)
        {
            m_timeOneMinuteTrigger = DateTime.Now;

            m_scenePlanes.MakePlanes();

            List<GameObject> walls = m_scenePlanes.GetActivePlanes(Microsoft.MixedReality.Toolkit.SpatialAwareness.SpatialAwarenessSurfaceTypes.Wall);
            List<GameObject> floors = m_scenePlanes.GetActivePlanes(Microsoft.MixedReality.Toolkit.SpatialAwareness.SpatialAwarenessSurfaceTypes.Floor);
            List<GameObject> ceilings = m_scenePlanes.GetActivePlanes(Microsoft.MixedReality.Toolkit.SpatialAwareness.SpatialAwarenessSurfaceTypes.Ceiling);

            Renderer r;

            foreach ( GameObject w in walls )
            {
                r = w.GetComponent<Renderer>();
                r.material = Resources.Load(m_wallsColor, typeof(Material)) as Material;
                if (w.GetComponent<NavMeshSourceTag>() == null)
                {
                    w.AddComponent<NavMeshSourceTag>();
                }
            }

            foreach(GameObject f in floors)
            {
                r = f.GetComponent<Renderer>();
                r.material = Resources.Load(m_floorsColor, typeof(Material)) as Material;
                if (f.GetComponent<NavMeshSourceTag>() == null)
                {
                    f.AddComponent<NavMeshSourceTag>();
                }
            }

            foreach (GameObject c in ceilings)
            {
                r = c.GetComponent<Renderer>();
                r.material = Resources.Load(m_floorsColor, typeof(Material)) as Material;
                if (c.GetComponent<NavMeshSourceTag>() == null)
                {
                    c.AddComponent<NavMeshSourceTag>();
                }
            }

            MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Places computed - Number of elements detected - Walls: " + walls.Count + " Floors: " + floors.Count + " Ceilings: " + ceilings.Count);

        }*/

    }
}
