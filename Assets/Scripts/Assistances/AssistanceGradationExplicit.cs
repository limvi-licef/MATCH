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
using NPBehave;
using System;
using System.Reflection;
using Microsoft.MixedReality.Toolkit.Input;

namespace MATCH
{
    namespace Assistances
    {
        public class AssistanceGradationExplicit : MATCH.Scenarios.BehaviorTrees.BehaviorTree
        {
            //private Inferences.ObjectFocused InfFocusedOnAssistance;
            //private Inferences.Timer InfTimer2Minutes;
            private Inferences.Manager InfManager;
            //private Inferences.ObjectLostFocused InfFocusLost;
            //private Inferences.DistanceComing InfIsClose;
            //private Inferences.Timer InfTimer30Seconds;
            //private Inferences.DistanceLeaving InfIsFar;

            /*public enum AssistanceStatus
            {
                Stop = 0,
                Run = 1,
                Pause = 2
            }*/

            private Assistances.StateMachine AssistancesGradation;
            //int CurrentAssistanceIndex; // Index in the list AssistancesGradation

            //AssistanceStatus Status;

            Utilities.EventHandlerArgs.ButtonAndAssistanceGradationAttention ArgsOnHelpButtonClicked;

            // Conditions
            string ConditionStartAssistance             = "StartAssistance";
            string ConditionIsDisplayed                 = "IsDisplayed";
            string ConditionsIsFocused                  = "IsFocused";
            string ConditionHelpClicked                 = "HelpClicked";
            string ConditionDisplayedSince2Minutes      = "DisplayedSince2Minutes";
            string ConditionIsFar                       = "IsFar";
            string ConditionWaitingSince30Seconds       = "WaitingSince30Seconds";
            string ConditionMinimalConditionDisplayed   = "MinimalConditionDisplayed";

            // Inferences
            string InferenceFocused                     = "AssistanceGradationExplicit-Focused";
            string InferenceTimer2Minutes = "AssistanceGradationExplicit-Timer-2-Minutes";
            string InferenceTimerStart = "AssistanceGradationExplicit-Timer-Start";
            string InferenceTimer30Seconds = "AssistanceGradationExplicit-Timer-30-seconds";
            string InferenceComingClose = "AssistanceGradationExplicit-ComingClose";


            float DistanceFromObject = 1.5f;

            public static float FarFromAssistanceWhenLookingAtIt = 4.0f;
            public static int DelayBeforeShowingHelp = 3;

            public override void Awake()
            {
                base.Awake();
                SetId("Assistance gradation explicit");

                InfManager = Inferences.Factory.Instance.CreateManager(transform);

                AssistancesGradation = null;
                //InfTimer2Minutes = new Inferences.Timer("AssistanceGradationExplicitTimer", 10, CallbackInterenceTimer);

                //ArgsOnHelpButtonClicked = null;
            }

            public override void Start()
            {
                
                
                base.Start();

                //InitializeBehaviorTree();

                // Debug buttons to check if the BT has been correctly modeled
                /*AdminMenu.Instance.AddButton("BT - GradationExplicit- Trigger - Start assistance", delegate
                {
                    UpdateConditionWithMatrix(ConditionStartAssistance);
                }, AdminMenu.Panels.Right);
                AdminMenu.Instance.AddButton("BT - GradationExplicit- Trigger - Stop assistance", delegate
                {
                    UpdateConditionWithMatrix(ConditionStartAssistance);
                    UpdateCondition(ConditionStartAssistance, false);
                }, AdminMenu.Panels.Right);
                AdminMenu.Instance.AddButton("BT - GradationExplicit- Trigger - Is Displayed", delegate
                {
                    UpdateConditionWithMatrix(ConditionIsDisplayed);
                }, AdminMenu.Panels.Right);

                AdminMenu.Instance.AddButton("BT - GradationExplicit- Trigger - Is focused", delegate
                {
                    UpdateConditionWithMatrix(ConditionsIsFocused);
                }, AdminMenu.Panels.Right);

                AdminMenu.Instance.AddButton("BT - GradationExplicit- Trigger - Displayed since 2 minutes", delegate
                {
                    UpdateConditionWithMatrix(ConditionDisplayedSince2Minutes);
                }, AdminMenu.Panels.Right);

                AdminMenu.Instance.AddButton("BT - GradationExplicit- Trigger - Is far", delegate
                {
                    UpdateConditionWithMatrix(ConditionIsFar);
                }, AdminMenu.Panels.Right);

                AdminMenu.Instance.AddButton("BT - GradationExplicit- Trigger - Help clicked", delegate
                {
                    UpdateConditionWithMatrix(ConditionHelpClicked);
                }, AdminMenu.Panels.Right);

                AdminMenu.Instance.AddButton("BT - GradationExplicit- Trigger - Waiting since 30s", delegate
                {
                    UpdateConditionWithMatrix(ConditionWaitingSince30Seconds);
                }, AdminMenu.Panels.Right);*/

                /*AdminMenu.Instance.AddInputWithButton(DistanceFromObject.ToString(), "Distance from object for hand", delegate (System.Object o, EventArgs e)
                {
                    Utilities.EventHandlerArgs.String arg = (Utilities.EventHandlerArgs.String)e;
                    DistanceFromObject = float.Parse(arg.m_text);
                }, AdminMenu.Panels.Right);*/

                // To check if the description of the assistance windows is correctly updated
                //AssistancesDebugWindow.SetDescription("Test for gradation explicit");
            }

