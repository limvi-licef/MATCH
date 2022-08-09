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

/**
 * Abstract class to handles assistances showing a text and a button with a text. 
 * */
namespace MATCH
{
    namespace Assistances
    {
        namespace Buttons
        {
            public abstract class Button : MonoBehaviour
            {
                public enum ButtonType
                {
                    Undefined = -1,
                    Yes = 0,
                    No = 1,
                    QuestionMark = 2,
                    CustomChoice1 = 3,
                    CustomChoice2 = 4,
                    CustomChoice3 = 5,
                    CustomChoice4 = 6

                }

                public ButtonType Type { get; set; }

                public event EventHandler EventButtonClicked;

                protected void OnButtonClicked()
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Button clicked");
                    MATCH.Utilities.EventHandlerArgs.Button args = new Utilities.EventHandlerArgs.Button();
                    args.ButtonType = Type;

                    EventButtonClicked?.Invoke(this, args);
                }

                public abstract void Show(EventHandler e);

                public abstract void Hide(EventHandler e);
            }

        }
    }
}
