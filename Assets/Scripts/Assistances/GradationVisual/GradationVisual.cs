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
using System.Linq;

namespace MATCH
{
    namespace Assistances
    {
        namespace GradationVisual
        {
            public class GradationVisual : MonoBehaviour
            {
                List<Assistance> Gradation;
                int GradationCurrent = -1;

                public event EventHandler EventHelpClicked; // contains an argument which is an enum with the type of buttons clicked
                public event EventHandler IsShown;
                public event EventHandler IsHidden;

                /*public AssistanceGradationAttention()
                {

                }*/

                private void Awake()
                {
                    Gradation = new List<Assistance>();
                }

                private void Start()
                {
                    //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called");
                }

                /*
                 * Don't forget to add a non generic name to the assistance, because it will be used to check if it has already been registered to the list. Indeed, as we can use decorators, the same EventHandler can be triggered several times, hence this mechanism.
                 * */
                public Assistance AddAssistance(Assistance assistance)
                {
                    /*assistance.EventHelpButtonClicked += delegate (System.Object o, EventArgs e)
                    {
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called 2");
                    };*/

                    // We first look if the assistance has already been registered. If so, then the EventHandler won't be subscribed again.
                    bool isAlreadyThere = false;
                    foreach (Assistance a in Gradation)
                    {
                        if (a.name == assistance.name)
                        {
                            isAlreadyThere = true;
                            break;
                        }
                    }

                    // Now we add the assistance to the list
                    Gradation.Add(assistance);
                    if (GradationCurrent == -1)
                    {
                        GradationCurrent = 0;
                    }

                    /*if (assistance.IsDecorator() == false)
                    {
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Going to subscribe to the EventHelpButtonClicked event of the assistance " + assistance.GetTransform().name);

                        Gradation.Last().EventHelpButtonClicked += CHelpButtonClicked;
                    }*/

                    //If the assistance was not registered yet, then the event handler is subcribed.
                    if (isAlreadyThere == false)
                    {
                        //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Adding EventHandler for assistance " + assistance.name);

                        Gradation.Last().EventHelpButtonClicked += CHelpButtonClicked;
                    }
                    else
                    {
                        //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Assistance " + assistance.name + " is already there, so no EventHandler will be added");
                    }

                    return assistance;
                }

                private void CHelpButtonClicked(System.Object o, EventArgs e)
                {
                    DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Help button clicked for assistance " + GetCurrentAssistance().name);

                    MATCH.Utilities.EventHandlerArgs.Button args = (Utilities.EventHandlerArgs.Button)e;

                    EventHelpClicked?.Invoke(this, e);
                }

                /**
                 * Returns True if there is a new level of gradation shown, False if the current displayed level of gradation is the last one
                 * */
                public bool ShowNextGradation(EventHandler callback)
                {
                    bool toReturn = false;

                    if (GradationCurrent > -1 && ++GradationCurrent < Gradation.Count)
                    {
                        //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Next gradation is going to be shown");
                        Gradation[GradationCurrent].Show(callback, false);
                        toReturn = true;
                    }
                    else
                    {
                        GradationCurrent--;
                        DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Maximum level of attention gradation reached");
                    }

                    MATCH.Utilities.Logger.Instance.Log("\t", MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Gradation attention " + GradationCurrent);

                    return toReturn;
                }

                public bool IsLastGradationLevel()
                {
                    bool toReturn = false;

                    if (GradationCurrent+1 >= Gradation.Count)
                    {
                        toReturn = true;
                    }

                    return toReturn;
                }

                /**
                 * Show the minimal level of gradation. I.e. even if a higher level of gradation is displayed, will go back to the minimal level
                 * */
                public void ShowMinimalGradation(EventHandler callback)
                {
                    if (GradationCurrent > -1)
                    {
                        Gradation[GradationCurrent].Hide(delegate (System.Object o, EventArgs e)
                        {
                            GradationCurrent--;

                            if (GradationCurrent <= 0)
                            {
                                GradationCurrent = 0;
                                Gradation[GradationCurrent].Show(callback, false);
                                IsShown?.Invoke(this, EventArgs.Empty);
                                MATCH.Utilities.Logger.Instance.Log("\t", MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Gradation attention " + GradationCurrent + " (Minimal)");
                            }
                            else
                            {
                                ShowMinimalGradation(callback);
                            }
                        }, false);
                    }
                    else
                    {
                        DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "No assistances to display. Please add assistances first.");
                    }
                }

                public void HideCurrentGradation(EventHandler callback)
                {
                    if (GradationCurrent > -1)
                    {
                        Gradation[GradationCurrent].Hide(callback);
                        IsHidden?.Invoke(this, EventArgs.Empty);
                        GradationCurrent = 0;
                    }
                    else
                    {
                        DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "No assistances to hide. Please add assistances first.");
                    }
                }

                public void ShowHelpCurrentGradation(bool show, EventHandler callback)
                {
                    if (GradationCurrent > -1)
                    {
                        GradationCurrent = 0;
                        Gradation[GradationCurrent].ShowHelp(show, callback);
                    }
                    else
                    {
                        DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "No help to hide. Please add assistances first.");
                    }
                }

                /**
                 * Returns null if no assistances have been added yet
                 * */
                public Assistance GetCurrentAssistance()
                {
                    Assistance toReturn = null;

                    if (GradationCurrent > -1)
                    {
                        toReturn = Gradation[GradationCurrent];
                    }

                    return toReturn;
                }
            }
        }
    }
}