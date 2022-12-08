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

namespace MATCH
{
    namespace Inferences
    {
        public class ObjectFocused : Inference
        {
            GameObject ObjectToFocus;
            Utilities.HologramInteractions InteractionComponent;
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

                if (ObjectToFocus.TryGetComponent<Utilities.HologramInteractions>(out InteractionComponent) == false)
                {
                     InteractionComponent = ObjectToFocus.AddComponent<Utilities.HologramInteractions>();
                }

                SecondsToFocus = secondsToFocus;

                InteractionComponent.EventFocusOn += CallbackFocusOn;
                InteractionComponent.EventFocusOff += CallbackFocusOff;
            }

            public override bool Evaluate()
            {
                bool toReturn = false;

                if (FocusOn)
                {
                    TimeSpan elapsed = DateTime.Now.Subtract(StartTime);
                    if (elapsed.Seconds >= SecondsToFocus)
                    {
                        //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Object focused !!!");
                        toReturn = true;
                    }
                }

                return toReturn;
            }

            void CallbackFocusOn(System.Object o, EventArgs e)
            {
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Object focused");
                FocusOn = true;
                StartTime = DateTime.Now;
            }

            void CallbackFocusOff(System.Object o, EventArgs e)
            {
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Object not focused anymore");
                FocusOn = false;
            }

            public override void Unregistered()
            {
                InteractionComponent.EventFocusOn -= CallbackFocusOn;
                InteractionComponent.EventFocusOff -= CallbackFocusOff;
            }
        }
    }
}