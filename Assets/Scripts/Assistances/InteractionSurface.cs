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
using System.Reflection;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

/**
 * Manages the table interaction surface
 * */
namespace MATCH
{
    namespace Assistances
    {
        public class InteractionSurface : MonoBehaviour
        {
            Transform View;
            
            public event EventHandler EventInteractionSurfaceTableTouched;
            public event EventHandler EventInteractionSurfaceScaled;
            public event EventHandler EventInteractionSurfaceMoved;

            string Color = Utilities.Materials.Colors.GreenGlowing; // Default color if the user does not set one

            bool SurfaceInitialized;

            private void Awake()
            {
                // Initialize variables
                SurfaceInitialized = false;

                // Children
                View = gameObject.transform.Find("InteractionSurfaceChild");
            }

            // Start is called before the first frame update
            void Start()
            {
                BoundsControl boundsControl = View.GetComponent<BoundsControl>();

                boundsControl.ScaleStopped.AddListener(delegate
                {
                    EventInteractionSurfaceScaled?.Invoke(this, EventArgs.Empty);
                });

                ObjectManipulator objectManipulator = View.GetComponent<ObjectManipulator>();
                objectManipulator.OnManipulationEnded.AddListener(delegate (ManipulationEventData data)
                {
                    //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called");
                    EventInteractionSurfaceMoved?.Invoke(this, EventArgs.Empty);
                });
            }

            public Transform GetInteractionSurface()
            {
                return View;
            }

            /**
             * The name of the color should reference an object present in the "resources" directory
             * **/
            public void SetColor(string colorName)
            {
                //DebugMessagesManager.Instance.displayMessage("MousePopulateSurfaceTableWithCubes", "callbackOnTapToPlaceFinished", DebugMessagesManager.MessageLevel.Info, "Loading color");

                Color = colorName;

                View.GetComponent<Renderer>().material = Resources.Load(Color, typeof(Material)) as Material;
            }

            public void SetScaling(Vector3 scaling)
            {
                View.GetComponent<BoundsControl>().enabled = false;
                View.localScale = scaling;
                View.GetComponent<BoundsControl>().enabled = true;
            }

            public void SetLocalPosition(Vector3 position)
            {
                View.localPosition = position;
            }

            public Vector3 GetLocalPosition()
            {
                return View.localPosition;
            }

            public Vector3 GetLocalScale()
            {
                return View.localScale;
            }

            void CallbackHologramInteractionSurfaceMovedFinished()
            {
                //DebugMessagesManager.Instance.displayMessage("MousePopulateSurfaceTableWithCubes", "callbackOnTapToPlaceFinished", DebugMessagesManager.MessageLevel.Info, "Called");

                // Bring specific components to the center of the interaction surface
                gameObject.transform.position = View.transform.position;
                View.transform.localPosition = new Vector3(0, 0f, 0);
            }

            void CallbackShow()
            {
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Callback showing / hiding interaction surface called");

                ShowInteractionSurfaceTable(!(View.GetComponent<BoundsControl>().enabled));
            }

            public void ShowInteractionSurfaceTable(bool show)
            {


                if (SurfaceInitialized == false)
                {
                    View.gameObject.SetActive(true); // If it happens that the surface is not displayed

                    // Connect the callbacks
                    View.GetComponent<BoundsControl>().ScaleStopped.AddListener(CallbackHologramInteractionSurfaceMovedFinished); // Use the same callback than for taptoplace as the process to do is the same
                    View.GetComponent<Interactable>().GetReceiver<InteractableOnTouchReceiver>().OnTouchStart.AddListener(delegate ()
                    {
                        EventInteractionSurfaceTableTouched?.Invoke(this, EventArgs.Empty);
                    }); // Only have to forward the event


                    SurfaceInitialized = true;

                }

                View.GetComponent<Renderer>().enabled = show; // To hide the surface while keeping it interactable, then the renderer is disabled if show==false;
                View.GetComponent<BoundsControl>().enabled = show;
                View.GetComponent<ObjectManipulator>().enabled = show;
            }

            public void CallbackBring()
            {
                gameObject.transform.position = new Vector3(Camera.main.transform.position.x + 1.5f, Camera.main.transform.position.y - 0.5f, Camera.main.transform.position.z);

                //DebugMessagesManager.Instance.displayMessage("MouseUtilitiesAdminMenu", "callbackBringInteractionSurface", DebugMessagesManager.MessageLevel.Info, "Called - Camera position: " + Camera.main.transform.position + " New position of the object: " + gameObject.transform.position);
            }

            public void SetAdminButtons(string interfaceSurfaceId, AdminMenu.Panels panel = AdminMenu.Panels.Middle)
            {
                AdminMenu.Instance.AddSwitchButton("Interaction surface " + interfaceSurfaceId + " - Hide", CallbackShow, panel, AdminMenu.ButtonType.Hide);
                AdminMenu.Instance.AddButton("Interaction surface" + interfaceSurfaceId + " - Bring", CallbackBring, panel);
            }

            public void SetObjectResizable(bool enable)
            {
                if (enable)
                {
                    View.GetComponent<BoundsControl>().ScaleHandlesConfig.ScaleBehavior = Microsoft.MixedReality.Toolkit.UI.BoundsControlTypes.HandleScaleMode.NonUniform;
                }
                else
                {
                    View.GetComponent<BoundsControl>().ScaleHandlesConfig.ScaleBehavior = Microsoft.MixedReality.Toolkit.UI.BoundsControlTypes.HandleScaleMode.Uniform;
                }

            }



            public void SetPreventResizeY(bool prevent)
            {
                if (prevent)
                {
                    View.GetComponent<BoundsControl>().FlattenAxis = Microsoft.MixedReality.Toolkit.UI.BoundsControlTypes.FlattenModeType.FlattenY;
                }
                else
                {
                    View.GetComponent<BoundsControl>().FlattenAxis = Microsoft.MixedReality.Toolkit.UI.BoundsControlTypes.FlattenModeType.DoNotFlatten;
                }
            }

            /**
             * Trigger the touch event from the script.
             * */
            public void TriggerTouchEvent()
            {
                EventInteractionSurfaceTableTouched?.Invoke(this, EventArgs.Empty);
            }
        }

    }
}
