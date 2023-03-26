/*Copyright 2022 Léri Lamour & Louis Marquet

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
    namespace Assistances
    {
        namespace Adapters
        {
            class IAssistanceToIPanel2 : Assistance, IPanel2
            {
                readonly IAssistance AssistanceToAdapt;
                readonly IPanel2 AssistanceDecorated;
                public IAssistanceToIPanel2(IAssistance assistanceToAdapt, IPanel2 assistanceDecorated)
                {
                    AssistanceDecorated = assistanceDecorated;
                    AssistanceToAdapt = assistanceToAdapt;
                    //AssistanceToAdapt.GetAssistance().Show(Utilities.Utility.GetEventHandlerEmpty(), false);
                }
                /*
                public override void Show(EventHandler callback, bool withAnimation)
                {
                    this.GetAssistance().Show(delegate (System.Object o, EventArgs e)
                    {
                        callback?.Invoke(o, e);
                    }, withAnimation);
                }

                public override void Hide(EventHandler callback, bool withAnimation)
                {
                    this.GetAssistance().Hide(delegate (System.Object o, EventArgs e)
                    {
                        callback?.Invoke(o, e);
                    }, withAnimation);
                }
                */
                
                public override void Hide(EventHandler callback, bool withAnimation)
                {
                    if (IsDisplayed)
                    {
                        AssistanceDecorated.GetAssistance().Hide(delegate (System.Object o, EventArgs e)
                        {
                            IsDisplayed = false;
                            DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Hide Adapter");

                            callback?.Invoke(o, e);
                        }, withAnimation);

                    }
                    else
                    {
                        Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Hide Adapter ?");

                        args.Success = false;
                        callback?.Invoke(this, args);
                    }
                }

                public override void Show(EventHandler callback, bool withAnimation)
                {

                    if (IsDisplayed == false)
                    {
                        IsDisplayed = true;

                        AssistanceDecorated.GetAssistance().Show(delegate (System.Object o, EventArgs e)
                        {
                            DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Show Adapter");

                            callback?.Invoke(this, e);
                        }, withAnimation);
                    }
                    else
                    {
                        // Check first if the decorated assistance needs to be displayed
                        AssistanceDecorated.GetAssistance().Show(delegate (System.Object o, EventArgs e)
                        {
                            Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();
                            DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Show Adapter ?");

                            args.Success = false;
                            callback?.Invoke(this, args);
                        }, withAnimation);
                    }
                }

                public Transform GetBackgroundMessage()
                {
                    return AssistanceDecorated.GetBackgroundMessage();
                }

                public Transform GetBackgroundIcon()
                {
                    return AssistanceDecorated.GetBackgroundIcon();
                }

                public Transform GetBackground()
                {
                    return AssistanceDecorated.GetBackground();
                }

                public void EnableWeavingHand(bool enable)
                {
                    AssistanceDecorated.EnableWeavingHand(enable);
                }

                public Assistance GetDecoratedAssistance()
                {
                    return AssistanceToAdapt.GetDecoratedAssistance();
                }

                public Assistance GetAssistance()
                {
                    return AssistanceToAdapt.GetAssistance();
                }

                public Transform GetSound()
                {
                    return AssistanceToAdapt.GetSound();
                }

                
                public override Transform GetTransform()
                {
                    return AssistanceToAdapt.GetAssistance().GetTransform(); //Decorated ?
                }

                public override bool IsDecorator()
                {
                    return true;
                }

                public override void ShowHelp(bool show, EventHandler callback, bool withAnimation)
                {
                    AssistanceToAdapt.GetAssistance().ShowHelp(show, callback, withAnimation); //Decorated ?
                }
            }
        }
    }
}
//// Dans la fonction pour créer l'adapter
//Adapters.IAssi
//stanceToIPanel2 adapter = new Adapters.IAssistanceToIPanel2(sound, (IPanel2)sound.GetDecoratedAssistance());