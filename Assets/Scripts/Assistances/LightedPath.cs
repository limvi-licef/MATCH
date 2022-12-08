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

/**
 * This class manages a basic show / hide for a child that should be named ... "child" (I am a creative person)
 * An event is emitted if the nested hologram is touched.
 * Appears / disappears in place. I told you it was basic.
 * */
namespace MATCH
{
    namespace Assistances
    {
        public class LightedPath : Assistance
        {
            public PathFinding.PathFinding PathFindingEngine;

            /*private Vector3 PointBegin;
            private Vector3 PointEnd;*/
            // References to the objects so that the path can be updated if the objects move
            Transform ObjectBegin;
            Transform ObjectEnd;

            private Vector3[] PathPoints;

            private LineRenderer PathLine;

            private void Awake()
            {

            }

            private void Start()
            {
                PathLine = transform.Find("Path").GetComponent<LineRenderer>();
            }

            public void SetBeginAndEndObjects(Transform begin, Transform end)
            {
                ObjectBegin = begin;
                ObjectEnd = end;

                PathPoints = PathFindingEngine.ComputePath(begin, end);
            }

            public override void Show(EventHandler eventHandler)
            {

                if (IsDisplayed == false)
                {
                    PathLine.positionCount = PathPoints.Length;

                    for (int i = 0; i < PathPoints.Length; i ++)
                    {
                        Vector3 point = PathPoints[i];
                        PathLine.SetPosition(i, point);
                    }

                    IsDisplayed = true;
                }

            }

            public override void Hide(EventHandler eventHandler)
            {
                if (IsDisplayed)
                {
                    PathLine.positionCount = 1;
                    PathLine.gameObject.SetActive(false);

                    IsDisplayed = false;
                }
            }

            public override void ShowHelp(bool show, EventHandler callback)
            {

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
        }
    }
}