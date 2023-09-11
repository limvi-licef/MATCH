/*Copyright 2022 Guillaume Spalla, Emma Foulon

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
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using System.Linq;

/**
 * For graphical details of the behavior tree implemented here, refer to the documentation
 */
namespace MATCH
{
    namespace Scenarios
    {
        namespace BehaviorTrees
        {
            public class DustingTable : Scenarios.BehaviorTrees.BehaviorTree
            {
                private Inferences.Manager InferenceManager;
                public Assistances.Surfaces.Manager SurfacesManager;

                string ConditionTableCleaned = "TableCleaned";
                string ConditionRagNotTakenButHelpReceived = "RagNotTakenButHelpReceived";
                string ConditionRagTaken = "RagTaken";
                string ConditionDidNotStartCleaning = "DidNotStartCleaning";
                string ConditionCleaningInterrupted = "CleaningInterrupted";
                string ConditionNewPartCleaned = "NewPartCleaned";
                string ConditionProcessRelatedToNewPartsCleanedDone = "ProcessRelatedToNewPartsCleanedDone";
                string ConditionTableTouchedButNoRag = "TableTouchedButNoRag";
                string ConditionDidNotGoToCalendar = "DidNotGoToCalendar";
                string ConditionStandBy = "StandBy";
                string ConditionCloseToCalendar = "CloseToCalendar";
                string ConditionReceivedHelpForCalendarButNoAction = "ReceivedHelpForCalendarButDidNotTakeAction";
                string ConditionAskedHelpWithTodoList = "AskedForHelpWithTodoList";

                string InferenceDidNotStartDusting = "DidNotStartCleaning";
                string InferenceInterruptDusting = "InterruptedDusting";
                string InferenceFarFromRag = "FarFromRag";
                float InferenceFarFromRagDistance = 4.0f;

                string InferenceDidNotGoToCalendar = "DidNotGoToCalendar";
                string InferenceCloseToCalendar = "CloseToCalendar";
                string InferenceCalendarHelp = "CalendarHelp";

                string InferenceCloseToRag = "CloseToRag";

                // Interaction surface
                Assistances.InteractionSurface InteractionSurfaceTable;
                Assistances.InteractionSurface InteractionRag;
                Assistances.InteractionSurface InteractionHelpToDoList;
                Assistances.InteractionSurface InteractionSponge;

                //public event EventHandler TableTouchedEvent;

                Dictionary<Assistances.AssistanceGradationExplicit, bool> AssistancesDusting;
                Dictionary<Utilities.Utility.GreekAlphabet, Assistances.AssistanceGradationExplicit> AssistancesDustingById;

                DustingTableAssistances AssistancesDB;

                //Transform ToDoListView;
                Assistances.Dialogs.ToDoList ToDoListView;
                Utilities.ToDoList ToDoListController;
                Transform ToDoListAssistanceCanva;

                public override void Awake()
                {
                    base.Awake();
                    SetId("Epousseter la table");
                    AssistancesDusting = new Dictionary<Assistances.AssistanceGradationExplicit, bool>();
                    AssistancesDustingById = new Dictionary<Utilities.Utility.GreekAlphabet, Assistances.AssistanceGradationExplicit>();
                }

                public override void Start()
                {

                    ToDoListController = transform.root.Find("ToDoList").GetComponent<Utilities.ToDoList>();
                    ToDoListView = ToDoListController.GetAssistance();
                    ToDoListView.Show(Utilities.Utility.GetEventHandlerEmpty(), false);
                    //ToDoList = toDoListView.GetComponent<Assistances.Dialogs.Dialog1>();
                    ToDoListAssistanceCanva = ToDoListController.GetAssistance().GetAssistanceCanva();

                    ToDoListController.GetAssistance().EventElementClicked += delegate (System.Object o, EventArgs e)
                    {
                        UpdateConditionWithMatrix(ConditionAskedHelpWithTodoList);
                    };

                    Scenarios.Manager.Instance.AddScenario(this);

                    // Initialize assistances
                    InitializeInteractionSurfaces();
                    AssistancesDB = new DustingTableAssistances(InteractionSurfaceTable.transform, InteractionRag.transform, ToDoListView, ToDoListAssistanceCanva, InteractionHelpToDoList.transform);

                    // Initialize inference manager
                    InferenceManager = MATCH.Inferences.Factory.Instance.CreateManager(transform);

                    // Call base function
                    base.Start();

                    Init();

                    // Add button to restart scenario
                    MATCH.AdminMenu.Instance.AddButton("Dusting table - restart scenario", delegate
                    {
                        SetConditionsTo(false);
                        MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "----------Scenario restarted----------");
                    }, AdminMenu.Panels.Right);

                    // Add button to modify the distance from rag for the inference
                    AdminMenu.Instance.AddInputWithButton(InferenceFarFromRagDistance.ToString(), "DustingTable - Update distance from rag", delegate (System.Object o, EventArgs e)
                    {
                        Utilities.EventHandlerArgs.String arg = (Utilities.EventHandlerArgs.String)e;
                        InferenceFarFromRagDistance = float.Parse(arg.m_text);

                        if (InferenceManager.GetInference(InferenceFarFromRag) != null)
                        {
                            RegisterInferenceFarFromRag(); // Register the inference with the new value, only if it has been registered already
                        }


                    }, AdminMenu.Panels.Right);

                    // Initialize debug buttons
                    //InitializeDebugButtons();
                }

