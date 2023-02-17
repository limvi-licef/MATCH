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
        public class TimeDidNotComeToObject : Inference
        {
            GameObject ObjectToMonitor;
            float Distance;
            bool MonitoringOngoing;
            DateTime StartTime;
            int NbSecondsBeforeTrigger;

            public TimeDidNotComeToObject(string id, EventHandler callback, GameObject gameObject, int nbSecondsBeforeTrigger, float distance = 1.5f): base(id, callback)
            {
                MonitoringOngoing = false;

                ObjectToMonitor = gameObject;
                Distance = distance;

                NbSecondsBeforeTrigger = nbSecondsBeforeTrigger;
            }

            public override bool Evaluate()
            {
                bool toReturn = false;

                float distance = Vector3.Distance(Camera.main.transform.position, ObjectToMonitor.transform.position);
                
                
                if (MonitoringOngoing == false && distance > Distance)
                {
                    //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Person far from object, starting counter");
                    MonitoringOngoing = true;
                    StartTime = DateTime.Now;
                }
                else if (distance < Distance)
                {
                    //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Person close to object, stop counter");
                    MonitoringOngoing = false;
                }
                else
                {
                    TimeSpan elapsed = DateTime.Now.Subtract(StartTime);
                    if (elapsed.Seconds >= NbSecondsBeforeTrigger)
                    {
                        toReturn = true;
                    }
                }

                return toReturn;
            }

            public override void Unregistered()
            {

            }
        }
    }
}