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
        public class StateMachine: ScriptableObject
        {
            Dictionary<AssistanceGradationAttention, Dictionary<Buttons.Button.ButtonType, AssistanceGradationAttention>> AssistanceGradation;

            public AssistanceGradationAttention AssistanceCurrent { get; set; }

            private AssistanceGradationAttention AssistanceRoot;

            public StateMachine(AssistanceGradationAttention root)
            {
                AssistanceGradation = new Dictionary<AssistanceGradationAttention, Dictionary<Buttons.Button.ButtonType, AssistanceGradationAttention>>();
                AssistanceCurrent = null;
                AssistanceRoot = root;
                AssistanceCurrent = root;
            }

            /**
             * The eventargs of the callback contains 3 arguments: a reference to the AssistanceGradationAttention and the type of button clicked, a reference to the next assistance to display
             * */
            public event EventHandler EventOnButtonClicked;

            public void AddButton (AssistanceGradationAttention assistance, Buttons.Button.ButtonType type, ref AssistanceGradationAttention assistanceTarget)
            {
                if (AssistanceGradation.ContainsKey(assistance) == false)
                {
                    AssistanceGradation.Add(assistance, new Dictionary<Buttons.Button.ButtonType, AssistanceGradationAttention>());
                    assistance.EventHelpClicked += COnButtonClickedInternal;
                }

                AssistanceGradation[assistance].Add(type, assistanceTarget);
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

                Utilities.EventHandlerArgs.ButtonAndAssistanceGradationAttention argsToSend = new Utilities.EventHandlerArgs.ButtonAndAssistanceGradationAttention();                
                argsToSend.AssistanceCurrent = AssistanceCurrent;
                argsToSend.AssistanceNext = AssistanceGradation[AssistanceCurrent][args.ButtonType];
                argsToSend.ButtonType = args.ButtonType;

                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Help button " + args.ButtonType.ToString() + " clicked for assistance " + argsToSend.AssistanceCurrent.GetCurrentAssistance().name + ". Next assistance: " + argsToSend.AssistanceNext.GetCurrentAssistance().name);

                AssistanceCurrent = argsToSend.AssistanceNext;

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