                protected override Root InitializeBehaviorTree()
                {
                    // Making the conditions update. SEE EXCEL SHEET TO GENERATE THE CODE BELOW
                    AddCondition(ConditionTableCleaned, false);
                    AddCondition(ConditionRagNotTakenButHelpReceived, false);
                    AddCondition(ConditionRagTaken, false);
                    AddCondition(ConditionDidNotStartCleaning, false);
                    AddCondition(ConditionCleaningInterrupted, false);
                    AddCondition(ConditionNewPartCleaned, false);
                    AddCondition(ConditionProcessRelatedToNewPartsCleanedDone, false);
                    AddCondition(ConditionTableTouchedButNoRag, false);
                    AddCondition(ConditionDidNotGoToCalendar, false);
                    AddCondition(ConditionStandBy, true);
                    AddCondition(ConditionCloseToCalendar, false);
                    AddCondition(ConditionReceivedHelpForCalendarButNoAction, false);
                    AddCondition(ConditionAskedHelpWithTodoList, false);

                    int nbConditions = GetNumberOfConditions();

                    AddConditionsUpdate(ConditionTableCleaned, new bool[] { true, false, false, false, false, false, false, false, false, false, false, false, false });
                    AddConditionsUpdate(ConditionRagNotTakenButHelpReceived, new bool[] { false, true, false, false, false, false, false, false, false, false, false, false, false });
                    AddConditionsUpdate(ConditionRagTaken, new bool[] { false, false, true, false, false, false, false, false, false, false, false, false, false });
                    AddConditionsUpdate(ConditionDidNotStartCleaning, new bool[] { false, false, true, true, false, false, false, false, false, false, false, false, false });
                    AddConditionsUpdate(ConditionCleaningInterrupted, new bool[] { false, false, true, false, true, false, false, false, false, false, false, false, false });
                    AddConditionsUpdate(ConditionNewPartCleaned, new bool[] { false, false, true, false, false, true, false, false, false, false, false, false, false });
                    AddConditionsUpdate(ConditionProcessRelatedToNewPartsCleanedDone, new bool[] { false, false, true, false, false, false, true, false, false, false, false, false, false });
                    AddConditionsUpdate(ConditionTableTouchedButNoRag, new bool[] { false, false, false, false, false, false, false, true, false, false, false, false, false });
                    AddConditionsUpdate(ConditionDidNotGoToCalendar, new bool[] { false, false, false, false, false, false, false, false, true, false, false, false, false });
                    AddConditionsUpdate(ConditionStandBy, new bool[] { false, false, false, false, false, false, false, false, false, true, false, false, false });
                    AddConditionsUpdate(ConditionCloseToCalendar, new bool[] { false, false, false, false, false, false, false, false, false, false, true, false, false });
                    AddConditionsUpdate(ConditionReceivedHelpForCalendarButNoAction, new bool[] { false, false, false, false, false, false, false, false, false, false, false, true, false });
                    AddConditionsUpdate(ConditionAskedHelpWithTodoList, new bool[] { false, false, false, false, false, false, false, false, false, false, false, false, true });

                    // End of code generation using the EXCEL file

                    // Defining the BT
                    Selector srRagNotTaken = new Selector(
                        new BlackboardCondition(ConditionDidNotGoToCalendar, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceKappa()),
                        new BlackboardCondition(ConditionCloseToCalendar, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceMu()),
                        new BlackboardCondition(ConditionAskedHelpWithTodoList, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceXi()),
                        /*new BlackboardCondition(ConditionReceivedHelpForCalendarButNoAction, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceNu()),*/
                        new BlackboardCondition(ConditionTableTouchedButNoRag, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceIota()),
                        new BlackboardCondition(ConditionRagNotTakenButHelpReceived, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceEpsilon()),
                        AssistanceBeta()
                        );


                    Selector srTableNotCleaned = new Selector(
                        new BlackboardCondition(ConditionRagTaken, Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART, srRagNotTaken),
                        new Inverter(AssistanceEta()),
                        new BlackboardCondition(ConditionDidNotStartCleaning, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceGamma()),
                        new BlackboardCondition(ConditionNewPartCleaned, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceZeta()),
                        new BlackboardCondition(ConditionProcessRelatedToNewPartsCleanedDone, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceTheta()),
                        new BlackboardCondition(ConditionCleaningInterrupted, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceDelta()),
                        new WaitUntilStopped()
                        );

                    Selector srBegin = new Selector(
                        new BlackboardCondition(ConditionTableCleaned, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceAlpha()),
                        new BlackboardCondition(ConditionStandBy, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceLambda()),
                        srTableNotCleaned);

                    Root tree = new Root(/*Conditions*/Getconditions(), srBegin);

                    return tree;
                }

                void InitializeDebugButtons()
                {
                    // Debug buttons to check if the BT has been correctly modeled
                    AdminMenu.Instance.AddButton("BT - Dusting - Trigger - table cleaned", delegate
                    {
                        UpdateConditionWithMatrix(ConditionTableCleaned);
                    }, AdminMenu.Panels.Right);

                    AdminMenu.Instance.AddButton("BT - Dusting - Trigger - rag taken", delegate
                    {
                        UpdateConditionWithMatrix(ConditionRagTaken);
                    }, AdminMenu.Panels.Right);

                    AdminMenu.Instance.AddButton("BT - Dusting - Trigger - clean without rag", delegate
                    {
                        UpdateConditionWithMatrix(ConditionRagNotTakenButHelpReceived);
                    }, AdminMenu.Panels.Right);

                    AdminMenu.Instance.AddButton("BT - Dusting - Trigger - clean with rag", delegate
                    {
                        UpdateConditionWithMatrix(ConditionDidNotStartCleaning);
                    }, AdminMenu.Panels.Right);

                    AdminMenu.Instance.AddButton("BT - Dusting - Trigger - cleaning interrupted", delegate
                    {
                        UpdateConditionWithMatrix(ConditionCleaningInterrupted);
                    }, AdminMenu.Panels.Right);
                    AdminMenu.Instance.AddButton("BT - Dusting - Trigger - all false", delegate
                    {
                        UpdateConditionWithMatrix(ConditionTableCleaned);
                        UpdateCondition(ConditionTableCleaned, false);
                    }, AdminMenu.Panels.Right);


                }

                void InitializeInteractionSurfaces()
                {
                    InteractionSurfaceTable = Assistances.Factory.Instance.CreateInteractionSurface("DustingTable - Table", AdminMenu.Panels.Right, new Vector3(1.1f, 0.02f, 0.7f), new Vector3(-0.447f, -0.406f, 0.009f), Utilities.Materials.Colors.CyanGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);
                    //InteractionSurfaceTable.ShowInteractionSurfaceTable(false);

                    InteractionRag = Assistances.Factory.Instance.CreateInteractionSurface("DustingTable - Rag", AdminMenu.Panels.Right, new Vector3(0.1f, 0.1f, 0.1f), new Vector3(-1.378f, -0.364f, 2.743f), Utilities.Materials.Colors.OrangeGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);

                    InteractionRag.EventUserTouched += CallbackInteractionSurfaceRagTouched;

                    InteractionSurfaceTable.EventUserTouched += CallbackInteractionSurfaceTableTouched;

                    InteractionHelpToDoList = Assistances.Factory.Instance.CreateInteractionSurface("DustingTable - Calendar help", AdminMenu.Panels.Right, new Vector3(1.1f, 0.02f, 0.7f), new Vector3(-0.447f, -0.406f, 0.009f), Utilities.Materials.Colors.RedGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);

                    InteractionSponge = Assistances.Factory.Instance.CreateInteractionSurface("DustingTable - Sponge", AdminMenu.Panels.Right, new Vector3(0.1f, 0.1f, 0.1f), new Vector3(-1.378f, -0.364f, 2.743f), Utilities.Materials.Colors.GreenGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);
                    InteractionSponge.EventUserTouched += CallbackInteractionSurfaceRagTouched;
                }

                void ShowAssistanceHideOthers(Assistances.AssistanceGradationExplicit assistance)
                {
                    List<Assistances.AssistanceGradationExplicit> keys = new List<Assistances.AssistanceGradationExplicit>(AssistancesDusting.Keys);

                    //foreach(KeyValuePair<Assistances.AssistanceGradationExplicit, bool> assistanceDusting in AssistancesDusting)
                    foreach (Assistances.AssistanceGradationExplicit key in keys)
                    {
                        if (assistance == key)
                        {
                            if (AssistancesDusting[assistance] == false)
                            {
                                assistance.RunAssistance();
                                AssistancesDusting[assistance] = true;
                            }
                        }
                        else if (/*AssistancesDusting[key]*/ AssistancesDusting.ContainsKey(key))
                        {
                            key.StopAssistance();
                            AssistancesDusting[key] = false;
                        }
                    }
                }

