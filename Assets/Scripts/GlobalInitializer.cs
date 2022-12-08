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

/**
 * This class allows to tune some initialization parameters following if the compilation is done in the editor or for the Hololens
 * */
namespace MATCH
{
    public class GlobalInitializer : MonoBehaviour
    {
        public AdminMenu AdministrationMenu;
        public GameObject VirtualRoom;
        MATCH.Assistances.Dialog TodoList;
        public GameObject ObjectRecognition;
        public GameObject ObjectRecognitionInfoPanel;

        private void Awake()
        {
            TodoList = MATCH.Assistances.Factory.Instance.CreateToDoList("Choses � faire", ""); //create to do list
        }


        // Start is called before the first frame update
        void Start()
        {
            // Tuning parameters following if the software runs on the Unity editor or the Hololens
            if (Utilities.Utility.IsEditorSimulator() || Utilities.Utility.IsEditorGameView())
            {
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Editor simulator");
                AdminMenu.Instance.MenuStatic = true;
                if (ObjectRecognition != null)
                {
                    ObjectRecognition.SetActive(false);
                }
            }
            else
            { // Means running in the Hololens, so adjusting some parameters
                DebugMessagesManager.Instance.m_displayOnConsole = false;
                //AdministrationMenu.MenuStatic = false;
                AdminMenu.Instance.MenuStatic = false;
                VirtualRoom.SetActive(false); // In the editor, the user does what he wants, but in the hololens, this should surely be disabled.
            }

            /**
             * There is one and only one todo list to 
             * */
            AdminMenu.Instance.AddButton("Bring to do list window", delegate () { Utilities.Utility.BringObject(TodoList.transform); }); //add button to the admin menu
            AdminMenu.Instance.AddSwitchButton("Lock To Do List", CallbackLockToDo);
            InitializeTodoList();

            // Adding buttons to manage the object connecting to the server
            if (ObjectRecognitionInfoPanel != null)
            {
                /*if (ObjectRecognitionInfoPanel.activeSelf)
                {
                    ObjectRecognitionInfoPanel.SetActive(false);
                }*/
                AdminMenu.Instance.AddSwitchButton("Connection panel - Hide", delegate 
                {
                    ObjectRecognitionInfoPanel.SetActive(!ObjectRecognitionInfoPanel.activeSelf);
                }, AdminMenu.Panels.Middle, AdminMenu.ButtonType.Hide);

            }
        }

        void InitializeTodoList()
        {
            // First: check if some scenarios have been added, and if yes, add them to the GUI
            List<MATCH.Scenarios.Scenario> scenarios = MATCH.Scenarios.Manager.Instance.getScenarios();
            foreach (MATCH.Scenarios.Scenario scenario in scenarios)
            {
                AddScenarioToGUI(scenario);
            }

            // Second: be prepared in case new scenarios are added
            MATCH.Scenarios.Manager.Instance.s_scenarioAdded += CallbackNewScenarioInManager;
        }

        void CallbackNewScenarioInManager(System.Object o, EventArgs e)
        {
            DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called");

            AddScenarioToGUI(MATCH.Scenarios.Manager.Instance.getScenarios().Last());
        }

        /**
         * Thi function adds a button to the todolist GUI, and connects this button to the EventHandler of the scenario given an input parameter, so that the button background reflect the status of the scenario
         * */
        void AddScenarioToGUI(MATCH.Scenarios.Scenario scenario)
        {
            MATCH.Assistances.Buttons.Basic button = TodoList.AddButton(scenario.GetId(), true); //add button
            scenario.EventChallengeOnStart += button.CallbackSetButtonBackgroundCyan; //m_todo.callbackStartButton;
            scenario.EventChallengeOnSuccess += button.CallbackSetButtonBackgroundGreen; //m_todo.callbackCheckButton;
            scenario.EventChallengeOnStandBy += button.CallbackSetButtonBackgroundDefault; //m_todo.callbackCheckButton;
        }

        // Update is called once per frame
        void Update()
        {
            ToDoListConfig(); //config of the todo list for update the time
        }

        void ToDoListConfig()
        {
            string date = System.DateTime.Now.ToString("D", new System.Globalization.CultureInfo("fr-FR"));
            string hour = System.DateTime.Now.ToString("HH:mm");

            string textToDisplay = "Date : " + date + "                              Heure : " + hour + "\nSaison : " + GetSeason(System.DateTime.Now) + "\n\nT�ches � r�aliser : ";

            //if (textToDisplay != TodoList.GetDescription())
            //{ // To avoid updating the text at each frame
                TodoList.SetDescription(textToDisplay, 0.1f);
            //}
        }
        string GetSeason(DateTime date)
        {
            float value = (float)date.Month + date.Day / 100f;
            if (value < 3.21 || value >= 12.22) return "Hiver";
            else if (value < 6.21) return "Printemps";
            else if (value < 9.23) return "�t�";
            else return "Automne";
        }

        /**
         * To switch between a movable object or not
         * */
        void CallbackLockToDo()
        {
            if (TodoList.GetComponent<ObjectManipulator>().enabled)
                TodoList.GetComponent<ObjectManipulator>().enabled = false;
            else
                TodoList.GetComponent<ObjectManipulator>().enabled = true;
        }


    }
}