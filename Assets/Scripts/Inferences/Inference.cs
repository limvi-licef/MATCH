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

namespace MATCH
{
    namespace Inferences
    {

        public abstract class Inference
        {
            public string Id { get; private set; }
            public event EventHandler Callbacks;
            protected EventArgs CallbackArgs; // Sent as argument of the callback. So if you want to send specific arguments, this is the variable to set.

            protected Inference(string id, EventHandler callback)
            {
                Id = id;
                Callbacks += callback;
                CallbackArgs = EventArgs.Empty;
            }

            protected Inference(string id)
            {
                Id = id;
            }

            public void AddCallback(EventHandler callback)
            {
                Callbacks += callback;
            }

            public abstract bool Evaluate();

            /**
             * This function should unregister the callbacks and so on, i.e. ending properly the unregistration
             * */
            public abstract void Unregistered();

            //public string getId() => m_id;
            public void TriggerCallback() => Callbacks?.Invoke(this, CallbackArgs);

            /*~Inference()
            {
                for (int i = 0; i < Callbacks.GetInvocationList().Length; i++)
                {
                    Callbacks -= (EventHandler)Callbacks.GetInvocationList()[i];
                }
            }*/
        }
    }
}