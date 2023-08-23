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
        public class ArchWithTextAndHelp : Assistance
        {
            public event EventHandler EventHelpTouched;

            Transform HelpController;
            Transform LineView;
            Transform TextView;
            MATCH.Assistances.Dialogs.Dialog1 TextController;

            LineToObject LineController;

            Vector3 TextOriginalScaling;

            void Awake()
            {
                // Children
                LineView = gameObject.transform.Find("Line");
                LineController = LineView.GetComponent<LineToObject>();
                HelpController = MATCH.Utilities.Utility.FindChild(gameObject, "Help");
                TextView = gameObject.transform.Find("Text");
                TextController = TextView.GetComponent<MATCH.Assistances.Dialogs.Dialog1>();
                

                TextOriginalScaling = TextView.localScale;
            }

            // Start is called before the first frame update
            void Start()
            {
                TextController.EnableBillboard(false);

                if (LineView == null)
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Error, "Line hologram not initialized properly");
                }

                // Callbacks
                MATCH.Utilities.Utility.AddTouchCallback(HelpController, CallbackHelpTouched);
            }

            void CallbackHelpTouched()
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called");

                EventHelpTouched?.Invoke(this, EventArgs.Empty);
            }

            void ResetObject()
            {
                HelpController.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                Destroy(HelpController.GetComponent<Animation>());
            }

            bool m_mutexShow = false;
            /**
             * Choice to have an animation or not is only managed for the dialog
             */
            public override void Show(EventHandler eventHandler, bool withAnimation = true)
            {
                if (m_mutexShow == false)
                {
                    m_mutexShow = true;

                    TextView.position = new Vector3(LineController.PointOrigin.x, Camera.main.transform.position.y,  LineController.PointOrigin.z);
                    TextView.transform.LookAt(Camera.main.transform);
                    TextView.transform.Rotate(new Vector3(0, 1, 0), 180);

                    // Trick to start the line to the text position, i.e. to start at user's head's position
                    LineController.PointOrigin = TextView.position;
                    TextView.position = Vector3.MoveTowards(TextView.position, Camera.main.transform.position, 0.01f);

                    TextController.Show(delegate
                    {
                        m_mutexShow = false;
                    }, withAnimation);

                    // Showing line
                    LineView.GetComponent<LineToObject>().show(delegate
                       {
                           eventHandler?.Invoke(this, EventArgs.Empty);
                           m_mutexShow = false;
                       });
                }
                else
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Mutex locked - request ignored");
                }
            }

            bool m_mutexHide = false;
            /**
             * Choice to have an animation or not is only managed for the dialog
             */
            public override void Hide(EventHandler eventHandler, bool withAnimation)
            {
                if (m_mutexHide == false)
                {
                    m_mutexHide = true;

                    TextController.Hide(/*eventHandler*/ delegate(System.Object o, EventArgs e)
                    {
                        // Hiding line
                        if (LineView.gameObject.activeSelf)
                        {
                            LineView.GetComponent<LineToObject>().hide(eventHandler); // The eventhandler being already called above, we do not want it to be called twice, as this could create strange behaviors.
                            m_mutexHide = false;
                        }
                        else
                        {
                            DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Line already hidden: nothing to do");
                            eventHandler?.Invoke(this, EventArgs.Empty);
                            m_mutexHide = false;
                        }
                    }, withAnimation);
                }
                else
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Mutex locked - request ignored");
                }
            }

            public void SetArchStartAndEndPoint(Transform origin, Transform target)
            {
                /*LineController.m_hologramOrigin = origin.gameObject;
                LineController.m_hologramTarget = target.gameObject;*/

                LineController.PointOrigin = origin.position;
                LineController.PointEnd = target.position;
            }

            public override void ShowHelp(bool show, EventHandler callback, bool withAnimation)
            {
                if (show)
                {
                    MATCH.Utilities.Utility.AdjustObjectHeightToHeadHeight(HelpController);

                    if (withAnimation)
                    {
                        HelpController.gameObject.AddComponent<MATCH.Utilities.Animation>().AnimateAppearInPlaceToScaling(new Vector3(0.1f, 0.1f, 0.1f), delegate
                        {
                            DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called");

                            Destroy(HelpController.gameObject.GetComponent<Animation>());
                            callback?.Invoke(this, EventArgs.Empty);
                        }
                        );
                    }
                    else
                    {
                        HelpController.gameObject.SetActive(true);
                        callback?.Invoke(this, EventArgs.Empty);
                    }
                    
                }
                else
                {
                    if (withAnimation)
                    {
                        MATCH.Utilities.Utility.AnimateDisappearInPlace(HelpController.gameObject, new Vector3(0.1f, 0.1f, 0.1f), callback);
                    }
                    else
                    {
                        HelpController.gameObject.SetActive(false);
                        HelpController.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                        callback?.Invoke(this, EventArgs.Empty);
                    }

                }

            }
            public void SetDescription(string text, float fontSize = -1.0f)
            {
                TextController.SetDescription(text, fontSize);
            }

            public override Transform GetTransform()
            {
                return transform;
            }

            /*public override bool IsActive()
            {
                return LineController.gameObject.activeSelf;
            }*/

            public override bool IsDecorator()
            {
                return false;
            }

            public override void Emphasize(bool enable)
            {
                TextController.Emphasize(enable);
            }
        }

    }
}
