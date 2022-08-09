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
namespace MATCH
{
    namespace Assistances
    {
        public class CubeWithVisualGradation : Assistance
        {
            public Transform m_hologramView;
            CubeOpening m_hologramController;
            Vector3 m_hologramOriginalLocalPos;

            Transform m_lightView;
            //Light m_lightController;

            FiniteStateMachine.VisualGradationManager m_gradationManager;

            public Transform m_surfaceWithStarsView;
            public Transform m_surfaceWithStarsViewTarget; // If provided, allows to adjust the size of the stars to the size of this surface

            public Transform m_help;

            public EventHandler m_eventHologramStimulateLevel1Gradation1Or2Touched;

            public AudioClip m_audioClipToPlayOnTouchInteractionSurface;
            public AudioListener m_audioListener;

            public bool m_hasFocus;

            string m_materialBottomDefault;
            string m_materialTopLeftDefault;
            string m_materialTopRightDefault;

            string m_materialBottomVivid;
            string m_materialTopLeftVivid;
            string m_materialTopRightVivid;

            private void Awake()
            {
                // Variables
                m_hasFocus = false;

                // Children
                m_hologramView = transform.Find("CubeOpening");
                m_hologramController = m_hologramView.GetComponent<CubeOpening>();
                m_hologramOriginalLocalPos = m_hologramView.localPosition;

                m_lightView = transform.Find("Light");
                //m_lightController = m_lightView.GetComponent<Light>();

                m_surfaceWithStarsView = transform.Find("SurfaceWithStars");

                m_help = transform.Find("Help");

                // Default materials
                m_materialBottomDefault = "Mouse_Cyan_Glowing";
                m_materialTopLeftDefault = "Mouse_Cyan_Glowing";
                m_materialTopRightDefault = "Mouse_Cyan_Glowing";
                m_materialBottomVivid = "Mouse_Orange_Glowing";
                m_materialTopLeftVivid = "Mouse_Orange_Glowing";
                m_materialTopRightVivid = "Mouse_Orange_Glowing";
            }

            // Start is called before the first frame update
            void Start()
            {
                // Setting up the gradation manger
                m_gradationManager = transform.GetComponent<FiniteStateMachine.VisualGradationManager>();
                m_gradationManager.addNewAssistanceGradation("Default", CallbackGradationDefault);
                m_gradationManager.addNewAssistanceGradation("LowVivid", CallbackGradationLowVivid);

                // Callbacks
                m_hologramController.m_eventCubetouched += new EventHandler(delegate (System.Object o, EventArgs e)
                {
                    m_eventHologramStimulateLevel1Gradation1Or2Touched?.Invoke(this, EventArgs.Empty);

                });

                Utilities.HologramInteractions interactions = m_help.GetComponent<Utilities.HologramInteractions>();
                if (interactions == null)
                {
                    interactions = m_help.gameObject.AddComponent<Utilities.HologramInteractions>();
                }
                interactions.EventTouched += new EventHandler(delegate (System.Object o, EventArgs e)
                {
                    m_eventHologramStimulateLevel1Gradation1Or2Touched?.Invoke(this, EventArgs.Empty);

                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Help touched");

                });
                SolverHandler solver = m_help.GetComponent<SolverHandler>();
                if (solver == null)
                {
                    solver = m_help.gameObject.AddComponent<SolverHandler>();
                }
                solver.TrackedTargetType = Microsoft.MixedReality.Toolkit.Utilities.TrackedObjectType.Head;
            }

            public bool HasFocus()
            {
                return m_hasFocus;
            }

            bool m_mutexShow = false;
            public override void Show(EventHandler eventHandler)
            {
                if (m_mutexShow == false)
                {
                    m_mutexShow = true;

                    Animation animator = m_hologramView.gameObject.AddComponent<Animation>();

                    MATCH.Utilities.Utility.AdjustObjectHeightToHeadHeight(m_help);
                    m_hologramController.backupScaling();

                    if (m_surfaceWithStarsViewTarget != null)
                    { // Then adjusting the size of the stars to the size of this object
                        Vector3 newScale = new Vector3(m_surfaceWithStarsViewTarget.localScale.x, 
                            m_surfaceWithStarsView.localScale.y, // Important not to change the height scale
                            m_surfaceWithStarsViewTarget.localScale.z);
                        /*newScale.x = m_surfaceWithStarsViewTarget.localScale.x;
                        newScale.y = m_surfaceWithStarsView.localScale.y; // Important not to change the height scale
                        newScale.z = m_surfaceWithStarsViewTarget.localScale.z;*/

                        m_surfaceWithStarsView.localScale = newScale;
                    }

                    m_help.gameObject.SetActive(true);
                    m_surfaceWithStarsView.gameObject.SetActive(true);
                    eventHandler?.Invoke(this, EventArgs.Empty);
                    m_mutexShow = false;
                }
                else
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Mutex locked - Request ignored");
                }
            }

            public void SetPositionToOriginalLocation()
            {
                m_hologramView.localPosition = m_hologramOriginalLocalPos;
            }

