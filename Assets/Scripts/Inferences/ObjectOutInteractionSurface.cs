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

        public class ObjectOutInteractionSurface : Inferences.Inference
        {
            Assistances.InteractionSurface Surface;
            Utilities.PhysicalObjectInformation Objectdetected;
            BoxCollider Collider;

            string LastObject;

            public ObjectOutInteractionSurface(string id, EventHandler callback, string objectName, Assistances.InteractionSurface surface) : base(id, callback)
            {
                Surface = surface;
                LastObject = null;
                Objectdetected = null;
                //ObjectRecognition.ObjectInformation.Instance.UnregisterCallbackToObject(objectName);
                ObjectRecognition.ObjectInformation.Instance.RegisterCallbackToObject(objectName, callbackObjectDetection);
            }

            public override bool Evaluate()
            {
                bool toReturn = false;
                Collider = Surface.GetInteractionSurface().gameObject.GetComponent<BoxCollider>();
                
                if (Objectdetected != null)
                {
                    if (!Collider.bounds.Contains(Objectdetected.GetCenter()))
                    {
                        if (Objectdetected.GetObjectName() != LastObject) //last object for avoid spam when an object is out of a surface
                        {
                            toReturn = true;
                            LastObject = Objectdetected.GetObjectName();
                        }
                    }
                    else // the object is in the surface
                    {
                        LastObject = null;
                    }
                }
                return toReturn;
            }

            public void callbackObjectDetection(System.Object o, EventArgs e)
            {
                Utilities.EventHandlerArgObject objectInfo = (Utilities.EventHandlerArgObject)e;
                Objectdetected = objectInfo.ObjectDetected;
                CallbackArgs = new Utilities.EventHandlerArgObject(Objectdetected);
            }

            public override void Unregistered()
            {

            }
        }

    }
}