                Sequence AssistanceAlpha()
                {
                    Assistances.AssistanceGradationExplicit alpha = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("DustingTable-Alpha");
                    alpha.transform.parent = transform;

                    alpha.AddAssistance(AssistancesDB.Alpha[0], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Alpha[1]);
                    alpha.AddAssistance(AssistancesDB.Alpha[0], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Alpha[2]);

                    alpha.AddAssistance(AssistancesDB.Alpha[1], Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    alpha.AddAssistance(AssistancesDB.Alpha[2], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Alpha[0]); // Risk of cyclic redundancy?
                    alpha.AddAssistance(AssistancesDB.Alpha[2], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Alpha[3]);

                    alpha.AddAssistance(AssistancesDB.Alpha[3], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Alpha[6]);
                    alpha.AddAssistance(AssistancesDB.Alpha[3], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Alpha[4]);

                    alpha.AddAssistance(AssistancesDB.Alpha[4], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Alpha[1]);
                    alpha.AddAssistance(AssistancesDB.Alpha[4], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Alpha[5]);

                    alpha.AddAssistance(AssistancesDB.Alpha[5], Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    alpha.AddAssistance(AssistancesDB.Alpha[6], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Alpha[1]);
                    alpha.AddAssistance(AssistancesDB.Alpha[6], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Alpha[5]);

                    AssistancesDusting.Add(alpha, false);
                    AssistancesDustingById.Add(Utilities.Utility.GreekAlphabet.Alpha, alpha);

                    alpha.Init();

                    Sequence temp = new Sequence(


                        new NPBehave.Action(() =>
                        {
                            ShowAssistanceHideOthers(alpha);
                            InferenceManager.UnregisterAllInferences();
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Alpha");
                            OnChallengeSuccess();
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                Sequence AssistanceLambda()
                {


                    Sequence temp = new Sequence(
                         new NPBehave.Action(() =>
                         {
                             OnChallengeStart();

                             Inferences.Timer infTimerNotCameToCalendar = new Inferences.Timer(InferenceDidNotGoToCalendar, 15, delegate (System.Object oo, EventArgs ee)
                             {
                                 UpdateConditionWithMatrix(ConditionDidNotGoToCalendar);
                                 InferenceManager.UnregisterInference(/*InferenceInterruptDusting*/InferenceDidNotGoToCalendar);
                             });

                             InferenceManager.RegisterInference(infTimerNotCameToCalendar);

                             infTimerNotCameToCalendar.StartCounter();

                             Inferences.DistanceComing distanceComing = new Inferences.DistanceComing(InferenceCloseToCalendar, delegate (System.Object o, EventArgs e)
                             {
                                 infTimerNotCameToCalendar.StopCounter();
                                 InferenceManager.UnregisterInference(InferenceDidNotGoToCalendar);
                                 InferenceManager.UnregisterInference(InferenceCloseToCalendar);
                                 //SetConditionsTo(false);
                                 /*if (AssistancesDusting[AssistancesDustingById[Utilities.Utility.GreekAlphabet.Beta]] == false)
                                 {*/
                                     UpdateConditionWithMatrix(ConditionCloseToCalendar);
                                 //}
                                 
                                 //infTimerCalendarHelp.StartCounter();

                             }, ToDoListView.GetAssistance().gameObject, 2.0f);

                             MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Lambda");
                             InferenceManager.RegisterInference(distanceComing);
                         }),
                    new WaitUntilStopped()
                        );

                    return temp;
                }

                Sequence AssistanceBeta()
                {
                    Assistances.AssistanceGradationExplicit assistanceBeta = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("DustingTable-Beta");

                    assistanceBeta.transform.parent = transform;

                    assistanceBeta.AddAssistance(AssistancesDB.Beta[0], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Beta[1]);
                    assistanceBeta.AddAssistance(AssistancesDB.Beta[0], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Beta[10]);

                    assistanceBeta.AddAssistance(AssistancesDB.Beta[1], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Beta[11]);
                    assistanceBeta.AddAssistance(AssistancesDB.Beta[1], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Beta[2]);

                    assistanceBeta.AddAssistance(AssistancesDB.Beta[11], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Beta[10]);
                    assistanceBeta.AddAssistance(AssistancesDB.Beta[11], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Beta[2]);

                    assistanceBeta.AddAssistance(AssistancesDB.Beta[2], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Beta[10]);
                    assistanceBeta.AddAssistance(AssistancesDB.Beta[2], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Beta[3]);

                    assistanceBeta.AddAssistance(AssistancesDB.Beta[3], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Beta[10]);
                    assistanceBeta.AddAssistance(AssistancesDB.Beta[3], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Beta[4]);

                    assistanceBeta.AddAssistance(AssistancesDB.Beta[4], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Beta[10]);
                    assistanceBeta.AddAssistance(AssistancesDB.Beta[4], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Beta[5]);

                    assistanceBeta.AddAssistance(AssistancesDB.Beta[5], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Beta[10]);
                    assistanceBeta.AddAssistance(AssistancesDB.Beta[5], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Beta[6]);

                    AssistancesDB.ReusableComponentBeta06(ref assistanceBeta);

                    Assistances.IAssistance beta8IAssistance = ((Assistances.IAssistance)AssistancesDB.Beta[8].GetCurrentAssistance());
                    Assistances.Dialogs.Dialog2 beta8Dialog2 = ((Assistances.Dialogs.Dialog2)beta8IAssistance.GetRootDecoratedAssistance());
                    beta8Dialog2.ButtonsController.Last().EventButtonClicked += delegate (System.Object o, EventArgs e)
                    {
                        Inferences.DistanceComing distanceComing = new Inferences.DistanceComing(InferenceCloseToRag, delegate (System.Object o, EventArgs e)
                        {
                            InferenceManager.UnregisterInference(InferenceCloseToRag);
                            UpdateConditionWithMatrix(ConditionRagNotTakenButHelpReceived);
                        }, InteractionRag.GetInteractionSurface().gameObject, 2.0f);
                        InferenceManager.RegisterInference(distanceComing);
                    };

                    Assistances.IAssistance beta10IAssistance = ((Assistances.IAssistance)AssistancesDB.Beta[10].GetCurrentAssistance());
                    Assistances.Dialogs.Dialog2 beta10Dialog2 = ((Assistances.Dialogs.Dialog2)beta10IAssistance.GetRootDecoratedAssistance());

                    beta10Dialog2.EventIsShown += delegate (System.Object o, EventArgs e)
                    {
                        RegisterInferenceFarFromRag();
                    };

                    AssistancesDusting.Add(assistanceBeta, false);
                    AssistancesDustingById.Add(Utilities.Utility.GreekAlphabet.Beta, assistanceBeta);

                    assistanceBeta.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() =>
                        {
                            ShowAssistanceHideOthers(assistanceBeta); //assistanceBeta.RunAssistance();
                            UpdateTextAssistancesDebugWindow("Beta");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Beta");

                            //ToDoListController.ShowHelp(true);

                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                Sequence AssistanceEpsilon()
                {
                    Assistances.AssistanceGradationExplicit epsilon = Assistances.Factory.Instance.CreateAssistanceGradationExplicit("DustingTable-Epsilon");
                    epsilon.AddAssistance(AssistancesDB.Epsilon[0], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Epsilon[3]);
                    epsilon.AddAssistance(AssistancesDB.Epsilon[0], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Epsilon[1]);

                    epsilon.AddAssistance(AssistancesDB.Epsilon[1], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Epsilon[3]);
                    epsilon.AddAssistance(AssistancesDB.Epsilon[1], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Epsilon[2]);

                    epsilon.AddAssistance(AssistancesDB.Epsilon[2], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Epsilon[3]);
                    epsilon.AddAssistance(AssistancesDB.Epsilon[2], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Epsilon[4]);

                    epsilon.AddAssistance(AssistancesDB.Epsilon[3], Assistances.Buttons.Button.ButtonType.ClosingButton, null);
                    epsilon.AddAssistance(AssistancesDB.Epsilon[4], Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    Assistances.IAssistance epsilon3IAssistance = ((Assistances.IAssistance)AssistancesDB.Epsilon[3].GetCurrentAssistance());
                    Assistances.Dialogs.Dialog2 epsilon3Dialog2 = ((Assistances.Dialogs.Dialog2)epsilon3IAssistance.GetRootDecoratedAssistance());

                    epsilon3Dialog2.EventIsShown += delegate (System.Object o, EventArgs e)
                    {
                        epsilon.StopAssistance();
                        RegisterInferenceFarFromRag();
                    };

                    AssistancesDusting.Add(epsilon, false);
                    AssistancesDustingById.Add(Utilities.Utility.GreekAlphabet.Epsilon, epsilon);

                    epsilon.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() =>
                        {
                            ShowAssistanceHideOthers(epsilon);
                            InferenceManager.UnregisterInference(InferenceFarFromRag);

                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Epsilon");

                        }),
                        new WaitUntilStopped());

                    return temp;
                }

                Sequence AssistanceGamma()
                {
                    Assistances.AssistanceGradationExplicit gamma = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("DustingTable-Gamma");

                    gamma.AddAssistance(AssistancesDB.Gamma[0], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Delta[5]);
                    gamma.AddAssistance(AssistancesDB.Gamma[0], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Gamma[1]);
                    //AssistancesDB.ReusableComponentBeta10(ref gamma);

                    gamma.AddAssistance(AssistancesDB.Delta[5], Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesDB.ReusableComponentGamma01(ref gamma);

                    AssistancesDusting.Add(gamma, false);
                    AssistancesDustingById.Add(Utilities.Utility.GreekAlphabet.Gamma, gamma);

                    gamma.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() =>
                        {
                            //ShowAssistanceHideOthers(alpha);
                            gamma.RunAssistance();
                            AssistancesDusting[gamma] = true;
                            InferenceManager.UnregisterAllInferences();
                            UpdateTextAssistancesDebugWindow("Gamma");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Gamma");

                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                Sequence AssistanceTest()
                {
                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => UpdateTextAssistancesDebugWindow("Test")),
                        new WaitUntilStopped());

                    return temp;
                }

                Sequence AssistanceZeta()
                {
                    Sequence temp = new Sequence(
                        new NPBehave.Action(() =>
                        {
                            InferenceManager.UnregisterInference(InferenceDidNotStartDusting);
                            InferenceManager.UnregisterInference(InferenceInterruptDusting);
                            Inferences.Timer inf2 = new Inferences.Timer(InferenceInterruptDusting, 15, delegate (System.Object oo, EventArgs ee)
                            {
                                UpdateConditionWithMatrix(ConditionCleaningInterrupted);
                                InferenceManager.UnregisterInference(InferenceInterruptDusting);
                            });
                            InferenceManager.RegisterInference(inf2);
                            inf2.StartCounter();
                            //UpdateTextAssistancesDebugWindow("Zeta");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Zeta");
                            UpdateConditionWithMatrix(ConditionProcessRelatedToNewPartsCleanedDone);
                        }),
                        new WaitUntilStopped());

                    return temp;
                }

                Sequence AssistanceTheta()
                { // This assistance has only one goal: to be able to go back to Zeta.
                    Sequence temp = new Sequence(
                        new NPBehave.Action(() =>
                        {
                            //UpdateTextAssistancesDebugWindow("Theta");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Theta");
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                Sequence AssistanceDelta()
                {
                    Assistances.AssistanceGradationExplicit delta = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("DustingTable-Delta");

                    delta.AddAssistance(AssistancesDB.Delta[0], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Delta[1]);
                    delta.AddAssistance(AssistancesDB.Delta[0], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Delta[3]);

                    delta.AddAssistance(AssistancesDB.Delta[1], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Delta[5]);
                    delta.AddAssistance(AssistancesDB.Delta[1], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Delta[2]);

                    //AssistancesDB.ReusableComponentBeta10(ref delta);

                    delta.AddAssistance(AssistancesDB.Delta[2], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Delta[5]);
                    delta.AddAssistance(AssistancesDB.Delta[2], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Gamma[2]);

                    AssistancesDB.ReusableComponentGamma02(ref delta);

                    delta.AddAssistance(AssistancesDB.Delta[3], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Delta[4]);
                    delta.AddAssistance(AssistancesDB.Delta[3], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Delta[1]);

                    delta.AddAssistance(AssistancesDB.Delta[4], Assistances.Buttons.Button.ButtonType.ClosingButton, null);
                    delta.AddAssistance(AssistancesDB.Delta[4], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Gamma[2]);

                    delta.AddAssistance(AssistancesDB.Delta[5], Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesDusting.Add(delta, false);
                    AssistancesDustingById.Add(Utilities.Utility.GreekAlphabet.Delta, delta);

                    delta.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() =>
                        {
                            delta.RunAssistance();
                            AssistancesDusting[delta] = true;
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Delta");
                        }),
                        new WaitUntilStopped());

                    return temp;
                }

                Sequence AssistanceEta()
                {
                    Assistances.SurfaceToProcess surface = Assistances.Factory.Instance.CreateSurfaceToProcess(InteractionSurfaceTable.transform, InteractionSurfaceTable);
                    surface.EventSurfaceCleaned += delegate (System.Object o, EventArgs e)
                    {
                        surface.Hide(Utilities.Utility.GetEventHandlerEmpty(), false);
                        UpdateConditionWithMatrix(ConditionTableCleaned);
                    };
                    surface.EventNewPartCleaned += delegate (System.Object o, EventArgs e)
                    {
                        surface.Show(Utilities.Utility.GetEventHandlerEmpty(), false);
                        ShowAssistanceHideOthers(null);
                        UpdateConditionWithMatrix(ConditionNewPartCleaned);
                    };

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() =>
                        {
                            surface.Show(Utilities.Utility.GetEventHandlerEmpty(), false);
                            ShowAssistanceHideOthers(/*eta*/ null);
                            OnChallengeStart();
                            Inferences.Timer inf = new Inferences.Timer(InferenceDidNotStartDusting, 15, delegate (System.Object o, EventArgs e)
                            {
                                UpdateConditionWithMatrix(ConditionDidNotStartCleaning);
                                InferenceManager.UnregisterInference(InferenceDidNotStartDusting);
                            });

                            InferenceManager.RegisterInference(inf);
                            inf.StartCounter();

                            UpdateTextAssistancesDebugWindow("Eta");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Eta");
                        })
                        );

                    return temp;
                }

                Sequence AssistanceKappa()
                {
                    Assistances.AssistanceGradationExplicit kappa = Assistances.Factory.Instance.CreateAssistanceGradationExplicit("Dusting_Kappa");
                    kappa.transform.parent = transform;
                    kappa.AddAssistance(AssistancesDB.Kappa[0], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Kappa[1]);
                    kappa.AddAssistance(AssistancesDB.Kappa[0], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Kappa[4]);

                    kappa.AddAssistance(AssistancesDB.Kappa[1], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Kappa[4]);
                    kappa.AddAssistance(AssistancesDB.Kappa[1], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Kappa[2]);

                    kappa.AddAssistance(AssistancesDB.Kappa[2], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Kappa[3]);
                    kappa.AddAssistance(AssistancesDB.Kappa[2], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Kappa[3]);

                    kappa.AddAssistance(AssistancesDB.Kappa[3], Assistances.Buttons.Button.ButtonType.Undefined, null);
                    kappa.AddAssistance(AssistancesDB.Kappa[4], Assistances.Buttons.Button.ButtonType.Undefined, null);

                    AssistancesDB.Kappa[3].IsShown += delegate (System.Object o, EventArgs e)
                    {
                        DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Kappa > assistance 3: shown");
                    };

                    AssistancesDB.Kappa[4].IsHidden += delegate (System.Object o, EventArgs e)
                    {
                        DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Kappa > assistance 4: hidden");

                        //UpdateConditionWithMatrix(ConditionStandBy);
                        InferenceManager.UnregisterInference(InferenceCloseToCalendar);
                        SetConditionsTo(false);
                    };

                    AssistancesDusting.Add(kappa, false);
                    AssistancesDustingById.Add(Utilities.Utility.GreekAlphabet.Kappa, kappa);

                    kappa.Init();


                    Sequence temp = new Sequence(
                         new NPBehave.Action(() =>
                         {


                             ShowAssistanceHideOthers(kappa);
                             kappa.StopAssistance();
                             kappa.RunAssistance();
                             MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Kappa");

                             MATCH.DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Kappa triggered.");
                         }),
                         new WaitUntilStopped()
                        );

                    return temp;
                }

                Sequence AssistanceIota()
                {
                    Assistances.AssistanceGradationExplicit iota = Assistances.Factory.Instance.CreateAssistanceGradationExplicit("Dusting_Iota");
                    iota.transform.parent = transform;
                    iota.AddAssistance(AssistancesDB.Iota[0], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Iota[1]);
                    iota.AddAssistance(AssistancesDB.Iota[0], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Iota[3]);

                    iota.AddAssistance(AssistancesDB.Iota[1], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Iota[2]);
                    iota.AddAssistance(AssistancesDB.Iota[1], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Iota[3]);

                    iota.AddAssistance(AssistancesDB.Iota[2], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Iota[4]);
                    iota.AddAssistance(AssistancesDB.Iota[2], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Iota[4]);
                    iota.AddAssistance(AssistancesDB.Iota[4], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Iota[7]);
                    iota.AddAssistance(AssistancesDB.Iota[4], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Iota[5]);
                    iota.AddAssistance(AssistancesDB.Iota[5], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Iota[7]);
                    iota.AddAssistance(AssistancesDB.Iota[5], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Iota[6]);
                    iota.AddAssistance(AssistancesDB.Iota[6], Assistances.Buttons.Button.ButtonType.ClosingButton, null);
                    iota.AddAssistance(AssistancesDB.Iota[7], Assistances.Buttons.Button.ButtonType.ClosingButton, null);
                    //AssistancesDB.ReusableComponentBeta07(ref iota);

                    iota.AddAssistance(AssistancesDB.Iota[3], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Iota[7]);
                    iota.AddAssistance(AssistancesDB.Iota[3], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Iota[4]);

                    AssistancesDusting.Add(iota, false);
                    AssistancesDustingById.Add(Utilities.Utility.GreekAlphabet.Iota, iota);

                    iota.Init();

                    Assistances.IAssistance iota7IAssistance = ((Assistances.IAssistance)AssistancesDB.Iota[7].GetCurrentAssistance());
                    Assistances.Dialogs.Dialog2 iota7Dialog2 = ((Assistances.Dialogs.Dialog2)iota7IAssistance.GetRootDecoratedAssistance());

                    iota7Dialog2.EventIsShown += delegate (System.Object o, EventArgs e)
                    {
                        RegisterInferenceFarFromRag();
                    };

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() =>
                        {
                            InferenceManager.UnregisterInference(InferenceDidNotGoToCalendar);
                            ShowAssistanceHideOthers(iota);
                            iota.StopAssistance();
                            iota.RunAssistance();
                            AssistancesDusting[iota] = true;
                            //UpdateTextAssistancesDebugWindow("Iota");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Iota");
                            MATCH.DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Iota triggered.");

                        }),
                        new WaitUntilStopped()
                        );
                    return temp;
                }

                Sequence AssistanceMu()
                {
                    Sequence temp = new Sequence(
                         new NPBehave.Action(() =>
                         {
                             ShowAssistanceHideOthers(null);

                             Inferences.Timer infTimerCalendarHelp = new Inferences.Timer(InferenceCalendarHelp, 5, delegate (System.Object oo, EventArgs ee)
                             {
                                 ((Inferences.Timer)InferenceManager.GetInference(InferenceCalendarHelp)).StopCounter();
                                 SetConditionsTo(false);
                                 InferenceManager.UnregisterInference(InferenceCalendarHelp);
                             });



                             InferenceManager.RegisterInference(infTimerCalendarHelp);

                             infTimerCalendarHelp.StartCounter();
                             MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Mu");
                         }),
                    new WaitUntilStopped()
                        );

                    return temp;
                }

                /*Inferences.Factory.Instance.CreateDistanceLeavingInferenceOneShot(InferenceManager, "leavingCalendar", delegate (System.Object oo, EventArgs ee)
                                     {
                                         ToDoListController.GetAssistance().ShowAssistanceCanva(false);
            }, ToDoListView.gameObject);*/

                Sequence AssistanceXi()
                {
                    // Disable billboard for these assistance as they are linked to the todolist
                    // Be careful: assumes that all assistance are Dialog2 ...
                    // Todo: this part is not clean and could be improved.
                    foreach (Assistances.GradationVisual.GradationVisual xiGradationVisual in AssistancesDB.Xi)
                    {
                        Assistances.IAssistance xi0IAssistance = (Assistances.IAssistance)xiGradationVisual.GetCurrentAssistance();

                        Assistances.Dialogs.Dialog2 xi0Dialog2 = null;

                        try
                        {
                            xi0Dialog2 = (Assistances.Dialogs.Dialog2)xi0IAssistance.GetRootDecoratedAssistance();
                            xi0Dialog2.EventIsShown += delegate (System.Object o, EventArgs e)
                            {
                                xi0IAssistance.GetIcon().Hide(Utilities.Utility.GetEventHandlerEmpty(), false);
                                xi0Dialog2.GetBackgroundIcon().gameObject.SetActive(false);
                                //xi0Dialog2.GetIcon().Hide(Utilities.Utility.GetEventHandlerEmpty(), false);
                            };
                        }
                        catch
                        {
                            // So far you did not find it clean? Well, the best part comes ... 
                            try { // If this is not a Dialog2, most likely it is a PathWithTextAndHelp ^^
                                xi0Dialog2 = ((Assistances.PathWithTextAndHelp)xi0IAssistance.GetRootDecoratedAssistance()).GetDialog2();
                                xi0IAssistance.GetRootDecoratedAssistance().transform.localRotation = Quaternion.Euler(0, 0, 0);
                                //xi0Dialog2.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0)); // 180 degree rotation because ... it does not appear correctly rotated. This code is dirty, I want to show respect to it.
                            }
                            catch
                            {
                                //xi0Dialog2 = (Assistances.Dialogs.Dialog2)(((Assistances.Dialogs.Dialog2Contextualized)xi0IAssistance.GetRootDecoratedAssistance()));
                            }
                        }

                        string descriptionTemp = xi0Dialog2.GetDescription();

                        if (xi0Dialog2!= null)
                        {
                            xi0Dialog2.EnableBillboard(false);
                            xi0Dialog2.transform.localPosition = new Vector3(0, 0.0f, 0);
                            // To integrate better the dialog2 in the background of the todolist, hide the icon, and make it bigger
                            /*xi0Dialog2.GetIcon().Hide(Utilities.Utility.GetEventHandlerEmpty(), false);
                            xi0Dialog2.EventIsShown += delegate (System.Object o, EventArgs e)
                            {
                                xi0Dialog2.GetIcon().Hide(Utilities.Utility.GetEventHandlerEmpty(), false);
                            };*/
                            xi0Dialog2.transform.localScale = xi0Dialog2.transform.localScale * 2.5f;
                           
                            foreach(Assistances.Buttons.Basic button in xi0Dialog2.ButtonsController)
                            {
                                BoxCollider boxCollider = button.GetComponent<BoxCollider>();
                                boxCollider.size = new Vector3(boxCollider.size.x, boxCollider.size.y, 10);
                                boxCollider.center = new Vector3(boxCollider.center.x, boxCollider.center.y, -5);
                            }
                        }
                    }

                    Assistances.AssistanceGradationExplicit xi = Assistances.Factory.Instance.CreateAssistanceGradationExplicit("Dusting_Xi");
                    xi.transform.parent = transform;
                    xi.AddAssistance(AssistancesDB.Xi[0], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Xi[1]);
                    xi.AddAssistance(AssistancesDB.Xi[0], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Xi[10]);

                    xi.AddAssistance(AssistancesDB.Xi[1], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Xi[11]);
                    xi.AddAssistance(AssistancesDB.Xi[1], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Xi[2]);

                    xi.AddAssistance(AssistancesDB.Xi[11], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Xi[10]);
                    xi.AddAssistance(AssistancesDB.Xi[11], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Xi[2]);

                    xi.AddAssistance(AssistancesDB.Xi[2], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Xi[10]);
                    xi.AddAssistance(AssistancesDB.Xi[2], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Xi[3]);

                    xi.AddAssistance(AssistancesDB.Xi[3], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Xi[10]);
                    xi.AddAssistance(AssistancesDB.Xi[3], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Xi[4]);

                    xi.AddAssistance(AssistancesDB.Xi[4], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Xi[10]);
                    xi.AddAssistance(AssistancesDB.Xi[4], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Xi[5]);

                    xi.AddAssistance(AssistancesDB.Xi[5], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Xi[10]);
                    xi.AddAssistance(AssistancesDB.Xi[5], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Xi[6]);

                    xi.AddAssistance(AssistancesDB.Xi[6], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Xi[10]);
                    xi.AddAssistance(AssistancesDB.Xi[6], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Xi[7]);

                    xi.AddAssistance(AssistancesDB.Xi[7], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Xi[10]);
                    xi.AddAssistance(AssistancesDB.Xi[7], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Xi[8]);

                    xi.AddAssistance(AssistancesDB.Xi[8], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Xi[10]);
                    xi.AddAssistance(AssistancesDB.Xi[8], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Xi[9]);

                    xi.AddAssistance(AssistancesDB.Xi[9], Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    xi.AddAssistance(AssistancesDB.Xi[10], Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesDB.Xi[10].EventHelpClicked += delegate (System.Object o, EventArgs e)
                    {
                        xi.StopAssistance();
                        SetConditionsTo(false);
                        ToDoListController.GetAssistance().ShowAssistanceCanva(false);
                    };

                    AssistancesDusting.Add(xi, false);
                    AssistancesDustingById.Add(Utilities.Utility.GreekAlphabet.Xi, xi);

                    xi.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() =>
                        {
                            InferenceManager.UnregisterInference(InferenceCalendarHelp);
                            ToDoListController.GetAssistance().ShowAssistanceCanva(true);
                            ShowAssistanceHideOthers(xi);
                            xi.StopAssistance();
                            xi.RunAssistance();
                        }),
                        new WaitUntilStopped()
                        );
                    return temp;
                }

                void CallbackInteractionSurfaceRagTouched(System.Object o, EventArgs e)
                {
                    InferenceManager.UnregisterInference(InferenceDidNotGoToCalendar);
                    InferenceManager.UnregisterInference(InferenceFarFromRag);
                    UpdateConditionWithMatrix(ConditionRagTaken);
                }

                public void CallbackInteractionSurfaceTableTouched(System.Object o, EventArgs e)
                {
                    MATCH.DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Table touched.");
                    //UpdateCondition(ConditionTableTouchedButNoRag, true);
                    //UpdateConditionWithMatrix(ConditionTableTouchedButNoRag);
                    UpdateCondition(ConditionStandBy, false);
                    UpdateCondition(ConditionTableTouchedButNoRag, true);
                }

                void RegisterInferenceFarFromRag()
                {
                    DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Registering inference distance from rag with distance value: " + InferenceFarFromRagDistance);

                    InferenceManager.UnregisterInference(InferenceFarFromRag);

                    MATCH.Inferences.Factory.Instance.CreateDistanceLeavingInferenceOneShot(InferenceManager, InferenceFarFromRag, delegate (System.Object oo, EventArgs ee)
                    {
                        UpdateConditionWithMatrix(ConditionRagNotTakenButHelpReceived);
                    }, InteractionRag.gameObject, InferenceFarFromRagDistance);
                }
            }

            class DustingTableAssistances
            {
                public List<Assistances.GradationVisual.GradationVisual> Beta;
                public List<Assistances.GradationVisual.GradationVisual> Iota;
                public List<Assistances.GradationVisual.GradationVisual> Epsilon;
                public List<Assistances.GradationVisual.GradationVisual> Gamma;
                public List<Assistances.GradationVisual.GradationVisual> Delta;
                public List<Assistances.GradationVisual.GradationVisual> Alpha;
                public List<Assistances.GradationVisual.GradationVisual> Kappa;
                public List<Assistances.GradationVisual.GradationVisual> Xi;

                public DustingTableAssistances(Transform parentTable, Transform parentRag, Assistances.IAssistance toDoList, Transform toDoListAssistanceCanva, Transform parentHelpCalendar)
                {
                    Alpha = new List<Assistances.GradationVisual.GradationVisual>
                    {
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Alpha-00", "", "Avez-vous terminé votre activité?", false, "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, MATCH.Assistances.InteractionSurfaceFollower.Instance.transform),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2NoButton("DustingTable-Alpha-01", "", "En effet, vous avez terminé l'activité. Félicitations! Vous pouvez rendre le casque à la personne.", false, MATCH.Assistances.InteractionSurfaceFollower.Instance.transform),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Alpha-02", "", "Comment pouvez-vous savoir si vous avez terminé votre activité?", false, "Je sais", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, MATCH.Assistances.InteractionSurfaceFollower.Instance.transform),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Alpha-03", "", "Est-ce que vous vous rappelez du but de l'activité?", false, "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, MATCH.Assistances.InteractionSurfaceFollower.Instance.transform),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Alpha-04", "", "Vous deviez épousseter la table avec un chiffon. Qu'en pensez-vous?", false, "Oui c'est fait!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, MATCH.Assistances.InteractionSurfaceFollower.Instance.transform),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2NoButton("DustingTable-Alpha-05", "", "Oui c'est fait! Vous avez terminé l'activité!", false, MATCH.Assistances.InteractionSurfaceFollower.Instance.transform),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Alpha-06", "", "Parfait! Pensez-vousque vous avez atteint ce but?", false, "Oui!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, MATCH.Assistances.InteractionSurfaceFollower.Instance.transform)
                    };

                    Beta = new List<Assistances.GradationVisual.GradationVisual>
                    {
                    Assistances.GradationVisual.Factory.Instance.CreateExclamationMark("DustingTable-Beta-1", /*toDoList.GetAssistance().transform*/ parentHelpCalendar),
                    Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-2", "", "Avez-vous compris la tâche que vous devez réaliser ici?", "Oui!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, /*parentTable*/ /*toDoList.GetAssistance().transform*/ parentHelpCalendar),
                    Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-3", "", "Que pouvez-vous faire pour garder votre table propre?", "Je sais!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, /*parentTable*/ /*toDoList.GetAssistance().transform*/ parentHelpCalendar),
                    Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-4", "", "Que pouvez-vous faire pour nettoyer votre table?", "Je sais", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, /*parentTable*/ /*toDoList.GetAssistance().transform*/ parentHelpCalendar),
                    Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-4", "", "De quel objet avez-vous besoin pour nettoyer votre table?", "Je sais!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, /*parentTable*/ /*toDoList.GetAssistance().transform*/ parentHelpCalendar),
                    Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-6", "", "Vous devez nettoyer la table avec un chiffon", "Je sais où le trouver!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "J'ai besoin d'aide", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, /*parentTable*/ /*toDoList.GetAssistance().transform*/ parentHelpCalendar),
                    Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-7", "", "Où pouvez-vous regarder pour le trouver?", "Je sais!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, /*parentTable*/ /*toDoList.GetAssistance().transform*/ parentHelpCalendar),
                    Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtonsContextualized("DustingTable-Beta-8", "", "Avez-vous regardé <Location> ?", parentRag, "J'ai trouvé!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je n'ai pas trouvé", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, /*parentTable*/ /*toDoList.GetAssistance().transform*/ parentHelpCalendar),
                    Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-9", "", "Vous pouvez le trouver proche de vous dans cette pièce", "J'ai trouvé!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne l'ai pas trouvé", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, /*parentTable*/ /*toDoList.GetAssistance().transform*/ parentHelpCalendar),
                    //Assistances.GradationVisual.Factory.Instance.CreateArch("DustingTable-Beta-10", "Vous trouverez le chiffon au bout de cette flèche", parentTable, parentRag, parentTable),
                    Assistances.GradationVisual.Factory.Instance.CreateDialog2WithLinePath("DustingTable-Beta-10", "", "Vous trouverez le chiffon au bout de cette flèche", parentRag, parentHelpCalendar),
                    Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-11", "", "Ok! Nous vous laissons faire", "Ok!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, /*parentTable*/ /*toDoList.GetAssistance().transform*/ parentHelpCalendar),
                    Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-12", "", "Savez-vous comment réaliser cette activité?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, /*parentTable*/ /*toDoList.GetAssistance().transform*/ parentHelpCalendar)
                    };

