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

/**
 * Manages a basic state machine: you provide a list of states and when calling the increase of decrease function, will do so. Does nothing more.
 * */
namespace MATCH
{
    namespace FiniteStateMachine
    {
        public class VisualGradationManager : MonoBehaviour
        {
            List<AssistanceGradation> m_assistanceGradation;

            int m_assistanceGradationIndexCurrent;

            public struct AssistanceGradation
            {
                public AssistanceGradation(string name, EventHandler e)
                {
                    id = name;
                    callback = e;
                }

                public string id;
                public EventHandler callback;
            }

            private void Awake()
            {
                m_assistanceGradationIndexCurrent = -1; // i.e. no assistance in the list.
                m_assistanceGradation = new List<AssistanceGradation>();
            }

            // Start is called before the first frame update
            void Start()
            {

            }

            // Update is called once per frame
            void Update()
            {

            }

            public void addNewAssistanceGradation(string id, EventHandler callback)
            {
                m_assistanceGradation.Add(new AssistanceGradation(id, callback));

                if (m_assistanceGradationIndexCurrent == -1)
                { // If this is the first gradation, then select it as the current one
                    m_assistanceGradationIndexCurrent = 0;
                }
            }

            /**
             * Returns true if max gradation reached, false otherwise
             * */
            public bool isGradationMax()
            {
                int nbGradations = m_assistanceGradation.Count;
                bool toReturn = false;

                if (m_assistanceGradationIndexCurrent == nbGradations - 1)
                {
                    toReturn = true;
                }
                return toReturn;
            }

            /*
             * Return true if max gradation is reached, false otherwise
             * */
            public bool increaseGradation()
            {
                int nbGradations = m_assistanceGradation.Count;
                bool toReturn = false;

                if (m_assistanceGradationIndexCurrent < nbGradations)
                {
                    m_assistanceGradationIndexCurrent++;

                    m_assistanceGradation[m_assistanceGradationIndexCurrent].callback?.Invoke(this, EventArgs.Empty);
                }

                if (m_assistanceGradationIndexCurrent == nbGradations - 1)
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Maximum gradation level reached");
                    toReturn = true;
                }
                return toReturn;
            }

            /*
             * Return true if min gradation is reached, false otherwise
             * */
            public bool decreaseGradation()
            {
                bool toReturn = false;

                if (m_assistanceGradationIndexCurrent > 0) // 0 being the first element of the list, i.e. the minimal one
                {
                    m_assistanceGradationIndexCurrent--;

                    m_assistanceGradation[m_assistanceGradationIndexCurrent].callback?.Invoke(this, EventArgs.Empty);
                }

                if (m_assistanceGradationIndexCurrent == 0)
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Minimum gradation level reached");

                    toReturn = true;
                }

                return toReturn;
            }

            public void setGradationToMinimum()
            {
                if (m_assistanceGradationIndexCurrent == 0)
                { // If gradation is already to the minimum, then nothing to do
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Gradation already at minimal level - nothing to do");
                }
                else
                {
                    m_assistanceGradationIndexCurrent = 0;
                    m_assistanceGradation[m_assistanceGradationIndexCurrent].callback?.Invoke(this, EventArgs.Empty);
                }
            }
        }

    }
}
