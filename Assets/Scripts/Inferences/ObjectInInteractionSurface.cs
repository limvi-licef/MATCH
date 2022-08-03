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
        public class ObjectInInteractionSurface : Inference
        {
            Assistances.InteractionSurface m_surface;
            Utilities.PhysicalObjectInformation m_objectdetected;
            BoxCollider m_Collider;

            string lastObject;

            public ObjectInInteractionSurface(string id, EventHandler callback, string objectName, Assistances.InteractionSurface surface) : base(id, callback)
            {
                m_surface = surface;

                //m_objectdetected = null;
                lastObject = null;
                m_objectdetected = null;
                //m_objectdetected.setObjectParams("TEst", new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0)); //FOR TEST

                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Inference Object Launched");
                //ObjectRecognition.ObjectInformation.Instance.UnregisterCallbackToObject(objectName);
                ObjectRecognition.ObjectInformation.Instance.RegisterCallbackToObject(objectName, callbackObjectDetection);
            }

            //In the evaluate, the last object is saved for avoid spam when an object is in a surface. The return true is sent just one time (the first time that the object is detected in the surface).
            public override bool Evaluate()
            {
                bool toReturn = false;
                m_Collider = m_surface.GetInteractionSurface().gameObject.GetComponent<BoxCollider>();
                //MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Evaluate ok");
                if (m_objectdetected != null)
                {
                    //MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Object name ok");
                    if (m_Collider.bounds.Contains(m_objectdetected.GetCenter())) //check if the center of the object is in the surface area
                    {
                        //MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "In collider");
                        if (m_objectdetected.GetObjectName() != lastObject) //last object for avoid spam when an object is in a surface
                        {
                            toReturn = true;
                            lastObject = m_objectdetected.GetObjectName();
                            //MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Finally, object detected : " + m_objectdetected.getObjectName() + " with the center " + m_objectdetected.getCenter() + ". Center of storage : " + m_Collider.bounds.center);
                        }
                    }
                    else // the object isn't in the surface
                    {
                        lastObject = null;
                    }
                }
                return toReturn;
            }

            public void callbackObjectDetection(System.Object o, EventArgs e)
            {
                Utilities.EventHandlerArgObject objectInfo = (Utilities.EventHandlerArgObject)e;

                m_objectdetected = new Utilities.PhysicalObjectInformation();
                m_objectdetected = objectInfo.ObjectDetected;

            }

            public override void Unregistered()
            {

            }
        }

    }
}

