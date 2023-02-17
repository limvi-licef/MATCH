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
using System.Reflection;
using System;

/**
 * Draw an arch between two given objects. Uses a Bézier quadratic curve to draw the line.
 * */
namespace MATCH
{
    namespace Assistances
    {
        public class LineToObject : MonoBehaviour
        {
            /*public GameObject m_hologramTarget;
            public GameObject m_hologramOrigin;*/
            public Vector3 PointOrigin { get; set; }
            public Vector3 PointEnd { get; set; }
            public int m_numPoints = 1000;

            LineRenderer m_line;

            bool m_drawLine;

            private float m_timerWaitTime = 0.006f;
            private float m_timer = 0.0f;
            float m_drawWithAnimationT; // when drawing with animation
            Vector3 m_drawWithAnimationStartingPoint;
            Vector3 m_drawWithAnimationMidPoint;
            Vector3 m_drawWithAnimationEndPoint;

            event EventHandler m_eventProcessFinished;

            private void Awake()
            {
                m_line = gameObject.GetComponent<LineRenderer>();
                m_drawLine = false;
            }

            // Start is called before the first frame update
            void Start()
            {

            }

            // Update is called once per frame
            void Update()
            {
                if (m_drawLine)
                {
                    m_timer += Time.deltaTime;

                    // Check if we have reached beyond 2 seconds.
                    // Subtracting two is more accurate over time than resetting to zero.
                    if (m_timer > m_timerWaitTime)
                    {
                        // Draw a point
                        m_drawWithAnimationT += 1.0f / (float)m_numPoints;

                        /*if (m_drawWithAnimationT > (1.0f/(float)m_numPoints)*200.0f)
                        {*/
                            m_line.positionCount++;
                            m_line.SetPosition(m_line.positionCount - 1, calculateQuadraticBezierPoint(m_drawWithAnimationT, m_drawWithAnimationStartingPoint, m_drawWithAnimationMidPoint, m_drawWithAnimationEndPoint));

                            // Remove the recorded 2 seconds.
                            m_timer = m_timer - m_timerWaitTime;

                            // Stoping the process when the line is fully drawn
                            if (m_drawWithAnimationT >= 1.0f)
                            {
                                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Finished drawing the line");
                                m_drawLine = false;

                                m_eventProcessFinished?.Invoke(this, EventArgs.Empty);

                                m_mutexShow = false;
                           // }


                        }

                        
                    }
                    /*while (m_line.positionCount < m_numPoints)
                    {
                        m_line.positionCount++;
                        m_line.SetPosition(m_line.positionCount - 1, m_line.GetPosition(m_line.positionCount - 2));
                    }*/
                }
            }

            bool m_mutexShow = false;
            public void show(EventHandler eventHandler)
            {
                if (m_mutexShow == false)
                {
                    m_mutexShow = true;

                    if (gameObject.activeSelf == false)
                    {
                        //Vector3 startPoint = m_hologramOrigin.transform.position;
                        //Vector3 endPoint = m_hologramTarget.transform.position;
                        Vector3 midPoint = (PointOrigin + PointEnd) / 2;
                        midPoint.y += 1.0f;

                        gameObject.SetActive(true);

                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Drawing line with animation - Starting point: " + PointOrigin.ToString() + " Mid point: " + midPoint.ToString() + " End point: " + PointEnd.ToString());

                        m_drawWithAnimationT = 0.0f;
                        m_drawWithAnimationStartingPoint = PointOrigin;
                        m_drawWithAnimationMidPoint = midPoint;
                        m_drawWithAnimationEndPoint = PointEnd;

                        m_line.SetPosition(0, PointOrigin);

                        m_eventProcessFinished += eventHandler;

                        m_drawLine = true;
                    }
                    else
                    {
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Line already shown - nothing to do");
                        m_mutexShow = false;
                        eventHandler?.Invoke(this, EventArgs.Empty);
                    }
                }
                else
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Mutex locked - request ignored");
                }
            }

            void drawQuadraticCurve(Vector3 startPoint, Vector3 midPoint, Vector3 endPoint)
            {
                float t = 0.0f;

                m_line.SetPosition(0, startPoint);


                for (int i = 0; i < m_numPoints; i++)
                {
                    t = (float)i / (float)m_numPoints;

                    m_line.positionCount++;
                    m_line.SetPosition(m_line.positionCount - 1, calculateQuadraticBezierPoint(t, startPoint, midPoint, endPoint));
                }
            }

            // Source: https://www.youtube.com/watch?v=Xwj8_z9OrFw
            Vector3 calculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
            {
                // B(t) = (1-t)2P0 + 2(1-t)tP1 + t2P2 , 0 < t < 1
                Vector3 toReturn;

                toReturn = (1.0f - t) * (1.0f - t) * p0 + 2 * (1 - t) * t * p1 + t * t * p2;

                return toReturn;
            }

            bool m_mutexHide = false;
            public void hide(EventHandler eventHandler) // Does not work with animations
            {
                if (m_mutexHide == false)
                {
                    m_mutexHide = true;

                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Hiding line - setting position counter to 0, so that the points will be overwritten next time it is displayed");

                    m_line.positionCount = 1;

                    //m_line.Res

                    m_line.gameObject.SetActive(false);

                    eventHandler?.Invoke(this, EventArgs.Empty);

                    m_mutexHide = false;
                }
                else
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Mutex locked - request ignored");
                }

            }
        }

    }
}