                    Gamma = new List<Assistances.GradationVisual.GradationVisual>
                    {
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Gamma-00", "", "Que pouvez-vous faire avec le chiffon?", "Je sais!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Gamma-01", "", "Vous devez épousseter la table avec le chiffon jusqu'à que toute la surface de la table soit verte", "Ok!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, "J'ai besoin d'aide", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2NoButton("DustingTable-Gamma-02", "", "Je ne peux pas plus vous aider. Quelqu'un va venir vous voir", parentTable)
                    };

                    Delta = new List<Assistances.GradationVisual.GradationVisual>
                    {
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Delta-00", "", "Avez-vous fini votre activité?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Delta-01", "", "Savez-vous ce que vous devez faire pour réaliser votre activité?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Delta-02", "", "Vous devez épousseter la table avec un chiffon jusqu'à ce que la surface soit entièrement verte. Vous êtes sur la bonne voie!", "Ok!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "J'ai besoin d'aide", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Delta-03", "", "Êtes-vous sûr d'avoir terminé votre activité?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Delta-04", "", "En réalité non. Vous devez épousseter la table avec le chiffon jusqu'à que la surface soit entièrement verte", "Ok!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, "J'ai besoin d'aide", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Delta-05", "", "Ok! Nous vous laissons faire", "Ok!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, parentTable)
                    };

