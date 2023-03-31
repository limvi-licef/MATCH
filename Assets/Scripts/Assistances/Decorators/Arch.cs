/*Copyright 2022 Louis Marquet, Léri Lamour

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
            public class Arch : Assistance, IAssistance
            {
                IAssistance PanelToDecorate;
                //Transform LineView;
                //LineToObject LineController;

                //EventHandler classEvent;

                private LineRenderer lineRenderer;
                //private bool toShow=false;
                private void Awake()
                {
                    //LineView = gameObject.transform.Find("Line");
                    //LineController = LineView.GetComponent<LineToObject>();
                    lineRenderer = GetComponent<LineRenderer>();
                    lineRenderer.positionCount = 2;
                    lineRenderer.SetPosition(0, Vector3.zero);
                    lineRenderer.SetPosition(1, Vector3.zero);
                    lineRenderer.startWidth = 0.1f;
                    lineRenderer.endWidth = 0.1f;
                    lineRenderer.startColor = Color.red;
                    lineRenderer.endColor = Color.red;

                }

                public void Start()
                {

                }

                public void Update()
                {
                    /*
                LineView = gameObject.transform.Find("Line");
                LineController = LineView.GetComponent<LineToObject>();
                LineController.PointOrigin = Camera.main.transform.position;

                LineView.GetComponent<LineToObject>().show(delegate
                {
                    classEvent?.Invoke(this, EventArgs.Empty);
                    //m_mutexShow = false;
                });*/
                    //if (isActiveAndEnabled && toShow)
                    //{
                    if (IsDisplayed)
                    {
                        Vector3 PlayerPosFeet = new Vector3(Camera.main.transform.position.x, (Camera.main.transform.position.y) - 0.5f, Camera.main.transform.position.z);
                        lineRenderer.SetPosition(0, PlayerPosFeet);
                        lineRenderer.SetPosition(1, PanelToDecorate.GetAssistance().GetTransform().position);
                    }
                    //}
                }


                public void SetAssistanceToDecorate(IAssistance toDecorate)
                {
                    PanelToDecorate = toDecorate;
                    name = PanelToDecorate.GetAssistance().name + "_decoratorArch";             


                    /*
                    transform.parent = PanelToDecorate.GetRootDecoratedAssistance().GetTransform();
                    transform.localPosition = PanelToDecorate.GetRootDecoratedAssistance().GetTransform().localPosition;
                    */
                    Assistance temp = PanelToDecorate.GetRootDecoratedAssistance();
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
                            GetArch().gameObject.SetActive(false);
                            

                            IsDisplayed = false;

                            callback?.Invoke(o, e);
                        }, withAnimation);

                    }
                    else
                    {
                        //GetArch().gameObject.SetActive(false);
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
                            PanelToDecorate.GetRootDecoratedAssistance().Show(Utilities.Utility.GetEventHandlerEmpty(), false);

                            

                            //classEvent = callback;

                            //LineController.PointOrigin = Camera.main.transform.position;
                            //LineController.PointEnd = PanelToDecorate.GetAssistance().transform.position;
                            /*
                            LineView.GetComponent<LineToObject>().show(delegate
                            {
                                callback?.Invoke(this, EventArgs.Empty);
                                //m_mutexShow = false;
                            });
                            */
                            PanelToDecorate.GetArch().gameObject.SetActive(false); //The decorated panels transform become invisible
                            

                            callback?.Invoke(this, e);
                        }, withAnimation);
                    }
                    else
                    {
                        // Check first if the decorated assistance needs to be displayed
                        PanelToDecorate.GetAssistance().Show(delegate (System.Object o, EventArgs e)
                        {
                            Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();

                            PanelToDecorate.GetArch().gameObject.SetActive(false); //The decorated panels transform become invisible
                   
                            args.Success = false;
                            callback?.Invoke(this, args);
                        }, withAnimation);
                    }
                }


                public override void ShowHelp(bool show, EventHandler callback, bool withAnimation)
                {
                    PanelToDecorate.GetRootDecoratedAssistance().ShowHelp(show, callback, withAnimation);
                }

                public override Transform GetTransform()
                {
                    return PanelToDecorate.GetRootDecoratedAssistance().GetTransform();
                }

                public Assistance GetRootDecoratedAssistance()
                {
                    return PanelToDecorate.GetRootDecoratedAssistance();
                }

                public Assistance GetAssistance()
                {
                    return this;
                }

                public Assistance GetDecoratedAssistance()
                {
                    return PanelToDecorate.GetAssistance();
                }

                public override bool IsDecorator()
                {
                    return true;
                }

                public Transform GetSound()
                {
                    return PanelToDecorate.GetSound();// SoundTransform;
                }

                public Transform GetArch()
                {
                    return transform;
                }
            }
        }
    }
}



