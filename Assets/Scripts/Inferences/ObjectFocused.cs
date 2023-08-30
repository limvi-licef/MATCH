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
using Microsoft.MixedReality.Toolkit.Input;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;
using System.Reflection;
using System.Linq;

namespace MATCH
{
    namespace Inferences
    {
        public class ObjectFocused : Inference
        {
            GameObject ObjectToFocus;
            //Utilities.HologramInteractions InteractionComponent;
            EyeTrackingTarget EyeTracker;
            int SecondsToFocus = 3;

            DateTime StartTime;
            bool FocusOn;

            public ObjectFocused(string id, EventHandler callback, GameObject objectToFocus, int secondsToFocus): base(id, callback)
            {
                FocusOn = false;

                ObjectToFocus = objectToFocus;

                if (ObjectToFocus.GetComponent<BoxCollider>() == false)
                {
                    ObjectToFocus.AddComponent<BoxCollider>();
                }

                /*if (ObjectToFocus.TryGetComponent<Utilities.HologramInteractions>(out InteractionComponent) == false)
                {
                     InteractionComponent = ObjectToFocus.AddComponent<Utilities.HologramInteractions>();
                }*/

                if(ObjectToFocus.TryGetComponent<EyeTrackingTarget>(out EyeTracker) == false)
                {
                    //EyeTracker = ObjectToFocus.AddComponent<EyeTrackingTarget>();
                    MATCH.DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Error, "Cannot instanciate the inference, as the EyeTrackingTarget component is missing for the target object");
                }

                //EyeTracker = ObjectToFocus.GetComponent<EyeTrackingTarget>();

                SecondsToFocus = secondsToFocus;

                /*InteractionComponent.EventFocusOn += CallbackFocusOn;
                InteractionComponent.EventFocusOff += CallbackFocusOff;

                EyeTracker.OnLookAtStart.AddListener(delegate
                {
                    CallbackFocusOn(this, EventArgs.Empty);
                });

                EyeTracker.OnLookAway.AddListener(delegate
                {
                    CallbackFocusOff(this, EventArgs.Empty);
                });*/
                /*InteractionComponent.EyeFocusOn += CallbackFocusOn;
                InteractionComponent.EyeFocusOff += CallbackFocusOff;*/
                EyeTracker.OnLookAtStart.AddListener(CallbackFocusOn);
                EyeTracker.OnLookAway.AddListener(CallbackFocusOff);
            }

            public override bool Evaluate()
            {
                bool toReturn = false;

                if (FocusOn)
                {
                    TimeSpan elapsed = DateTime.Now.Subtract(StartTime);
                    if (elapsed.Seconds >= SecondsToFocus)
                    {
                        DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Callback object focused triggered");
                        toReturn = true;
                    }
                }

                return toReturn;
            }

            void CallbackFocusOn()
            {
                CallbackFocusOn(this, EventArgs.Empty);
            }

            void CallbackFocusOff()
            {
                CallbackFocusOff(this, EventArgs.Empty);
            }

            void CallbackFocusOn(System.Object o, EventArgs e)
            {
                DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Object focused on");
                FocusOn = true;
                StartTime = DateTime.Now;
            }

            void CallbackFocusOff(System.Object o, EventArgs e)
            {
                DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Object not focused anymore");
                FocusOn = false;
            }

            public override void Unregistered()
            {
                //InteractionComponent.EventFocusOn -= CallbackFocusOn;
                //InteractionComponent.EventFocusOff -= CallbackFocusOff;
                EyeTracker.OnLookAtStart.RemoveAllListeners();
                EyeTracker.OnLookAway.RemoveAllListeners();
            }
        }
    }
}