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

/*
 * This emits a signal if the hand's palm is detected. 
 * However, it is not recommended to use it, as there are many false positive with the Hololens algorithm.
 * */
public class MouseUtilitiesRefuseChallenge : MonoBehaviour
{
    int m_layerBitMask = 1 << 9; // I.e. only the object in the "Mouse" layer will be considered by the raycast.

    public event EventHandler m_eventChallengeRefused;

    bool m_palmFacingUser;
    bool m_statusEventTriggered;

    // Start is called before the first frame update
    void Start()
    {
        m_palmFacingUser = false;
        m_statusEventTriggered = false;

        gameObject.GetComponent<HandConstraintPalmUp>().OnFirstHandDetected.AddListener(delegate ()
        {
            m_palmFacingUser = true;

            MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Palm detected");
        });

        gameObject.GetComponent<HandConstraintPalmUp>().OnLastHandLost.AddListener(delegate ()
        {
            MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Palm not detected anymore");

            m_palmFacingUser = false;
        });
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(
                Camera.main.transform.position,
                Camera.main.transform.forward,
                out hitInfo,
                20.0f, m_layerBitMask))
        {
            // If the Raycast has succeeded and hit a hologram
            // hitInfo's point represents the position being gazed at
            // hitInfo's collider GameObject represents the hologram being gazed at
            if (m_palmFacingUser)
            {
                if (m_statusEventTriggered == false)
                {
                    m_eventChallengeRefused?.Invoke(this, EventArgs.Empty);
                    MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Event triggered, thanks to object: " + hitInfo.transform.gameObject.name);
                    m_statusEventTriggered = true;
                }
            }
            else
            {
                m_statusEventTriggered = false; // The palm is not opened anymore, so we reseat the boolean ensuring that only one event is trigerred when the palm is opened AND an object is focused by the user.
            }
        }
    }
}
