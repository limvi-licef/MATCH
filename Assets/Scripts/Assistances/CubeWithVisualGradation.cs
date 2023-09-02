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
            public Transform HologramView;
            CubeOpening HologramController;
            Vector3 HologramOriginalLocalPos;

            Transform LightView;
            //Light m_lightController;

            FiniteStateMachine.VisualGradationManager GradationManager;

            public Transform SurfaceWithStarsView;
            public Transform SurfaceWithStarsViewTarget; // If provided, allows to adjust the size of the stars to the size of this surface

            public Transform Help;

            public EventHandler EventHologramStimulateLevel1Gradation1Or2Touched;

            public AudioClip AudioClipToPlayOnTouchInteractionSurface;
            public AudioListener AudioListener;

            public bool HasFocusStatus;

            string MaterialBottomDefault;
            string MaterialTopLeftDefault;
            string MaterialTopRightDefault;

            string MaterialBottomVivid;
            string MaterialTopLeftVivid;
            string MaterialTopRightVivid;

            private void Awake()
            {
                // Variables
                HasFocusStatus = false;

                // Children
                HologramView = transform.Find("CubeOpening");
                HologramController = HologramView.GetComponent<CubeOpening>();
                HologramOriginalLocalPos = HologramView.localPosition;

                LightView = transform.Find("Light");
                //m_lightController = m_lightView.GetComponent<Light>();

                SurfaceWithStarsView = transform.Find("SurfaceWithStars");

                Help = transform.Find("Help");

                // Default materials
                MaterialBottomDefault = Utilities.Materials.Colors.CyanGlowing;
                MaterialTopLeftDefault = Utilities.Materials.Colors.CyanGlowing;
                MaterialTopRightDefault = Utilities.Materials.Colors.CyanGlowing;
                MaterialBottomVivid = Utilities.Materials.Colors.OrangeGlowing;
                MaterialTopLeftVivid = Utilities.Materials.Colors.OrangeGlowing;
                MaterialTopRightVivid = Utilities.Materials.Colors.OrangeGlowing;
            }

            // Start is called before the first frame update
            void Start()
            {
                // Setting up the gradation manger
                GradationManager = transform.GetComponent<FiniteStateMachine.VisualGradationManager>();
                GradationManager.addNewAssistanceGradation("Default", CallbackGradationDefault);
                GradationManager.addNewAssistanceGradation("LowVivid", CallbackGradationLowVivid);

                // Callbacks
                HologramController.m_eventCubetouched += new EventHandler(delegate (System.Object o, EventArgs e)
                {
                    EventHologramStimulateLevel1Gradation1Or2Touched?.Invoke(this, EventArgs.Empty);

                });

                Utilities.HologramInteractions interactions = Help.GetComponent<Utilities.HologramInteractions>();
                if (interactions == null)
                {
                    interactions = Help.gameObject.AddComponent<Utilities.HologramInteractions>();
                }
                interactions.EventTouched += new EventHandler(delegate (System.Object o, EventArgs e)
                {
                    EventHologramStimulateLevel1Gradation1Or2Touched?.Invoke(this, EventArgs.Empty);

                    DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Help touched");

                });
                SolverHandler solver = Help.GetComponent<SolverHandler>();
                if (solver == null)
                {
                    solver = Help.gameObject.AddComponent<SolverHandler>();
                }
                solver.TrackedTargetType = Microsoft.MixedReality.Toolkit.Utilities.TrackedObjectType.Head;
            }

            public bool HasFocus()
            {
                return HasFocusStatus;
            }

            bool MutexShow = false;
            /**
             * The "withAnimation" is ignored.
             */
            public override void Show(EventHandler eventHandler, bool withAnimation)
            {
                if (MutexShow == false)
                {
                    MutexShow = true;

                    //Animation animator = HologramView.gameObject.AddComponent<Animation>();

                    MATCH.Utilities.Utility.AdjustObjectHeightToHeadHeight(Help);
                    HologramController.backupScaling();

                    if (SurfaceWithStarsViewTarget != null)
                    { // Then adjusting the size of the stars to the size of this object
                        Vector3 newScale = new Vector3(SurfaceWithStarsViewTarget.localScale.x, 
                            SurfaceWithStarsView.localScale.y, // Important not to change the height scale
                            SurfaceWithStarsViewTarget.localScale.z);
                        
                        SurfaceWithStarsView.localScale = newScale;
                    }

                    IsDisplayed = true;
                    Help.gameObject.SetActive(true);
                    SurfaceWithStarsView.gameObject.SetActive(true);
                    eventHandler?.Invoke(this, EventArgs.Empty);
                    MutexShow = false;
                }
                else
                {
                    DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Mutex locked - Request ignored");
                }
            }

            public void SetPositionToOriginalLocation()
            {
                HologramView.localPosition = HologramOriginalLocalPos;
            }

            /**
             * withAnimation is ignored.
             */
            public override void Hide(EventHandler eventHandler, bool withAnimation)
            {
                SetGradationToMinimum();

                if (HologramView.gameObject.activeSelf)
                {
                    DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Cube is going to be hidden");

                    EventHandler[] eventHandlers = new EventHandler[] { new EventHandler(delegate (System.Object o, EventArgs e)
        {


            HologramView.gameObject.SetActive(false);
            HologramView.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            Destroy(HologramView.GetComponent<Animation>());

            DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Cube should be hidden now. New local position:" + HologramView.localPosition.ToString());
            IsDisplayed = false;
        }), eventHandler };

                    HologramController.closeCube(new EventHandler(delegate (System.Object o, EventArgs e)
                    {
                        HologramView.gameObject.AddComponent<MATCH.Utilities.Animation>().AnimateDiseappearInPlace(eventHandlers);
                    }));
                    LightView.gameObject.SetActive(false);
                }
                else if (SurfaceWithStarsView.gameObject.activeSelf)
                {
                    SurfaceWithStarsView.gameObject.SetActive(false);
                    Help.gameObject.SetActive(false);
                    eventHandler?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Assistance stimulate level 1 is disabled - no hide action to take");
                }
            }

            public override Transform GetTransform()
            {
                return HologramView;
            }

            /*public override bool IsActive()
            {
                return HologramView.gameObject.activeSelf;
            }*/

            public override void ShowHelp(bool show, EventHandler callback, bool withAnimation)
            {
                DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Not implemented yet");
            }

            public bool IncreaseGradation()
            {
                return GradationManager.increaseGradation();
            }

            public bool DecreaseGradation()
            {
                return GradationManager.decreaseGradation();
            }

            public void SetGradationToMinimum()
            {
                DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Gradation set to minimum");

                GradationManager.setGradationToMinimum();
            }

            void CallbackGradationDefault(System.Object o, EventArgs e)
            {
                SurfaceWithStarsView.gameObject.SetActive(true);
                Help.gameObject.SetActive(true);
                MATCH.Utilities.Utility.AdjustObjectHeightToHeadHeight(Help);
                HologramView.gameObject.SetActive(false);

                LightView.gameObject.SetActive(false);
                HologramController.GetComponent<Billboard>().enabled = true;
                GetComponent<RadialView>().enabled = false;
                HologramController.setScalingToOriginal();

                DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Showing default gradation");
            }

            void CallbackGradationLow(System.Object o, EventArgs e)
            {
                SurfaceWithStarsView.gameObject.SetActive(false);
                Help.gameObject.SetActive(false);

                MATCH.Utilities.Utility.AdjustObjectHeightToHeadHeight(HologramView, HologramOriginalLocalPos.y);
                HologramView.gameObject.SetActive(true);

                HologramController.updateMaterials(MaterialBottomDefault, MaterialTopLeftDefault, MaterialTopRightDefault);
                LightView.gameObject.SetActive(false);
                HologramController.GetComponent<Billboard>().enabled = true;
                GetComponent<RadialView>().enabled = false;
                HologramController.setScalingToOriginal();
            }

            void CallbackGradationLowVivid(System.Object o, EventArgs e)
            {
                Help.gameObject.SetActive(false);

                MATCH.Utilities.Utility.AdjustObjectHeightToHeadHeight(HologramView, HologramOriginalLocalPos.y);
                HologramView.gameObject.SetActive(true);
                HologramController.updateMaterials(MaterialBottomVivid, MaterialTopLeftVivid, MaterialTopRightVivid);
                LightView.gameObject.SetActive(true);
                HologramController.GetComponent<Billboard>().enabled = true;
                GetComponent<RadialView>().enabled = false;
                HologramController.setScalingToOriginal(); // setting back the scaling to the original one.

                DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Showing low vivid gradation");
            }

            void CallbackGradationHighFollow(System.Object o, EventArgs e)
            {
                // Add an offset in y to the cube to avoid that they remain too much "in front" of the user
                GetComponent<RadialView>().enabled = true;
                HologramView.gameObject.GetComponent<Billboard>().enabled = false; // The billboard is present on the parent object so that the cube can be shifted to avoid that it is too much in front of the user.
                LightView.gameObject.SetActive(false);

                // reducing the scale of the cube to have it less intrusive
                HologramController.setScalingReduced();
                HologramView.transform.localPosition = new Vector3(HologramView.localPosition.x, HologramOriginalLocalPos.y, HologramView.localPosition.z);

                DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Showing high gradation");
            }

            public void OpeningCube(EventHandler eventHandler)
            {
                HologramController.openCube(eventHandler);
            }

            public void SetCubeMaterialDefault(string bottom, string topLeft, string topRight)
            {
                MaterialBottomDefault = bottom;
                MaterialTopLeftDefault = topLeft;
                MaterialTopRightDefault = topRight;
            }

            public void SetCubeMaterialVivid(string bottom, string topLeft, string topRight)
            {
                MaterialBottomVivid = bottom;
                MaterialTopLeftVivid = topLeft;
                MaterialTopRightVivid = topRight;
            }

            public override bool IsDecorator()
            {
                return false;
            }

            public override void Emphasize(bool enable)
            {
                if (enable)
                {
                    Utilities.Emphasize emphasize = gameObject.AddComponent<Utilities.Emphasize>();

                    emphasize.AddMaterial(HologramView);
                    emphasize.EnableEmphasize(true);

                }
                else
                {
                    Utilities.Emphasize emphasize = null;

                    if (gameObject.TryGetComponent<Utilities.Emphasize>(out emphasize))
                    {
                        emphasize.EnableEmphasize(false);

                        Destroy(emphasize);
                    }
                }
            }
        }
    }
}
