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

namespace MATCH
{
    namespace Scenarios
    {
        namespace FiniteStateMachine
        {
            public class TakeOutGarbage : Scenario
            {
                public MATCH.Inferences.Manager m_inferenceManager;
                public MATCH.FiniteStateMachine.Display m_graph;

                MATCH.FiniteStateMachine.Manager m_gradationManager;


                MATCH.Inferences.Time m_inference19h00;
                MATCH.Inferences.Time m_inference19h30;

                EventHandler s_inference19h00;
                EventHandler s_inference19h30;

                public GameObject m_refInteractionSurface;
                public GameObject m_refCube;
                public GameObject m_refDialog;

                private void Awake()
                {
                    // Initialize variables
                    m_gradationManager = new MATCH.FiniteStateMachine.Manager();
                    SetId("Sortir les poubelles");
                }

                // Start is called before the first frame update
                void Start()
                {
                    DateTime tempTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 19, 0, 0);
                    m_inference19h00 = new MATCH.Inferences.Time("time19h00", tempTime, CallbackTime19h00);

                    tempTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 19, 30, 0);
                    m_inference19h30 = new MATCH.Inferences.Time("time19h30", tempTime, CallbackTime19h30);

                    InitializeScenario();
                }

                // Update is called once per frame
                /*void Update()
                {

                }*/

