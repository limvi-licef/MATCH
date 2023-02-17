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
        public class ObjectDetected : Inferences.Inference
        {
            Utilities.PhysicalObjectInformation Objectdetected;
            string ObjectName;

            bool FakeObjectStatus; // Used in case you want to test in Unity rather than deploying in the Hololens. If == true then it will check if a given gameobject is in the field of view of the camera or not. If yes, then it will evaluate to true and return the appropriate arguments.
            GameObject FakeObject; // Game object that will be searched for detection in case the FakeObjectStatus bool is == true;

            public ObjectDetected(string id, EventHandler callback, string objectName): base(id, callback)
            {
                ObjectName = objectName;

                Objectdetected = null;

                #if OBJECT_RECOGNITION
                ObjectRecognition.ObjectInformation.Instance.RegisterCallbackToObject(objectName, CallbackObjectDetected);
#endif

                FakeObjectStatus = false;
            }

            public override bool Evaluate()
            {
                bool toReturn = false;
                if (Objectdetected != null)
                {
                    toReturn = true;
                    Objectdetected = null;
                }

                if (FakeObjectStatus)
                {
                    // From https://answers.unity.com/questions/720447/if-game-object-is-in-cameras-field-of-view.html
                    Vector3 screenPoint = Camera.main.WorldToViewportPoint(FakeObject.transform.position);
                    bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

                    if (onScreen)
                    {
                        //DebugMessagesManager.Instance.displayMessage("ObjectDetected", "Evaluate", DebugMessagesManager.MessageLevel.Info, "Fake object detected"); // Class and method names are hard coded for performance reasons.

                        Utilities.PhysicalObjectInformation temp = new Utilities.PhysicalObjectInformation();
                        Vector3 fakeObjectCenter = FakeObject.transform.position;
                        Vector3 fakeObjectScaling = FakeObject.transform.localScale;
                        Vector3 fakeObjectTopLeft = fakeObjectCenter - fakeObjectScaling /2.0f;
                        Vector3 fakeObjectBottomRight = fakeObjectCenter + fakeObjectScaling / 2.0f;
                        temp.SetObjectParams("FakeObject", fakeObjectCenter, fakeObjectTopLeft, fakeObjectBottomRight);
                        CallbackArgs = new Utilities.EventHandlerArgs.PhysicalObject(temp);
                        toReturn = true;
                    }
                }
                //DebugMessagesManager.Instance.displayMessage("ObjectDetected", "Evaluate", DebugMessagesManager.MessageLevel.Info, "Object not detected");
                return toReturn;
            }

            void CallbackObjectDetected(System.Object o, EventArgs e)
            {
                Utilities.EventHandlerArgs.PhysicalObject objectInfo = (Utilities.EventHandlerArgs.PhysicalObject)e;
                Objectdetected = objectInfo.ObjectDetected;
                CallbackArgs = new Utilities.EventHandlerArgs.PhysicalObject(Objectdetected);
            }

            public void EnableFakeObjectDetection(GameObject fakeObject)
            {
                FakeObject = fakeObject;
                FakeObjectStatus = true;
            }
            
            public override void Unregistered()
            {
             
            }
        }
    }
}