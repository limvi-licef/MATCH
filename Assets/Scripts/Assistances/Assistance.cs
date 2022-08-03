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
        public abstract class Assistance: MonoBehaviour
        {
            public event EventHandler EventHelpButtonClicked;

            protected void OnHelpButtonClicked(Assistances.Buttons.Button.ButtonType type)
            {
                MATCH.Utilities.EventHandlerArgs.Button args = new Utilities.EventHandlerArgs.Button();
                args.ButtonType = type;

                EventHelpButtonClicked?.Invoke(this, args);
            }

            public abstract void Show(EventHandler callback);
            public abstract void Hide(EventHandler callback);

            /**
             * True: show; False: hide
             * */
            public abstract void ShowHelp(bool show);

            /**
             * Return the Transform associated to the assistance
             * */
            public abstract Transform GetTransform();

            /**
             * Callback to call when a button from the help is clicked
             * */
            protected void CButtonHelp(System.Object o, EventArgs e)
            {
                Utilities.EventHandlerArgs.Button args = (Utilities.EventHandlerArgs.Button)e;

                OnHelpButtonClicked(args.ButtonType);
            }

            public abstract bool IsDecorator(); // Must specify if the assistance is a decorator of another assistance or not
        }

    }
}

