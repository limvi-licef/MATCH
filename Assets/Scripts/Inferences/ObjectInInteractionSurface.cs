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
        public class ObjectInInteractionSurface : Inference
        {
            readonly Assistances.InteractionSurface m_surface;
            Utilities.PhysicalObjectInformation m_objectdetected;
            readonly BoxCollider m_Collider;

            /**
             * id: name of the inference
             * callback: function called when the inference is realized
             * objectName: name of the object to detect. Must be written as the name returned by the code performing the object recognition.
             * surface: surface where the presence of the object will be checked
             * */
            public ObjectInInteractionSurface(string id, EventHandler callback, string objectName, Assistances.InteractionSurface surface) : base(id, callback)
            {
                m_surface = surface;

                m_objectdetected = null;

                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Inference Object Launched");
#if OBJECT_RECOGNITION
                ObjectRecognition.ObjectInformation.Instance.RegisterCallbackToObject(objectName, callbackObjectDetection);
#endif

                m_Collider = m_surface.GetInteractionSurface().gameObject.GetComponent<BoxCollider>(); // Can't this be in the constructor?
            }

            //In the evaluate, the last object is saved for avoid spam when an object is in a surface. The return true is sent just one time (the first time that the object is detected in the surface).
            public override bool Evaluate()
            {
                bool toReturn = false;
                
                if (m_objectdetected != null && m_Collider.bounds.Contains(m_objectdetected.GetCenter()))
                {
                    toReturn = true;
                }
                return toReturn;
            }

            /**
             * Callback supposed to be called when the object is detected
             * */
            public void callbackObjectDetection(System.Object o, EventArgs e)
            {
                Utilities.EventHandlerArgs.PhysicalObject objectInfo = (Utilities.EventHandlerArgs.PhysicalObject)e;

                m_objectdetected = new Utilities.PhysicalObjectInformation();
                m_objectdetected = objectInfo.ObjectDetected;

            }

            public override void Unregistered()
            {

            }
        }

    }
}

