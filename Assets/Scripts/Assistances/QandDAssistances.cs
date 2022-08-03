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
            enum Gradation
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

            Dictionary<Gradation, Assistance> AssistancesStorage;

            // Start is called before the first frame update
            public QandDAssistances() {
                AssistancesStorage = new Dictionary<Gradation, Assistance>();
            }

            void AddAssistance(Gradation gradation, Assistance assistance)
            {
                AssistancesStorage.Add(gradation, assistance);
            }

            void ShowOneHideOthers(Gradation gradationToShow, EventHandler callback)
            {
                foreach (KeyValuePair<Gradation, Assistance> assistance in AssistancesStorage)
                {
                    if (assistance.Key == gradationToShow)
                    {
                        assistance.Value.Show(callback);
                    }
                    else
                    {
                        assistance.Value.Hide(Utilities.Utility.GetEventHandlerEmpty());
                    }
                }
            }
        }
    }
}