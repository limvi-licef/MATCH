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
        List<string> ClassNameFilter;

        public enum MessageLevel
        {
            Info,
            Warning,
            Error
        }

        public bool DisplayOnConsole;
        public bool DisplayMessages; // True: messages displayed; False otherwise

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
                ClassNameFilter = new List<string>();

                DisplayMessages = true;

                _instance = this;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            AdminMenu.Instance.AddButton("Debug window - Bring", CallbackDebugBringWindow);
            AdminMenu.Instance.AddButton("Debug window - Clear", CallbackDebugClearWindow);
            AdminMenu.Instance.AddSwitchButton("Debug window - Display in console", CallbackDebugDisplayDebugInWindow);
            AdminMenu.Instance.AddSwitchButton("Debug - Display messages", CallbackDisplayMessages);

            if (ClassNameFilter.Count > 0)
            {
                DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Message filtering enabled. Only the messages from the following classes will be displayed: " + ClassNameFilter.ToString());
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        void CallbackDebugBringWindow()
        {
            gameObject.transform.position = new Vector3(Camera.main.transform.position.x + 0.5f, Camera.main.transform.position.y, Camera.main.transform.position.z);
            gameObject.transform.LookAt(Camera.main.transform);
            gameObject.transform.Rotate(new Vector3(0, 1, 0), 180);
        }

        void CallbackDebugClearWindow()
        {
            TextMeshPro temp = gameObject.GetComponent<TextMeshPro>();
            temp.SetText("");
        }

        void CallbackDebugDisplayDebugInWindow()
        {
            DisplayOnConsole = !DisplayOnConsole;
        }

        void CallbackDisplayMessages()
        {
            DisplayMessages = !DisplayMessages;
        }

        public void DisplayMessage(string className, string functionName, MessageLevel messageLevel, string message)
        {
                if (DisplayMessages)
                {
                    if (ClassNameFilter.Count == 0 || ClassNameFilter.Contains(className))
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
                        if (DisplayOnConsole)
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
                        //TextMeshPro textMesh = gameObject.GetComponent<TextMeshPro>();
                        Transform eyeScroll = transform.Find("Eye Scroll");
                        Transform canvas = eyeScroll.Find("Canvas");
                        Transform scrollView = canvas.Find("Scroll View");
                        Transform viewport = scrollView.Find("Viewport");
                        Transform textMeshPro = viewport.Find("TextMeshPro Text");
                        TextMeshProUGUI textMesh = textMeshPro.GetComponent<TextMeshProUGUI>();
                        textMesh.SetText(textMesh.text + "\n" + messageToDisplay);
                        }
                    }
                }
            }
        }
    }