                    Epsilon = new List<Assistances.GradationVisual.GradationVisual>
                    {
                        Assistances.GradationVisual.Factory.Instance.CreateExclamationMark("DustingTable-Epsilon-00", parentRag),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Epsilon-01", "", "L'objet dont vous avez besoin pour compléter votre activité est ici", "Ok!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne comprends pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentRag),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Epsilon-02", "", "Vous devez utiliser le chiffon sous ce message pour nettoyer la table", "Ok!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne comprends pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentRag),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Epsilon-03", "", "Ok! Nous vous laissons faire", "Ok!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, parentRag),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2NoButton("DustingTable-Epsilon-04", "", "Je ne peux pas plus vous aider. Quelqu'un va venir vous voir", parentRag)
                    };

                    Iota = new List<Assistances.GradationVisual.GradationVisual>
                    {
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Iota-01", "", "Êtes-vous sûr que vous utilisez le bon objet pour nettoyer la table?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne suis pas sûr", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Iota-02", "", "Êtes-vous sûr que vous utilisez le chiffon pour nettoyer la table?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne suis pas sûr", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Iota-03", "", "Ce n'est pas le bon chiffon. L'avez-vous cherché?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Iota-04", "", "Non en effet. Où pouvez-vous regarder pour le trouver?", "Je sais", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtonsContextualized("DustingTable-Iota-5", "", "Avez-vous regardé <Location> ?", parentRag, "J'ai trouvé!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je n'ai pas trouvé", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Iota-06", "", "Vous pouvez le trouver proche de vous dans cette pièce", "J'ai trouvé!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne l'ai pas trouvé", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, /*parentTable*/ /*toDoList.GetAssistance().transform*/ parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithLinePath("DustingTable-Iota-07", "", "Vous trouverez le chiffon au bout de cette flèche", parentRag, parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Iota-08", "", "Ok! Nous vous laissons faire", "Ok!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, /*parentTable*/ /*toDoList.GetAssistance().transform*/ parentTable)
                    };

