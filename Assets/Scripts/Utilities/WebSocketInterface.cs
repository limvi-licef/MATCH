/*Copyright 2023 Guillaume Spalla

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
using System;
using Microsoft.MixedReality.Toolkit.Input;

namespace MATCH
{
    namespace Utilities
    {
        public class WebSocketInterface : MonoBehaviour
        {
            Transform AdminMenu;
            Transform WebSocket;
            Transform Scenarios;

            // Start is called before the first frame update
            void Start()
            {
                // Get the transform we need
                Scenarios = transform.root.Find("Scenarios").Find("BehaviorTrees");
                AdminMenu = transform.root.Find("AdminMenu");
                WebSocket = transform.root.Find("WebSocket");


                // Initialize the application if messages are received from the web socket
                //Transform match = transform.root.Find("MATCH");

                Utilities.WebSocket webSocket = WebSocket.GetComponent<Utilities.WebSocket>();
                webSocket.EventMessageReceived += delegate (System.Object o, EventArgs e)
                {
                    Utilities.EventHandlerArgs.Int arg = (Utilities.EventHandlerArgs.Int)e;
                    CallbackMessageReceived(arg.Data);
                };
            }

            void CallbackMessageReceived(int message)
            {
                switch (message)
                {
                    case 1:
                        StartTutorial();
                        break;
                    case 2:

                        StartDustingTable();
                        break;
                    case 3:
                        StartEyeCalibration();
                        break;
                    default:
                        break;
                }
            }    

            void StartTutorial()
            {
                ShowAdminMenu(false);
                HideAllScenario();
                ShowScenario("Tutorial", true);
            }

            void StartDustingTable()
            {
                ShowAdminMenu(false);
                HideAllScenario();
                ShowScenario("DustingTable", true);
            }

            void ShowAdminMenu(bool show)
            {
                AdminMenu.gameObject.SetActive(show);
                //MATCH.AdminMenu adminMenuController = AdminMenu.GetComponent<MATCH.AdminMenu>();
                /*AdminMenu.GetComponent<MeshRenderer>().enabled = show;
                AdminMenu.Find("PanelLeft").GetComponent<MeshRenderer>().enabled = show;
                AdminMenu.Find("PanelRight").GetComponent<MeshRenderer>().enabled = show;
                AdminMenu.Find("PanelMiddle").GetComponent<MeshRenderer>().enabled = show;*/
            }

            void HideAllScenario()
            {
                int nbScenarios = Scenarios.childCount;

                for (int i = 0; i < nbScenarios; i++)
                {
                    Scenarios.GetChild(i).gameObject.SetActive(false);
                }
            }

            void ShowScenario(string id, bool show)
            {
                PointerUtils.SetHandRayPointerBehavior(PointerBehavior.AlwaysOff);
                Scenarios.Find(id).gameObject.SetActive(show);
            }

            // From: https://stackoverflow.com/questions/56877610/can-i-trigger-the-hololens-calibration-sequence-from-inside-my-application
            void StartEyeCalibration()
            {
#if WINDOWS_UWP
    UnityEngine.WSA.Application.InvokeOnUIThread(async () =>
    {
        bool result = await global::Windows.System.Launcher.LaunchUriAsync(new System.Uri("ms-hololenssetup://EyeTracking"));
        if (!result)
        {
            Debug.LogError("Launching URI failed to launch.");
        }
    }, false);
#else
                Debug.LogError("Launching eye tracking not supported Windows UWP");
#endif
            }
        }
    }
}

