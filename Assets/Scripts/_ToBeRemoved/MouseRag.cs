/*Copyright 2022 Guillaume Spalla

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Reflection;
using System;

/**
 * Handles the rag virtual surface
 * */
public class MouseRag : MonoBehaviour
{
    public Transform m_interactionSurfaceRagView;
    
    public event EventHandler m_eventHologramInteractionSurfaceTouched;

    private void Awake()
    {
        // Get reference of the children
        m_interactionSurfaceRagView = gameObject.transform.Find("InteractionSurfaceRag");
    }

    // Start is called before the first frame update
    void Start()
    {
        

        // Connect the callbacks
        //m_interactionSurfaceRagView.GetComponent<TapToPlace>().OnPlacingStopped.AddListener(callbackHologramRagInteractionSurfaceMovedFinished);
        m_interactionSurfaceRagView.GetComponent<BoundsControl>().ScaleStopped.AddListener(callbackHologramRagInteractionSurfaceMovedFinished);
        MATCH.Utilities.Utility.AddTouchCallback(m_interactionSurfaceRagView, delegate ()
        {
            m_eventHologramInteractionSurfaceTouched?.Invoke(this, EventArgs.Empty);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void callbackHologramRagInteractionSurfaceMovedFinished()
    {
        MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Called");

        gameObject.transform.position = m_interactionSurfaceRagView.transform.position;
        m_interactionSurfaceRagView.transform.localPosition = new Vector3(0, 0f, 0);
    }
}
