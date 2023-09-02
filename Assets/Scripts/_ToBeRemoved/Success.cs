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

/**
 * Simple assistance to congratulate the user when he successfully finished a challenge
 * */
namespace MATCH
{
    namespace Assistances
    {
        public class Success : MonoBehaviour
        {
            public EventHandler m_eventHologramTouched;

            // Start is called before the first frame update
            void Start()
            {
                // Callbacks
                MATCH.Utilities.Utility.AddTouchCallback(transform, delegate () { m_eventHologramTouched?.Invoke(this, EventArgs.Empty); });
            }

            // Update is called once per frame
            void Update()
            {

            }

            public void show(EventHandler eventHandler)
            {
                if (gameObject.activeSelf == false)
                {
                    Utilities.Animation animator = gameObject.AddComponent<Utilities.Animation>();

                    EventHandler[] eventHandlers = new EventHandler[] { new EventHandler(delegate (System.Object o, EventArgs e)
                    {
                        Destroy(animator);
                    }), eventHandler };

                    DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Target scaling: " + transform.localScale.ToString());

                    animator.AnimateAppearInPlaceToScaling(transform.localScale, eventHandlers);
                }
                else
                {
                    DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Success hologram is enabled - no hide action to take");
                }
            }

            public void hide(EventHandler eventHandler)
            {
                if (gameObject.activeSelf)
                {
                    DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Cube is going to be hidden");


                    EventHandler[] eventHandlers = new EventHandler[] { new EventHandler(delegate (System.Object o, EventArgs e)
               {
                   DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Cube should be hidden now");

                   gameObject.SetActive(false);
                   transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                   Destroy(GetComponent<Animation>());
               }), eventHandler };

                    gameObject.AddComponent<Utilities.Animation>().AnimateDiseappearInPlace(eventHandlers);
                }
                else
                {
                    DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Success hologram is disabled - no hide action to take");
                }
            }
        }
    }
}

