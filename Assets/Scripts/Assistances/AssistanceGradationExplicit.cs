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
                AdminMenu.Instance.AddButton("BT - GradationExplicit- Trigger - Start assistance", delegate
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
                }, AdminMenu.Panels.Right);

                // To check if the description of the assistance windows is correctly updated
                //AssistancesDebugWindow.SetDescription("Test for gradation explicit");
            }

            protected override Root InitializeBehaviorTree()
            {
                /*InfManager.RegisterInference(InfTimer2Minutes);
                //InitializeInference30Seconds();

                Status = AssistanceStatus.Stop;
                CurrentAssistanceIndex = -1; // Means the process did not started yet

                InfFocusedOnAssistance = null; // Use InitializeInfereneFocusedAssistance to initialize it
                InfFocusLost = null;
                InfIsClose = null;*/

                /*Conditions[ConditionStartAssistance] = false;
                Conditions[ConditionIsDisplayed] = false;
                Conditions[ConditionsIsFocused] = false;
                Conditions[ConditionHelpClicked] = false;
                Conditions[ConditionDisplayedSince2Minutes] = false;
                Conditions[ConditionIsFar] = false;
                Conditions[ConditionWaitingSince30Seconds] = false;
                Conditions[ConditionMinimalConditionDisplayed] = true;*/
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
                //ResetConditions(); // Can also be used to initialize them

                // seIsWaitingSince30sec - begin
                /*Sequence seIsWaitingSince30sec = new Sequence(
                    new NPBehave.Action(() => DisplayHelpButtons()),
                    new NPBehave.WaitUntilStopped()
                    );
                BlackboardCondition cIsWaitingSince30sec = new BlackboardCondition("WaitingSince30Seconds", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, seIsWaitingSince30sec);
                // seIsWaitingSince30sec - end

                // seIsHelpClicked - begin
                Sequence seIsHelpClicked = new Sequence(
                    new NPBehave.Action(() => HideCurrentGradation()),
                    new NPBehave.WaitUntilStopped()
                    );
                BlackboardCondition cIsHelpClicked = new BlackboardCondition("HelpClicked", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, seIsHelpClicked);
                // seIsHelpClicked - end

                // seIsFar - begin
                Sequence seIsFar = new Sequence(
                    //cIsFar,
                    new NPBehave.Action(() => Debug.Log("Here should be displayed a symbol to inform the person the system understood he looked at the assistance")),
                    new NPBehave.WaitUntilStopped()
                    );

                BlackboardCondition cIsFar = new BlackboardCondition("IsFar", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, seIsFar);
                // seIsFar - end

                // srFocused - begin
                Sequence seFocusedInternal = new Sequence(
                    new NPBehave.Action(() => ShowAssistanceMinimalGradation(AssistancesGradation.AssistanceCurrent, CAssistanceDisplayed)),
                    new NPBehave.WaitUntilStopped()
                    );

                BlackboardCondition cIsNotMininal = new BlackboardCondition("MinimalConditionDisplayed", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART, seFocusedInternal);*/

                Selector srFocused = new Selector(
                    new Inverter(AssistanceBeta()),
                    new BlackboardCondition(ConditionIsFar, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceGamma()),
                    new BlackboardCondition(ConditionHelpClicked, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceEpsilon()),
                    new BlackboardCondition(ConditionWaitingSince30Seconds, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceZeta()),
                    new WaitUntilStopped()
                    //cIsHelpClicked,
                    //cIsNotMininal,
                    //cIsWaitingSince30sec,
                    //cIFar
                    ) ;
                // srFocused - end
                /*
                // seIsFocused - begin
                //BlackboardCondition cIsFocused = new BlackboardCondition("IsFocused", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, srFocused);
                // seIsFocused - end

                // srIsDisplayed - begin
                /*Sequence seInternalNotDisplayed = new Sequence(
                    AssistanceAlpha()
                    );*/

                //BlackboardCondition cIsDisplayed = new BlackboardCondition("IsDisplayed", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART, seInternalNotDisplayed);
                // srIsDisplayed - end

                // seIsDisplayedSince2Minutes - begin
                /*BlackboardCondition cIsDisplayedSince2Minutes = new BlackboardCondition("DisplayedSince2Minutes", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, new NPBehave.Action(() => Debug.Log("Displayed since 2 minutes")));

                Sequence seIsDisplayedSince2Minutes = new Sequence(
                    cIsDisplayedSince2Minutes,
                    new NPBehave.Action(() => IncreaseAttentionGrabbingGradation()),
                    new NPBehave.WaitUntilStopped());
                // seIsDisplayedSince2Minutes - end
                */
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

            /* public void ResetConditions()
             {
                 DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Resetting conditions");

                 UpdateConditionWithMatrix(ConditionIsFar);
                 UpdateCondition(ConditionMinimalConditionDisplayed, true);
                 UpdateCondition(ConditionIsDisplayed, false);
             }*/

             private void ShowAssistanceMinimalGradation(GradationVisual.GradationVisual assistance, EventHandler callback)
             {
                 assistance.ShowMinimalGradation(callback);
                UpdateCondition(ConditionMinimalConditionDisplayed, true);
                 //Conditions["MinimalConditionDisplayed"] = true;
             }

             /*private void HideCurrentGradation()
             {
                 ArgsOnHelpButtonClicked.AssistanceCurrent.HideCurrentGradation(delegate (System.Object o, EventArgs e)
                 {
                     //The next assistance will be shown automatically when resetting the conditions
                     ResetConditions();
                 });
             }*/

            private Sequence AssistanceEta()
            {
                Sequence temp = new Sequence(
                    new NPBehave.Action(() => {
                        //AssistancesGradation.AssistanceCurrent.HideCurrentGradation(Utilities.Utility.GetEventHandlerEmpty());

                        //Inferences.Timer timer = new Inferences.Timer(InferenceTimerStart, 20, delegate (System.Object o, EventArgs e)
                        //{
                            //UpdateConditionWithMatrix(ConditionStartAssistance);
                            //InfManager.UnregisterInference(InferenceTimerStart);
                        //});
                        //InfManager.RegisterInference(timer);
                        //timer.StartCounter();*/

                        InfManager.UnregisterAllInferences();

                        AssistancesGradation.AssistanceCurrent.HideCurrentGradation(delegate(System.Object o, EventArgs e)
                        {
                            AssistancesGradation.BackToRoot();
                        });

                        AssistancesDebugWindow.SetDescription("Eta");
                        MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Eta");
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
                            UpdateConditionWithMatrix(ConditionWaitingSince30Seconds);
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

                            AssistancesDebugWindow.SetDescription("Alpha");
                                MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Alpha");
                            }
                        });

                        
                        
                    }),
                    new WaitUntilStopped());

                return temp;

                /*CurrentAssistanceIndex = 0;
                ShowAssistanceMinimalGradation(AssistancesGradation.AssistanceCurrent, CAssistanceDisplayed);
                Conditions["IsDisplayed"] = true;

                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Display first assistance for " + AssistancesGradation.AssistanceCurrent.name);

                InitializeInferenceFocusedAssistance(CurrentAssistanceIndex);

                InfTimer2Minutes.StartCounter();*/
            }

            private Sequence AssistanceBeta()
            {
                Sequence temp = new Sequence(
                    new NPBehave.Action(() => {
                        AssistancesGradation.AssistanceCurrent.ShowMinimalGradation(Utilities.Utility.GetEventHandlerEmpty());

                        Inferences.Timer timer = new Inferences.Timer(InferenceTimer30Seconds, 10, delegate (System.Object o, EventArgs e)
                        {
                            UpdateConditionWithMatrix(ConditionWaitingSince30Seconds);
                            InfManager.UnregisterInference(InferenceTimer30Seconds);
                        });
                        InfManager.RegisterInference(timer);
                        timer.StartCounter();

                        AssistancesDebugWindow.SetDescription("Beta");
                        MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Beta");
                    })
                    );

                return temp;
            }

            private Sequence AssistanceDelta()
            {
                Sequence temp = new Sequence(
                    new NPBehave.Action(() => {
                        AssistancesGradation.AssistanceCurrent.ShowNextGradation(Utilities.Utility.GetEventHandlerEmpty());
                        AssistancesDebugWindow.SetDescription("Delta");
                        MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Delta");

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
                        AssistancesDebugWindow.SetDescription("Gamma");
                        MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Gamma");
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
                        
                        AssistancesDebugWindow.SetDescription("Epsilon");
                        MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Epsilon");
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
                        AssistancesDebugWindow.SetDescription("Zeta");
                        MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Zeta");
                    }),
                    new WaitUntilStopped()
                    );

                return temp;
            }

            /*private void CallbackInterenceTimer(System.Object o, EventArgs e)
            {
                Conditions["DisplayedSince2Minutes"] = true;
            }*/

            /*
            //Inferences.ObjectFocused TempInf;
            Inferences.ObjectFocused TempInf;
            private void IncreaseAttentionGrabbingGradation()
            {
                Conditions["DisplayedSince2Minutes"] = false; // The timer is going to be restarted, so the condition is not true anymore.

                if (AssistancesGradation.AssistanceCurrent.ShowNextGradation(Utilities.Utility.GetEventHandlerEmpty()))
                {
                    Conditions["MinimalConditionDisplayed"] = false;
                    InfTimer2Minutes.StartCounter(); // The counter is started again to continue increasing the attention gradation if necessary
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Next gradation for attention grabbing shown");
                }
                else
                {
                    Assistance currentAssistance = AssistancesGradation.AssistanceCurrent.GetCurrentAssistance();
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Maximum gradation for attention grabbing shown for " + currentAssistance.name);
                }


                InitializeInferenceFocusedAssistance(CurrentAssistanceIndex); // In any case, the timer for focus should be enabled
            }*/

            private void COnHelpButtonClicked(System.Object o, EventArgs e)
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Button clicked");

                ArgsOnHelpButtonClicked = (Utilities.EventHandlerArgs.ButtonAndAssistanceGradationAttention)e;

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

            /*public void DisplayHelpButtons()
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Assistance focused for 30 seconds");
                //Conditions["IsFocused"] = false;
                //Conditions["WaitingSince30Seconds"] = false;

                AssistancesGradation.AssistanceCurrent.ShowHelpCurrentGradation(true, Utilities.Utility.GetEventHandlerEmpty());
            }*/

            /**
             * This function start the assistance from scratch, i.e. it will not remuse if it has been interrupted. If you want to resume the assistance after it has been interrupted, please use the ResumeAssistance function
             * The callback is called when the process is finished (finished, i.e. not interrupted)
             * */
            /*public void RunAssistance(EventHandler callback)
            {
                Status = AssistanceStatus.Run;

                // Initialize the BT
                if (Tree == null)
                {
                    //InitializeBT();
                    //Tree.Start();

                    
                }
                else
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "BT already running, so nothing to do");
                }
                
            }*/
            
            public void RunAssistance()
            {
                StartTree();
                UpdateConditionWithMatrix(ConditionStartAssistance);
            }

            /*public void PauseAssistance()
            {
                Status = AssistanceStatus.Pause;
            }*/

            public void StopAssistance()
            {
                //Status = AssistanceStatus.Stop;
                //StopTree();
                SetConditionsTo(false);
                /*AssistancesGradation.AssistanceCurrent.HideCurrentGradation(delegate(System.Object o, EventArgs e)
                {
                    AssistancesGradation.BackToRoot();
                    //UpdateConditionWithMatrix(ConditionStartAssistance);
                    //UpdateCondition(ConditionStartAssistance, false);
                    SetConditionsTo(false);
                });*/
            }

            /*public AssistanceStatus GetAssistanceStatus()
            {
                return Status;
            }*/

            /*private void InitializeInferenceFocusedAssistance(int indexAssistanceToMonitor)
            {
                GameObject currentAssistance = AssistancesGradation.AssistanceCurrent.GetCurrentAssistance().GetTransform().gameObject;

                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Going to initialize the focus assistance for " + currentAssistance.name);

                if (InfFocusedOnAssistance != null)
                {
                    InfManager.UnregisterInference(InfFocusedOnAssistance);
                    InfFocusedOnAssistance = null;
                }

                InfFocusedOnAssistance = new Inferences.ObjectFocused("AssistanceFocus", CallbackAssistanceFocused, currentAssistance, 3);
                InfManager.RegisterInference(InfFocusedOnAssistance);
            }*/

            /*private void InitializeInferenceLostFocus(int indexAssistanceToMonitor)
            {
                if (InfFocusLost != null)
                {
                    InfManager.UnregisterInference(InfFocusLost);
                    InfFocusLost = null;
                }

                InfFocusLost = new Inferences.ObjectLostFocused("InferenceGoLostFocus", CallbackInferenceLostFocus, AssistancesGradation.AssistanceCurrent.GetCurrentAssistance().GetTransform().gameObject);
                InfManager.RegisterInference(InfFocusLost);
            }*/

            /*private void InitializeInfIsClose(int indexAssistanceToMonitor)
            {
                GameObject currentAssistance = AssistancesGradation.AssistanceCurrent.GetCurrentAssistance().GetTransform().gameObject;

                if (InfIsClose != null)
                {
                    InfManager.UnregisterInference(InfIsClose);
                    InfIsClose = null;
                }

                InfIsClose = new Inferences.DistanceComing("InferenceCloseToObject", CInfClose, currentAssistance, 1.5f);
                InfManager.RegisterInference(InfIsClose);
            }*/

            /*private void InitializeInfIsFar(int indexAssistanceToMonitor)
            {
                GameObject currentAssistance = AssistancesGradation.AssistanceCurrent.GetCurrentAssistance().GetTransform().gameObject;

                if (InfIsFar != null)
                {
                    InfManager.UnregisterInference(InfIsFar);
                    InfIsFar = null;
                }

                InfIsFar = new Inferences.DistanceLeaving("AssistanceGradationExplicitIsFar", CInfIsFar, currentAssistance, 1.5f);
                InfManager.RegisterInference(InfIsFar);
            }*/

            /*private void InitializeInference30Seconds()
            {
                if (InfTimer30Seconds != null)
                {
                    InfManager.UnregisterInference(InfTimer30Seconds);
                    InfTimer30Seconds = null;
                }

                InfTimer30Seconds = new Inferences.Timer("Inf30Seconds", 5, CallbackInf30Seconds);
                InfManager.RegisterInference(InfTimer30Seconds);
                InfTimer30Seconds.StartCounter();
            }*/

            /*private void CallbackInf30Seconds(System.Object o, EventArgs e)
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Timer 30 seconds called");
                Conditions["WaitingSince30Seconds"] = true;
            }*/

            /*private void CInfClose (System.Object o, EventArgs e)
            {
                Conditions["IsFar"] = false;

                // Means the person is close to the object: consequently, some inferences are not necessary
                //InfManager.UnregisterInference(InfTimer30Seconds);
                InfManager.UnregisterInference(InfIsClose);
                InitializeInfIsFar(CurrentAssistanceIndex);
                //InitializeInfDistance
                InitializeInference30Seconds();
            }*/

            /*private void CInfIsFar(System.Object o, EventArgs e)
            {
                Conditions["IsFar"] = true;

                InfManager.UnregisterInference(InfIsFar);
                InitializeInfIsClose(CurrentAssistanceIndex);

                // Far: initializing inferences
                //InitializeInference30Seconds();
            }*/

            /*private void CallbackInferenceLostFocus(System.Object o, EventArgs e)
            {
                //Conditions["IsFocused"] = false;
                //InfTimer2Minutes.StartCounter();
                //InfManager.UnregisterInference(InfFocusLost);
                //InfManager.UnregisterInference(InfIsClose);
                //InfTimer30Seconds.StopCounter();
            }*/

            /*private void CallbackAssistanceFocused(System.Object o, EventArgs e)
            {
                Conditions["IsFocused"] = true;
                Conditions["DisplayedSince2Minutes"] = false;
                InfTimer2Minutes.StopCounter();
                InfManager.UnregisterInference(InfFocusedOnAssistance);
                InfManager.UnregisterInference(InfFocusLost);
                InfManager.UnregisterInference(InfIsClose);
                InfManager.UnregisterInference(InfIsFar);                
                //InfTimer30Seconds.StartCounter();
            }*/

            /*private void CAssistanceDisplayed(System.Object o, EventArgs e)
            {
                InitializeInferenceFocusedAssistance(CurrentAssistanceIndex);
                InitializeInferenceLostFocus(CurrentAssistanceIndex);
                InitializeInfIsClose(CurrentAssistanceIndex);
                InitializeInfIsFar(CurrentAssistanceIndex);
            }*/

            /*private void CallbackClickOnButton(System.Object o, EventArgs e)
            {
                //Conditions["HelpButtonDisplayed"] = true;
                InfTimer30Seconds.StopCounter();
            }*/
        }
    }
}
