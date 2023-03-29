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
                }
                
                public override void Hide(EventHandler callback, bool withAnimation)
                {
                    //useless method ? (necessary to be an assistance (only assistances can be added to gradation visual)
                    AssistanceToAdapt.GetAssistance().Hide(callback, withAnimation);
                }

                public override void Show(EventHandler callback, bool withAnimation)
                {
                    //useless method ? (necessary to be an assistance (only assistances can be added to gradation visual)
                    AssistanceToAdapt.GetAssistance().Show(callback, withAnimation);
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

                public Assistance GetRootDecoratedAssistance()
                {
                    return AssistanceToAdapt.GetRootDecoratedAssistance();
                }

                public Assistance GetAssistance()
                {
                    return AssistanceToAdapt.GetAssistance();
                }

                public Assistance GetDecoratedAssistance()
                {
                    return AssistanceDecorated.GetAssistance();
                }

                public Transform GetSound()
                {
                    return AssistanceToAdapt.GetSound();
                }

                
                public override Transform GetTransform()
                {
                    return AssistanceToAdapt.GetRootDecoratedAssistance().GetTransform(); //Decorated ?
                }

                public override bool IsDecorator()
                {
                    return true;
                }

                public override void ShowHelp(bool show, EventHandler callback, bool withAnimation)
                {
                    AssistanceToAdapt.GetRootDecoratedAssistance().ShowHelp(show, callback, withAnimation); //Decorated ?
                }
            }
        }
    }
}