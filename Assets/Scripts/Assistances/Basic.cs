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
 * This class manages a basic show / hide for a child that should be named ... "child" (I am a creative person)
 * An event is emitted if the nested hologram is touched.
 * Appears / disappears in place. I told you it was basic.
 * */

namespace MATCH
{
    namespace Assistances
    {
        public class Basic : Assistance, IBasic
        {
            public Transform ChildView;

            Vector3 ChildScaleOrigin;

            /*MouseUtilitiesMutex m_mutexShow;
            MouseUtilitiesMutex m_mutexHide;*/

            public EventHandler s_touched;

            Dialog Help;

            public bool AdjustHeightOnShow { private get; set; }

            private void Awake()
            {
                // Initialize variables

                // Children
                ChildView = gameObject.transform.Find("Child");

                // Scale origin
                ChildScaleOrigin = ChildView.localScale;

                // Adding the touch event
                Utilities.HologramInteractions interactions = ChildView.GetComponent<Utilities.HologramInteractions>();
                if (interactions == null)
                {
                    interactions = ChildView.gameObject.AddComponent<Utilities.HologramInteractions>();
                }
                interactions.EventTouched += delegate (System.Object sender, EventArgs args)
                {
                    s_touched?.Invoke(sender, args);
                };
                //interactions.s

                AdjustHeightOnShow = true;

               
            }

            private void Start()
            {
                // Help buttons
                //if (!transform.Find("ExclamationMarkButtons"))
                //{
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Help buttons instanciated for " + gameObject.name);

                List<string> buttonsText = new List<string>();
                buttonsText.Add("Oui");
                buttonsText.Add("Non");
                List<EventHandler> buttonsCallback = new List<EventHandler>();
                buttonsCallback.Add(CButtonHelp);
                buttonsCallback.Add(CButtonHelp);
                List<Assistances.Buttons.Button.ButtonType> buttonsType = new List<Buttons.Button.ButtonType>();
                buttonsType.Add(Buttons.Button.ButtonType.Yes);
                buttonsType.Add(Buttons.Button.ButtonType.No);
                Help = Assistances.Factory.Instance.CreateButtons("", "Besoin d'aide?", buttonsText, buttonsCallback, buttonsType, transform);
                Help.AdjustToHeight = false;
                Help.gameObject.name = "ExclamationMarkButtons";
                //Help.GetTransform().localPosition = new Vector3(ChildView.localPosition.x, ChildView.localPosition.y - 0.3f, ChildView.localPosition.z);
                Help.GetTransform().localPosition = new Vector3(0, -0.2f, 0);
                //Help.Hide(Utilities.Utility.GetEventHandlerEmpty());
                //}


            }

            //bool MutexShow = false;
            public override void Show(EventHandler eventHandler)
            {
                Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();

                /*if (MutexShow == false)
                {
                    MutexShow = true;*/
                if (IsDisplayed == false)
                {
                    if (AdjustHeightOnShow)
                    {
                        MATCH.Utilities.Utility.AdjustObjectHeightToHeadHeight(transform);
                    }

                    MATCH.Utilities.Utility.AnimateAppearInPlace(ChildView.gameObject, ChildScaleOrigin, delegate (System.Object o, EventArgs e)
                    {
                        IsDisplayed = true;
                        //MutexShow = false;
                        args.Success = true;
                        eventHandler?.Invoke(this, args);
                        //Help.Show(Utilities.Utility.GetEventHandlerEmpty());
                    });
                }
                else
                {
                    args.Success = false;
                    eventHandler?.Invoke(this, args);
                }
                //}
            }

            //bool MutexHide = false;
            public override void Hide(EventHandler eventHandler)
            {
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Hiding " + name);

                /*if (MutexHide == false)
                {
                    MutexHide = true;*/

                Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();

                if (IsDisplayed)
                {

                    MATCH.Utilities.Utility.AnimateDisappearInPlace(ChildView.gameObject, ChildScaleOrigin, delegate (System.Object o, EventArgs e)
                    {
                        ChildView.gameObject.transform.localScale = ChildScaleOrigin;
                        //MutexHide = false;
                        IsDisplayed = false;

                        if (Help.IsDisplayed)
                        {
                            ShowHelp(false, eventHandler);
                        }
                        else
                        {
                            args.Success = true;
                            eventHandler?.Invoke(this, args);
                        }

                        //eventHandler?.Invoke(this, EventArgs.Empty);
                    });
                }
                else
                {
                    args.Success = false;
                    eventHandler?.Invoke(this, args);
                }
                    
                //}
            }

            public override void ShowHelp(bool show, EventHandler callback)
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "ShowHelp function called for " + Help.name + " show=" + show.ToString());
                if (show)
                {
                    //Help.GetTransform().localPosition = new Vector3(ChildView.localPosition.x, ChildView.localPosition.y - 0.3f, ChildView.localPosition.z);
                    //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Help is going to be shown");

                    Help.Show(delegate(System.Object o, EventArgs e)
                    {
                        //Help.GetTransform().localPosition = new Vector3(0, 0.3f, 0);
                        //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Help should be visible now");
                        //Help.EventHelpButtonClicked += CButtonHelp;
                        Help.ShowHelp(true, callback);
                    });
                }
                else
                {
                    //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Help is going to be hidden");

                    Help.Hide(callback);
                }
            }

            public override Transform GetTransform()
            {
                return ChildView;
            }

            public void SetMaterial(string materialName)
            {
                Renderer renderer = ChildView.GetComponent<Renderer>();
                if (renderer != null)
                {
                    //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Material set to " + materialName);
                    renderer.material = Resources.Load(materialName, typeof(Material)) as Material;
                }
                else
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "No renderer component for the child - no action will be done");
                }
            }

            /*public void SetAdjustHeightOnShow(bool enable)
            {
                m_adjustHeight = enable;
            }*/

            public void SetScale(float x, float y, float z)
            {
                SetScale(new Vector3(x, y, z));
                //m_childView.transform.localScale = new Vector3(x, y, z);
                //m_childScaleOrigin = m_childView.transform.localScale;
            }

            public void SetScale(Vector3 scale)
            {
                ChildView.transform.localScale = scale;
                ChildScaleOrigin = ChildView.transform.localScale;
            }

            public Vector3 GetScale()
            {
                return ChildView.transform.localScale;
            }

            public void SetLocalPosition(float x, float y, float z)
            {
                //m_childView.transform.localPosition = new Vector3(x, y, z);
                SetLocalPosition(new Vector3(x, y, z));
            }

            public void SetLocalPosition(Vector3 localPosition)
            {
                ChildView.transform.localPosition = localPosition;
            }

            public Vector3 GetLocalPosition()
            {
                return ChildView.transform.localPosition;
            }

            public void SetBillboard(bool enable)
            {
                ChildView.GetComponent<Billboard>().enabled = enable;
            }

            public void TriggerTouch()
            {
                s_touched?.Invoke(this, EventArgs.Empty);
            }

            public Transform GetChildTransform()
            {
                return ChildView.transform;
            }

            public Assistance GetAssistance()
            {
                return this;
            }

            public override bool IsDecorator()
            {
                return false;
            }

            /*public override bool IsActive()
            {
                return ChildView.gameObject.activeSelf;
            }*/
        }
    }
}