                    Kappa = new List<Assistances.GradationVisual.GradationVisual>
                    {
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Kappa-00", "", "Savez-vous quelle activité vous avez à faire dans cette pièce?", false, "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, MATCH.Assistances.InteractionSurfaceFollower.Instance.transform),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Kappa-01", "", "Où pouvez-vous trouver des détails sur l'activité à faire?", false, "Je sais", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, MATCH.Assistances.InteractionSurfaceFollower.Instance.transform),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Kappa-02", "", "Avez-vous regardé sur les murs?", false, "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, MATCH.Assistances.InteractionSurfaceFollower.Instance.transform),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithLinePath("DustingTable-Kappa-03", "", "Vous trouverez les détails de l'activité au bout de la ligne", false, toDoList.GetAssistance().transform, MATCH.Assistances.InteractionSurfaceFollower.Instance.transform),
                        //Assistances.GradationVisual.Factory.Instance.CreateArch("DustingTable-Kappa-03", "Vous trouverez les détails de l'activité au bout de cette arche", MATCH.Assistances.InteractionSurfaceFollower.Instance.transform, parentToDoList, MATCH.Assistances.InteractionSurfaceFollower.Instance.transform),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Kappa-04", "", "Ok! Nous vous laissons faire", false, "Ok!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, MATCH.Assistances.InteractionSurfaceFollower.Instance.transform)
                    };

