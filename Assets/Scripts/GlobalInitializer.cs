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

using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;
using System.Reflection;
using System.Linq;

/**
 * This class allows to tune some initialization parameters following if the compilation is done in the editor or for the Hololens
 * */
namespace MATCH
{
    public class GlobalInitializer : MonoBehaviour
    {
        public AdminMenu AdministrationMenu;
        public GameObject VirtualRoom;
        public GameObject ObjectRecognition;
        public GameObject ObjectRecognitionInfoPanel;
        //public GameObject WebSocket;
        //public GameObject Scenarios;

        // Start is called before the first frame update
        void Start()
        {
            //TodoList = MATCH.Assistances.Factory.Instance.CreateToDoList("Choses � faire", "", transform.parent); //create to do list

            // Tuning parameters following if the software runs on the Unity editor or the Hololens
            if (Utilities.Utility.IsEditorSimulator() || Utilities.Utility.IsEditorGameView())
            {
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Editor simulator");
                AdminMenu.Instance.MenuStatic = true;

#if !OBJECT_RECOGNITION
                /*if (ObjectRecognition != null)
                {
                    ObjectRecognition.SetActive(false);
                }*/
#endif

            }
            else
            { // Means running in the Hololens, so adjusting some parameters
                DebugMessagesManager.Instance.DisplayOnConsole = false;
                DebugMessagesManager.Instance.DisplayMessages = false; // Means the software runs in the hololens, and for better performance, we disable the display of debug messages by default
                DebugMessagesManager.Instance.ShowWindow(false); // Window is hidden by default if running on the hololens

                //AdministrationMenu.MenuStatic = false;
                AdminMenu.Instance.MenuStatic = false;
                VirtualRoom.SetActive(false); // In the editor, the user does what he wants, but in the hololens, this should surely be disabled.
            }

            /**
             * There is one and only one todo list to 
             * */
            /*AdminMenu.Instance.AddButton("Bring to do list window", delegate () { Utilities.Utility.BringObject(TodoList.transform); }); //add button to the admin menu
            AdminMenu.Instance.AddSwitchButton("Lock To Do List", CallbackLockToDo);
            InitializeTodoList();*/

            // Adding buttons to manage the object connecting to the server

            if (ObjectRecognitionInfoPanel != null)
            {
#if !OBJECT_RECOGNITION
                if (ObjectRecognitionInfoPanel.activeSelf)
                {
                    ObjectRecognitionInfoPanel.SetActive(false);
                }
#endif

#if OBJECT_RECOGNITION
                AdminMenu.Instance.AddSwitchButton("Connection panel - Hide", delegate
                {
                    ObjectRecognitionInfoPanel.SetActive(!ObjectRecognitionInfoPanel.activeSelf);
                }, AdminMenu.Panels.Middle, AdminMenu.ButtonType.Hide);
#endif
            }

            // Disable teleport system
            CoreServices.TeleportSystem.Disable();
        }


    }
}