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

public class MouseUtilitiesTimer : MonoBehaviour
{
    public int m_timerDuration = 2; // in Seconds
    bool m_timerStart;
    int m_timerDurationInternal; // To convert the seconds in FPS, as the timer uses the Update function to run

    public event EventHandler m_eventTimerFinished;

    // Start is called before the first frame update
    void Start()
    {
        m_timerStart = false;
        m_timerDurationInternal = m_timerDuration * 60;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_timerStart)
        {
            m_timerDurationInternal -= 1;

            if (m_timerDurationInternal <= 0)
            {
                MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Timer finished!");
               
                m_timerStart = false;
                m_timerDurationInternal = m_timerDuration * 60;

                m_eventTimerFinished?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public void startTimerOneShot()
    {
        if ( m_timerStart == false )
        {
            m_timerStart = true;
            m_timerDurationInternal = m_timerDuration * 60;

            MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Timer started");
        }
        else
        {
            MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Warning, "Timer already running, so will not be started - please call again this function when this timer will be finished");
        }
    }

    public void stopTimer()
    {
        if (m_timerStart)
        {
            m_timerStart = false;

        }
        else
        {
            MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Warning, "No timer currently running, so nothing to do");
        }
    }
}
