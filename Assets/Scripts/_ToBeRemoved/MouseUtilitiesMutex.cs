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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System;
using System.Reflection;
using System.Linq;

/**
 * Basic mutex, which as from a real mutex basically just the name. Don't expect that to solve all your issues. Used here to filter several request to a function that has not yet finished its process.
 * */
public class MouseUtilitiesMutex
{
    bool m_mutex;

    public MouseUtilitiesMutex ()
    {
        m_mutex = false;
    }

    public void lockMutex()
    {
        m_mutex = true;
        MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Mutex locked");
    }

    public void unlockMutex()
    {
        m_mutex = false;
        MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Mutex unlocked");
    }

    public bool isLocked()
    {
        return m_mutex;
    }

    /*
     * Returns a handler unlocking the mutex when invoked
     * */
    EventHandler unlockMutexHandler()
    {
        return new EventHandler(delegate (System.Object o, EventArgs e)
       {
           m_mutex = false;
           MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Mutex unlocked - from handler");
       });
    }
}
