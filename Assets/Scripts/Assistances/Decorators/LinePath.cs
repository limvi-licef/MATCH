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

                List<Vector3> points = new List<Vector3>();
                private Inferences.Manager InfManager;
                //EventHandler classEvent;

                private LineRenderer lineRenderer;
                //private bool toShow=false;
                PathFinding.PathFinding PathFinderEngine;
                Assistances.InteractionSurface FollowObject;
                

                private void Awake()
                {
                    
                    //LineView = gameObject.transform.Find("Line");
                    //LineController = LineView.GetComponent<LineToObject>();
                    lineRenderer = GetComponent<LineRenderer>();
                    lineRenderer.startWidth = 0.017f;
                    lineRenderer.endWidth = 0.017f;
                    //lineRenderer.material = Resources.Load(Utilities.Materials.Colors.GreenGlowing, typeof(Material)) as Material;
                    lineRenderer.positionCount = 0;
                    lineRenderer.startColor = Color.red;
                    lineRenderer.endColor = Color.red;
                    InfManager = Inferences.Factory.Instance.CreateManager(transform);
                    PathFinderEngine = GameObject.Find("PathFinderEngine").GetComponent<PathFinding.PathFinding>();
                    FollowObject = Assistances.InteractionSurfaceFollower.Instance.GetInteractionSurface();
                    

                }

                public void Start()
                {

                }

                public void Update()
                {
                    if (IsLineVisible)
                    {
                        updateLightPath();
                    }
                }


                public void SetAssistanceToDecorate(IAssistance toDecorate, bool archVisible)
                {
                    PanelToDecorate = toDecorate;
                    name = PanelToDecorate.GetAssistance().name + "_decoratorArch";

                    transform.parent = PanelToDecorate.GetRootDecoratedAssistance().GetTransform();
                    transform.localPosition = PanelToDecorate.GetRootDecoratedAssistance().GetTransform().localPosition;

                    IsLineVisible = archVisible;

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

                 

                void ShowLightpath()
                {
                    

                    Vector3[] corners = PathFinderEngine.ComputePath(FollowObject.transform, GetRootDecoratedAssistance().GetTransform());

                    GameObject gameObjectForLine = new GameObject("Line redering mdr");
                    lineRenderer = gameObjectForLine.AddComponent<LineRenderer>();
                    lineRenderer.positionCount = corners.Length;
                    for (int i = 0; i < corners.Length; i++)
                    {
                        Vector3 corner = corners[i];

                        lineRenderer.SetPosition(i, corner);

                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Corner : " + corner);
                    }
                       
                }

                void updateLightPath()
                {
                    GameObject gameObjectForLine = GameObject.Find("Line redering mdr");
                    Destroy(gameObjectForLine);
                    ShowLightpath();
                }
            }
        }
    }
}



