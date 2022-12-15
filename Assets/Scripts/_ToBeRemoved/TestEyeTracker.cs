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

public class TestEyeTracker : MonoBehaviour
{
    MATCH.Utilities.HologramInteractions InteractionMATCH;
    MATCH.Inferences.Manager InferencesManager;

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        InferencesManager = MATCH.Inferences.Factory.Instance.CreateManager(transform);

        InferencesManager.OnStartFinished += InitializeInferences;
    }

    void InitializeInferences(System.Object o, EventArgs e)
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();

        MATCH.Inferences.ObjectFocused objectFocused = new MATCH.Inferences.ObjectFocused("test", delegate (System.Object o, EventArgs e)
        {
            MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Object focused on");

            renderer.material = MATCH.Utilities.Utility.LoadMaterial(MATCH.Utilities.Materials.Colors.PurpleGlowing);
        }, gameObject, 3);

        InferencesManager.RegisterInference(objectFocused);

        InteractionMATCH = gameObject.AddComponent<MATCH.Utilities.HologramInteractions>();
        InteractionMATCH.EyeFocusOn += delegate (System.Object o, EventArgs e)
        {
            MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Object focused on");

            renderer.material = MATCH.Utilities.Utility.LoadMaterial(MATCH.Utilities.Materials.Colors.CyanGlowing);
        };

        InteractionMATCH.EyeFocusOff += delegate (System.Object o, EventArgs e)
        {
            MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Object focused off");

            renderer.material = MATCH.Utilities.Utility.LoadMaterial(MATCH.Utilities.Materials.Colors.OrangeGlowing);
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
