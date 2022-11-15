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
using NPBehave;
using System;
using System.Reflection;
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;

namespace MATCH
{
    namespace Scenarios
    {
        namespace BehaviorTrees
        {
            public class BehaviorTree : MATCH.Scenarios.Scenario
            {
                protected NPBehave.Root Tree;
                protected Blackboard Conditions;
                private Dictionary<string, bool[]> ConditionsUpdate;

                protected Assistances.Dialog BehaviorTreeDebugWindow;
                protected Assistances.Dialog AssistancesDebugWindow;

                public virtual void Awake()
                {
                    Conditions = new Blackboard(UnityContext.GetClock());
                    ConditionsUpdate = new Dictionary<string, bool[]>();
                }

                // Start is called before the first frame update
                public virtual void Start()
                {
                    // Debug object to display the status of the BT conditions
                    BehaviorTreeDebugWindow = Assistances.Factory.Instance.CreateDialogNoButton("BT conditions - " + GetId(), "Empty for now", transform);
                    BehaviorTreeDebugWindow.Show(Utilities.Utility.GetEventHandlerEmpty());
                    BehaviorTreeDebugWindow.GetTransform().gameObject.AddComponent<ObjectManipulator>();
                    AdminMenu.Instance.AddButton("BT debug - " + GetId() + " - Bring", CallbackBringBTDebugWindow, AdminMenu.Panels.Left);
                    AdminMenu.Instance.AddSwitchButton("BT debug - " + GetId() + " - Hide", delegate ()
                    {
                        BehaviorTreeDebugWindow.gameObject.SetActive(!BehaviorTreeDebugWindow.gameObject.activeSelf);
                    }, AdminMenu.Panels.Left, AdminMenu.ButtonType.Hide);

                    AssistancesDebugWindow = Assistances.Factory.Instance.CreateDialogNoButton("Assistances status - " + GetId(), "Empty for now", transform);
                    AssistancesDebugWindow.Show(Utilities.Utility.GetEventHandlerEmpty());
                    AssistancesDebugWindow.GetTransform().gameObject.AddComponent<ObjectManipulator>();
                    AdminMenu.Instance.AddButton("Assistances debug - " + GetId() + " - Bring", CallbackBringAssistancesDebugWindow, AdminMenu.Panels.Left);
                    AdminMenu.Instance.AddSwitchButton("Assistances debug - " + GetId() + " - Hide", delegate ()
                    {
                        AssistancesDebugWindow.gameObject.SetActive(!AssistancesDebugWindow.gameObject.activeSelf);
                    }, AdminMenu.Panels.Left, AdminMenu.ButtonType.Hide);
                }

                public void CallbackBringBTDebugWindow()
                {
                    BehaviorTreeDebugWindow.transform.position = new Vector3(Camera.main.transform.position.x + 0.5f, Camera.main.transform.position.y, Camera.main.transform.position.z);
                }

                public void CallbackBringAssistancesDebugWindow()
                {
                    AssistancesDebugWindow.transform.position = new Vector3(Camera.main.transform.position.x + 0.5f, Camera.main.transform.position.y, Camera.main.transform.position.z);
                }

                protected void UpdateConditionWithMatrix(string id)
                {
                    if (ConditionsUpdate.ContainsKey(id))
                    {
                        bool[] values = ConditionsUpdate[id];
                        for (int i = 0; i < values.Length; i ++)
                        {
                            Conditions[Conditions.Keys[i]] = values[i];
                        }
                        UpdateTextDebugBehaviorTreeWindow();
                    }
                    else
                    {
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Condition not present in the dictionary, nothing will be done");
                    }
                }

                /**
                 * Does not use the condition update dictionary
                 */ 
                protected void UpdateCondition(string id, bool value)
                {
                    Conditions[id] = value;

                    UpdateTextDebugBehaviorTreeWindow();
                }

                private void UpdateTextDebugBehaviorTreeWindow()
                {
                    // Making the text to display
                    string textToDisplay = "Scenario: " + GetId();

                    foreach (string key in Conditions.Keys)
                    {
                        textToDisplay += "\n" + key + " = " + Conditions[key];
                    }

                    // Display the text
                    BehaviorTreeDebugWindow.SetDescription(textToDisplay, 0.08f);
                }

                /**
                 * All conditions will be set to the provided value, excepted for the key that will always set to true
                 */
                protected void AddConditionsUpdate(string key, bool value)
                {
                    int nbConditions = Conditions.Keys.Count;

                    bool[] values = new bool[nbConditions];

                    for (int i = 0; i < nbConditions; i ++)
                    {
                        values[i] = value;
                    }

                    AddConditionsUpdate(key, values);
                }

                protected void AddConditionsUpdate(string key, bool[] values)
                {
                    ConditionsUpdate.Add(key, values);
                }
            }
        }
    }
}