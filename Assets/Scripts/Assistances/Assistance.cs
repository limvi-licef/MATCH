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
            public event EventHandler EventHelpButtonClicked; // Returns as argument a MATCH.Utilities.EventHandlerArgs.Button object, to inform about which button has been clicked
            public event EventHandler EventIsShown; // Emitted when the assistance is shown. Does not replace the event handler, comes with it.
            public event EventHandler EventIsHidden; // Emitted when the assistance is hidden. Does not replace the event handler, comes with it.
            public bool IsDisplayed { get; protected set; } = false; // Different from "activeself": in those assistances the parent component is alsways active, so "activeself" is not a good indicator to know if the assistance is shown or not. Use this function instead.

            private Transform Hand = null;

            protected void OnHelpButtonClicked(Assistances.Buttons.Button.ButtonType type)
            {
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "EventHelpButtonClicked is going to be triggered for assistance " + name);
                MATCH.Utilities.EventHandlerArgs.Button args = new MATCH.Utilities.EventHandlerArgs.Button();
                args.ButtonType = type;

                EventHelpButtonClicked?.Invoke(this, args);
            }

            /**
             * Should be called by each assistance inheriting from this abstract class
             * caller: assistance calling this function
             * args: arguments to embed during the event call
             * */
            protected void OnIsShown (Assistance caller, EventArgs args)
            {
                EventIsShown?.Invoke(caller, args);
            }

            /**
             * Should be called by each assistance inheriting from this abstract class
             * caller: assistance calling this function
             * args: arguments to embed during the event call
             * */
            protected void OnIsHidden(Assistance caller, EventArgs args)
            {
                EventIsHidden?.Invoke(caller, args);
            }

            public abstract void Show(EventHandler callback, bool withAnimation = true);
            public abstract void Hide(EventHandler callback, bool withAnimation=true);

            /**
             * True: show; False: hide
             * */
            public abstract void ShowHelp(bool show, EventHandler callback, bool withAnimation=true);

            /**
             * Return the Transform associated to the assistance
             * */
            public abstract Transform GetTransform();

            /**
             * Callback to call when a button from the help is clicked
             * */
            protected void CButtonHelp(System.Object o, EventArgs e)
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Button clicked");
                Utilities.EventHandlerArgs.Button args = (Utilities.EventHandlerArgs.Button)e;

                OnHelpButtonClicked(args.ButtonType);
            }

            public abstract bool IsDecorator(); // Must specify if the assistance is a decorator of another assistance or not

            public void ShowMovingHand(bool show)
            {
                if (Hand == null)
                {

                    //Hand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    Hand = Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.AssistanceHand);
                    Hand.gameObject.AddComponent<Assistances.GradationVisual.MovingObject>();
                    Hand.transform.parent = transform;
                    //Hand.transform.localScale = new Vector3(0.05f, 0.1f, 0.05f);
                    Hand.GetComponent<Assistances.GradationVisual.MovingObject>().SetMoveShape(GradationVisual.MovingObject.MovingType.HalfCircle);
                    Hand.gameObject.SetActive(false);

                    Hand.transform.Rotate(0, 0, -22.5f);

                    Hand.transform.localPosition = transform.localPosition;
                    Hand.transform.localPosition = new Vector3(Hand.transform.localPosition.x, Hand.transform.localPosition.y+0.3f, Hand.transform.localPosition.z);
                }

                if (show)
                {
                    Hand.gameObject.SetActive(true);
                    // Start rotation
                    Hand.GetComponent<Assistances.GradationVisual.MovingObject>().StartMove(true);
                }
                else
                {
                    Hand.gameObject.SetActive(false);
                }
            }
        }

    }
}

