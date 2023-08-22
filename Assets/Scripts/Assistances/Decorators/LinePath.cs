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
using static UnityEngine.UI.GridLayoutGroup;
using MATCH.Scenarios.FiniteStateMachine;

namespace MATCH
{
    namespace Assistances
    {
        namespace Decorators
        {
            public class LinePath : Assistance, IAssistance
            {
                IAssistance PanelToDecorate;
                //Transform LineView;
                //LineToObject LineController;
                bool IsLineVisible;

                List<Vector3> points;
                private Inferences.Manager InfManager;
                //EventHandler classEvent;

                private LineRenderer Line;
                //private bool toShow=false;
                PathFinding.PathFinding PathFinderEngine;
                Assistances.InteractionSurface FollowObject;
                private bool HeightToFollowInteractionSurface;

                static public float Threshold = 3.0f;

                private void Awake()
                {
                    points = new List<Vector3>();
                    //LineView = gameObject.transform.Find("Line");
                    //LineController = LineView.GetComponent<LineToObject>();
                    Line = GetComponent<LineRenderer>();
                    Line.startWidth = 0.017f;
                    Line.endWidth = 0.017f;
                    //lineRenderer.material = Resources.Load(Utilities.Materials.Colors.GreenGlowing, typeof(Material)) as Material;
                    Line.positionCount = 0;
                    Line.startColor = Color.red;
                    Line.endColor = Color.red;
                    InfManager = Inferences.Factory.Instance.CreateManager(transform);
                    PathFinderEngine = GameObject.Find("PathFinderEngine").GetComponent<PathFinding.PathFinding>();
                    FollowObject = Assistances.InteractionSurfaceFollower.Instance.GetInteractionSurface();

                    HeightToFollowInteractionSurface = false;
                }

                public void Start()
                {
                    /*AdminMenu.Instance.AddInputWithButton(Threshold.ToString(), "Threshold compute LinePath", delegate (System.Object o, EventArgs e)
                    {
                        Utilities.EventHandlerArgs.String arg = (Utilities.EventHandlerArgs.String)e;
                        Threshold = float.Parse(arg.m_text);
                    }, AdminMenu.Panels.Right);*/
                }

                public void Update()
                {
                    if (IsDisplayed)
                    {
                        // Allows to recalculate the line path if the person moves away from the line
                        float dist = Utilities.Utility.CalculateMinDistanceOfALine(Line);
                        if (dist > Threshold)
                        {
                            ShowLightpath();
                        }
                    }


                }

                /*float CalculateMinDistance()
                {
                    float dMin = 10000;
                    float d = -1;
                    Vector3 corner;

                    Vector3 cameraPos = Camera.main.transform.position;

                    for (int i = 0; i < Line.positionCount; i++)
                    {
                        corner = Line.GetPosition(i);

                        d = Utilities.Utility.CalculateDistancePoints(cameraPos, corner);

                        if (d < dMin)
                        {
                            dMin = d;
                        }
                    }

                    return dMin;
                }*/

                public void SetHeightToFollowInteractionSurface(bool heightToFollow)
                {
                    HeightToFollowInteractionSurface = heightToFollow;
                }

                public void SetAssistanceToDecorate(IAssistance toDecorate, bool lineVisible)
                {
                    PanelToDecorate = toDecorate;
                    name = PanelToDecorate.GetAssistance().name + "_decoratorLinePath";

                    transform.parent = PanelToDecorate.GetRootDecoratedAssistance().GetTransform();
                    transform.localPosition = PanelToDecorate.GetRootDecoratedAssistance().GetTransform().localPosition;

                    IsLineVisible = lineVisible;

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
                            GetLinePath().gameObject.SetActive(false);


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

                            // This is in case the line path decorates a previous line path. In this case, the previous one is hidden
                            Transform decoratedLinePath = PanelToDecorate.GetLinePath();

                            if (decoratedLinePath != null)
                            {
                                decoratedLinePath.gameObject.SetActive(false); //The decorated panels transform become invisible
                            }

                            ShowLightpath();

                            callback?.Invoke(this, e);
                        }, withAnimation);
                    }
                    else
                    {
                        // Check first if the decorated assistance needs to be displayed
                        PanelToDecorate.GetAssistance().Show(delegate (System.Object o, EventArgs e)
                        {
                            Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();

                            PanelToDecorate.GetLinePath().gameObject.SetActive(false); //The decorated panels transform become invisible

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
                    return PanelToDecorate.GetArch();
                }

                public Assistances.Icon GetIcon()
                {
                    return PanelToDecorate.GetIcon();
                }

                public Transform GetLinePath()
                {
                    return transform;
                }

                void ShowLightpath()
                {
                    IsDisplayed = false;

                    Vector3 startPoint = Vector3.forward * (float)1.5;

                    Vector3 startPointWorld = Camera.main.transform.TransformPoint(startPoint);

                    Line.useWorldSpace = true;

                    Vector3[] corners = PathFinderEngine.ComputePath(/*FollowObject.transform*/ startPointWorld, GetRootDecoratedAssistance().GetTransform().position);

                    Utilities.Utility.Linear coeff = Utilities.Utility.CalculateLinearCoefficients(0, /*corners[0].y*/Assistances.InteractionSurfaceFollower.Instance.transform.position.y, corners.Length - 1, /*corners[corners.Length-1].y*/ GetRootDecoratedAssistance().GetTransform().position.y);

                    Line.positionCount = corners.Length;
                    for (int i = 0; i < corners.Length; i++)
                    {
                        Vector3 corner = corners[i];

                        if (HeightToFollowInteractionSurface)
                        {
                            corner.y = coeff.a * i + coeff.b;// Assistances.InteractionSurfaceFollower.Instance.transform.position.y;
                        }

                        Line.SetPosition(i, corner);
                    }

                    IsDisplayed = true;
                }
            }
        }
    }
}



