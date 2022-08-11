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
        public class Timer : Inference
        {
            DateTime Time;
            bool ImmediateTrigger;
            bool StartTimer;
            int Seconds;

            public Timer(string id, int seconds, EventHandler callback) : base(id, callback)
            {
                Seconds = seconds;

                ImmediateTrigger = false;
                StartTimer = false;

                AdminMenu.Instance.AddButton("Immediate trigger for " + id, CallbackImmediateTrigger);

            }

            public override bool Evaluate()
            {
                bool toReturn = false;

                if (ImmediateTrigger == false && StartTimer)
                {
                    TimeSpan elapsed = DateTime.Now.Subtract(Time);

                    if ( elapsed.Seconds >= Seconds)
                    {
                        ImmediateTrigger = true;
                    }
                }
                
                if (ImmediateTrigger)
                {
                    ImmediateTrigger = false;
                    StartTimer = false;
                    toReturn = true;
                    //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Timer triggered");
                }


                return toReturn;
            }

            public void StartCounter()
            {
                Time = DateTime.Now;
                StartTimer = true;
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Starting timer");
            }

            public void StopCounter()
            {
                StartTimer = false;
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Stoping counter");
            }

            public void CallbackImmediateTrigger()
            {
                ImmediateTrigger = true;
            }

            public override void Unregistered()
            {
                
            }
        }
    }
}