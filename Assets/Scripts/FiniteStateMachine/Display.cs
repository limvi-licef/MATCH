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
    namespace FiniteStateMachine
    {
        public class Display : MonoBehaviour
        {
            Manager Manager;
            MouseUtilitiesGradationAssistanceAbstract InitialState;
            public GameObject RefLabel;

            Transform ParentLabelsView;

            Transform RefConnectorView;

            Dictionary<string, GameObject> States;
            Dictionary<(string, string), GameObject> Connectors;

            MouseUtilitiesGradationAssistanceAbstract CurrentHighlightedState;

            private void Awake()
            {
                States = new Dictionary<string, GameObject>();
                Connectors = new Dictionary<(string, string), GameObject>();
                ParentLabelsView = gameObject.transform.Find("ButtonParent");

                RefConnectorView = gameObject.transform.Find("Line");

                CurrentHighlightedState = null;


            }

            // Start is called before the first frame update
            void Start()
            {
                AdminMenu.Instance.AddButton("Bring graph window", CallbackBringWindow);
            }

            // Update is called once per frame
            void Update()
            {

            }

            public void CallbackBringWindow()
            {
                gameObject.transform.position = new Vector3(Camera.main.transform.position.x + 0.5f, Camera.main.transform.position.y, Camera.main.transform.position.z);
                gameObject.transform.LookAt(Camera.main.transform);
                gameObject.transform.Rotate(new Vector3(0, 1, 0), 180);
            }


            public void SetManager(Manager manager)
            {
                Manager = manager;

                InitialState = Manager.getInitialAssistance();

                DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Original state: " + InitialState.getId());
                DisplayStates(InitialState, 0);
                //displayStatesV2();

                DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Table surface should be touchable again");

                // Subscribing to the signal
                Manager.s_newStateSelected += CallbackNewStateSelected;
            }

            void DisplayStatesV2()
            {
                foreach (MouseUtilitiesGradationAssistanceAbstract state in Manager.getListOfStates())
                {
                    AddState(state);
                }

                foreach (MouseUtilitiesGradationAssistanceAbstract currentState in Manager.getListOfStates())
                {
                    foreach (MouseUtilitiesGradationAssistanceAbstract nextState in currentState.getNextStates().Values.ToList())
                    {
                        AddConnector(currentState, nextState);
                    }
                }
            }


            void DisplayStates(MouseUtilitiesGradationAssistanceAbstract currentstate, int nbLevels)
            {
                //m_manager.get

                AddState(currentstate);

                foreach (KeyValuePair<string, MouseUtilitiesGradationAssistanceAbstract> nextState in currentstate.getNextStates())
                {
                    /*if(nextState.Key != m_initialState.getId())
                    {
                        displayStates(nextState.Value, nbLevels+1);
                    }

                    addConnector(currentstate, nextState.Value)
                     */

                    AddState(nextState.Value);

                    if (AddConnector(currentstate, nextState.Value))
                    {
                        DisplayStates(nextState.Value, nbLevels + 1);
                    }
                }
            }

            void AddState(MouseUtilitiesGradationAssistanceAbstract state)
            {
                if (States.ContainsKey(state.getId()) == false)
                {
                    GameObject temp = Instantiate(RefLabel, ParentLabelsView);
                    temp.transform.GetComponent<ButtonConfigHelper>().IconStyle = ButtonIconStyle.None;

                    temp.transform.Find("IconAndText").Find("TextMeshPro").GetComponent<TextMeshPro>().SetText(state.getId());

                    ParentLabelsView.GetComponent<GridObjectCollection>().UpdateCollection();

                    States.Add(state.getId(), temp);
                }
            }

            /**
             * Return true if the connector has been added, false otherwise
             **/
            bool AddConnector(MouseUtilitiesGradationAssistanceAbstract stateStart, MouseUtilitiesGradationAssistanceAbstract stateEnd)
            {
                bool toReturn = true;

                if (Connectors.ContainsKey((stateStart.getId(), stateEnd.getId())) == false)
                {
                    Transform temp = Instantiate(RefConnectorView, gameObject.transform);

                    Utilities.LineBetweenTwoPoints line = temp.GetComponent<Utilities.LineBetweenTwoPoints>();

                    RectTransform transformGrid = ParentLabelsView.GetComponent<RectTransform>();

                    line.DrawLineWithArrow(States[stateStart.getId()], States[stateEnd.getId()]);
                    Connectors.Add((stateStart.getId(), stateEnd.getId()), temp.gameObject);
                }
                else
                {
                    toReturn = false;
                }

                return toReturn;
            }

            public void CallbackNewStateSelected(System.Object o, EventArgs args)
            {
                MouseUtilisiesGradationAssistanceArgCurrentState currentState = (MouseUtilisiesGradationAssistanceArgCurrentState)args;

                if (CurrentHighlightedState != null)
                {
                    States[CurrentHighlightedState.getId()].transform.Find("BackPlate").Find("Quad").GetComponent<Renderer>().material = Resources.Load(Utilities.Materials.Textures.HolographicBackPlate, typeof(Material)) as Material;
                }

                States[currentState.m_currentState.getId()].transform.Find("BackPlate").Find("Quad").GetComponent<Renderer>().material = Resources.Load(Utilities.Materials.Colors.CyanGlowing, typeof(Material)) as Material;

                // Brut force to highlight the connectors
                foreach (KeyValuePair<(string, string), GameObject> connector in Connectors)
                {
                    if (connector.Key.Item1 == currentState.m_currentState.getId())
                    {
                        connector.Value.GetComponent<Utilities.LineBetweenTwoPoints>().HighlightConnector(true);
                    }
                    else
                    {
                        connector.Value.GetComponent<Utilities.LineBetweenTwoPoints>().HighlightConnector(false);
                    }
                }

                CurrentHighlightedState = currentState.m_currentState;
            }
        }

    }
}
