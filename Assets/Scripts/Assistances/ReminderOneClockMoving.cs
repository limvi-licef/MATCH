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
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System;
using System.Reflection;
using System.Linq;

namespace MATCH
{
    namespace Assistances
    {
        /**
 * Used as an "escape" where the user can ask to be reminded later about the challenge
 * */
        public class ReminderOneClockMoving : Assistance
        {


            Transform m_clockView; // Because we can have multiple clocks

            Transform m_hologramWindowReminderView;
            MATCH.Assistances.Dialogs.Dialog1 m_dialogController;

            public event EventHandler EventHologramClockTouched;
            public event EventHandler EventHologramWindowButtonOkTouched;
            public event EventHandler EventHologramWindowButtonBackTouched;

            Vector3 m_positionLocalOrigin;
            float m_yOffsetOrigin;

            // As the clock does not have an attached script, storing the required information here
            Vector3 m_clockScalingOriginal;
            //Vector3 m_clockScalingReduced;
            //Vector3 m_clockOriginalPosition;

            List<Transform> m_objectsToBeClose;

            bool m_newObjectToFocus;
            Transform m_newObjectToFocusTransform;

            private void Awake()
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called");

                // Initialize variables
                m_objectsToBeClose = new List<Transform>();

                m_positionLocalOrigin = transform.localPosition;
                m_yOffsetOrigin = transform.localPosition.y;

                m_newObjectToFocus = false;

                m_newObjectToFocusTransform = null;
            }

            // Start is called before the first frame update
            void Start()
            {
                //MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Called");

                // Let's get the children
                m_clockView = gameObject.transform.Find("Clock");
                m_hologramWindowReminderView = gameObject.transform.Find("MouseAssistanceDialog"); //gameObject.transform.Find("Text");
                m_dialogController = m_hologramWindowReminderView.GetComponent<MATCH.Assistances.Dialogs.Dialog1>();
                m_dialogController.SetDescription("Très bien! J'apparaitrai de nouveau demain à la même heure. Est-ce que cela vous convient?", 0.15f);
                m_dialogController.AddButton("Parfait!", true);
                m_dialogController.ButtonsController[0].EventButtonClicked += new EventHandler(delegate (System.Object o, EventArgs e)
                {
                    EventHologramWindowButtonOkTouched?.Invoke(this, EventArgs.Empty);
                });
                m_dialogController.AddButton("Je me suis trompé de bouton! Revenir en arrière...", true);
                m_dialogController.ButtonsController[1].EventButtonClicked += delegate
                {
                    EventHologramWindowButtonBackTouched?.Invoke(this, EventArgs.Empty);
                };


                // Getting informations related to the clock
                m_clockScalingOriginal = new Vector3(0.1f, 0.1f, 0.1f); // harcoding the scaling like this might create some issues, but getting the scaling directly from the gameobject does not work with the Hololens (although it works here in Unity) 
                //m_clockScalingReduced = m_clockScalingOriginal / 3.0f;
                //m_clockOriginalPosition = m_clockView.localPosition;

            }

            // Update is called once per frame
            void Update()
            {
                if (m_newObjectToFocus)
                {
                    m_newObjectToFocus = false; // Managed so disable to avoid this to be called each time

                    gameObject.transform.position = m_newObjectToFocusTransform.position;
                    m_positionLocalOrigin = gameObject.transform.localPosition;
                }
            }

            public void AddObjectToBeClose(Transform o) // The parent the clock will belong to
            {
                m_objectsToBeClose.Add(o);

                Utilities.HologramInteractions interactions = o.gameObject.GetComponent<Utilities.HologramInteractions>();

                if (interactions == null)
                {
                    interactions = o.gameObject.AddComponent<Utilities.HologramInteractions>();
                    if (interactions == null)
                    { // Most likely BoxCollider is missing
                        o.gameObject.AddComponent<BoxCollider>();
                        // Try to add the interactions component again
                        interactions = o.gameObject.AddComponent<Utilities.HologramInteractions>();
                    }
                }

                interactions.EventFocusOn += CallbackOnObjectFocus;
            }

            public override Transform GetTransform()
            {
                return transform;
            }

            /*public override bool IsActive()
            {
                return m_clockView.gameObject.activeSelf;
            }*/

            void CallbackOnClockTouched(System.Object sender, EventArgs e)
            {// If a clock is touched, all other clocks are hidden

                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Clock touched callback called");

                MATCH.Utilities.Utility.AnimateDisappearInPlace(m_clockView.gameObject, new Vector3(0.1f, 0.1f, 0.1f), delegate
                {
                    m_hologramWindowReminderView.position = new Vector3(m_hologramWindowReminderView.position.x, m_clockView.position.y, m_hologramWindowReminderView.position.z);

                    m_dialogController.Show(MATCH.Utilities.Utility.GetEventHandlerEmpty(), true);
                    EventHologramClockTouched?.Invoke(this, EventArgs.Empty);
                });
            }

