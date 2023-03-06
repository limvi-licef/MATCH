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
            public class BackgroundColorMessage2 : Assistance, IPanel2
            {
                IPanel2 PanelToDecorate;
                //string BackgroundColorDecorated;

                //Transform BackgroundParent;
                Transform BackgroundMessage;
                //Transform BackgroundIcon;

                //List<BackgroundColorFor2> ToHideOnShow;

                private void Awake()
                {
                    /*BackgroundParent = gameObject.transform.Find("Dialog");
                    BackgroundMessage = BackgroundParent.Find("Modale-Support_Cube.010");
                    BackgroundIcon = BackgroundParent.Find("Modale-Rond_Cylinder.003");*/

                    BackgroundMessage = transform.Find("Modale-Support_Cube.010");
                    //BackgroundIcon = transform.Find("Modale-Rond_Cylinder.003");

                    //ToHideOnShow = new List<BackgroundColorFor2>();
                }

                public void Start()
                {
                    
                }

                /*public void AddToHideOnShow(BackgroundColorFor2 toHide)
                {
                    ToHideOnShow.Add(toHide);
                }*/

                public void SetAssistanceToDecorate(IPanel2 toDecorate)
                {
                    PanelToDecorate = toDecorate;

                    // Set the size of the background plate to fit the one of the decorated panel
                    //BackgroundViewMain.parent = PanelToDecorate.GetAssistance().transform;
                    name = PanelToDecorate.GetAssistance().name + "_decoratorBackgroundColorFor2";
                    //BackgroundParent.parent = PanelToDecorate.GetAssistance().transform;
                    //BackgroundParent.position = PanelToDecorate.GetAssistance().transform.position;

                    transform.parent = PanelToDecorate.GetDecoratedAssistance().transform;
                    //transform.position = PanelToDecorate.GetAssistance().transform.position;
                    transform.localPosition = PanelToDecorate.GetBackground().transform.localPosition;

                    //BackgroundParent.transform.SetParent(PanelToDecorate.GetAssistance().transform);

                    //BackgroundParent.localPosition = PanelToDecorate.GetAssistance().transform.localPosition;
                    //BackgroundParent.localScale = PanelToDecorate.GetAssistance().transform.localScale;


                    /*BackgroundViewMain.rotation = PanelToDecorate.GetBackground().rotation;
                    BackgroundViewMain.localScale = PanelToDecorate.GetBackground().localScale;
                    BackgroundViewIcon.rotation = PanelToDecorate.GetBackgroundIcon().rotation;
                    BackgroundViewIcon.localScale = PanelToDecorate.GetBackgroundIcon().localScale;*/

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
                            BackgroundMessage.gameObject.SetActive(false);
                            //BackgroundIcon.gameObject.SetActive(false);
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
                    BackgroundMessage.position = PanelToDecorate.GetBackgroundMessage().position;
                    BackgroundMessage.localScale = PanelToDecorate.GetBackgroundMessage().localScale;

                    //BackgroundIcon.position = PanelToDecorate.GetBackgroundIcon().position;
                    //BackgroundIcon.localScale = PanelToDecorate.GetBackgroundIcon().localScale;

                    BackgroundMessage.gameObject.SetActive(true);
                    //BackgroundIcon.gameObject.SetActive(true);

                    if (IsDisplayed == false)
                    {
                        IsDisplayed = true;

                        //PanelToDecorate.GetDecoratedAssistance().Show(delegate(System.Object o, EventArgs e)
                        PanelToDecorate.GetAssistance().Show(delegate (System.Object o, EventArgs e)
                        {
                            //BackgroundMessage.gameObject.SetActive(true);
                            //Back

                            /*foreach (Transform child in PanelToDecorate.GetAssistance().transform)
                            {
                                if (child.name == "Dialog")
                                {
                                    foreach (Transform dialogComponent in child)
                                    {
                                        dialogComponent.gameObject.SetActive(false);
                                    }
                                }
                            }*/

                            /*foreach (BackgroundColorFor2 toHide in ToHideOnShow)
                            {
                                toHide.Hide(Utilities.Utility.GetEventHandlerEmpty(), false);
                            }*/

                            //PanelToDecorate.GetBackground().gameObject.SetActive(false);

                            PanelToDecorate.GetDecoratedAssistance().Show(Utilities.Utility.GetEventHandlerEmpty(), false);
                            //PanelToDecorate.GetBackground().gameObject.SetActive(false);
                            //PanelToDecorate.GetBackgroundIcon().gameObject.SetActive(false);
                            PanelToDecorate.GetBackgroundMessage().gameObject.SetActive(false);

                            /*BackgroundParent.position = PanelToDecorate.GetBackground().position;
                            BackgroundParent.localScale = PanelToDecorate.GetBackground().localScale;
                            BackgroundParent.rotation = PanelToDecorate.GetBackground().rotation;*/

                            /*transform.position = PanelToDecorate.GetBackground().position;
                            transform.localScale = PanelToDecorate.GetBackground().localScale;
                            transform.rotation = PanelToDecorate.GetBackground().rotation;*/

                            transform.localPosition = ((IPanel2)PanelToDecorate.GetDecoratedAssistance()).GetBackground().localPosition;
                            transform.localScale = ((IPanel2)PanelToDecorate.GetDecoratedAssistance()).GetBackground().localScale;
                            transform.rotation = ((IPanel2)PanelToDecorate.GetDecoratedAssistance()).GetBackground().rotation;




                            callback?.Invoke(this, e);
                        }, withAnimation);
                        /*DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called with color "  + BackgroundColorDecorated);   */                              
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
                    return BackgroundMessage;
                }
            }
        }
    }
}


