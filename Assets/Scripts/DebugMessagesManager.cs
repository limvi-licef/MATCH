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
using TMPro;
using System;
using System.Reflection;

/**
 * Manages the debug messages: can be sent to an hologram or to the console.
 * A filter to get the message only from certain classes is also implemented.
 * */
namespace MATCH
{
    public class DebugMessagesManager : MonoBehaviour
    {
        List<string> m_classNameFilter;

        public enum MessageLevel
        {
            Info,
            Warning,
            Error
        }

        public bool m_displayOnConsole;
        public bool m_displayMessages; // True: messages displayed; False otherwise

        private static DebugMessagesManager _instance;

        public static DebugMessagesManager Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                m_classNameFilter = new List<string>();

                // For now, filtering is hard coded
                //m_classNameFilter.Add("MouseChallengeCleanTableReminderOneClockMoving");
                //m_classNameFilter.Add("MouseUtilitiesGradationAssistanceManager");
                /*m_classNameFilter.Add("MouseUtilitiesHolograms");
                m_classNameFilter.Add("MouseCueing");*/

                m_displayMessages = true;

                _instance = this;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            AdminMenu.Instance.AddButton("Debug window - Bring", callbackDebugBringWindow);
            AdminMenu.Instance.AddButton("Debug window - Clear", callbackDebugClearWindow);
            AdminMenu.Instance.AddSwitchButton("Debug window - Display in console", callbackDebugDisplayDebugInWindow);

            if (m_classNameFilter.Count > 0)
            {
                displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Message filtering enabled. Only the messages from the following classes will be displayed: " + m_classNameFilter.ToString());
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void callbackDebugBringWindow()
        {
            gameObject.transform.position = new Vector3(Camera.main.transform.position.x + 0.5f, Camera.main.transform.position.y, Camera.main.transform.position.z);
            gameObject.transform.LookAt(Camera.main.transform);
            gameObject.transform.Rotate(new Vector3(0, 1, 0), 180);
        }

        public void callbackDebugClearWindow()
        {
            TextMeshPro temp = gameObject.GetComponent<TextMeshPro>();
            temp.SetText("");
        }

        public void callbackDebugDisplayDebugInWindow()
        {
            m_displayOnConsole = !m_displayOnConsole;
        }

        public void displayMessage(string className, string functionName, MessageLevel messageLevel, string message)
        {
            if (m_displayMessages)
            {
                if (m_classNameFilter.Count == 0 || m_classNameFilter.Contains(className))
                {
                    // Building message
                    string messageToDisplay = "[" + className + "::" + functionName + "] ";

                    switch (messageLevel)
                    {
                        case MessageLevel.Info:
                            messageToDisplay += "Info";
                            break;
                        case MessageLevel.Warning:
                            messageToDisplay += "Warning";
                            break;
                        case MessageLevel.Error:
                            messageToDisplay += "Error";
                            break;
                    }

                    messageToDisplay += " - " + message;

                    // Message is processed differently following if we want to have it shown in the console or in the Hololens
                    if (m_displayOnConsole)
                    {
                        switch (messageLevel)
                        {
                            case MessageLevel.Info:
                                Debug.Log(messageToDisplay);
                                break;
                            case MessageLevel.Warning:
                                Debug.LogWarning(messageToDisplay);
                                break;
                            case MessageLevel.Error:
                                Debug.LogError(messageToDisplay);
                                break;
                        }
                    }
                    else
                    {
                        TextMeshPro textMesh = gameObject.GetComponent<TextMeshPro>();
                        textMesh.SetText(textMesh.text + "\n" + messageToDisplay);
                    }
                }
            }
        }
    }

}