            void CallbackOnObjectFocus(System.Object sender, EventArgs e)
            {
                if (m_clockView.gameObject.activeSelf)
                { // I.e. not active if the dialog is displayed
                    if (m_newObjectToFocusTransform == null || (m_newObjectToFocusTransform.name != ((GameObject)sender).name))
                    { // To avoid doing unnecessary processes if the situation did not change
                        m_newObjectToFocusTransform = ((GameObject)sender).transform;
                        m_newObjectToFocus = true;
                    }
                }
            }

            bool MutexHide = false;
            public override void Hide(EventHandler e, bool withAnimation)
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called");

                if (MutexHide == false)
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Mutex locked");
                    MutexHide = true;

                    if (m_clockView.gameObject.activeSelf)
                    {
                        if (withAnimation)
                        {
                            MATCH.Utilities.Utility.AnimateDisappearInPlace(m_clockView.gameObject, new Vector3(0.1f, 0.1f, 0.1f), delegate
                            {
                                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Mutex unlocked - clock hidden");
                                MutexHide = false;
                                IsDisplayed = false;
                            });
                        }
                        else
                        {
                            m_clockView.gameObject.SetActive(false);
                            m_clockView.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                            MutexHide = false;
                            IsDisplayed = false;
                        }
                        
                    }
                    else
                    { // Only two children for know so fine this way. But to be extended if it happens that more children are added in the future
                        m_dialogController.Hide(delegate
                        {
                            DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Mutex unlocked - dialog hidden");

                            MutexHide = false; //.unlockMutex();
                            e?.Invoke(this, EventArgs.Empty);
                        }, withAnimation);
                    }


                }
            }

            bool MutexShow = false;
            bool ShowFirstTime = false;
            public override void Show(EventHandler eventHandler, bool withAnimation)
            {
                m_clockScalingOriginal = new Vector3(0.1f, 0.1f, 0.1f);
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Clock is going to appear to scaling: " + m_clockScalingOriginal);

                if (MutexShow == false)
                {
                    MutexShow = true;

                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Mutex locked");

                    MATCH.Utilities.Utility.AdjustObjectHeightToHeadHeight(transform, m_yOffsetOrigin);

                    if (withAnimation)
                    {
                        MATCH.Utilities.Animation animator = m_clockView.gameObject.AddComponent<MATCH.Utilities.Animation>();
                        animator.AnimateAppearInPlaceToScaling(m_clockScalingOriginal, new EventHandler(delegate (System.Object oo, EventArgs ee)
                        {
                            eventHandler?.Invoke(this, EventArgs.Empty);

                            if (ShowFirstTime == false)
                            {
                                Utilities.HologramInteractions temp = m_clockView.GetComponent<Utilities.HologramInteractions>();
                                temp.EventTouched += CallbackOnClockTouched;

                                ShowFirstTime = true;
                            }

                            Destroy(m_clockView.gameObject.GetComponent<MATCH.Utilities.Animation>());
                            MutexShow = false;
                            IsDisplayed = true;
                        }
                            ));
                    }
                    else
                    {
                        m_clockView.gameObject.SetActive(true);
                        m_clockView.transform.localScale = m_clockScalingOriginal;
                        eventHandler?.Invoke(this, EventArgs.Empty);

                            if (ShowFirstTime == false)
                            {
                                Utilities.HologramInteractions temp = m_clockView.GetComponent<Utilities.HologramInteractions>();
                                temp.EventTouched += CallbackOnClockTouched;

                                ShowFirstTime = true;
                            }

                            MutexShow = false;
                            IsDisplayed = true;
                    }
                    
                }
                else
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Mutex already locked - nothing to do");
                }
            }

            /**
             * Todo: to implement
             */
            public override void ShowHelp(bool show, EventHandler callback, bool withAnimation)
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Not implemented yet");
            }

            // Be aware that this function does not send the object back to its original position
            /*void resetObject()
            {
                m_hologramWindowReminderView.gameObject.transform.localScale = new Vector3(1, 1, 1);
                m_hologramWindowReminderView.gameObject.SetActive(false);
            }*/

            public void SetObjectToOriginalPosition()
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Changing position");

                transform.localPosition = m_positionLocalOrigin;
            }

            public override bool IsDecorator()
            {
                return false;
            }
        }

    }
}


