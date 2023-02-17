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

/**
 * Settings are applied to the first level of children
 **/
public class MouseUtilitiesHolograms : MonoBehaviour
{
    public bool m_showHideChildren = false;
    public bool m_lookAtUser = false;

    public bool m_useHeadHeightForPlacement = false; // Means that when the hologram becomes active, the hologram's height is adjusted to head's height

    bool m_headHeightAdjusted;

    // Start is called before the first frame update
    void Start()
    {
        m_headHeightAdjusted = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_lookAtUser)
        {
            if (Vector3.Distance(Camera.main.transform.position, transform.position) > 1)
            {
                gameObject.transform.LookAt(Camera.main.transform);
            }
        }

        if ( m_useHeadHeightForPlacement )
        {
            // The adjustment is made only the first frame when the hologram is made active
            if (gameObject.activeSelf && m_headHeightAdjusted == false)
            {
                m_headHeightAdjusted = true;

                MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Height adjusted for height of the head disabled for now. Here are some debug information: " + " Camera y position in world space: " + Camera.main.transform.position.y.ToString() + "  | object local position: " + gameObject.transform.localPosition.y.ToString());

                gameObject.transform.position = new Vector3(gameObject.transform.position.x, Camera.main.transform.position.y+gameObject.transform.localPosition.y, gameObject.transform.position.z);

                MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "New y position: " + gameObject.transform.position.y.ToString());
            }
            else if (gameObject.activeSelf == false && m_headHeightAdjusted == true)
            {
                m_headHeightAdjusted = false;
            }
        }
    }

    private void OnEnable()
    {
        if (m_showHideChildren)
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                gameObject.transform.GetChild(i).gameObject.SetActive(true);
            }
        }

    }

    private void OnDisable()
    {
        if (m_showHideChildren)
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                gameObject.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    /*
 * The position is given in parameter, as the "reference" position is the responsibility of the parent gameobject (most likely the one that is the chief orchestra, i.e. the responsible of the challenge)
 * */
    public void resetHologram(Vector3 position)
    {
        // Deactivate the window
        gameObject.SetActive(false);

        // Setting back the scaling and position to 1.0
        gameObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        gameObject.transform.localPosition = position;

        Destroy(gameObject.GetComponent<Animation>());
    }
}
