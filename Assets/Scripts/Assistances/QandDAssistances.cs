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
        public class QandDAssistances
        {
            public enum Gradation
            {
                Alpha = 0,
                Beta = 1,
                Gamma = 2,
                Delta = 3,
                Epsilon = 4,
                Zeta = 5,
                Eta = 6,
                Theta = 7
            }

            readonly Dictionary<Gradation, Assistance> AssistancesStorage;

            // Start is called before the first frame update
            public QandDAssistances() {
                AssistancesStorage = new Dictionary<Gradation, Assistance>();
            }


            public void AddAssistance(Gradation gradation, Assistance assistance)
            {
                AssistancesStorage.Add(gradation, assistance);
            }

            public Assistance GetAssistance(Gradation gradation)
            {
                return AssistancesStorage[gradation];
            }

            public void ShowOneHideOthers(Gradation gradationToShow, EventHandler callback)
            {
                if (gradationToShow == Gradation.Delta)
                {
                    DebugMessagesManager.Instance.displayMessage("QAndDAssistances", "ShowOneHideOthers", DebugMessagesManager.MessageLevel.Info, "Showing assistance Delta");
                }

                foreach (KeyValuePair<Gradation, Assistance> assistance in AssistancesStorage)
                {
                    //DebugMessagesManager.Instance.displayMessage("ShowOneHideOthers", "Evaluate", DebugMessagesManager.MessageLevel.Info, "Storage " + assistance.Value.GetTransform().name);
                    if (assistance.Key == gradationToShow)
                    {
                        if (assistance.Value.IsActive() == false)
                        {
                            assistance.Value.Show(callback);
                        }
                        //DebugMessagesManager.Instance.displayMessage("ShowOneHideOthers", "Evaluate", DebugMessagesManager.MessageLevel.Info, "Show " + assistance.Value.GetTransform().name); // Class and method names are hard coded for performance reasons.
                    }
                    else
                    {
                        //DebugMessagesManager.Instance.displayMessage("ShowOneHideOthers", "Evaluate", DebugMessagesManager.MessageLevel.Info, "Hide"); // Class and method names are hard coded for performance reasons.
                        if (assistance.Value.IsActive())
                        {
                            assistance.Value.Hide(Utilities.Utility.GetEventHandlerEmpty());
                        }
                            
                    }
                }
            }

            public void HideAll()
            {
                //DebugMessagesManager.Instance.displayMessage("ShowOneHideOthers", "Evaluate", DebugMessagesManager.MessageLevel.Info, "Hide All");
                foreach (KeyValuePair<Gradation, Assistance> assistance in AssistancesStorage)
                {
                    if (assistance.Key == Gradation.Alpha)
                    {
                        DebugMessagesManager.Instance.displayMessage("QAndDAssistances", "Hideall", DebugMessagesManager.MessageLevel.Info, "Hiding assistance alpha (congratulation cube)");
                    }
                    if (assistance.Key == Gradation.Delta)
                    {
                        DebugMessagesManager.Instance.displayMessage("QAndDAssistances", "Hideall", DebugMessagesManager.MessageLevel.Info, "Hiding assistance delta");
                    }

                    if (assistance.Value.IsActive())
                    {
                        assistance.Value.Hide(Utilities.Utility.GetEventHandlerEmpty());
                    }
                    else
                    {
                        DebugMessagesManager.Instance.displayMessage("QAndDAssistances", "Hideall", DebugMessagesManager.MessageLevel.Info, "The assistance is already hidden: nothing to do");
                    }

                        
                }
            }
        }
    }
}