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
        public class Time : Inference
        {
            readonly DateTime m_time;
            bool m_useOneMinuteTrigger;
            DateTime m_timeOneMinuteTrigger;

            public Time(string id, DateTime time, EventHandler callback) : base(id, callback)
            {
                m_time = time;

                m_useOneMinuteTrigger = false;

                AdminMenu.Instance.AddButton("One minute trigger for " + id, CallbackOneMinuteTrigger);

            }

            public Time(string id, DateTime time) : base(id)
            {
                m_time = time;

                AdminMenu.Instance.AddButton("One minute trigger for " + id, CallbackOneMinuteTrigger);
            }

            public override bool Evaluate()
            {
                bool toReturn = false;

                if (m_useOneMinuteTrigger == false)
                {
                    if (DateTime.Now.Hour == m_time.Hour && DateTime.Now.Minute == m_time.Minute)
                    {
                        toReturn = true;
                    }
                }
                else
                {
                    TimeSpan elapsed = DateTime.Now.Subtract(m_timeOneMinuteTrigger);

                    if ( /*elapsed.Minutes >= 1*/ elapsed.Seconds >= 10)
                    {
                        //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called");

                        m_useOneMinuteTrigger = false;
                        toReturn = true;
                    }
                }


                return toReturn;
            }

            public void CallbackOneMinuteTrigger()
            {
                DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Callback one minute triggered called");

                m_timeOneMinuteTrigger = DateTime.Now;
                m_useOneMinuteTrigger = true;
            }

            public override void Unregistered()
            {

            }
        }
    }
}