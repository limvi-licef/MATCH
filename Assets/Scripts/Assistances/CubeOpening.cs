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
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

/**
 * Manages a cube where when touched, the top part splits in 2. Might be useful if you want something to "appear" from the cube.
 * */
namespace MATCH
{
    namespace Assistances
    {
        public class CubeOpening : MonoBehaviour
        {
            Transform m_cubeTopLeftPartView;
            Transform m_cubeTopRightPartView;
            Transform m_cubeBottomPartView;

            bool m_mutexClosingOngoing;

            public event EventHandler m_eventCubetouched;

            Vector3 m_scalingOriginal;
            Vector3 m_scalingReduced;

            bool m_animateCubeOnTouched;

            private void Awake()
            {
                // Children
                m_cubeTopRightPartView = gameObject.transform.Find("TopRightPart");
                m_cubeTopLeftPartView = gameObject.transform.Find("TopLeftPart");
                m_cubeBottomPartView = gameObject.transform.Find("BottomPart");

                m_animateCubeOnTouched = false;
            }

            // Start is called before the first frame update
            void Start()
            {


                m_mutexClosingOngoing = false;

                // Add callbacks
                gameObject.GetComponent<Interactable>().GetReceiver<InteractableOnTouchReceiver>().OnTouchStart.AddListener(callbackCubeTouched);

                // Values will most likely br wrong, because will be intialized whereas the cube won't be displayed. So will be updated in one of the gradation state.
                backupScaling();
            }

            // Update is called once per frame
            void Update()
            {

            }

            void callbackCubeTouched()
            {
                MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Cube touched");

                if (m_animateCubeOnTouched)
                {
                    openCube(m_eventCubetouched);
                }
                else
                { // Otherwise behaves like a normal cube and thus amit the event directly
                    m_eventCubetouched?.Invoke(this, EventArgs.Empty);
                }

            }

            public void updateMaterials(string materialNameBottom, string materialNameTopLeft, string materialNameTopRight)
            {
                m_cubeBottomPartView.GetComponent<Renderer>().material = Resources.Load(materialNameBottom, typeof(Material)) as Material;
                m_cubeTopLeftPartView.GetComponent<Renderer>().material = Resources.Load(materialNameTopRight, typeof(Material)) as Material; // What left when preparing the meterial is actually right here
                m_cubeTopRightPartView.GetComponent<Renderer>().material = Resources.Load(materialNameTopLeft, typeof(Material)) as Material; // What left when preparing the meterial is actually left here
            }

            public void openCube(EventHandler callback)
            {
                // Moving the parts
                Vector3 worldDestPosLeftPart = gameObject.transform.TransformPoint(new Vector3(0.75f, 0.5f, 0f));
                Vector3 worldDestPosRightPart = gameObject.transform.TransformPoint(new Vector3(-0.75f, 0.5f, 0f));
                MATCH.Utilities.Animation animatorLeftPart = m_cubeTopLeftPartView.gameObject.AddComponent<MATCH.Utilities.Animation>();
                animatorLeftPart.AnimationSpeed = 0.5f;
                MATCH.Utilities.Animation animatorRightPart = m_cubeTopRightPartView.gameObject.AddComponent<MATCH.Utilities.Animation>();
                animatorRightPart.AnimationSpeed = 0.5f;


                animatorLeftPart.AnimateMoveToPosition(worldDestPosLeftPart, MATCH.Utilities.Utility.GetEventHandlerEmpty());
                animatorRightPart.AnimateMoveToPosition(worldDestPosRightPart, new EventHandler(delegate (System.Object o, EventArgs e) { callback?.Invoke(this, EventArgs.Empty); }));
            }

            public void closeCube(EventHandler callback)
            {
                if (m_mutexClosingOngoing)
                { // Do not accept a new request if the process of the current one is not yet finished
                    MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Warning, "A request to close the cube is currently being processed. This one is ignored.");
                }
                else
                {
                    m_mutexClosingOngoing = true; // Locking the mutex

                    MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Closing cube ...");

                    // Moving the parts
                    Vector3 worldDestPosLeftPart = gameObject.transform.TransformPoint(new Vector3(0.25f, 0.5f, 0f));
                    Vector3 worldDestPosRightPart = gameObject.transform.TransformPoint(new Vector3(-0.25f, 0.5f, 0f));
                    MATCH.Utilities.Animation animatorLeftPart = m_cubeTopLeftPartView.gameObject.AddComponent<MATCH.Utilities.Animation>();
                    animatorLeftPart.AnimationSpeed = 0.5f;
                    MATCH.Utilities.Animation animatorRightPart = m_cubeTopRightPartView.gameObject.AddComponent<MATCH.Utilities.Animation>();
                    animatorRightPart.AnimationSpeed = 0.5f;


                    animatorLeftPart.AnimateMoveToPosition(worldDestPosLeftPart, MATCH.Utilities.Utility.GetEventHandlerEmpty());
                    animatorRightPart.AnimateMoveToPosition(worldDestPosRightPart, new EventHandler(delegate (System.Object o, EventArgs e) {
                        MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Cube closed ");
                        callback?.Invoke(this, EventArgs.Empty);
                        m_mutexClosingOngoing = false; // Process finished: unlocking the mutex
                    }));
                }
            }

            public void setScalingToOriginal()
            {
                transform.localScale = m_scalingOriginal;

                MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Set scaling to original: " + transform.localScale.ToString());
            }

            public void backupScaling()
            {
                m_scalingOriginal = gameObject.transform.localScale;
                m_scalingReduced = m_scalingOriginal / 3.0f;
            }

            public void setScalingReduced()
            {
                transform.localScale = m_scalingReduced;

                MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Reducing scale to: " + m_scalingReduced.ToString());
            }

            public void animateCubeOnTouch(bool animate)
            {
                m_animateCubeOnTouched = animate;
            }
        }

    }
}
