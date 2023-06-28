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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

/**
 * Material will be set when calling the "show" function
 * */
namespace MATCH
{
    namespace Assistances
    {
        namespace Decorators
        {
            public class Material : Assistance, IBasic
            {
                IBasic AssistanceToDecorate;
                String MaterialName;

                public void SetAssistanceToDecorate(IBasic toDecorate)
                {
                    AssistanceToDecorate = toDecorate;

                    // Relaying the eventhandler
                    Assistance temp = (Assistance)AssistanceToDecorate;
                    temp.EventHelpButtonClicked += delegate (System.Object o, EventArgs e)
                    {
                        MATCH.Utilities.EventHandlerArgs.Button args = (MATCH.Utilities.EventHandlerArgs.Button)e;

                        //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "EventHelpButtonClicked caught by decorated object. Relaying the event ...");

                        OnHelpButtonClicked(args.ButtonType);
                    };
                }

                public override Transform GetTransform()
                {
                    return AssistanceToDecorate.GetAssistance().GetTransform();
                }

                public override void Hide(EventHandler callback, bool withAnimation)
                {
                    AssistanceToDecorate.GetAssistance().Hide(callback, withAnimation);
                }

                public void SetMaterial(string materialName)
                {
                    MaterialName = materialName;
                }

                public override void Show(EventHandler callback, bool withAnimation)
                {
                    if (AssistanceToDecorate.GetAssistance().GetTransform().gameObject.activeSelf == false)
                    {
                        AssistanceToDecorate.SetMaterial(MaterialName);
                        AssistanceToDecorate.GetAssistance().Show(callback, withAnimation);
                    }
                    else
                    {
                        AssistanceToDecorate.GetAssistance().Hide(delegate (System.Object o, EventArgs e)
                        {
                            AssistanceToDecorate.SetMaterial(MaterialName);
                            AssistanceToDecorate.GetAssistance().Show(callback, withAnimation);
                        }, withAnimation);
                    }
                }

                public override void ShowHelp(bool show, EventHandler callback, bool withAnimation)
                {
                    AssistanceToDecorate.GetAssistance().ShowHelp(show, callback, withAnimation);
                }

                public Assistance GetAssistance()
                {
                    return this;//AssistanceToDecorate.GetAssistance();
                }

                public Assistance GetRootDecoratedAssistance()
                {
                    return AssistanceToDecorate.GetRootDecoratedAssistance();
                }

                public Assistance GetDecoratedAssistance()
                {
                    return AssistanceToDecorate.GetAssistance();
                }

                public override bool IsDecorator()
                {
                    return true;
                }

                public Transform GetSound()
                {
                    return AssistanceToDecorate.GetSound();
                }

                public Transform GetArch()
                {
                    return AssistanceToDecorate.GetArch();
                }

                public Assistances.Icon GetIcon()
                {
                    return AssistanceToDecorate.GetIcon();
                }

                public Transform GetLinePath()
                {
                    return AssistanceToDecorate.GetLinePath();
                }
            }
        }
    }
}