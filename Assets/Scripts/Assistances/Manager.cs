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
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Reflection;
using System;

namespace MATCH
{
    namespace Assistances
    {
        public class Manager
        {
            Dictionary<String, Assistance> AssistancesStorage;

            // Start is called before the first frame update
            public Manager()
            {
                AssistancesStorage = new Dictionary<string, Assistance>();
            }

            void Start()
            {
                
            }

            public bool AddAssistance(String id, Assistance assistance)
            {
                bool toReturn = true;

                if (AssistancesStorage.ContainsKey(id))
                {
                    toReturn = false;
                }
                else
                {
                    AssistancesStorage.Add(id, assistance);
                }

                return toReturn;
            }

            public void HideAllBut(string id)
            {
                foreach (KeyValuePair<string, Assistance> pair in AssistancesStorage)
                {
                    if (pair.Key == id)
                    {
                        pair.Value.Show(Utilities.Utility.GetEventHandlerEmpty());
                    }
                    else
                    {
                        pair.Value.Hide(Utilities.Utility.GetEventHandlerEmpty());
                    }
                }
            }

            public void Show(string id)
            {
                AssistancesStorage[id].Show(Utilities.Utility.GetEventHandlerEmpty());
            }

            public void Hide(string id)
            {
                AssistancesStorage[id].Hide(Utilities.Utility.GetEventHandlerEmpty());
            }
        }
    }
}
