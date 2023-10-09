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
using WebSocketSharp;
using System;
using System.Reflection;

namespace MATCH
{
    namespace Utilities
    {
        // Inspired from: https://github.com/sta/websocket-sharp#websocket-server
        public class WebSocket : MonoBehaviour
        {
            WebSocketSharp.WebSocket Socket;

            public event EventHandler EventMessageReceived;
            private Queue<int> MessagesReceived;

            // Start is called before the first frame update
            void Start()
            {
                MessagesReceived = new Queue<int>();

            string webSocketAdress = "ws://192.168.1.100/MATCH";
                /*if (Utilities.Utility.IsEditorSimulator() || Utilities.Utility.IsEditorGameView())
                    {
                    webSocketAdress = "ws://192.168.1.100/MATCH";
                    }
                else
                    {
                    webSocketAdress = "ws://192.168.1.101/MATCH";
                    }*/

                Socket = new WebSocketSharp.WebSocket(webSocketAdress);

                Socket.OnMessage += delegate(System.Object sender, MessageEventArgs e)
                {
                    Debug.Log("Message received !!! Here is the message: " + e.Data.ToString());
                    //DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Message received !!! Here is the message: " + e.Data.ToString());
                    try
                    {
                        int data = Int32.Parse(e.Data.ToString());
                        MessagesReceived.Enqueue(data);
                    }
                    catch(Exception)
                    {
                        Socket.Send("I cannot interpret this message. Please send me an integer.\n");
                    }

                };
                
                Socket.Connect();
                Socket.Send("Please send me back one of the following number to enable the related functionality: \n 1.Enable tutorial in evaluation mode \n 2.Enable Dusting table scenario in evaluation mode\n 3. Run eye calibration\n Your wishes are my commands, so please enter your wish: \n");
            }

            // Update is called once per frame
            void Update()
            {
                if (MessagesReceived.Count > 0)
                {
                    int message = MessagesReceived.Dequeue();

                    Utilities.EventHandlerArgs.Int toEmit = new EventHandlerArgs.Int(message);
                    EventMessageReceived?.Invoke(this, toEmit);
                }
            }
        }
    }
}