            protected override Root InitializeBehaviorTree()
            {
                AddCondition(ConditionStartAssistance, false);
                AddCondition(ConditionIsDisplayed, false);
                AddCondition(ConditionsIsFocused, false);
                AddCondition(ConditionHelpClicked, false);
                AddCondition(ConditionDisplayedSince2Minutes, false);
                AddCondition(ConditionIsFar, false);
                AddCondition(ConditionWaitingSince30Seconds, false);
                AddCondition(ConditionMinimalConditionDisplayed, true);


                // Set the matrix
                //int nbConditions = Conditions.Keys.Count;
                int nbConditions = GetNumberOfConditions();

                AddConditionsUpdate(ConditionStartAssistance, new bool[] { true, false, false, false, false, false, false, false });
                AddConditionsUpdate(ConditionIsDisplayed, new bool[] { true, true, false, false, false, false, false, false });
                AddConditionsUpdate(ConditionsIsFocused, new bool[] { true, true, true, false, false, false, false, false });
                AddConditionsUpdate(ConditionHelpClicked, new bool[] { true, true, true, true, false, false, false, false });
                AddConditionsUpdate(ConditionDisplayedSince2Minutes, new bool[] { true, true, false, false, true, false, false, false });
                AddConditionsUpdate(ConditionIsFar, new bool[] { true, true, true, false, false, true, false, false });
                AddConditionsUpdate(ConditionWaitingSince30Seconds, new bool[] { true, true, true, false, false, false, true, false });
                AddConditionsUpdate(ConditionMinimalConditionDisplayed, new bool[] { true, true, false, false, false, false, false, true });

                // Initialize the BT
                Selector srFocused = new Selector(
                    new Inverter(AssistanceBeta()),
                    new BlackboardCondition(ConditionIsFar, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceGamma()),
                    new BlackboardCondition(ConditionHelpClicked, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceEpsilon()),
                    new BlackboardCondition(ConditionWaitingSince30Seconds, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceZeta()),
                    new WaitUntilStopped()
                    ) ;

                // seBegin - begin
                Selector srBegin = new Selector(
                    //cIsDisplayed,
                    new BlackboardCondition(ConditionStartAssistance, Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART, AssistanceEta()),
                    new BlackboardCondition(ConditionIsDisplayed, Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART, AssistanceAlpha()),
                    new BlackboardCondition(ConditionsIsFocused, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, srFocused),
                    new BlackboardCondition(ConditionDisplayedSince2Minutes, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceDelta())
                    //seIsDisplayedSince2Minutes
                    );
                // seBegin - end

                Root tree = new Root(/*Conditions*/Getconditions(), srBegin);


                return tree;
            }

             private void ShowAssistanceMinimalGradation(GradationVisual.GradationVisual assistance, EventHandler callback)
             {
                 assistance.ShowMinimalGradation(callback);
                UpdateCondition(ConditionMinimalConditionDisplayed, true);
                 //Conditions["MinimalConditionDisplayed"] = true;
             }

            private Sequence AssistanceEta()
            {
                Sequence temp = new Sequence(
                    new NPBehave.Action(() => {
                        InfManager.UnregisterAllInferences();

                        AssistancesGradation.AssistanceCurrent.HideCurrentGradation(delegate(System.Object o, EventArgs e)
                        {
                            AssistancesGradation.BackToRoot();
                        });

                        //AssistancesDebugWindow.SetDescription("Eta");
                        //base.UpdateTextAssistancesDebugWindow("BTGradation - Eta");
                        MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "BTGradation - Eta");
                    }),
                    new WaitUntilStopped());

