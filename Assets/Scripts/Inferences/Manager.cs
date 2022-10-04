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
 * Inherits MonoBehaviour because we want the inferences to be evaluated at each frame
 * Inferences are evaluated each frame: your responsibility to manage the triggers, and to unregister when it's ok for you.
 * Based more or less on the observer pattern.
 * */
namespace MATCH
{
    namespace Inferences
    {
        public class Manager : MonoBehaviour
        {
            Dictionary<string, Inference> InferencesStorage;

            Assistances.Dialog BehaviorTreeDebugWindow;

            private void Awake()
            {
                InferencesStorage = new Dictionary<string, Inference>();
            }

            private void Start()
            {
                // Debug object to display the status of the BT conditions
                BehaviorTreeDebugWindow = Assistances.Factory.Instance.CreateDialogNoButton("Inferences status", "Empty for now", transform);
                BehaviorTreeDebugWindow.Show(Utilities.Utility.GetEventHandlerEmpty());
                BehaviorTreeDebugWindow.GetTransform().gameObject.AddComponent<ObjectManipulator>();
                MATCH.AdminMenu.Instance.AddButton("Bring inferences status", CallbackBringWindow, AdminMenu.Panels.Obstacles);
            }

            public void CallbackBringWindow()
            {
                BehaviorTreeDebugWindow.transform.position = new Vector3(Camera.main.transform.position.x + 0.5f, Camera.main.transform.position.y, Camera.main.transform.position.z);
            }

            void UpdateInferencesStatus(List<string> ids, List<bool> evaluations)
            {
                // Making the text to display
                string textToDisplay = "";

                for ( int i = 0; i < ids.Count; i ++)
                {
                    textToDisplay += ids[i] + " = " + evaluations[i] + "\n";
                }

                // Display the text
                BehaviorTreeDebugWindow.SetDescription(textToDisplay, 0.08f);
            }

            // Update is called once per frame
            void Update()
            {
                try
                {
                    List<string> ids = new List<string>();
                    List<bool> evaluations = new List<bool>();

                    bool atLeastOneEval = false;

                    foreach (KeyValuePair<string, Inference> inference in InferencesStorage)
                    {
                        bool eval = inference.Value.Evaluate();

                        ids.Add(inference.Key);
                        evaluations.Add(eval);

                        // Mettre tout ça dans un thread? Pour pas que ce soit bloquant?
                        if (eval)
                        {
                            inference.Value.TriggerCallback();

                            atLeastOneEval = true;
                        }
                    }

                    if (atLeastOneEval)
                    {
                        UpdateInferencesStatus(ids, evaluations); // Update the display only when an inference becomes true
                    }

                }
                catch (InvalidOperationException)
                {
                    //DebugMessagesManager.Instance.displayMessage("Manager", "Update", DebugMessagesManager.MessageLevel.Warning, "Inference dictionary has changed, no update performed this frame"); // Class and method names are hard coded for performance reasons.
                }
            }

            public void RegisterInference(Inference inference)
            {
                if (InferencesStorage.ContainsKey(inference.Id) == false)
                {
                    InferencesStorage.Add(inference.Id, inference);
                    //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Inference " + inference.Id + " added");

                    List<string> ids = new List<string>();
                    List<bool> evaluations = new List<bool>();

                    foreach (KeyValuePair<string, Inference> inf in InferencesStorage)
                    {
                        ids.Add(inf.Key);
                        evaluations.Add(false);
                    }

                    UpdateInferencesStatus(ids, evaluations);
                }
                else
                {
                    //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Inference already registered - nothing to do");
                }
            }

            public void UnregisterInference(Inference inference)
            {
                if (inference != null )
                {
                    UnregisterInference(inference.Id);
                }
                else
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Variable not intialized, nothing to do");
                }
                
            }

            public void UnregisterInference(string id)
            {
                if (InferencesStorage.ContainsKey(id))
                {
                    InferencesStorage[id].Unregistered();
                    InferencesStorage.Remove(id);
                    //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Inference " + id + " unregistered");

                    List<string> ids = new List<string>();
                    List<bool> evaluations = new List<bool>();

                    foreach (KeyValuePair<string, Inference> inf in InferencesStorage)
                    {
                        ids.Add(inf.Key);
                        evaluations.Add(false);
                    }

                    UpdateInferencesStatus(ids, evaluations);
                }
                else
                {
                    //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "No inference registered - nothing to do");
                }
            }

            public Inference GetInference(string id)
            {
                Inference toReturn = null;

                if (InferencesStorage.ContainsKey(id))
                {
                    toReturn = InferencesStorage[id];
                }

                return toReturn;
            }
        }
    }
}