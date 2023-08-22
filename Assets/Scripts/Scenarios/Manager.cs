/*Copyright 2022 Louis Marquet

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
using System.Text.RegularExpressions;
/**
 * Singleton that handles the scenario. It is the responsibility of each scenario to register to this singleton.
 * */
namespace MATCH
{
    namespace Scenarios
    {
        public class Manager : MonoBehaviour
        {
            private List<MATCH.Scenarios.Scenario> Scenarios; //List of scenario

            private static Manager InstanceInternal;

            public static Manager Instance { get { return InstanceInternal; } }

            private void Awake()
            {
                if (InstanceInternal != null && InstanceInternal != this)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Scenarios = new List<MATCH.Scenarios.Scenario>();

                    InstanceInternal = this;
                }
            }

            public EventHandler EventScenarioAdded;

            public void AddScenario(MATCH.Scenarios.Scenario scenario)
            {
                bool absent = true;
                foreach (MATCH.Scenarios.Scenario challenge in Scenarios)
                {
                    if (challenge == scenario)
                    {
                        absent = false;
                        break;
                    }
                }
                if (absent)
                {
                    Scenarios.Add(scenario); //add scenario in the list of scenarios
                    Utilities.EventHandlerArgs.String arg = new Utilities.EventHandlerArgs.String(scenario.GetId()); //set a name to the scenario
                    EventScenarioAdded?.Invoke(this, arg); //send information of new scenario => callback (MouseGlobalInitializer) => add new button (MouseAssistanceDialog)
                }
            }

            public List<MATCH.Scenarios.Scenario> getScenarios()
            {
                return Scenarios;
            }
        }
    }
}
