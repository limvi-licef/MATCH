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

/**
 * This class aims at having a similar role than the Interaction MRTK component, but with providing the name of the sender with the interaction event.
 * */
namespace MATCH
{
    namespace Utilities
    {
        public class HologramInteractions : /*MonoBehaviour*/BaseEyeFocusHandler, IMixedRealityTouchHandler, IMixedRealityFocusHandler
        {
            public event EventHandler EventTouched;
            public event EventHandler EventFocusOn;
            public event EventHandler EventFocusOff;

            // Start is called before the first frame update
            void Start()
            {

            }

            void IMixedRealityTouchHandler.OnTouchStarted(HandTrackingInputEventData eventData)
            {
                EventTouched?.Invoke(this, EventArgs.Empty);
            }

            // Here because it has to be to complete the implementation of the interface
            void IMixedRealityTouchHandler.OnTouchCompleted(HandTrackingInputEventData eventData) { }
            void IMixedRealityTouchHandler.OnTouchUpdated(HandTrackingInputEventData eventData) { }

            public override void OnFocusEnter(FocusEventData eventData)
            {
               // DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Object focused");
                EventFocusOn?.Invoke(this.gameObject, EventArgs.Empty);
            }

            public override void OnFocusExit(FocusEventData eventData)
            {
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Object not focused anymore");
                EventFocusOff?.Invoke(this.gameObject, EventArgs.Empty);
            }

            // Not sure this works - at least I was not able to make it working on the Unity editor
            protected override void OnEyeFocusStart()
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Object focused by eye tracking");
            }

            // Not sure this works - at least I was not able to make it working on the Unity editor
            protected override void OnEyeFocusStay()
            {
                base.OnEyeFocusStay();
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Object focused by eye tracking");
            }

            //public void On
        }

    }
}

