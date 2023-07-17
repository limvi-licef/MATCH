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

namespace MATCH
{
    namespace Assistances
    {
        public class StateMachine
        {
            Dictionary<GradationVisual.GradationVisual, Dictionary<Buttons.Button.ButtonType, GradationVisual.GradationVisual>> AssistanceGradation;

            public GradationVisual.GradationVisual AssistanceCurrent { get; set; }

            private GradationVisual.GradationVisual AssistanceRoot;

            public StateMachine(GradationVisual.GradationVisual root)
            {
                AssistanceGradation = new Dictionary<GradationVisual.GradationVisual, Dictionary<Buttons.Button.ButtonType, GradationVisual.GradationVisual>>();
                AssistanceCurrent = null;
                AssistanceRoot = root;
                AssistanceCurrent = root;
            }

            /**
             * The eventargs of the callback contains 3 arguments: a reference to the AssistanceGradationAttention and the type of button clicked, a reference to the next assistance to display
             * */
            public event EventHandler EventOnButtonClicked;

            public void AddAssistance (GradationVisual.GradationVisual assistance, Buttons.Button.ButtonType type, ref GradationVisual.GradationVisual assistanceTarget)
            {
                if (AssistanceGradation.ContainsKey(assistance) == false)
                {
                    AssistanceGradation.Add(assistance, new Dictionary<Buttons.Button.ButtonType, GradationVisual.GradationVisual>());
                    assistance.EventHelpClicked += COnButtonClickedInternal;
                }

                AssistanceGradation[assistance].Add(type, assistanceTarget);

                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Adding button " + type.ToString() + " for assistance " + assistance.name);
            }

            /*
            public void ChangeAssistance(GradationVisual.GradationVisual assistance, Buttons.Button.ButtonType type, ref GradationVisual.GradationVisual assistanceTarget)
            {
                if (AssistanceGradation.ContainsKey(assistance) == true)
                {
                    AssistanceGradation[assistance] = new Dictionary<Buttons.Button.ButtonType, GradationVisual.GradationVisual>();
                }
            }
            */

            public void BackToRoot()
            {
                AssistanceCurrent = AssistanceRoot;
            }

            public List<Buttons.Button.ButtonType> GetButtonTypesCurrentAssistance()
            {
                List<Buttons.Button.ButtonType> toReturn = null;

                if (AssistanceGradation.ContainsKey(AssistanceCurrent))
                {
                    toReturn = new List<Buttons.Button.ButtonType>(AssistanceGradation[AssistanceCurrent].Keys);
                }
                return toReturn;
            }

            /*public AssistanceGradationAttention GetNextAssistance(AssistanceGradationAttention assistance, Buttons.Button.ButtonType button)
            {
                AssistanceGradationAttention toReturn = null;

                if (AssistanceGradation.ContainsKey(assistance) && AssistanceGradation[assistance].ContainsKey(button))
                {
                    toReturn = AssistanceGradation[assistance][button];
                }

                return toReturn;
            }*/

            private void COnButtonClickedInternal(System.Object o, EventArgs e)
            {
                Utilities.EventHandlerArgs.Button args = (Utilities.EventHandlerArgs.Button)e;

                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Help button " + args.ButtonType.ToString() + " clicked for assistance " + AssistanceCurrent.name);

                if(AssistanceGradation.ContainsKey(AssistanceCurrent))
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "The dictionary contains the assistance");
                }
                else
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "The dictionary DOES NOT contain the assistance");
                }

                Utilities.EventHandlerArgs.ButtonAndAssistanceGradationAttention argsToSend = new Utilities.EventHandlerArgs.ButtonAndAssistanceGradationAttention();                
                argsToSend.AssistanceCurrent = AssistanceCurrent;
                if (AssistanceGradation[AssistanceCurrent].ContainsKey(args.ButtonType))
                {
                    argsToSend.AssistanceNext = AssistanceGradation[AssistanceCurrent][args.ButtonType];
                    //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Next assistance: " + argsToSend.AssistanceNext.GetCurrentAssistance().name);
                }
                else
                {
                    argsToSend.AssistanceNext = null;
                }
                
                argsToSend.ButtonType = args.ButtonType;

                

                //AssistanceCurrent = argsToSend.AssistanceNext;

                EventOnButtonClicked?.Invoke(this, argsToSend);
            }
        }

        /*public class StateMachineStruct
        {
            public AssistanceGradationAttention AssistanceGradation;
            public Dictionary<Buttons.Button.ButtonType, AssistanceGradationAttention> AssistancesNext;

            public StateMachineStruct (AssistanceGradationAttention assistance)
            {
                AssistanceGradation = assistance;
            }

            public void AddButton(Buttons.Button.ButtonType type, ref AssistanceGradationAttention assistanceTarget)
            {
                AssistancesNext.Add(type, assistanceTarget);
            }

            public AssistanceGradationAttention GetNextAssistance(Buttons.Button.ButtonType type)
            {
                return AssistancesNext[type];
            }
        }*/
    }
}