                    Xi = new List<Assistances.GradationVisual.GradationVisual>
                    {
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtonsWithoutIcon("DustingTable-Xi-00", "", "Avez-vous besoin d'aide?", false, "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, toDoListAssistanceCanva),
                        //Assistances.GradationVisual.Factory.Instance.CreateDialogTwoButtons("DustingTable-Xi-00", "", "Avez-vous besoin d'aide?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, toDoListAssistanceCanva),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtonsWithoutIcon("DustingTable-Xi-01", "", "Avez-vous compris la tâche que vous devez réaliser ici?", false, "Oui!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, toDoListAssistanceCanva),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtonsWithoutIcon("DustingTable-Xi-02", "", "Que pouvez-vous faire pour garder votre table propre?", false, "Je sais!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, toDoListAssistanceCanva),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtonsWithoutIcon("DustingTable-Xi-03", "", "Que pouvez-vous faire pour nettoyer votre table?", false, "Je sais", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, toDoListAssistanceCanva),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtonsWithoutIcon("DustingTable-Xi-04", "", "De quel objet avez-vous besoin pour nettoyer votre table?", false, "Je sais!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, toDoListAssistanceCanva),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtonsWithoutIcon("DustingTable-Xi-05", "", "Vous devez nettoyer la table avec un chiffon", false, "Je sais où le trouver!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "J'ai besoin d'aide", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, toDoListAssistanceCanva),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtonsWithoutIcon("DustingTable-Xi-06", "", "Où pouvez-vous regarder pour le trouver?", false, "Je sais!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, toDoListAssistanceCanva),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtonsWithoutIconContextualized("DustingTable-Xi-07", "", "Avez-vous regardé <Location> ?", false, parentRag, "J'ai trouvé!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je n'ai pas trouvé", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, toDoListAssistanceCanva),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtonsWithoutIcon("DustingTable-Xi-08", "", "Vous pouvez le trouver proche de vous dans cette pièce", false, "J'ai trouvé!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne l'ai pas trouvé", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, toDoListAssistanceCanva),
                        //Assistances.GradationVisual.Factory.Instance.CreateArch("DustingTable-Beta-10", "Vous trouverez le chiffon au bout de cette flèche", parentTable, parentRag, parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithoutIconWithLinePath("DustingTable-Xi-09", "", "Vous trouverez le chiffon au bout de cette flèche", false, parentRag, toDoListAssistanceCanva),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtonsWithoutIcon("DustingTable-Xi-10", "", "Ok! Nous vous laissons faire", false, "Ok!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, toDoListAssistanceCanva),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtonsWithoutIcon("DustingTable-Xi-11", "", "Savez-vous comment réaliser cette activité?", false, "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, toDoListAssistanceCanva)
                    };
                }

