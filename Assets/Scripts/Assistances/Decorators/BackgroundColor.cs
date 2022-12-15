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

                Transform BackgroundView;

                private void Awake()
                {
                    BackgroundView = gameObject.transform.Find("ContentBackPlate");
                }

                public void Start()
                {
                    
                }

                public void SetAssistanceToDecorate(IPanel toDecorate)
                {
                    PanelToDecorate = toDecorate;

                    // Set the size of the background plate to fit the one of the decorated panel
                    BackgroundView.parent = PanelToDecorate.GetAssistance().transform;
                    BackgroundView.rotation = PanelToDecorate.GetBackground().rotation;
                    BackgroundView.localScale = PanelToDecorate.GetBackground().localScale;

                    // Relaying the eventhandler
                    Assistance temp = (Assistance)PanelToDecorate;
                    temp.EventHelpButtonClicked += delegate (System.Object o, EventArgs e)
                    {
                        MATCH.Utilities.EventHandlerArgs.Button args = (MATCH.Utilities.EventHandlerArgs.Button)e;
                        OnHelpButtonClicked(args.ButtonType);
                    };
                }

                public override void Hide(EventHandler callback, bool withAnimation)
                {
                    if (IsDisplayed)
                    {
                        PanelToDecorate.GetAssistance().Hide(delegate(System.Object o, EventArgs e)
                        {
                            BackgroundView.gameObject.SetActive(false);
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
                        
                        PanelToDecorate.GetAssistance().Show(delegate(System.Object o, EventArgs e)
                        {
                            PanelToDecorate.GetBackground().gameObject.SetActive(false);
                            BackgroundView.position = PanelToDecorate.GetBackground().position;
                            BackgroundView.gameObject.SetActive(true);

                            callback?.Invoke(this, e);
                        }, withAnimation);
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called with color "  + BackgroundColorDecorated);                                 
                    }
                    else
                    {
                        // Check first if the decorated assistance needs to be displayed
                        PanelToDecorate.GetAssistance().Show(delegate (System.Object o, EventArgs e)
                        {
                            Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();
                            PanelToDecorate.GetBackground().gameObject.SetActive(false);
                            args.Success = false;
                            callback?.Invoke(this, args);
                        }, withAnimation);
                    }
                }

                public override void ShowHelp(bool show, EventHandler callback, bool withAnimation)
                {
                    PanelToDecorate.GetAssistance().ShowHelp(show, callback, withAnimation);
                }

                public void EnableWeavingHand(bool enable)
                {
                    PanelToDecorate.EnableWeavingHand(enable);
                }

                public override Transform GetTransform()
                {
                    return PanelToDecorate.GetAssistance().GetTransform();
                }

                public Assistance GetAssistance()
                {
                    return this;
                }

                public override bool IsDecorator()
                {
                    return true;
                }

                public Transform GetBackground()
                {
                    return BackgroundView;
                }
            }
        }
    }
}


