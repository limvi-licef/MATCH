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
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;
using System.Reflection;
using System.Linq;
using TMPro;

namespace MATCH
{
    namespace Utilities
    {

            public class LineBetweenTwoPoints : MonoBehaviour
            {
                string DefaultMaterial = Utilities.Materials.Colors.OrangeGlowing;
                string HighlightMaterial = Utilities.Materials.Colors.CyanGlowing;

                LineRenderer Controller;

                private void Awake()
                {
                    Controller = gameObject.GetComponent<LineRenderer>();
                }

                // Start is called before the first frame update
                void Start()
                {
                    //LineRenderer temp = gameObject.GetComponent<LineRenderer>();
                    Controller.useWorldSpace = false;
                }

                // Update is called once per frame
                void Update()
                {

                }

                public void DrawLine(GameObject start, GameObject end)
                {
                    LineRenderer line = gameObject.AddComponent<LineRenderer>();
                    line.startColor = Color.blue;
                    line.endColor = Color.blue;
                    line.material = new Material(Resources.Load(DefaultMaterial, typeof(Material)) as Material);
                    line.startWidth = 0.001f;
                    line.endWidth = 0.001f;
                    line.positionCount = 2;
                    line.useWorldSpace = true;

                    line.SetPosition(0, start.transform.position);
                    line.SetPosition(1, end.transform.position);
                }

                public void HighlightConnector(bool highlight)
                {
                    if (highlight)
                    {
                        Controller.material = new Material(Resources.Load(HighlightMaterial, typeof(Material)) as Material);
                    }
                    else
                    {
                        Controller.material = new Material(Resources.Load(DefaultMaterial, typeof(Material)) as Material);
                    }
                }

                /**
                 * Be careful: Z is ignored!
                 * */
                public void DrawLineWithArrow(GameObject start, GameObject end, float offsetX = 0.0f, float offsetY = 0.0f)
                {
                    // Initializing

                    Controller.startColor = Color.blue;
                    Controller.endColor = Color.blue;
                    Controller.material = new Material(Resources.Load(DefaultMaterial, typeof(Material)) as Material);
                    Controller.startWidth = 0.001f;
                    Controller.endWidth = 0.001f;
                    Controller.positionCount = 5;
                    Controller.useWorldSpace = true;

                    Vector3 p1 = start.transform.localPosition;//new Vector2(start.transform.localPosition.x, start.transform.localPosition.y);
                    Vector3 p2 = end.transform.localPosition;// new Vector2(end.transform.localPosition.x, end.transform.localPosition.y) ;

                    p1 = gameObject.transform.InverseTransformPoint(start.transform.position); // Trick to draw the line in local space and thus moving it with the parent's object when moved;
                    p2 = gameObject.transform.InverseTransformPoint(end.transform.position); // Same

                    // Angle between the line defined by (x=0,y=0)(x=1,y=0) and the line defined by (p1)(p2)
                    Vector3 pEndLineRef = new Vector3(p2.x, p2.y - 1.0f, p2.z);

                    Vector3 p2pE = p2 - pEndLineRef;
                    Vector3 p2p1 = p2 - p1;

                    float angle = Vector3.SignedAngle(p2pE, p2p1, new Vector3(0, 0, 1));

                    // Calculations for the arrow
                    float length = Mathf.Sqrt(Mathf.Pow(p1.x - p2.x, 2) + Mathf.Pow(p1.y - p2.y, 2));
                    float lengthArrow = 0.005f;//length * proportion;
                    float v3dx = p2.x + Mathf.Cos(70 * Mathf.PI / 180.0f); // Vector destination for later determining p3
                    float v3dy = p2.y + Mathf.Sin(70 * Mathf.PI / 180.0f);
                    float v4dx = p2.x + Mathf.Cos(110 * Mathf.PI / 180.0f); // Vector destination for later determining p4
                    float v4dy = p2.y + Mathf.Sin(110 * Mathf.PI / 180.0f);
                    float v3x = p2.x - v3dx; // Vector direction for p3
                    float v3y = p2.y - v3dy;
                    float v4x = p2.x - v4dx; // Vector direction for p4
                    float v4y = p2.y - v4dy;
                    Vector3 p3 = new Vector3(p2.x + v3x * lengthArrow, p2.y + v3y * lengthArrow, p2.z); // First point for arrow
                    Vector3 p4 = new Vector3(p2.x + v4x * lengthArrow, p2.y + v4y * lengthArrow, p2.z); // Second point for arrow

                    Vector3 dir3 = p3 - p2;
                    Vector3 dir4 = p4 - p2;

                    Vector3 p3r = (Quaternion.AngleAxis(angle, new Vector3(0, 0, 1)) * new Vector3(dir3.x, dir3.y)) + p2;
                    Vector3 p4r = (Quaternion.AngleAxis(angle, new Vector3(0, 0, 1)) * new Vector3(dir4.x, dir4.y)) + p2;

                    // Drawing
                    Controller.SetPosition(0, p1);
                    Controller.SetPosition(1, p2);
                    Controller.SetPosition(2, p3r);
                    Controller.SetPosition(3, p4r);
                    Controller.SetPosition(4, p2);
                }
            }

        }
    }