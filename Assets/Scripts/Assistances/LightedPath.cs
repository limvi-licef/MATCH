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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Reflection;
using System;

namespace MATCH
{
    namespace Assistances
    {
        public class LightedPath : Assistance
        {
            static public float Threshold = 3.0f;

            public PathFinding.PathFinding PathFindingEngine;

            /*private Vector3 PointBegin;
            private Vector3 PointEnd;*/
            // References to the objects so that the path can be updated if the objects move
            Transform ObjectBegin;
            bool ObjectBeginFollower;
            Transform ObjectEnd;

            private Vector3[] PathPoints;

            private LineRenderer PathLine;
            private Material PathLineMaterial;
            private string PathLinePulseDistanceProperty = "_PulseDistance";
            private float PathLinePulseDistanceToAdd;

            private bool HeightToFollowInteractionSurface;

            private void Awake()
            {
                PathLine = transform.Find("Path").GetComponent<LineRenderer>();
                PathLine.startWidth = 0.017f;
                PathLine.endWidth = 0.017f;
                PathLine.positionCount = 0;
                PathLine.startColor = Color.cyan;
                PathLine.endColor = Color.cyan;
                PathLine.useWorldSpace = true;
                HeightToFollowInteractionSurface = false;
                ObjectBeginFollower = false;

                if (Utilities.Utility.IsEditorSimulator() || Utilities.Utility.IsEditorGameView())
                {
                    PathLinePulseDistanceToAdd = 0.001f;
                }
                else
                { // Means running in the Hololens: the frame rate being lower, the value is higher, to have a faster color change
                    PathLinePulseDistanceToAdd = 0.05f;
                }
            }

            public void SetHeightToFollowInteractionSurface(bool heightToFollow)
            {
                HeightToFollowInteractionSurface = heightToFollow;
            }

            public void Update()
            {
                if (IsDisplayed)
                {
                    // Determine if the line needs to be recomputed
                    float dist = Utilities.Utility.CalculateMinDistanceOfALine(PathLine);
                    if (dist > Threshold)
                    {
                        ShowLinePath();
                    }

                    // Move the pulse
                    for (int i = 1; i < 4; i++)
                    {
                        PathLineMaterial.SetFloat(PathLinePulseDistanceProperty+i, PathLineMaterial.GetFloat(PathLinePulseDistanceProperty+i) + PathLinePulseDistanceToAdd);
                        if (PathLineMaterial.GetFloat(PathLinePulseDistanceProperty+i) >= 0.98f)
                        {
                            PathLineMaterial.SetFloat(PathLinePulseDistanceProperty+i, 0.0f);
                        }
                    }
                    
                }
            }

            private void ShowLinePath()
            {
                IsDisplayed = false;

                PathLine.gameObject.SetActive(false);

                Vector3 positionBegining;

                if (ObjectBeginFollower)
                {
                    Vector3 startPoint = Vector3.forward * (float)1;

                    positionBegining = Camera.main.transform.TransformPoint(startPoint);
                }
                else
                {
                    positionBegining = ObjectBegin.position;
                }

                Vector3[] corners = PathFindingEngine.ComputePath(positionBegining, ObjectEnd.position);

                Utilities.Utility.Linear coeff = Utilities.Utility.CalculateLinearCoefficients(0, Assistances.InteractionSurfaceFollower.Instance.transform.position.y, corners.Length - 1, ObjectEnd.position.y);

                PathLine.positionCount = corners.Length;
                for (int i = 0; i < corners.Length; i++)
                {
                    Vector3 corner = corners[i];

                    if (HeightToFollowInteractionSurface)
                    {
                        corner.y = coeff.a * i + coeff.b;
                    }

                    PathLine.SetPosition(i, corner);
                }

                for (int i = 1; i < 4; i ++)
                {
                    PathLineMaterial.SetFloat(PathLinePulseDistanceProperty+i, 0.3f*i-0.3f); // Pulse is initialized at the beginning of the line
                }
                

                PathLine.gameObject.SetActive(true);

                IsDisplayed = true;
            }

            private void Start()
            {
                PathLine = transform.Find("Path").GetComponent<LineRenderer>();
                PathLine.startWidth = 0.017f;
                PathLine.endWidth = 0.017f;
                PathLine.positionCount = 0;
                PathLine.startColor = Color.cyan;
                PathLine.endColor = Color.cyan;

                PathLine.useWorldSpace = true;
                PathLine.GetComponent<Renderer>().material = Utilities.Utility.LoadMaterial(Utilities.Materials.Colors.CyanPulse);
                PathLineMaterial = PathLine.GetComponent<Renderer>().material;

                //controller.SetHeightToFollowInteractionSurface(heightToFollowInteractionSurface);
            }

            public void SetBeginAndEndObjects(Transform begin, Transform end)
            {
                ObjectBegin = begin;
                ObjectBeginFollower = false;
                ObjectEnd = end;

                PathPoints = PathFindingEngine.ComputePath(begin, end);
            }

            /**
             * In this case, the begin object is in front of the follower
             * */
            public void SetEndObject(Transform end)
            {
                ObjectEnd = end;
                ObjectBegin = MATCH.Assistances.InteractionSurfaceFollower.Instance.GetInteractionSurface().transform;
                ObjectBeginFollower = true;
            }

            /**
             * withAnimation: ignored for now
             */
            public override void Show(EventHandler eventHandler, bool withAnimation)
            {
                Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();

                if (IsDisplayed == false)
                {
                    ShowLinePath();

                    IsDisplayed = true;
                    args.Success = true;
                    eventHandler?.Invoke(this, args);
                    OnIsShown(this, args);
                }
                else
                {
                    args.Success = false;
                    eventHandler?.Invoke(this, args);
                    OnIsShown(this, args);
                }

            }

            /**
             * Todo: withAnimation not implemented yet
             */
            public override void Hide(EventHandler eventHandler, bool withAnimation)
            {
                Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();

                if (IsDisplayed)
                {
                    PathLine.positionCount = 1;
                    PathLine.gameObject.SetActive(false);

                    IsDisplayed = false;
                    args.Success = true;
                    eventHandler?.Invoke(this, args);
                    OnIsHidden(this, args);
                }
                else
                {
                    args.Success = false;
                    eventHandler?.Invoke(this, args);
                    OnIsHidden(this, args);
                }
            }

            /**
             * Todo: to implement
             */
            public override void ShowHelp(bool show, EventHandler callback, bool withAnimation)
            {
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Not implemented yet");
            }

            public override Transform GetTransform()
            {
                return transform;
            }

            public Assistance GetAssistance()
            {
                return this;
            }

            public override bool IsDecorator()
            {
                return false;
            }

            public override void Emphasize(bool enable)
            {
                if (enable)
                {
                    Utilities.Emphasize emphasize = gameObject.AddComponent<Utilities.Emphasize>();

                    emphasize.AddMaterial(PathLine.transform);
                    emphasize.EnableEmphasize(true);

                }
                else
                {
                    Utilities.Emphasize emphasize = null;

                    if (gameObject.TryGetComponent<Utilities.Emphasize>(out emphasize))
                    {
                        emphasize.EnableEmphasize(false);

                        Destroy(emphasize);
                    }
                }
            }
        }
    }
}