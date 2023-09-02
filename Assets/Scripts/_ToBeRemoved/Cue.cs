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
 * Assistance to show a message with a "I don't understand" button
 * */
namespace MATCH
{
    namespace Assistances
    {
        namespace Buttons
        {
            public class Cue : Button
            {


                //public event EventHandler m_eventHelpButtonClicked;

                public Transform m_text;
                Transform m_button;
                Transform m_hologramButtonClicked;

                private void Awake()
                {
                    // Children
                    m_text = gameObject.transform.Find("Text");
                    m_button = gameObject.transform.Find("WindowMenu");
                    m_hologramButtonClicked = m_button.Find("ButtonHelp");
                }

                // Start is called before the first frame update
                void Start()
                {
                    // Callback
                    MATCH.Utilities.Utility.AddTouchCallback(m_hologramButtonClicked, callbackButtonHelpClicked);
                }

                void callbackButtonHelpClicked()
                {
                    //s_buttonClicked?.Invoke(this, EventArgs.Empty);
                    OnButtonClicked();

                    DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Clicked");
                }

                bool m_mutexShow = false;
                public override void Show(EventHandler eventHandler)
                {
                    if (m_mutexShow == false)
                    {
                        m_mutexShow = true;

                        MATCH.Utilities.Utility.AdjustObjectHeightToHeadHeight(transform);

                        m_text.gameObject.AddComponent<MATCH.Utilities.Animation>().AnimateAppearInPlace(new EventHandler(delegate (System.Object o, EventArgs e) {
                            EventHandler[] temp = new EventHandler[] {new EventHandler(delegate (System.Object oo, EventArgs ee) {
                    Destroy(m_button.gameObject.GetComponent<MATCH.Utilities.Animation>());
                    m_mutexShow = false;
            }), eventHandler };

                            m_button.gameObject.AddComponent<MATCH.Utilities.Animation>().AnimateAppearInPlace(temp);

                            Destroy(m_text.gameObject.GetComponent<MATCH.Utilities.Animation>());
                        }));
                    }
                }

                // With animation, compatible with the gradation manager
                bool m_mutexHide = false;
                public override void Hide(EventHandler eventHandler)
                {
                    if (m_mutexHide == false)
                    {
                        m_mutexHide = true;

                        m_text.gameObject.AddComponent<MATCH.Utilities.Animation>().AnimateDiseappearInPlace(new EventHandler(delegate (System.Object o, EventArgs e) {
                            EventHandler[] temp = new EventHandler[] {new EventHandler(delegate (System.Object oo, EventArgs ee) {
                    Destroy(m_button.gameObject.GetComponent<MATCH.Utilities.Animation>());
                    m_mutexHide = false;
            }), eventHandler };

                            m_button.gameObject.AddComponent<MATCH.Utilities.Animation>().AnimateDiseappearToPosition(m_button.position, temp);

                            Destroy(m_text.gameObject.GetComponent<Animation>());
                        }));
                    }
                }

                // Update is called once per frame
                void Update()
                {

                }
            }

        }
    }
}
