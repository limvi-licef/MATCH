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
            public class Sound : Assistance, IPanel2
            {
                IPanel2 PanelToDecorate;
               

                private void Awake()
                {
                
                }

                public void Start()
                {

                }

             

                public void SetAssistanceToDecorate(IPanel2 toDecorate)
                {
                    PanelToDecorate = toDecorate;

                   
                    name = PanelToDecorate.GetAssistance().name + "_decoratorSound";
                    

                    transform.parent = PanelToDecorate.GetDecoratedAssistance().transform;
                    transform.localPosition = PanelToDecorate.GetBackground().transform.localPosition;

                    
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
                        PanelToDecorate.GetAssistance().Hide(delegate (System.Object o, EventArgs e)
                        {
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

                        PanelToDecorate.GetAssistance().Show(delegate (System.Object o, EventArgs e)
                        {
                            
                            DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Sound played ");


                            PanelToDecorate.GetDecoratedAssistance().Show(Utilities.Utility.GetEventHandlerEmpty(), false);
                           

                            callback?.Invoke(this, e);
                        }, withAnimation);
                    }
                    else
                    {
                        // Check first if the decorated assistance needs to be displayed
                        PanelToDecorate.GetDecoratedAssistance().Show(delegate (System.Object o, EventArgs e)
                        {
                            Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();
                            //PanelToDecorate.GetBackground().gameObject.SetActive(false);
                            PanelToDecorate.GetBackgroundMessage().gameObject.SetActive(false);
                            //PanelToDecorate.GetBackgroundIcon().gameObject.SetActive(false);
                            args.Success = false;
                            callback?.Invoke(this, args);
                        }, withAnimation);
                    }
                }

                public override void ShowHelp(bool show, EventHandler callback, bool withAnimation)
                {
                    PanelToDecorate.GetDecoratedAssistance().ShowHelp(show, callback, withAnimation);
                }

                public void EnableWeavingHand(bool enable)
                {
                    PanelToDecorate.EnableWeavingHand(enable);
                }

                public override Transform GetTransform()
                {
                    return PanelToDecorate.GetDecoratedAssistance().GetTransform();
                }

                public Assistance GetDecoratedAssistance()
                {
                    return PanelToDecorate.GetDecoratedAssistance();
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
                    return /*BackgroundParent*/transform;
                }

                public Transform GetBackgroundIcon()
                {
                    return PanelToDecorate.GetBackgroundIcon()/*BackgroundIcon*/;
                }

                public Transform GetBackgroundMessage()
                {
                    return PanelToDecorate.GetBackgroundMessage();
                }
            }
        }
    }
}



