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
using System;
using System.Reflection;

/**
 * Assistance showing an arch between two objects, with a text at the beginning, and a help cube at the end.
 * */
namespace MATCH
{
    namespace Assistances
    {
        public class ArchWithTextAndHelp : MonoBehaviour
        {
            public event EventHandler m_eventHologramHelpTouched;

            public Transform m_hologramHelp;
            Transform m_hologramLineView;
            public Transform m_textView;
            MATCH.Assistances.Dialog m_textController;

            LineToObject m_hologramLineController;

            Vector3 m_textOriginalScaling;

            void Awake()
            {
                // Children
                m_hologramLineView = gameObject.transform.Find("Line");
                m_hologramLineController = m_hologramLineView.GetComponent<LineToObject>();
                m_hologramHelp = MATCH.Utilities.Utility.FindChild(gameObject, "Help");
                m_textView = gameObject.transform.Find("TextNew");
                m_textController = m_textView.GetComponent<MATCH.Assistances.Dialog>();

                m_textOriginalScaling = m_textView.localScale;
            }

            // Start is called before the first frame update
            void Start()
            {
                if (m_hologramLineView == null)
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Error, "Line hologram not initialized properly");
                }

                // Callbacks
                MATCH.Utilities.Utility.AddTouchCallback(m_hologramHelp, callbackHelpTouched);
            }

            // Update is called once per frame
            void Update()
            {

            }

            void callbackHelpTouched()
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called");

                m_eventHologramHelpTouched?.Invoke(this, EventArgs.Empty);
            }

            void resetObject()
            {
                m_hologramHelp.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                Destroy(m_hologramHelp.GetComponent<Animation>());
            }

            bool m_mutexShow = false;
            public void show(EventHandler eventHandler)
            {
                if (m_mutexShow == false)
                {
                    m_mutexShow = true;

                    m_textView.position = m_hologramLineController.m_hologramOrigin.transform.position;
                    MATCH.Utilities.Utility.AdjustObjectHeightToHeadHeight(m_textView);
                    // Trick to start the line to the text position, i.e. to start at user's head's position
                    m_hologramLineController.m_hologramOrigin.transform.position = m_textView.position;

                    m_textController.Show(delegate
                    {
                        //m_textController.enableBillboard(true);
                        m_mutexShow = false;
                    });

                    // Showing line
                    m_hologramLineView.GetComponent<LineToObject>().show(delegate {
                        EventHandler[] temp = new EventHandler[] { delegate
                    {
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called");

                        Destroy(m_hologramHelp.gameObject.GetComponent<Animation>());

                        m_mutexShow = false;
                    }, eventHandler };

                        MATCH.Utilities.Utility.AdjustObjectHeightToHeadHeight(m_hologramHelp);

                        m_hologramHelp.gameObject.AddComponent<MATCH.Utilities.Animation>().AnimateAppearInPlaceToScaling(new Vector3(0.1f, 0.1f, 0.1f), temp);
                    });
                }
                else
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Mutex locked - request ignored");
                }
            }

            bool m_mutexHide = false;
            public void hide(EventHandler eventHandler)
            {
                if (m_mutexHide == false)
                {
                    m_mutexHide = true;

                    m_textController.Hide(eventHandler);
                    MATCH.Utilities.Utility.AnimateDisappearInPlace(m_hologramHelp.gameObject, new Vector3(0.1f, 0.1f, 0.1f), delegate {
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Mutex unlocked - all elements from the arch should be hidden");
                        m_mutexHide = false;
                    });

                    // Hiding line
                    if (m_hologramLineView.gameObject.activeSelf)
                    {
                        m_hologramLineView.GetComponent<LineToObject>().hide(MATCH.Utilities.Utility.GetEventHandlerEmpty()); // The eventhandler being already called above, we do not want it to be called twice, as this could create strange behaviors.
                    }
                    else
                    {
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Line already hidden: nothing to do");
                    }
                }
                else
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Mutex locked - request ignored");
                }
            }

            public void setArchStartAndEndPoint(Transform origin, Transform target)
            {
                m_hologramLineController.m_hologramOrigin = origin.gameObject;
                m_hologramLineController.m_hologramTarget = target.gameObject;
            }
        }

    }
}
