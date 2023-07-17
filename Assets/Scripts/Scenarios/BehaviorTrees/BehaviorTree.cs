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
            public abstract class BehaviorTree : MATCH.Scenarios.Scenario
            {
                private NPBehave.Root Tree;
                private Blackboard Conditions;
                private List<string> ConditionsIds; // To ease browsing of Conditions with updating the matrix
                public Dictionary<string, bool[]> ConditionsUpdate;

                private MATCH.Assistances.Dialogs.Dialog1 BehaviorTreeDebugWindow = null;
                private MATCH.Assistances.Dialogs.Dialog1 AssistancesDebugWindow = null;

                public virtual void Awake()
                {
                    Conditions = new Blackboard(UnityContext.GetClock());
                    ConditionsIds = new List<string>();
                    ConditionsUpdate = new Dictionary<string, bool[]>();
                }

                // Start is called before the first frame update
                public virtual void Start()
                {
                    InitializeDebugWindows();

                    // Debug object to display the status of the BT conditions
                    /*BehaviorTreeDebugWindow.Show(Utilities.Utility.GetEventHandlerEmpty(), false);
                    BehaviorTreeDebugWindow.GetTransform().gameObject.AddComponent<ObjectManipulator>();
                    AdminMenu.Instance.AddButton("BT debug - " + GetId() + " - Bring", CallbackBringBTDebugWindow, AdminMenu.Panels.Left);
                    AdminMenu.Instance.AddSwitchButton("BT debug - " + GetId() + " - Hide", delegate ()
                    {
                        BehaviorTreeDebugWindow.gameObject.SetActive(!BehaviorTreeDebugWindow.gameObject.activeSelf);
                    }, AdminMenu.Panels.Left, AdminMenu.ButtonType.Hide);

                    AssistancesDebugWindow.Show(Utilities.Utility.GetEventHandlerEmpty(), false);
                    AssistancesDebugWindow.GetTransform().gameObject.AddComponent<ObjectManipulator>();
                    AdminMenu.Instance.AddButton("Assistances debug - " + GetId() + " - Bring", CallbackBringAssistancesDebugWindow, AdminMenu.Panels.Left);
                    AdminMenu.Instance.AddSwitchButton("Assistances debug - " + GetId() + " - Hide", delegate ()
                    {
                        AssistancesDebugWindow.gameObject.SetActive(!AssistancesDebugWindow.gameObject.activeSelf);
                    }, AdminMenu.Panels.Left, AdminMenu.ButtonType.Hide);*/
                }

                /**
                 * Function to call when all the assistances have added to the object, i.e. to start the behavior tree
                 */
                public void Init()
                {
                    //InitializeDebugWindows(); // Debug windows are initialized in two places, because sometimes the Start function is called before the init and vice-versa

                    // Initialize the behavior tree
                    Tree = InitializeBehaviorTree();

                    // NP debugger
                    NPBehave.Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
                    debugger.BehaviorTree = Tree;

                    // Start BT
                    Tree.Start();
                }

                protected void AddCondition(string id, bool initialValue)
                {
                    Conditions[id] = initialValue;
                    ConditionsIds.Add(id);
                }

                protected int GetNumberOfConditions()
                {
                    return ConditionsIds.Count;
                }

                protected Blackboard Getconditions()
                {
                    return Conditions;
                }

                private void InitializeDebugWindows()
                {
                    if (BehaviorTreeDebugWindow == null)
                    {
                        BehaviorTreeDebugWindow = Assistances.Factory.Instance.CreateDialogNoButton("BT conditions - " + GetId(), "Empty for now", transform);
                    }

                    if (AssistancesDebugWindow == null)
                    {
                        AssistancesDebugWindow = Assistances.Factory.Instance.CreateDialogNoButton("Assistances status - " + GetId(), "Empty for now", transform);
                    }
                }

                protected void StartTree()
                {
                    /*if (Tree == null)
                    {
                        Start();
                    }
                    else*/ if (Tree.IsActive == false)
                    {
                        Tree.Start();
                    }

                    /*if (Tree != null && Tree.IsActive == false)
                    {
                        Tree.Start();
                    }*/
                    
                }

                protected void StopTree()
                {
                    if(Tree.IsActive)
                    {
                        Tree.Stop();
                    }
                }

                /*
                 * Should return the behavior tree root, that will be started by the Start function of this base class. Hence don't forget to call the base Start function of this class from your inherited class!
                 */
                protected abstract Root InitializeBehaviorTree();

                public void CallbackBringBTDebugWindow()
                {
                    BehaviorTreeDebugWindow.transform.position = new Vector3(Camera.main.transform.position.x + 0.5f, Camera.main.transform.position.y, Camera.main.transform.position.z);
                }

                public void CallbackBringAssistancesDebugWindow()
                {
                    AssistancesDebugWindow.transform.position = new Vector3(Camera.main.transform.position.x + 0.5f, Camera.main.transform.position.y, Camera.main.transform.position.z);
                }

                protected void SetConditionsTo(bool value)
                {
                    List<string> keys = new List<string>(ConditionsUpdate.Keys);

                    foreach(string key in keys)
                    {
                        Conditions[key] = value;
                    }
                       
                    UpdateTextDebugBehaviorTreeWindow();
                }

                protected void UpdateConditionWithMatrix(string id)
                {
                    if (ConditionsUpdate.ContainsKey(id))
                    {
                        bool[] values = ConditionsUpdate[id];
                        for (int i = 0; i < values.Length; i ++)
                        {
                            Conditions[/*Conditions.Keys[i]*/ConditionsIds[i]] = values[i];
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

                protected void UpdateTextAssistancesDebugWindow(string text)
                {
                    if (AssistancesDebugWindow != null)
                    {
                        AssistancesDebugWindow.SetDescription(text);
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, text);
                    }
                }

                private void UpdateTextDebugBehaviorTreeWindow()
                {
                    if (BehaviorTreeDebugWindow != null)
                    {
                        // Making the text to display
                        string textToDisplay = "Scenario: " + GetId();

                        foreach (string key in /*Conditions.Keys*/ConditionsIds)
                        {
                            textToDisplay += "\n" + key + " = " + Conditions[key];
                        }

                        // Display the text
                        BehaviorTreeDebugWindow.SetDescription(textToDisplay, 0.08f);
                        Debug.Log(textToDisplay);
                    }
                }

                /**
                 * All conditions will be set to the provided value, excepted for the key that will always set to true
                 */
                protected void AddConditionsUpdate(string key, bool value)
                {
                    int nbConditions = /*Conditions.Keys.Count*/ConditionsIds.Count;

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