                void InitializeScenario()
                {

                    Manager.Instance.addScenario(this);


                    // Instanciate assistances
                    GameObject garbageInteractionSurfaceView = Instantiate(m_refInteractionSurface, gameObject.transform);
                    Assistances.InteractionSurface garbageInteractionSurfaceController = garbageInteractionSurfaceView.GetComponent<Assistances.InteractionSurface>();
                    garbageInteractionSurfaceController.SetAdminButtons("garbage");
                    garbageInteractionSurfaceController.SetColor(Utilities.Materials.Colors.PurpleGlowing);
                    garbageInteractionSurfaceController.ShowInteractionSurfaceTable(true);
                    garbageInteractionSurfaceView.transform.localPosition = new Vector3(-0.92f, 0.383f, 3.881f);
                    garbageInteractionSurfaceController.SetObjectResizable(true);


                    GameObject doorInteractionSurfaceView = Instantiate(m_refInteractionSurface, gameObject.transform);
                    Assistances.InteractionSurface doorInteractionSurfaceController = doorInteractionSurfaceView.GetComponent<Assistances.InteractionSurface>();
                    doorInteractionSurfaceController.SetAdminButtons("door");
                    doorInteractionSurfaceController.SetColor(Utilities.Materials.Colors.GreenGlowing);
                    doorInteractionSurfaceController.ShowInteractionSurfaceTable(true);
                    doorInteractionSurfaceView.transform.localPosition = new Vector3(-1.032f, 0.544f, 2.617f);
                    doorInteractionSurfaceController.SetObjectResizable(true);

                    GameObject exclamationMarkView = Instantiate(m_refCube, garbageInteractionSurfaceView.transform);
                    Assistances.Basic exclamationMarkController = exclamationMarkView.GetComponent<Assistances.Basic>();
                    exclamationMarkController.SetMaterial(Utilities.Materials.Textures.Exclamation);

                    GameObject solutionView = Instantiate(m_refDialog, garbageInteractionSurfaceView.transform);
                    Assistances.Dialog solutionController = solutionView.GetComponent<Assistances.Dialog>();
                    solutionController.SetDescription("Il est l'heure de sortir les poubelles!", 0.3f);
                    solutionController.EnableBillboard(true);

                    GameObject highlightGarbageView = Instantiate(m_refCube, garbageInteractionSurfaceView.transform);
                    Assistances.Basic highlightGarbageController = highlightGarbageView.GetComponent<Assistances.Basic>();
                    //highlightGarbageController.SetAdjustHeightOnShow(false);
                    highlightGarbageController.AdjustHeightOnShow = false;
                    highlightGarbageController.SetMaterial(Utilities.Materials.Colors.CyanGlowing);
                    highlightGarbageController.SetScale(0.2f, 0.6f, 0.2f);
                    highlightGarbageController.SetLocalPosition(0, -0.35f, 0);
                    highlightGarbageController.SetBillboard(false);
                    garbageInteractionSurfaceController.EventInteractionSurfaceScaled += delegate
                    {
                        highlightGarbageController.SetScale(garbageInteractionSurfaceController.GetInteractionSurface().localScale.x,
                            highlightGarbageController.GetChildTransform().localScale.y,
                            garbageInteractionSurfaceController.GetInteractionSurface().localScale.z);
                    };

                    GameObject highlightGarbageVividView = Instantiate(m_refCube, garbageInteractionSurfaceView.transform);
                    Assistances.Basic highlightGarbageVividController = highlightGarbageVividView.GetComponent<Assistances.Basic>();
                    //highlightGarbageVividController.SetAdjustHeightOnShow(false);
                    highlightGarbageVividController.AdjustHeightOnShow = false;
                    highlightGarbageVividController.SetMaterial(Utilities.Materials.Colors.OrangeGlowing);
                    highlightGarbageVividController.SetScale(0.2f, 0.6f, 0.2f);
                    highlightGarbageVividController.SetLocalPosition(0, -0.35f, 0);
                    highlightGarbageVividController.SetBillboard(false);
                    garbageInteractionSurfaceController.EventInteractionSurfaceScaled += delegate
                    {
                        highlightGarbageVividController.SetScale(garbageInteractionSurfaceController.GetInteractionSurface().localScale.x,
                            highlightGarbageVividController.GetChildTransform().localScale.y,
                            garbageInteractionSurfaceController.GetInteractionSurface().localScale.z);
                    };

                    GameObject successView = Instantiate(m_refCube, garbageInteractionSurfaceView.transform);
                    Assistances.Basic successController = successView.GetComponent<Assistances.Basic>();
                    successController.SetMaterial(Utilities.Materials.Textures.Congratulation);

                    // Add inferences
                    m_inferenceManager.RegisterInference(m_inference19h00);

                    // Set states
                    MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistance sStandBy = m_gradationManager.addNewAssistanceGradation("StandBy");
                    sStandBy.addFunctionShow(delegate (EventHandler e, bool animate)
                    {
                        m_inferenceManager.RegisterInference(m_inference19h00);
                        OnChallengeStandBy();
                    }, Utilities.Utility.GetEventHandlerEmpty(), true);
                    sStandBy.setFunctionHide(delegate (EventHandler e, bool animate)
                    {
                        e?.Invoke(this, EventArgs.Empty);
                    }, Utilities.Utility.GetEventHandlerEmpty(), true);
                    MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistance sHighlightGarbage = m_gradationManager.addNewAssistanceGradation("HighlightGarbage");
                    sHighlightGarbage.setFunctionHideAndShow(highlightGarbageController);
                    sHighlightGarbage.addFunctionShow(delegate (EventHandler e, bool animate)
                    {
                        m_inferenceManager.RegisterInference(m_inference19h30);
                        OnChallengeStart();
                    }, Utilities.Utility.GetEventHandlerEmpty(), true);
                    MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistance sExclamationMark = m_gradationManager.addNewAssistanceGradation("ExclamationMark");
                    sExclamationMark.addFunctionShow(exclamationMarkController, true);
                    sExclamationMark.addFunctionShow(highlightGarbageVividController, true);
                    sExclamationMark.setFunctionHide(delegate (EventHandler e, bool animate)
                    {
                        exclamationMarkController.Hide(e, animate);
                        highlightGarbageVividController.Hide(Utilities.Utility.GetEventHandlerEmpty(), animate);
                    }, Utilities.Utility.GetEventHandlerEmpty(), true);
                    //sExclamationMark.setFunctionHideAndShow(exclamationMarkController);
                    MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistance sSolution = m_gradationManager.addNewAssistanceGradation("Solution");
                    sSolution.setFunctionHideAndShow(solutionController);
                    MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistance sGarbageGrabbed = m_gradationManager.addNewAssistanceGradation("Garbage grabbed");
                    sGarbageGrabbed.addFunctionShow(delegate (EventHandler e, bool animate)
                    {

                    }, Utilities.Utility.GetEventHandlerEmpty(), true);
                    sGarbageGrabbed.setFunctionHide(delegate (EventHandler e, bool animate)
                    {
                        e?.Invoke(this, EventArgs.Empty);
                    }, Utilities.Utility.GetEventHandlerEmpty(), true);
                    MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistance sSuccess = m_gradationManager.addNewAssistanceGradation("Success");
                    sSuccess.setFunctionHideAndShow(successController);
                    sSuccess.addFunctionShow(delegate (EventHandler e, bool animate)
                    {
                        OnChallengeSuccess();
                    }, Utilities.Utility.GetEventHandlerEmpty(), true);

                    // Connections between states
                    s_inference19h00 += sStandBy.goToState(sHighlightGarbage);
                    garbageInteractionSurfaceController.EventInteractionSurfaceTableTouched += sStandBy.goToState(/*sSuccess*/sGarbageGrabbed);
                    highlightGarbageController.s_touched += sHighlightGarbage.goToState(sSolution);
                    s_inference19h30 += sHighlightGarbage.goToState(sExclamationMark);
                    garbageInteractionSurfaceController.EventInteractionSurfaceTableTouched += sHighlightGarbage.goToState(/*sSuccess*/sGarbageGrabbed);
                    exclamationMarkController.s_touched += sExclamationMark.goToState(sSolution);
                    highlightGarbageVividController.s_touched += delegate (System.Object o, EventArgs e) { exclamationMarkController.TriggerTouch(); };

                    garbageInteractionSurfaceController.EventInteractionSurfaceTableTouched += sExclamationMark.goToState(/*sSuccess*/sGarbageGrabbed);
                    garbageInteractionSurfaceController.EventInteractionSurfaceTableTouched += sSolution.goToState(/*sSuccess*/sGarbageGrabbed);
                    doorInteractionSurfaceController.EventInteractionSurfaceTableTouched += sGarbageGrabbed.goToState(sSuccess);
                    successController.s_touched += sSuccess.goToState(sStandBy);

                    m_gradationManager.setGradationInitial("StandBy");



                    // Display graph
                    m_graph.SetManager(m_gradationManager);
                }

                void CallbackTime19h00(System.Object o, EventArgs e)
                {
                    m_inferenceManager.UnregisterInference(m_inference19h00);

                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Time 19h00 triggered");

                    s_inference19h00?.Invoke(this, EventArgs.Empty);
                }

                void CallbackTime19h30(System.Object o, EventArgs e)
                {
                    m_inferenceManager.UnregisterInference(m_inference19h30);

                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Time 19h30 triggered");

                    s_inference19h30?.Invoke(this, EventArgs.Empty);
                }

                public MATCH.Inferences.Time GetInference19h()
                {
                    return m_inference19h00;
                }

                public MATCH.Inferences.Time GetInference19h30()
                {
                    return m_inference19h30;
                }
            }

        }
    }
}