                return temp;
            }

            private Sequence AssistanceAlpha()
            {
                Sequence temp = new Sequence(
                    new NPBehave.Action(() => {
                        ShowAssistanceMinimalGradation(AssistancesGradation.AssistanceCurrent, delegate(System.Object o, EventArgs e)
                        {
                        List<Buttons.Button.ButtonType> buttons = AssistancesGradation.GetButtonTypesCurrentAssistance();
                        if (buttons != null && buttons.Count == 1 && buttons[0] == Buttons.Button.ButtonType.ClosingButton)
                        {
                                Inferences.Timer temp = new Inferences.Timer("GradationExplicit-TimerToMakeClosingButtonAppear", 2, delegate (System.Object oo, EventArgs ee)
                                {
                                    UpdateConditionWithMatrix(ConditionWaitingSince30Seconds);
                                    ((Inferences.Timer)InfManager.GetInference("GradationExplicit-TimerToMakeClosingButtonAppear")).StopCounter();
                                    InfManager.UnregisterInference("GradationExplicit-TimerToMakeClosingButtonAppear");
                                });

                                InfManager.RegisterInference(temp);
                                temp.StartCounter();
                        }
                        else
                        {
                            Inferences.ObjectFocused inf = new Inferences.ObjectFocused(InferenceFocused, delegate (System.Object o, EventArgs e)
                            {
                                UpdateConditionWithMatrix(ConditionsIsFocused);
                                InfManager.UnregisterInference(InferenceFocused);
                                InfManager.UnregisterInference(InferenceTimer2Minutes);
                            }, AssistancesGradation.AssistanceCurrent.GetCurrentAssistance().GetTransform().gameObject, 1);
                            InfManager.RegisterInference(inf);

                            Inferences.Timer timer = new Inferences.Timer(InferenceTimer2Minutes, 15, delegate (System.Object o, EventArgs e)
                            { // Currently, manages only one level of visual gradation - to manage more than one, an extra level in the behavior tree might be required
                                UpdateConditionWithMatrix(ConditionDisplayedSince2Minutes);
                                InfManager.UnregisterInference(InferenceTimer2Minutes);
                            });
                            InfManager.RegisterInference(timer);
                            timer.StartCounter();

                                //UpdateTextAssistancesDebugWindow("BTGradation - Alpha");
                                MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "BTGradation - Alpha");
                            }
                        });

                        
                        
                    }),
                    new WaitUntilStopped());

                return temp;

            }

            private Sequence AssistanceBeta()
            {
                Sequence temp = new Sequence(
                    new NPBehave.Action(() => {
                        AssistancesGradation.AssistanceCurrent.ShowMinimalGradation(Utilities.Utility.GetEventHandlerEmpty());

                        Vector3 worldPositionAssistanceBeta = AssistancesGradation.AssistanceCurrent.GetCurrentAssistance().transform.position;

                        if (Utilities.Utility.CalculateDistancePoints(Camera.main.transform.position, worldPositionAssistanceBeta) > FarFromAssistanceWhenLookingAtIt)
                        {
                            UpdateConditionWithMatrix(ConditionIsFar);

                            Inferences.DistanceComing distanceComing = new Inferences.DistanceComing(InferenceComingClose, delegate (System.Object o, EventArgs e)
                            {
                                Inferences.Timer timer = new Inferences.Timer(InferenceTimer30Seconds, 10, delegate (System.Object o, EventArgs e)
                                {
                                    UpdateConditionWithMatrix(ConditionWaitingSince30Seconds);
                                    InfManager.UnregisterInference(InferenceTimer30Seconds);
                                });
                                InfManager.RegisterInference(timer);
                                timer.StartCounter();
                                InfManager.UnregisterInference(InferenceComingClose);
                                AssistancesGradation.AssistanceCurrent.GetCurrentAssistance().Emphasize(false);
                            }, AssistancesGradation.AssistanceCurrent.GetCurrentAssistance().gameObject, DistanceFromObject);

                            InfManager.RegisterInference(distanceComing);

                        }
                        else
                        {
                            Inferences.Timer timer = new Inferences.Timer(InferenceTimer30Seconds, /*10*/ DelayBeforeShowingHelp, delegate (System.Object o, EventArgs e)
                            {
                                UpdateConditionWithMatrix(ConditionWaitingSince30Seconds);
                                InfManager.UnregisterInference(InferenceTimer30Seconds);
                            });
                            InfManager.RegisterInference(timer);
                            timer.StartCounter();
                        }

                        

                        //UpdateTextAssistancesDebugWindow("BTGradation - Beta");
                        MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "BTGradation - Beta");
                    })
                    );

