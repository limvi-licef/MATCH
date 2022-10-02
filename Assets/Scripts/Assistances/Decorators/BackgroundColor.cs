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
    namespace Assistances
    {
        namespace Decorators
        {
            public class BackgroundColor : Assistance, IPanel
            {
                IPanel PanelToDecorate;
                string BackgroundColorDecorated;

                /*public BackgroundColor(IPanel panelToDecorate, string backgroundColor)
                {
                    PanelToDecorate = panelToDecorate;
                    BackgroundColorDecorated = backgroundColor;
                }*/

                public void SetAssistanceToDecorate(IPanel toDecorate)
                {
                    PanelToDecorate = toDecorate;

                    // Relaying the eventhandler
                    Assistance temp = (Assistance)PanelToDecorate;
                    temp.EventHelpButtonClicked += delegate (System.Object o, EventArgs e)
                    {
                        MATCH.Utilities.EventHandlerArgs.Button args = (MATCH.Utilities.EventHandlerArgs.Button)e;

                        //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "EventHelpButtonClicked caught by decorated object. Relaying the event ...");

                        OnHelpButtonClicked(args.ButtonType);
                    };
                }

                public override void Hide(EventHandler callback)
                {
                    PanelToDecorate.GetAssistance().Hide(callback);
                    IsDisplayed = false;
                }

                public override void Show(EventHandler callback)
                {
                    /*if (IsDisplayed == false)
                    {*/
                        PanelToDecorate.SetBackgroundColor(BackgroundColorDecorated);
//                        IsDisplayed = true;
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called with color "  + BackgroundColorDecorated);
  //                  }
                    
    /*                if (PanelToDecorate.GetAssistance().IsDisplayed == false)
                    {*/
                                 PanelToDecorate.GetAssistance().Show(callback);
                    //}
                        
                    

                    /*if (PanelToDecorate.GetAssistance().GetTransform().gameObject.activeSelf == false)
                    {
                        PanelToDecorate.SetBackgroundColor(BackgroundColorDecorated);
                        PanelToDecorate.GetAssistance().Show(callback);
                    }
                    else
                    {
                        PanelToDecorate.GetAssistance().Hide(delegate (System.Object o, EventArgs e)
                        {
                            PanelToDecorate.SetBackgroundColor(BackgroundColorDecorated);
                            PanelToDecorate.GetAssistance().Show(callback);
                        });
                    }*/ 
                }

                public override void ShowHelp(bool show, EventHandler callback)
                {
                    PanelToDecorate.GetAssistance().ShowHelp(show, callback);
                }

                public void SetBackgroundColor(string colorName)
                {
                    BackgroundColorDecorated = colorName;
                    //PanelToDecorate.SetBackgroundColor(colorName);
                }

                public void SetEdgeColor(string colorName)
                {
                    PanelToDecorate.SetEdgeColor(colorName);
                }

                public void SetEdgeThickness(float thickness)
                {
                    PanelToDecorate.SetEdgeThickness(thickness);
                }

                public void EnableWeavingHand(bool enable)
                {
                    PanelToDecorate.EnableWeavingHand(enable);
                }

                public override Transform GetTransform()
                {
                    return PanelToDecorate.GetAssistance().GetTransform();
                }

                public override bool IsActive()
                {
                    return PanelToDecorate.GetAssistance().IsActive();
                }

                public Assistance GetAssistance()
                {
                    return this;//PanelToDecorate.GetAssistance();
                }

                public override bool IsDecorator()
                {
                    return true;
                }

                public Transform GetBackground()
                {
                    return PanelToDecorate.GetBackground();
                }
            }
        }
    }
}


