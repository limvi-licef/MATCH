/*Copyright 2022 Louis Marquet

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

        public class ObjectOutInteractionSurface : Inferences.Inference
        {
            readonly Assistances.InteractionSurface Surface;
            Utilities.PhysicalObjectInformation Objectdetected;
            readonly BoxCollider Collider;

            public ObjectOutInteractionSurface(string id, EventHandler callback, string objectName, Assistances.InteractionSurface surface) : base(id, callback)
            {
                Surface = surface;
                Objectdetected = null;

#if OBJECT_RECOGNITION
                ObjectRecognition.ObjectInformation.Instance.RegisterCallbackToObject(objectName, CallbackObjectDetection);
#endif

                Collider = Surface.GetInteractionSurface().gameObject.GetComponent<BoxCollider>();
            }

            public override bool Evaluate()
            {
                bool toReturn = false;
                
                if (Objectdetected != null && !Collider.bounds.Contains(Objectdetected.GetCenter()))
                {
                    toReturn = true;
                }
                return toReturn;
            }

            public void CallbackObjectDetection(System.Object o, EventArgs e)
            {
                Utilities.EventHandlerArgs.PhysicalObject objectInfo = (Utilities.EventHandlerArgs.PhysicalObject)e;
                Objectdetected = objectInfo.ObjectDetected;
                CallbackArgs = new Utilities.EventHandlerArgs.PhysicalObject(Objectdetected);
            }

            public override void Unregistered()
            {

            }
        }

    }
}
