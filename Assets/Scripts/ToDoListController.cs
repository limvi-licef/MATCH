/*Copyright 2022 Louis Marquet, L�ri Lamour, Aur�lie Le Guidec, Guillaume Merviel.

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
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;
using System.Reflection;
using System.Linq;
using TMPro;

namespace MATCH
{
    public class ToDoListController : MonoBehaviour
    {
        MATCH.Assistances.Dialogs.Dialog1 ToDoList;
        private void Awake()
        {
            //create to do list
            //ToDoList = MATCH.Assistances.Factory.Instance.CreateToDoList("Choses � faire", ""); --> �a ne marche pas lol 
        }
        void Start()
        {
            ToDoList = MATCH.Assistances.Factory.Instance.CreateToDoList("Choses � faire", "");
            //AdminMenu.Instance.AddButton("Bring ToDo List window", callbackBringAgenda);
            AdminMenu.Instance.AddButton("Bring to do list window", delegate () { Utilities.Utility.BringObject(ToDoList.transform); }); //add button to the admin menu
            AdminMenu.Instance.AddSwitchButton("Lock To Do List", CallbackLockToDo);
            InitializeTodoList();
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
        /**
         * return local date 
         * */
        public static string GetSeason(DateTime date) // public static (?)
        {
            float value = (float)date.Month + date.Day / 100f;
            if (value < 3.21 || value >= 12.22) return "Hiver";
            else if (value < 6.21) return "Printemps";
            else if (value < 9.23) return "�t�";
            else return "Automne";
        }
        void CallbackNewScenarioInManager(System.Object o, EventArgs e)
        {
            DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called");

            AddScenarioToGUI(MATCH.Scenarios.Manager.Instance.getScenarios().Last());
        }

        /**
         * This function adds a button to the todolist GUI, and connects this button to the EventHandler of the scenario given an input parameter, so that the button background reflect the status of the scenario
         * */
        void AddScenarioToGUI(MATCH.Scenarios.Scenario scenario)
        {
            MATCH.Assistances.Buttons.Basic button = ToDoList.AddButton(scenario.GetId(), true); //add button
            scenario.EventChallengeOnStart += button.CallbackSetButtonBackgroundCyan; //m_todo.callbackStartButton;
            scenario.EventChallengeOnSuccess += button.CallbackSetButtonBackgroundGreen; //m_todo.callbackCheckButton;
            scenario.EventChallengeOnStandBy += button.CallbackSetButtonBackgroundDefault; //m_todo.callbackCheckButton;
        }
        /**
        * To switch between a movable object or not
        * */
        void CallbackLockToDo()
        {
            if (ToDoList.GetComponent<ObjectManipulator>().enabled)
                ToDoList.GetComponent<ObjectManipulator>().enabled = false;
            else
                ToDoList.GetComponent<ObjectManipulator>().enabled = true;
        }
    }
}