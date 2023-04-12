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
 * Very basic for now: when the show function is called, it will hide the assistance and display the help buttons
 * */
namespace MATCH
{
    namespace Assistances
    {
        namespace Decorators
        {
            public class Help : IBasic
            {
                IBasic AssistanceToDecorate;

                public Help (IBasic assistanceToDecorate)
                {
                    AssistanceToDecorate = assistanceToDecorate;
                }

                public Transform GetTransform()
                {
                    return AssistanceToDecorate.GetAssistance().GetTransform();
                }

                public void Hide(EventHandler callback)
                {
                    AssistanceToDecorate.GetAssistance().Hide(callback);
                }

                public void SetMaterial(string materialName)
                {
                    AssistanceToDecorate.SetMaterial(materialName);
                }

                public void Show(EventHandler callback)
                {
                    AssistanceToDecorate.GetAssistance().Show(delegate(System.Object o, EventArgs e)
                    {
                        AssistanceToDecorate.GetAssistance().ShowHelp(true, callback);
                    });

                    /*AssistanceToDecorate.Hide(delegate (System.Object o, EventArgs e)
                    {
                        AssistanceToDecorate.ShowHelp(true);
                        AssistanceToDecorate.Show(callback);
                    });*/
                }

                public void ShowHelp(bool show, EventHandler callback)
                {
                    AssistanceToDecorate.GetAssistance().ShowHelp(show, callback);
                }

                public Assistance GetAssistance()
                {
                    return AssistanceToDecorate.GetAssistance();
                }

                public Assistance GetRootDecoratedAssistance()
                {
                    return AssistanceToDecorate.GetRootDecoratedAssistance();
                }

                public Assistance GetDecoratedAssistance()
                {
                    return AssistanceToDecorate.GetAssistance();
                }

                public Transform GetSound()
                {
                    return AssistanceToDecorate.GetSound();
                }

                public Transform GetArch()
                {
                    return AssistanceToDecorate.GetArch();
                }

                public Transform GetIcon()
                {
                    return AssistanceToDecorate.GetIcon();
                }
            }
        }
    }
}