                public void ReusableComponentBeta06(ref Assistances.AssistanceGradationExplicit assistanceToAdd)
                {
                    assistanceToAdd.AddAssistance(Beta[6], Assistances.Buttons.Button.ButtonType.Yes, Beta[10]);
                    assistanceToAdd.AddAssistance(Beta[6], Assistances.Buttons.Button.ButtonType.No, Beta[7]);

                    ReusableComponentBeta07(ref assistanceToAdd);
                }

                public void ReusableComponentBeta07(ref Assistances.AssistanceGradationExplicit assistanceToAdd)
                {
                    assistanceToAdd.AddAssistance(Beta[7], Assistances.Buttons.Button.ButtonType.Yes, Beta[10]);
                    assistanceToAdd.AddAssistance(Beta[7], Assistances.Buttons.Button.ButtonType.No, Beta[8]);

                    assistanceToAdd.AddAssistance(Beta[8], Assistances.Buttons.Button.ButtonType.Yes, Beta[10]);
                    assistanceToAdd.AddAssistance(Beta[8], Assistances.Buttons.Button.ButtonType.No, Beta[9]);

                    assistanceToAdd.AddAssistance(Beta[9], Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    ReusableComponentBeta10(ref assistanceToAdd);

                }

                public void ReusableComponentBeta10(ref Assistances.AssistanceGradationExplicit assistanceToAdd)
                {
                    assistanceToAdd.AddAssistance(Beta[10], Assistances.Buttons.Button.ButtonType.ClosingButton, null);
                }

                public void ReusableComponentGamma01(ref Assistances.AssistanceGradationExplicit assistanceToAdd)
                {
                    assistanceToAdd.AddAssistance(Gamma[1], Assistances.Buttons.Button.ButtonType.ClosingButton, null);
                    assistanceToAdd.AddAssistance(Gamma[1], Assistances.Buttons.Button.ButtonType.No, Gamma[2]);

                    ReusableComponentGamma02(ref assistanceToAdd);
                }

                public void ReusableComponentGamma02(ref Assistances.AssistanceGradationExplicit assistancetoAdd)
                {
                    assistancetoAdd.AddAssistance(Gamma[2], Assistances.Buttons.Button.ButtonType.ClosingButton, null);
                }
            }
        }
    }
}
