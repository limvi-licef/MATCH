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

using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;
using System.Reflection;
using System.Linq;

/**
 * Assistance to who a cube, that has 3 gradation level: fix with discrete colors, vivid colors, and that follows the user
 * */
public class MouseAssistanceStimulateLevel1 : MonoBehaviour
{
    public MouseDebugMessagesManager m_debug;

    public Transform m_hologramView;
    MouseCubeOpening m_hologramController;
    Vector3 m_hologramOriginalLocalPos;
    Transform m_lightView;
    MouseUtilitiesLight m_lightController;
    MouseUtilitiesGradationManager m_gradationManager;

    public EventHandler m_eventHologramStimulateLevel1Gradation1Or2Touched;

    public AudioClip m_audioClipToPlayOnTouchInteractionSurface;
    public AudioListener m_audioListener;

    public bool m_hasFocus;

    private void Awake()
    {
        // Variables
        m_hasFocus = false;

        // Children
        m_hologramView = transform.Find("CubeOpening");
        m_hologramController = m_hologramView.GetComponent<MouseCubeOpening>();
        m_hologramOriginalLocalPos = m_hologramView.localPosition;

        m_lightView = transform.Find("Light");
        m_lightController = m_lightView.GetComponent<MouseUtilitiesLight>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Setting up the gradation manger
        m_gradationManager = transform.GetComponent<MouseUtilitiesGradationManager>();
        m_gradationManager.addNewAssistanceGradation("Default", callbackGradationDefault);
        m_gradationManager.addNewAssistanceGradation("LowVivid", callbackGradationLowVivid);
        m_gradationManager.addNewAssistanceGradation("HighFollow", callbackGradationHighFollow);

        // Callbacks
        m_hologramController.m_eventCubetouched += new EventHandler(delegate (System.Object o, EventArgs e)
        {
            m_eventHologramStimulateLevel1Gradation1Or2Touched?.Invoke(this, EventArgs.Empty);

        });
    }

    public bool hasFocus()
    {
        return m_hasFocus;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    bool m_mutexShow = false;
    public void show(EventHandler eventHandler)
    {
        if (m_mutexShow == false)
        {
            m_mutexShow = true;

            if (m_hologramView.gameObject.activeSelf == false)
            {
                MouseUtilitiesAnimation animator = m_hologramView.gameObject.AddComponent<MouseUtilitiesAnimation>();

                MouseUtilities.adjustObjectHeightToHeadHeight(m_debug, transform, m_hologramOriginalLocalPos.y);

                EventHandler[] eventHandlers = new EventHandler[] { new EventHandler(delegate (System.Object o, EventArgs e)
                    {
                        Destroy(animator);
                        m_hologramController.backupScaling();
                        m_mutexShow = false;

                        m_debug.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "(animation finished) Scale: " + m_hologramView.transform.localScale.ToString() + " Local position: " + m_hologramView.transform.localPosition.ToString());
                    }), eventHandler };

                animator.animateAppearInPlaceToScaling(m_hologramView.transform.localScale, m_debug, eventHandlers);
            }
            else
            {
                m_debug.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Warning, "Assistance stimulate level 1 is enabled - no hide action to take");

                m_mutexShow = false;
            }
        }
        else
        {
            m_debug.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Warning, "Mutex locked - Request ignored");
        }   
    }

    public void setPositionToOriginalLocation()
    {
        m_hologramView.localPosition = m_hologramOriginalLocalPos;
    }

    public void hide(EventHandler eventHandler)
    {
        if (m_hologramView.gameObject.activeSelf)
        {
            m_debug.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Cube is going to be hidden");

            EventHandler[] eventHandlers = new EventHandler[] { new EventHandler(delegate (System.Object o, EventArgs e)
        {
                   

            m_hologramView.gameObject.SetActive(false);
            m_hologramView.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            Destroy(m_hologramView.GetComponent<MouseUtilitiesAnimation>());

            m_debug.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Cube should be hidden now. New local position:" + m_hologramView.localPosition.ToString());
        }), eventHandler };

            m_hologramController.closeCube(new EventHandler(delegate (System.Object o, EventArgs e)
            {
                m_hologramView.gameObject.AddComponent<MouseUtilitiesAnimation>().animateDiseappearInPlace(m_debug, eventHandlers);
            }));
            m_lightView.gameObject.SetActive(false);
        }
        else
        {
            m_debug.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Warning, "Assistance stimulate level 1 is disabled - no hide action to take");
        }
    }

    public bool increaseGradation()
    {
        return m_gradationManager.increaseGradation();
    }

    public bool decreaseGradation()
    {
        return m_gradationManager.decreaseGradation();
    }

    public void setGradationToMinimum()
    {
        m_gradationManager.setGradationToMinimum();
    }

    void callbackGradationDefault(System.Object o, EventArgs e)
    {
        m_hologramController.updateMaterials("Mouse_Clean_Bottom", "Mouse_Clean_Top-Left", "Mouse_Clean_Top-Right");
        m_lightView.gameObject.SetActive(false);
        m_hologramController.GetComponent<Billboard>().enabled = true;
        GetComponent<RadialView>().enabled = false;
        m_hologramController.setScalingToOriginal();

        m_debug.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Showing default gradation");
    }

    void callbackGradationLowVivid(System.Object o, EventArgs e)
    {
        m_hologramController.updateMaterials("Mouse_Clean_Bottom_Vivid", "Mouse_Clean_Top-Left_Vivid", "Mouse_Clean_Top-Right_Vivid");
        m_lightView.gameObject.SetActive(true);
        m_hologramController.GetComponent<Billboard>().enabled = true;
        GetComponent<RadialView>().enabled = false;
        m_hologramController.setScalingToOriginal(); // setting back the scaling to the original one.

        m_debug.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Showing low vivid gradation");
    }

    void callbackGradationHighFollow(System.Object o, EventArgs e)
    {
        // Add an offset in y to the cube to avoid that they remain too much "in front" of the user
        GetComponent<RadialView>().enabled = true;
        m_hologramView.gameObject.GetComponent<Billboard>().enabled = false; // The billboard is present on the parent object so that the cube can be shifted to avoid that it is too much in front of the user.
        m_lightView.gameObject.SetActive(false);

        // reducing the scale of the cube to have it less intrusive
        m_hologramController.setScalingReduced();

        m_debug.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Showing high gradation");
    }

    public void openingCube(EventHandler eventHandler)
    {
        m_hologramController.openCube(eventHandler);
    }
}