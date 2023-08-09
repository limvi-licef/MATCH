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
using System.Linq;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;

/**
     * Describes the hologram that is used to populate the surface to clean.
     * Emits an event when touched
     * */
namespace MATCH
{
    namespace Assistances
    {
        public class ProcessingSurfaceElement : MonoBehaviour, IMixedRealityTouchHandler
        {
            public Material MatWhenTouched;
            public event EventHandler CubeTouchedEvent;

            void IMixedRealityTouchHandler.OnTouchStarted(HandTrackingInputEventData eventData)
            {
                //DebugMessagesManager.Instance.displayMessage("MouseCubeInteractions", "IMixedRealityTouchHandler.OnTouchStarted", DebugMessagesManager.MessageLevel.Info, "Touched");

                gameObject.GetComponent<Renderer>().material =  MatWhenTouched;
                CubeTouchedEvent?.Invoke(this, EventArgs.Empty);
            }

            public void SetDefaultColor(string name)
            {
                Renderer renderer = GetComponent<Renderer>();
                renderer.material = Utilities.Utility.LoadMaterial(name);
            }

            public void SetTouchedColor(string name)
            {
                MatWhenTouched = Utilities.Utility.LoadMaterial(name);
            }


            // Here because it has to be to complete the implementation of the interface
            void IMixedRealityTouchHandler.OnTouchCompleted(HandTrackingInputEventData eventData) { }
            void IMixedRealityTouchHandler.OnTouchUpdated(HandTrackingInputEventData eventData) { }
        }

    }
}

