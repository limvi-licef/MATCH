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
        public class ObjectLostFocused : Inference
        {
            GameObject ObjectToFocus;
            Utilities.HologramInteractions InteractionComponent;
            bool FocusOff;

            public ObjectLostFocused(string id, EventHandler callback, GameObject objectToMonitor) : base(id, callback)
            {
                FocusOff = false;

                ObjectToFocus = objectToMonitor;

                if (ObjectToFocus.TryGetComponent<Utilities.HologramInteractions>(out InteractionComponent) == false)
                {
                    InteractionComponent = ObjectToFocus.AddComponent<Utilities.HologramInteractions>();
                }

                InteractionComponent.EventFocusOn += CallbackFocusOn;
                InteractionComponent.EventFocusOff += CallbackFocusOff;
            }

            public override bool Evaluate()
            {
                return FocusOff;
            }

            void CallbackFocusOn(System.Object o, EventArgs e)
            {
                FocusOff = false;
            }

            void CallbackFocusOff(System.Object o, EventArgs e)
            {
                FocusOff = true;
            }

            public override void Unregistered()
            {

            }
        }
    }
}