                return temp;
            }

            private Sequence AssistanceDelta()
            {
                Sequence temp = new Sequence(
                    new NPBehave.Action(() => {
                        AssistancesGradation.AssistanceCurrent.ShowNextGradation(Utilities.Utility.GetEventHandlerEmpty());
                        //UpdateTextAssistancesDebugWindow("BTGradation - Delta");
                        MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "BTGradation - Delta");

                        if (AssistancesGradation.AssistanceCurrent.IsLastGradationLevel() == false)
                        {
                            UpdateCondition(ConditionDisplayedSince2Minutes, false);
                            Inferences.Timer timer = new Inferences.Timer(InferenceTimer2Minutes, 15, delegate (System.Object o, EventArgs e)
                            { 
                                UpdateConditionWithMatrix(ConditionDisplayedSince2Minutes);

                                InfManager.UnregisterInference(InferenceTimer2Minutes);
                            });
                            InfManager.RegisterInference(timer);
                            timer.StartCounter();
                        }
                        }),
                    new WaitUntilStopped()
                    );

                return temp;
            }

            private Sequence AssistanceGamma()
            {
                Sequence temp = new Sequence(
                    new NPBehave.Action(() => {
                        //UpdateTextAssistancesDebugWindow("BTGradation - Gamma");
                        MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "BTGradation - Gamma");
                        AssistancesGradation.AssistanceCurrent.GetCurrentAssistance().Emphasize(true);
                    }),
                    new WaitUntilStopped()
                    );

                return temp;
            }

            private Sequence AssistanceEpsilon()
            {


                Sequence temp = new Sequence(
                    new NPBehave.Action(() =>
                    {
                        //AssistancesGradation.AssistanceCurrent.sho
                        if (ArgsOnHelpButtonClicked != null)
                        {
                            // Hiding current assistance
                            AssistancesGradation.AssistanceCurrent.HideCurrentGradation(Utilities.Utility.GetEventHandlerEmpty());

                            if (ArgsOnHelpButtonClicked.AssistanceNext != null)
                            {
                                // Preparing to show the next assistance
                                AssistancesGradation.AssistanceCurrent = ArgsOnHelpButtonClicked.AssistanceNext;
                                ArgsOnHelpButtonClicked = null; // One shot: the button is not support to be clicked again

                                //AssistancesGradation..AssistanceCurrent.
                                UpdateConditionWithMatrix(ConditionStartAssistance);
                            }
                        }

                        //UpdateTextAssistancesDebugWindow("BTGradation - Epsilon");
                        MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "BTGradation - Epsilon");
                    }),
                    new WaitUntilStopped()
                    );

                return temp;
            }

            private Sequence AssistanceZeta()
            {
                Sequence temp = new Sequence(
                    new NPBehave.Action(() =>
                    {
                        AssistancesGradation.AssistanceCurrent.ShowHelpCurrentGradation(true, Utilities.Utility.GetEventHandlerEmpty());
                        //UpdateTextAssistancesDebugWindow("BTGradation - Zeta");
                        MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "BTGradation - Zeta");
                    }),
                    new WaitUntilStopped()
                    );

                return temp;
            }

            private void COnHelpButtonClicked(System.Object o, EventArgs e)
            {
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Button clicked");

                ArgsOnHelpButtonClicked = (Utilities.EventHandlerArgs.ButtonAndAssistanceGradationAttention)e;


                InfManager.UnregisterAllInferences();
                /*if (ArgsOnHelpButtonClicked.AssistanceNext != null)
                {*/
                //Conditions["HelpClicked"] = true;
                UpdateCondition(ConditionHelpClicked, true);    
                //}
            }

            /**
             * The order you add the assistances is important: it determines their order of appearance
             * */
            public void AddAssistance(GradationVisual.GradationVisual assistance, Buttons.Button.ButtonType type, GradationVisual.GradationVisual assistanceTarget)
            {
                if (AssistancesGradation == null)
                {
                    AssistancesGradation = new StateMachine(assistance);
                    AssistancesGradation.EventOnButtonClicked += COnHelpButtonClicked;
                }

                AssistancesGradation.AddAssistance(assistance, type, ref assistanceTarget);
            }
            
            public void RunAssistance()
            {
                StartTree();
                UpdateConditionWithMatrix(ConditionStartAssistance);
            }

            public void StopAssistance()
            {
                SetConditionsTo(false);
            }
        }
    }
}
