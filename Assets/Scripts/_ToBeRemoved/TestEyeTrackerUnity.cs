using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using Microsoft.MixedReality.Toolkit.Input;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;
using System.Reflection;
using System.Linq;

public class TestEyeTrackerUnity : MonoBehaviour
{
    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        EyeTrackingTarget eyeTrackingTarget = gameObject.GetComponent<EyeTrackingTarget>();
        Renderer renderer = gameObject.GetComponent<Renderer>();

        eyeTrackingTarget.OnLookAtStart.AddListener(delegate 
        {
            MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Object focused on");

            renderer.material = MATCH.Utilities.Utility.LoadMaterial(MATCH.Utilities.Materials.Colors.CyanGlowing);
        });

        eyeTrackingTarget.OnLookAway.AddListener(delegate 
        {
            MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Object focused off");

            renderer.material = MATCH.Utilities.Utility.LoadMaterial(MATCH.Utilities.Materials.Colors.OrangeGlowing);
        });
    }



    // Update is called once per frame
    void Update()
    {
        
    }


}
