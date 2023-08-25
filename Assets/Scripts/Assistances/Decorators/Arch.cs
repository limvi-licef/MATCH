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
                bool IsArchVisible;

                List<Vector3> points = new List<Vector3>();
                private Inferences.Manager InfManager;
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
                    InfManager = Inferences.Factory.Instance.CreateManager(transform);

                }

                public void Start()
                {

                }

                public void Update()
                {
                
                }


                public void SetAssistanceToDecorate(IAssistance toDecorate, bool archVisible)
                {
                    PanelToDecorate = toDecorate;
                    name = PanelToDecorate.GetAssistance().name + "_decoratorArch";             

                    transform.parent = PanelToDecorate.GetRootDecoratedAssistance().GetTransform();
                    transform.localPosition = PanelToDecorate.GetRootDecoratedAssistance().GetTransform().localPosition;

                    IsArchVisible = archVisible;

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

                            PanelToDecorate.GetArch().gameObject.SetActive(false); //The decorated panels transform become invisible

                            if (IsArchVisible)
                            {
                                drawArch();
                            }

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

                public Assistances.Icon GetIcon()
                {
                    return PanelToDecorate.GetIcon();
                }

                public Transform GetLinePath()
                {
                    return PanelToDecorate.GetLinePath();
                }

                private void drawArch()
                {
                    if (IsDisplayed)
                    {
                        MATCH.Assistances.InteractionSurfaceFollower.Instance.GetInteractionSurface().EventUserMoved += delegate (System.Object o, EventArgs e)
                        {
                            Utilities.EventHandlerArgs.Position pos = (Utilities.EventHandlerArgs.Position)e;

                            Vector3 PlayerPosInFrontOfUser = pos.PositionWorld;
                            Vector3 FinalPos = PanelToDecorate.GetAssistance().GetTransform().position;
                            FinalPos.y = FinalPos.y - 0.2f; //end of the arch under the assistance to avoid hiding the text

                            Vector3 LineUserAssistance = FinalPos - PlayerPosInFrontOfUser;
                            Vector3 NormalizedLine = new Vector3(-LineUserAssistance.z, 0, LineUserAssistance.x).normalized; //normalize the line in the xz plane
                            //Vector3 NormalizedLine = new Vector3(-LineUserAssistance.y, LineUserAssistance.x, 0).normalized; //normalizes the line for a vertical arch


                            Inferences.Timer temp = new Inferences.Timer("ArchUpdate", 1, delegate (System.Object oo, EventArgs ee)
                            {
                                ((Inferences.Timer)InfManager.GetInference("ArchUpdate")).StopCounter();
                                InfManager.UnregisterInference("ArchUpdate");
                                Vector3 CornerPoint = Vector3.Reflect(Camera.main.transform.position - PlayerPosInFrontOfUser * 1.25f, NormalizedLine) + PlayerPosInFrontOfUser * 1.25f; //1.25f so that the point is a little further
                                float distanceCornerUser = /*Vector3.Distance(Camera.main.transform.position, CornerPoint);*/ Mathf.Sqrt(Mathf.Pow(Camera.main.transform.position.x - CornerPoint.x, 2) + Mathf.Pow(Camera.main.transform.position.z - CornerPoint.z, 2));
                                float distanceFrontAssistance = Mathf.Sqrt(Mathf.Pow(FinalPos.x - PlayerPosInFrontOfUser.x, 2) + Mathf.Pow(FinalPos.z - PlayerPosInFrontOfUser.z, 2));
                                float distanceUserAssistance = Mathf.Sqrt(Mathf.Pow(Camera.main.transform.position.x - FinalPos.x, 2) + Mathf.Pow(Camera.main.transform.position.z - FinalPos.z, 2));
                            
                            

                                if (distanceCornerUser > 0.5f || distanceFrontAssistance < distanceUserAssistance)
                                {
                                    distanceCornerUser = 4f;
                                }

                                CornerPoint = Vector3.Reflect(Camera.main.transform.position - PlayerPosInFrontOfUser * 5f / distanceCornerUser, NormalizedLine) + PlayerPosInFrontOfUser * 5f / distanceCornerUser;
                            
                                points = MATCH.Utilities.Utility.CalculateBezierCurve(PlayerPosInFrontOfUser, FinalPos, CornerPoint);
                                lineRenderer.positionCount = points.Count;
                                for (int i = 0; i < points.Count; i++)
                                {
                                    lineRenderer.SetPosition(i, points[i]);
                                }
                            });


                            InfManager.RegisterInference(temp);
                            temp.StartCounter();
                        };                    
                    }
                }

                public override void Emphasize(bool enable)
                {
                    PanelToDecorate.GetAssistance().Emphasize(enable);
                }
            }
        }
    }
}