            public override void Hide(EventHandler eventHandler)
            {
                SetGradationToMinimum();

                if (m_hologramView.gameObject.activeSelf)
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Cube is going to be hidden");

                    EventHandler[] eventHandlers = new EventHandler[] { new EventHandler(delegate (System.Object o, EventArgs e)
        {


            m_hologramView.gameObject.SetActive(false);
            m_hologramView.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            Destroy(m_hologramView.GetComponent<Animation>());

            DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Cube should be hidden now. New local position:" + m_hologramView.localPosition.ToString());
        }), eventHandler };

                    m_hologramController.closeCube(new EventHandler(delegate (System.Object o, EventArgs e)
                    {
                        m_hologramView.gameObject.AddComponent<MATCH.Utilities.Animation>().AnimateDiseappearInPlace(eventHandlers);
                    }));
                    m_lightView.gameObject.SetActive(false);
                }
                else if (m_surfaceWithStarsView.gameObject.activeSelf)
                {
                    m_surfaceWithStarsView.gameObject.SetActive(false);
                    m_help.gameObject.SetActive(false);
                    eventHandler?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Assistance stimulate level 1 is disabled - no hide action to take");
                }
            }

            public override Transform GetTransform()
            {
                return m_hologramView;
            }

            public override void ShowHelp(bool show)
            {
                //
            }

            public bool IncreaseGradation()
            {
                return m_gradationManager.increaseGradation();
            }

            public bool DecreaseGradation()
            {
                return m_gradationManager.decreaseGradation();
            }

            public void SetGradationToMinimum()
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Gradation set to minimum");

                m_gradationManager.setGradationToMinimum();
            }

            void CallbackGradationDefault(System.Object o, EventArgs e)
            {
                m_surfaceWithStarsView.gameObject.SetActive(true);
                m_help.gameObject.SetActive(true);
                MATCH.Utilities.Utility.AdjustObjectHeightToHeadHeight(m_help);
                m_hologramView.gameObject.SetActive(false);

                m_lightView.gameObject.SetActive(false);
                m_hologramController.GetComponent<Billboard>().enabled = true;
                GetComponent<RadialView>().enabled = false;
                m_hologramController.setScalingToOriginal();

                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Showing default gradation");
            }

            void CallbackGradationLow(System.Object o, EventArgs e)
            {
                m_surfaceWithStarsView.gameObject.SetActive(false);
                m_help.gameObject.SetActive(false);

                MATCH.Utilities.Utility.AdjustObjectHeightToHeadHeight(m_hologramView, m_hologramOriginalLocalPos.y);
                m_hologramView.gameObject.SetActive(true);

                m_hologramController.updateMaterials(m_materialBottomDefault, m_materialTopLeftDefault, m_materialTopRightDefault);
                m_lightView.gameObject.SetActive(false);
                m_hologramController.GetComponent<Billboard>().enabled = true;
                GetComponent<RadialView>().enabled = false;
                m_hologramController.setScalingToOriginal();
            }

            void CallbackGradationLowVivid(System.Object o, EventArgs e)
            {
                m_help.gameObject.SetActive(false);

                MATCH.Utilities.Utility.AdjustObjectHeightToHeadHeight(m_hologramView, m_hologramOriginalLocalPos.y);
                m_hologramView.gameObject.SetActive(true);
                m_hologramController.updateMaterials(m_materialBottomVivid, m_materialTopLeftVivid, m_materialTopRightVivid);
                m_lightView.gameObject.SetActive(true);
                m_hologramController.GetComponent<Billboard>().enabled = true;
                GetComponent<RadialView>().enabled = false;
                m_hologramController.setScalingToOriginal(); // setting back the scaling to the original one.

                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Showing low vivid gradation");
            }

            void CallbackGradationHighFollow(System.Object o, EventArgs e)
            {
                // Add an offset in y to the cube to avoid that they remain too much "in front" of the user
                GetComponent<RadialView>().enabled = true;
                m_hologramView.gameObject.GetComponent<Billboard>().enabled = false; // The billboard is present on the parent object so that the cube can be shifted to avoid that it is too much in front of the user.
                m_lightView.gameObject.SetActive(false);

                // reducing the scale of the cube to have it less intrusive
                m_hologramController.setScalingReduced();
                m_hologramView.transform.localPosition = new Vector3(m_hologramView.localPosition.x, m_hologramOriginalLocalPos.y, m_hologramView.localPosition.z);

                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Showing high gradation");
            }

            public void OpeningCube(EventHandler eventHandler)
            {
                m_hologramController.openCube(eventHandler);
            }

            public void SetCubeMaterialDefault(string bottom, string topLeft, string topRight)
            {
                m_materialBottomDefault = bottom;
                m_materialTopLeftDefault = topLeft;
                m_materialTopRightDefault = topRight;
            }

            public void SetCubeMaterialVivid(string bottom, string topLeft, string topRight)
            {
                m_materialBottomVivid = bottom;
                m_materialTopLeftVivid = topLeft;
                m_materialTopRightVivid = topRight;
            }

            public override bool IsDecorator()
            {
                return false;
            }
        }
    }
}
