/*Copyright 2022 Guillaume Spalla, Louis Marquet

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

namespace MATCH
{
    namespace Scenarios
    {
        public abstract class Scenario : MonoBehaviour
        {

            string m_scenarioId;

            public event EventHandler s_challengeOnStandBy; // Contains as EventArg a String object, which contains the id of the scenario
            public event EventHandler s_challengeOnSuccess; // Contains as EventArg a String object, which contains the id of the scenario
            public event EventHandler s_challengeOnStart;   // Contains as EventArg a String object, which contains the id of the scenario

            public string getId()
            {
                return this.m_scenarioId;
            }

            public void setId(string id)
            {
                this.m_scenarioId = id;
            }

            /**
             * Function to be called when the scenario enters the stand by mode
             * */
            protected void onChallengeStandBy()
            {
                Utilities.EventHandlerArgs.String arg = new Utilities.EventHandlerArgs.String(m_scenarioId);
                s_challengeOnStandBy?.Invoke(this, arg);
            }

            /**
             * Function to be called when the scenario enters the success mode
             * */
            protected void onChallengeSuccess()
            {
                Utilities.EventHandlerArgs.String arg = new Utilities.EventHandlerArgs.String(m_scenarioId);
                s_challengeOnSuccess?.Invoke(this, arg);
            }

            /**
             * Function to be called when the scenario begins, wherever it is, i.e. this function might be called at several places of the scenario
             * */
            protected void onChallengeStart()
            {
                Utilities.EventHandlerArgs.String arg = new Utilities.EventHandlerArgs.String(m_scenarioId);
                s_challengeOnStart?.Invoke(this, arg);
            }
        }
    }
}

