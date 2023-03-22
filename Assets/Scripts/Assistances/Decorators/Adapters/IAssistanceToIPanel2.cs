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
            class IAssistanceToIPanel2 : /*Assistance,*/ IPanel2
            {
                readonly IAssistance AssistanceToAdapt;
                readonly IPanel2 AssistanceDecorated;
                public IAssistanceToIPanel2(IAssistance assistanceToAdapt, IPanel2 assistanceDecorated)
                {
                    AssistanceDecorated = assistanceDecorated;
                    AssistanceToAdapt = assistanceToAdapt;
                }
                /*public override void Show(EventHandler callback, bool withAnimation)
                {
                    AssistanceToAdapt.GetAssistance().Show(Utilities.Utility.GetEventHandlerEmpty(), false);
                }

                public override void Hide(EventHandler callback, bool withAnimation)
                {
                    AssistanceToAdapt.GetAssistance().Hide(Utilities.Utility.GetEventHandlerEmpty(), false);
                }

                public override void Hide(EventHandler callback, bool withAnimation)
                {
                    if (IsDisplayed)
                    {
                        AssistanceToAdapt.GetAssistance().Hide(delegate (System.Object o, EventArgs e)
                        {
                            //audioSource.Stop();
                            DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Sound stopped ");

                            IsDisplayed = false;

                            callback?.Invoke(o, e);
                        }, withAnimation);

                    }
                    else
                    {
                        Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();
                        args.Success = false;
                        callback?.Invoke(this, args);
                    }
                }

                public override void Show(EventHandler callback, bool withAnimation)
                {

                    if (IsDisplayed == false)
                    {
                        IsDisplayed = true;

                        AssistanceToAdapt.GetAssistance().Show(delegate (System.Object o, EventArgs e)
                        {
                            //audioSource = GetComponent<AudioSource>();
                            //audioClip = MATCH.Utilities.Materials.Sounds.Load(MATCH.Utilities.Materials.Sounds.Debug);

                            DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Sound played ");


                            AssistanceToAdapt.GetDecoratedAssistance().Show(Utilities.Utility.GetEventHandlerEmpty(), false);


                            callback?.Invoke(this, e);
                        }, withAnimation);
                    }
                    else
                    {
                        // Check first if the decorated assistance needs to be displayed
                        AssistanceToAdapt.GetDecoratedAssistance().Show(delegate (System.Object o, EventArgs e)
                        {
                            Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();
                            //PanelToDecorate.GetBackground().gameObject.SetActive(false);
                            ///////PanelToDecorate.GetBackgroundMessage().gameObject.SetActive(false);
                            //PanelToDecorate.GetBackgroundIcon().gameObject.SetActive(false);
                            args.Success = false;
                            callback?.Invoke(this, args);
                        }, withAnimation);
                    }
                }*/

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

                public AudioClip GetSound()
                {
                    return AssistanceToAdapt.GetSound();
                }


                /*
                public override Transform GetTransform()
                {
                    return AssistanceToAdapt.GetDecoratedAssistance().GetTransform();
                }

                public override bool IsDecorator()
                {
                    return true;
                }

                public override void ShowHelp(bool show, EventHandler callback, bool withAnimation)
                {
                    AssistanceToAdapt.GetDecoratedAssistance().ShowHelp(show, callback, withAnimation);
                }*/
            }
        }
    }
}
//// Dans la fonction pour créer l'adapter
//Adapters.IAssistanceToIPanel2 adapter = new Adapters.IAssistanceToIPanel2(sound, (IPanel2)sound.GetDecoratedAssistance());