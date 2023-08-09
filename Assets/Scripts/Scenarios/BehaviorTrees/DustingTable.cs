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

                string InferenceDidNotStartDusting = "DidNotStartCleaning";
                string InferenceInterruptDusting = "InterruptedDusting";
                string InferenceFarFromRag = "FarFromRag";
                float InferenceFarFromRagDistance = 4.0f;

                string InferenceDidNotGoToCalendar = "DidNotGoToCalendar";
                string InferenceCloseToCalendar = "CloseToCalendar";

                // Interaction surface
                Assistances.InteractionSurface InteractionSurfaceTable;
                Assistances.InteractionSurface InteractionRag;

                public event EventHandler TableTouchedEvent;

                Dictionary<Assistances.AssistanceGradationExplicit, bool> AssistancesDusting;

                DustingTableAssistances AssistancesDB;

                //Transform ToDoListView;
                Assistances.IAssistance ToDoList;

                public override void Awake()
                {
                    base.Awake();
                    SetId("Nettoyer la table");
                    AssistancesDusting = new Dictionary<Assistances.AssistanceGradationExplicit, bool>();
                }

                public override void Start()
                {

                    Utilities.ToDoList toDoList = transform.root.Find("ToDoList").GetComponent<Utilities.ToDoList>();
                    ToDoList = toDoList.GetAssistance();
                    //ToDoList = toDoListView.GetComponent<Assistances.Dialogs.Dialog1>();

                    Scenarios.Manager.Instance.addScenario(this);

                    // Initialize assistances
                    InitializeAssistances();
                    AssistancesDB = new DustingTableAssistances(InteractionSurfaceTable.transform, InteractionRag.transform, ToDoList);

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
                        
                        if (InferenceManager.GetInference(InferenceFarFromRag)!=null)
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

                    int nbConditions = GetNumberOfConditions();

                    AddConditionsUpdate(ConditionTableCleaned, new bool[]                           { true,  false, false, false, false, false, false, false, false, false });
                    AddConditionsUpdate(ConditionRagNotTakenButHelpReceived, new bool[]             { false, true,  false, false, false, false, false, false, false, false });
                    AddConditionsUpdate(ConditionRagTaken, new bool[]                               { false, false, true,  false, false, false, false, false, false, false });
                    AddConditionsUpdate(ConditionDidNotStartCleaning, new bool[]                    { false, false, true,  true,  false, false, false, false, false, false });
                    AddConditionsUpdate(ConditionCleaningInterrupted, new bool[]                    { false, false, true,  false, true,  false, false, false, false, false });
                    AddConditionsUpdate(ConditionNewPartCleaned, new bool[]                         { false, false, true,  false, false, true,  false, false, false, false });
                    AddConditionsUpdate(ConditionProcessRelatedToNewPartsCleanedDone, new bool[]    { false, false, true,  false, false, false, true,  false, false, false });
                    AddConditionsUpdate(ConditionTableTouchedButNoRag, new bool[]                   { false, false, false, false, false, false, false, true,  false, false });
                    AddConditionsUpdate(ConditionDidNotGoToCalendar, new bool[]                     { false, false, false,  false, false, false, false, false, true, false  });
                    AddConditionsUpdate(ConditionStandBy, new bool[] { false, false, false, false, false, false, false, false, false, true });

                    // End of code generation using the EXCEL file

                    // Defining the BT
                    Selector srRagNotTaken = new Selector(
                        new BlackboardCondition(ConditionDidNotGoToCalendar, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceKappa()),
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

                void InitializeAssistances()
                {
                    InteractionSurfaceTable = Assistances.Factory.Instance.CreateInteractionSurface("DustingTable - Table", AdminMenu.Panels.Right, new Vector3(1.1f, 0.02f, 0.7f), new Vector3(-0.447f, -0.406f, 0.009f), Utilities.Materials.Colors.CyanGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);
                    //InteractionSurfaceTable.ShowInteractionSurfaceTable(false);

                    InteractionRag = Assistances.Factory.Instance.CreateInteractionSurface("DustingTable - Rag", AdminMenu.Panels.Right, new Vector3(0.1f, 0.1f, 0.1f), new Vector3(-1.378f, -0.364f, 2.743f), Utilities.Materials.Colors.OrangeGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);

                    InteractionRag.EventUserTouched += CallbackInteractionSurfaceRagTouched;

                    InteractionSurfaceTable.EventUserTouched += CallbackInteractionSurfaceTableTouched;
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
                    /*Assistances.GradationVisual.GradationVisual alpha1 = Assistances.Factory.Instance.CreateAssistanceGradationAttention("DustingTable-Beta-1");*/

                    /*Assistances.GradationVisual.GradationVisual alpha1 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Alpha-1", "", "Vous avez termin� l'activit�! F�licitations!", "Terminer", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, InteractionSurfaceTable.transform);*/

                    /*Assistances.Basic assistanceBase = Assistances.Factory.Instance.CreateCube(Utilities.Materials.Colors.PurpleGlowing, InteractionSurfaceTable.transform);
                    assistanceBase.name = name + "_base";
                    alpha1.AddAssistance(Assistances.Decorators.Factory.Instance.CreateMaterial(assistanceBase, Utilities.Materials.Textures.Congratulation));*/


                    Assistances.AssistanceGradationExplicit alpha = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("DustingTable-Alpha");
                    alpha.transform.parent = transform;

                    /*alpha.AddAssistance(alpha1, Assistances.Buttons.Button.ButtonType.ClosingButton, null);*/

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

                    alpha.Init();

                    Sequence temp = new Sequence(


                        new NPBehave.Action(() => {
                            ShowAssistanceHideOthers(alpha);
                            InferenceManager.UnregisterAllInferences();
                            //UpdateTextAssistancesDebugWindow("Alpha");
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
                             Inferences.Timer infCalendarTimer = new Inferences.Timer(InferenceDidNotGoToCalendar, 15, delegate (System.Object oo, EventArgs ee)
                             {
                                 UpdateConditionWithMatrix(ConditionDidNotGoToCalendar);
                                 InferenceManager.UnregisterInference(InferenceInterruptDusting);
                             });

                            InferenceManager.RegisterInference(infCalendarTimer);

                             infCalendarTimer.StartCounter();

                             

                             Inferences.DistanceComing distanceComing = new Inferences.DistanceComing(InferenceCloseToCalendar, delegate (System.Object o, EventArgs e)
                             {
                                 infCalendarTimer.StopCounter();
                                 InferenceManager.UnregisterInference(InferenceDidNotGoToCalendar);
                                 InferenceManager.UnregisterInference(InferenceCloseToCalendar);
                                 SetConditionsTo(false);

                             }, ToDoList.GetAssistance().gameObject, 2.0f);

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
                    //assistanceBeta.InfManager = InferenceManager;

                    /*assistanceBeta.AddAssistance(AssistancesDB.Beta01, Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Beta02);
                    assistanceBeta.AddAssistance(AssistancesDB.Beta01, Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Beta11);

                    assistanceBeta.AddAssistance(AssistancesDB.Beta02, Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Beta11);
                    assistanceBeta.AddAssistance(AssistancesDB.Beta02, Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Beta03);

                    assistanceBeta.AddAssistance(AssistancesDB.Beta03, Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Beta11);
                    assistanceBeta.AddAssistance(AssistancesDB.Beta03, Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Beta04);

                    assistanceBeta.AddAssistance(AssistancesDB.Beta04, Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Beta11);
                    assistanceBeta.AddAssistance(AssistancesDB.Beta04, Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Beta05);

                    assistanceBeta.AddAssistance(AssistancesDB.Beta05, Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Beta11);
                    assistanceBeta.AddAssistance(AssistancesDB.Beta05, Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Beta06);

                    assistanceBeta.AddAssistance(AssistancesDB.Beta06, Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Beta11);
                    assistanceBeta.AddAssistance(AssistancesDB.Beta06, Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Beta07);*/

                    assistanceBeta.AddAssistance(AssistancesDB.Beta[0], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Beta[1]);
                    assistanceBeta.AddAssistance(AssistancesDB.Beta[0], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Beta[10]);

                    assistanceBeta.AddAssistance(AssistancesDB.Beta[1], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Beta[10]);
                    assistanceBeta.AddAssistance(AssistancesDB.Beta[1], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Beta[2]);

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
                        UpdateConditionWithMatrix(ConditionRagNotTakenButHelpReceived);
                    };

                    Assistances.IAssistance beta10IAssistance = ((Assistances.IAssistance)AssistancesDB.Beta[10].GetCurrentAssistance());
                    Assistances.Dialogs.Dialog2 beta10Dialog2 = ((Assistances.Dialogs.Dialog2)beta10IAssistance.GetRootDecoratedAssistance());
                    /*beta10Dialog2.ButtonsController.Last().EventButtonClicked += delegate (System.Object o, EventArgs e)
                    {
                        RegisterInferenceFarFromRag();
                    };*/
                    beta10Dialog2.EventIsShown += delegate (System.Object o, EventArgs e)
                    {
                        RegisterInferenceFarFromRag();
                    };

                    /*assistanceBeta.AddAssistance(AssistancesDB.Beta07, Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Beta11);
                    assistanceBeta.AddAssistance(AssistancesDB.Beta07, Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Beta08);

                    assistanceBeta.AddAssistance(AssistancesDB.Beta08, Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Beta11);
                    assistanceBeta.AddAssistance(AssistancesDB.Beta08, Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Beta09);

                    assistanceBeta.AddAssistance(AssistancesDB.Beta09, Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Beta11);
                    assistanceBeta.AddAssistance(AssistancesDB.Beta09, Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Beta10);

                    assistanceBeta.AddAssistance(AssistancesDB.Beta10, Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    assistanceBeta.AddAssistance(AssistancesDB.Beta11, Assistances.Buttons.Button.ButtonType.ClosingButton, null);*/

                    AssistancesDusting.Add(assistanceBeta, false);

                    assistanceBeta.Init();


                    // For assistance beta
                    /*Assistances.GradationVisual.GradationVisual beta1 = Assistances.GradationVisual.Factory.Instance.CreateExclamationMark("DustingTable-Beta-1", InteractionSurfaceTable.transform);

                    Assistances.GradationVisual.GradationVisual beta2 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-2", "", "Il y a une activit� � faire ici", "Je sais!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);
                    
                    Assistances.GradationVisual.GradationVisual beta3 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-3", "", "Vous devez nettoyer la table", "Ok!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "J'ai besoin d'aide", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);
                    
                    Assistances.GradationVisual.GradationVisual beta4 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-4", "", "Savez-vous de quel objet vous avez besoin pour nettoyer la table?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);

                    Assistances.GradationVisual.GradationVisual beta5 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-5", "", "Vous devez utiliser le chiffon. Savez-vous o� il est?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);

                    Assistances.GradationVisual.GradationVisual beta6 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-6", "", "O� le trouvez-vous habituellement?", "Je sais!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);

                    Assistances.GradationVisual.GradationVisual beta7 = Assistances.GradationVisual.Factory.Instance.CreateArch("DustingTable-Beta-7", "Vous trouverez le chiffon au bout de cette arche", InteractionSurfaceTable.transform, InteractionRag.transform, InteractionSurfaceTable.transform);

                    Assistances.GradationVisual.GradationVisual beta8 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-8", "", "Parfait! Nous vous laissons faire.", "Ok!", delegate(System.Object o, EventArgs e)
                    {
                        RegisterInferenceFarFromRag();
                    }, Assistances.Buttons.Button.ButtonType.Yes, InteractionSurfaceTable.transform);

                    Assistances.GradationVisual.GradationVisual beta9 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-5", "", "Parfait! Savez-vous o� il est?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);

                    Assistances.AssistanceGradationExplicit assistanceBeta = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("DustingTable-Beta");

                    assistanceBeta.transform.parent = transform;
                    //assistanceBeta.InfManager = InferenceManager;

                    assistanceBeta.AddAssistance(beta1, Assistances.Buttons.Button.ButtonType.Yes, beta2);
                    assistanceBeta.AddAssistance(beta1, Assistances.Buttons.Button.ButtonType.No, beta8);

                    assistanceBeta.AddAssistance(beta2, Assistances.Buttons.Button.ButtonType.No, beta3);
                    assistanceBeta.AddAssistance(beta2, Assistances.Buttons.Button.ButtonType.Yes, beta8);

                    assistanceBeta.AddAssistance(beta3, Assistances.Buttons.Button.ButtonType.No, beta4);
                    assistanceBeta.AddAssistance(beta3, Assistances.Buttons.Button.ButtonType.Yes, beta8);

                    assistanceBeta.AddAssistance(beta4, Assistances.Buttons.Button.ButtonType.Yes, beta9);
                    assistanceBeta.AddAssistance(beta4, Assistances.Buttons.Button.ButtonType.No, beta5);

                    assistanceBeta.AddAssistance(beta9, Assistances.Buttons.Button.ButtonType.Yes, beta8);
                    assistanceBeta.AddAssistance(beta9, Assistances.Buttons.Button.ButtonType.No, beta6);

                    assistanceBeta.AddAssistance(beta5, Assistances.Buttons.Button.ButtonType.Yes, beta8);
                    assistanceBeta.AddAssistance(beta5, Assistances.Buttons.Button.ButtonType.No, beta6);

                    assistanceBeta.AddAssistance(beta6, Assistances.Buttons.Button.ButtonType.Yes, beta8);
                    assistanceBeta.AddAssistance(beta6, Assistances.Buttons.Button.ButtonType.No, beta7);

                    assistanceBeta.AddAssistance(beta8, Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesDusting.Add(assistanceBeta, false);

                    assistanceBeta.Init();*/

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            OnChallengeStart();
                            ShowAssistanceHideOthers(assistanceBeta); //assistanceBeta.RunAssistance();
                            UpdateTextAssistancesDebugWindow("Beta");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Beta");
                            
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

                    //epsilon.AddAssistance(AssistancesDB.Gamma[2], Assistances.Buttons.Button.ButtonType.ClosingButton, null);
                    //AssistancesDB.ReusableComponentGamma02(ref epsilon);

                    //AssistancesDB.ReusableComponentBeta10(ref epsilon);

                    epsilon.AddAssistance(AssistancesDB.Epsilon[3], Assistances.Buttons.Button.ButtonType.ClosingButton, null);
                    epsilon.AddAssistance(AssistancesDB.Epsilon[4], Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    Assistances.IAssistance epsilon3IAssistance = ((Assistances.IAssistance)AssistancesDB.Epsilon[3].GetCurrentAssistance());
                    Assistances.Dialogs.Dialog2 epsilon3Dialog2 = ((Assistances.Dialogs.Dialog2)epsilon3IAssistance.GetRootDecoratedAssistance());
                    /*epsilon3Dialog2.ButtonsController.Last().EventButtonClicked += delegate (System.Object o, EventArgs e)
                    {
                        epsilon.StopAssistance();
                        RegisterInferenceFarFromRag();
                    };*/
                    epsilon3Dialog2.EventIsShown += delegate (System.Object o, EventArgs e)
                    {
                        epsilon.StopAssistance();
                        RegisterInferenceFarFromRag();
                    };

                    AssistancesDusting.Add(epsilon, false);

                    epsilon.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() =>
                        {
                            //ShowAssistanceHideOthers(epsilon);
                            InferenceManager.UnregisterInference(InferenceFarFromRag);
                            epsilon.StopAssistance();
                            epsilon.RunAssistance();
                            //UpdateTextAssistancesDebugWindow("Epsilon");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Epsilon");
                        
                        }),
                        new WaitUntilStopped());

                    return temp;
                }
                    
                Sequence AssistanceGamma()
                {
                    /*Assistances.GradationVisual.GradationVisual gamma1 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Gamma-1", "", "Vous devez commencer � nettoyer la table avec le chiffon", "Ok", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne comprends pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);
                    Assistances.GradationVisual.GradationVisual gamma2 = Assistances.GradationVisual.Factory.Instance.CreateAlreadyConfigured(Assistances.GradationVisual.Factory.AlreadyConfigured.LetGoDialog2, "DustingTable-Gamma2", InteractionSurfaceTable.transform);
                    Assistances.GradationVisual.GradationVisual gamma3 = Assistances.GradationVisual.Factory.Instance.CreateAlreadyConfigured(Assistances.GradationVisual.Factory.AlreadyConfigured.SomeoneComingToHelpDialog2, "DustingTable-Gamma-3", InteractionSurfaceTable.transform);

                    Assistances.AssistanceGradationExplicit gamma = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("DustingTable-Gamma");
                    gamma.transform.parent = transform;
                    //alpha.InfManager = InferenceManager;


                    gamma.AddAssistance(gamma1, Assistances.Buttons.Button.ButtonType.Yes, gamma2);
                    gamma.AddAssistance(gamma1, Assistances.Buttons.Button.ButtonType.No, gamma3);
                    gamma.AddAssistance(gamma2, Assistances.Buttons.Button.ButtonType.ClosingButton, null);
                    gamma.AddAssistance(gamma3, Assistances.Buttons.Button.ButtonType.ClosingButton, null);*/

                    Assistances.AssistanceGradationExplicit gamma = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("DustingTable-Gamma");

                    gamma.AddAssistance(AssistancesDB.Gamma[0], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Delta[5]);
                    gamma.AddAssistance(AssistancesDB.Gamma[0], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Gamma[1]);
                    //AssistancesDB.ReusableComponentBeta10(ref gamma);

                    gamma.AddAssistance(AssistancesDB.Delta[5], Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesDB.ReusableComponentGamma01(ref gamma);

                    AssistancesDusting.Add(gamma, false);

                    gamma.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
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
                        new NPBehave.Action(() => {
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
                    /*Assistances.GradationVisual.GradationVisual delta1 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Delta-1", "", "Avez-vous fini de nettoyer la table?", "Oui!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);
                    Assistances.GradationVisual.GradationVisual delta2 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Delta-2", "", "�tes-vous s�r?", "Oui!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non ...", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);
                    Assistances.GradationVisual.GradationVisual delta3 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Delta-3", "", "En fait, la table n'est pas enti�rement d�poussi�r�e. Avez-vous besoin d'aide pour continuer � effectuer cette t�che ?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);
                    Assistances.GradationVisual.GradationVisual delta4 = Assistances.GradationVisual.Factory.Instance.CreateAlreadyConfigured(Assistances.GradationVisual.Factory.AlreadyConfigured.SomeoneComingToHelpDialog2, "DustingTable-Delta-4", InteractionSurfaceTable.transform);
                    Assistances.GradationVisual.GradationVisual delta5 = Assistances.GradationVisual.Factory.Instance.CreateAlreadyConfigured(Assistances.GradationVisual.Factory.AlreadyConfigured.LetGoDialog2, "DustingTable-Delta-5", InteractionSurfaceTable.transform);
                    Assistances.GradationVisual.GradationVisual delta6 = Assistances.GradationVisual.Factory.Instance.CreateAlreadyConfigured(Assistances.GradationVisual.Factory.AlreadyConfigured.DoYouNeedHelpDialog2, "DustingTable-Delta-6", InteractionSurfaceTable.transform);

                    Assistances.AssistanceGradationExplicit delta = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("DustingTable-Delta");
                    delta.transform.parent = transform;
                    delta.AddAssistance(delta1, Assistances.Buttons.Button.ButtonType.Yes, delta2);
                    delta.AddAssistance(delta1, Assistances.Buttons.Button.ButtonType.No, delta6);
                    delta.AddAssistance(delta2, Assistances.Buttons.Button.ButtonType.Yes, delta3);
                    delta.AddAssistance(delta2, Assistances.Buttons.Button.ButtonType.No, delta3);
                    delta.AddAssistance(delta3, Assistances.Buttons.Button.ButtonType.Yes, delta4);
                    delta.AddAssistance(delta3, Assistances.Buttons.Button.ButtonType.No, delta5);
                    delta.AddAssistance(delta6, Assistances.Buttons.Button.ButtonType.Yes, delta4);
                    delta.AddAssistance(delta6, Assistances.Buttons.Button.ButtonType.No, delta5);
                    delta.AddAssistance(delta4, Assistances.Buttons.Button.ButtonType.ClosingButton, null);
                    delta.AddAssistance(delta5, Assistances.Buttons.Button.ButtonType.ClosingButton, null);*/

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

                    delta.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            //ShowAssistanceHideOthers(delta);
                            delta.RunAssistance();
                            AssistancesDusting[delta] = true;
                            //UpdateTextAssistancesDebugWindow("Delta");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Delta");
                        }),
                        new WaitUntilStopped());

                    return temp;
                }

                Sequence AssistanceEta()
                {
                    Assistances.AssistanceGradationExplicit eta = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("Dusting - Eta");
                    eta.transform.parent = transform;
                    //eta.InfManager = InferenceManager;

                    Assistances.GradationVisual.GradationVisual eta1 = Assistances.GradationVisual.Factory.Instance.CreateSurfaceToProcess("DustingTable-Eta-1", delegate(System.Object o, EventArgs e)
                    {
                        ShowAssistanceHideOthers(eta);
                        UpdateConditionWithMatrix(ConditionNewPartCleaned);
                    }, delegate(System.Object o, EventArgs e)
                    {
                        UpdateConditionWithMatrix(ConditionTableCleaned);
                    }, InteractionSurfaceTable, InteractionSurfaceTable.transform);
                    //eta1


                    /*Assistances.GradationVisual.GradationVisual beta2 = Assistances.GradationVisual.Factory.Instance.CreateDialogTwoButtons("DustingTable-Gamma-2", "", "Il y a une activit� � faire ici", "Je sais!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);*/

                    eta.AddAssistance(eta1, Assistances.Buttons.Button.ButtonType.Undefined, null);

                    AssistancesDusting.Add(eta, false);

                    eta.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() =>
                        {
                            ShowAssistanceHideOthers(eta);
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

                    AssistancesDusting.Add(kappa, false);

                    kappa.Init();


                    Sequence temp = new Sequence(
                         new NPBehave.Action(() =>
                         {
                             ShowAssistanceHideOthers(kappa);
                             kappa.StopAssistance();
                             kappa.RunAssistance();


                             MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Kappa triggered.");
                         }),
                         new WaitUntilStopped()
                        );

                    return temp;
                }

                Sequence AssistanceIota()
                {
                    /*Assistances.GradationVisual.GradationVisual iota1 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Iota-1", "", "Vous devez d'abord prendre un chiffon pour nettoyer la table.", "Je sais!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);
                    Assistances.GradationVisual.GradationVisual iota2 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Iota-3", "", "O� trouvez-vous le chiffon habituellement?", "Je sais!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);
                    Assistances.GradationVisual.GradationVisual iota3 = Assistances.GradationVisual.Factory.Instance.CreateArch("DustingTable-Iota-4", "Vous trouverez le chiffon au bout de cette arche", InteractionSurfaceTable.transform, InteractionRag.transform, InteractionSurfaceTable.transform);
                    Assistances.GradationVisual.GradationVisual iota4 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Iota-5", "", "Parfait! Nous vous laissons faire.", "Ok!", delegate (System.Object o, EventArgs e)
                    {
                        RegisterInferenceFarFromRag();
                    }, Assistances.Buttons.Button.ButtonType.Yes, InteractionSurfaceTable.transform);


                    Assistances.AssistanceGradationExplicit iota = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("Dusting - Iota");
                    iota.transform.parent = transform;
                    iota.AddAssistance(iota1, Assistances.Buttons.Button.ButtonType.Yes, iota4);
                    iota.AddAssistance(iota1, Assistances.Buttons.Button.ButtonType.No, iota2);
                    iota.AddAssistance(iota2, Assistances.Buttons.Button.ButtonType.Yes, iota4);
                    iota.AddAssistance(iota2, Assistances.Buttons.Button.ButtonType.No, iota3);

                    iota.Init();*/

                    Assistances.AssistanceGradationExplicit iota = Assistances.Factory.Instance.CreateAssistanceGradationExplicit("Dusting_Iota");
                    iota.transform.parent = transform;
                    iota.AddAssistance(AssistancesDB.Iota[0], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Iota[1]);
                    iota.AddAssistance(AssistancesDB.Iota[0], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Iota[3]);

                    iota.AddAssistance(AssistancesDB.Iota[1], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Iota[2]);
                    iota.AddAssistance(AssistancesDB.Iota[1], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Iota[3]);

                    iota.AddAssistance(AssistancesDB.Iota[2], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Beta[7]);
                    iota.AddAssistance(AssistancesDB.Iota[2], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Beta[7]);

                    /*iota.AddAssistance(AssistancesDB.Beta07, Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Beta11);
                    iota.AddAssistance(AssistancesDB.Beta07, Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Beta08);

                    iota.AddAssistance(AssistancesDB.Beta08, Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Beta11);
                    iota.AddAssistance(AssistancesDB.Beta08, Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Beta09);

                    iota.AddAssistance(AssistancesDB.Beta09, Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Beta11);
                    iota.AddAssistance(AssistancesDB.Beta09, Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Beta10);*/

                    AssistancesDB.ReusableComponentBeta07(ref iota);

                    iota.AddAssistance(AssistancesDB.Iota[3], Assistances.Buttons.Button.ButtonType.Yes, AssistancesDB.Beta[10]);
                    iota.AddAssistance(AssistancesDB.Iota[3], Assistances.Buttons.Button.ButtonType.No, AssistancesDB.Beta[7]);

                    //AssistancesDB.ReusableComponentBeta10(ref iota);

                    AssistancesDusting.Add(iota, false);

                    iota.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            ShowAssistanceHideOthers(iota);
                            iota.StopAssistance();
                            iota.RunAssistance();
                            AssistancesDusting[iota] = true;
                            //UpdateTextAssistancesDebugWindow("Iota");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Iota");
                            MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Iota triggered.");

                        }),
                        new WaitUntilStopped()
                        );
                    return temp;
                }


                void CallbackInteractionSurfaceRagTouched(System.Object o, EventArgs e)
                {
                    InferenceManager.UnregisterInference(InferenceFarFromRag);
                    UpdateConditionWithMatrix(ConditionRagTaken);
                }

                public void CallbackInteractionSurfaceTableTouched(System.Object o, EventArgs e)
                {
                        MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Table touched.");
                        UpdateCondition(ConditionTableTouchedButNoRag, true);
                }

                void RegisterInferenceFarFromRag()
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Registering inference distance from rag with distance value: " + InferenceFarFromRagDistance);

                    InferenceManager.UnregisterInference(InferenceFarFromRag);

                    MATCH.Inferences.Factory.Instance.CreateDistanceLeavingInferenceOneShot(InferenceManager, InferenceFarFromRag, delegate (System.Object oo, EventArgs ee)
                    {
                        UpdateConditionWithMatrix(ConditionRagNotTakenButHelpReceived);
                    }, InteractionRag.gameObject, InferenceFarFromRagDistance);
                }
            }

            class DustingTableAssistances
            {
                /*public Assistances.GradationVisual.GradationVisual Beta01;
                public Assistances.GradationVisual.GradationVisual Beta02;
                public Assistances.GradationVisual.GradationVisual Beta03;
                public Assistances.GradationVisual.GradationVisual Beta04;
                public Assistances.GradationVisual.GradationVisual Beta05;
                public Assistances.GradationVisual.GradationVisual Beta06;
                public Assistances.GradationVisual.GradationVisual Beta07;
                public Assistances.GradationVisual.GradationVisual Beta08;
                public Assistances.GradationVisual.GradationVisual Beta09;
                public Assistances.GradationVisual.GradationVisual Beta10;
                public Assistances.GradationVisual.GradationVisual Beta11;*/

                public List<Assistances.GradationVisual.GradationVisual> Beta;
                public List<Assistances.GradationVisual.GradationVisual> Iota;
                public List<Assistances.GradationVisual.GradationVisual> Epsilon;
                //public List<Assistances.GradationVisual.GradationVisual> Eta;
                public List<Assistances.GradationVisual.GradationVisual> Gamma;
                public List<Assistances.GradationVisual.GradationVisual> Delta;
                public List<Assistances.GradationVisual.GradationVisual> Alpha;
                public List<Assistances.GradationVisual.GradationVisual> Kappa;

                public DustingTableAssistances(Transform parentTable, Transform parentRag, Assistances.IAssistance toDoList)
                {
                    Alpha = new List<Assistances.GradationVisual.GradationVisual>
                    {
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Alpha-00", "", "Avez-vous termin� votre activit�?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, MATCH.Assistances.InteractionSurfaceFollower.Instance.transform),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2NoButton("DustingTable-Alpha-01", "", "En effet, vous avez termin� l'activit�. F�licitations!", MATCH.Assistances.InteractionSurfaceFollower.Instance.transform),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Alpha-02", "", "Comment pouvez-vous savoir si vous avez termin� votre activit�?", "Je sais", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, MATCH.Assistances.InteractionSurfaceFollower.Instance.transform),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Alpha-03", "", "Est-ce que vous vous rappelez du but de l'activit�?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, MATCH.Assistances.InteractionSurfaceFollower.Instance.transform),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Alpha-04", "", "Vous deviez �pousseter la table avec un chiffon. Qu'en pensez-vous?", "Oui c'est fait!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, MATCH.Assistances.InteractionSurfaceFollower.Instance.transform),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2NoButton("DustingTable-Alpha-05", "", "Oui c'est fait! Vous avez termin� l'activit�!", MATCH.Assistances.InteractionSurfaceFollower.Instance.transform),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Alpha-06", "", "Parfait! Pensez-vousque vous avez atteint ce but?", "Oui!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, MATCH.Assistances.InteractionSurfaceFollower.Instance.transform)
                    };

                    Beta = new List<Assistances.GradationVisual.GradationVisual>
                    {
                    Assistances.GradationVisual.Factory.Instance.CreateExclamationMark("DustingTable-Beta-1", /*parentTable*/ toDoList.GetAssistance().transform),
                    Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-2", "", "Que pouvez-vous faire ici?", "Je sais!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, /*parentTable*/ toDoList.GetAssistance().transform),
                    Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-3", "", "Que pouvez-vous faire pour garder votre table propre?", "Je sais!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, /*parentTable*/ toDoList.GetAssistance().transform),
                    Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-4", "", "Que pouvez-vous faire pour nettoyer votre table?", "Je sais", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, /*parentTable*/ toDoList.GetAssistance().transform),
                    Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-4", "", "De quel objet avez-vous besoin pour nettoyer votre table?", "Je sais!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, /*parentTable*/ toDoList.GetAssistance().transform),
                    Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-6", "", "Vous devez nettoyer la table avec un chiffon", "Je sais o� le trouver!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "J'ai besoin d'aide", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, /*parentTable*/ toDoList.GetAssistance().transform),
                    Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-7", "", "O� pouvez-vous regarder pour le trouver?", "Je sais!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, /*parentTable*/ toDoList.GetAssistance().transform),
                    Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtonsContextualized("DustingTable-Beta-8", "", "Avez-vous regard� <Location> ?", parentRag, "J'ai trouv�!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je n'ai pas trouv�", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, /*parentTable*/ toDoList.GetAssistance().transform),
                    Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-9", "", "Vous pouvez le trouver proche de vous dans cette pi�ce", "J'ai trouv�!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne l'ai pas trouv�", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, /*parentTable*/ toDoList.GetAssistance().transform),
                    Assistances.GradationVisual.Factory.Instance.CreateArch("DustingTable-Beta-10", "Vous trouverez le chiffon au bout de cette fl�che", parentTable, parentRag, parentTable),
                    Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-11", "", "Ok! Nous vous laissons faire", "Ok!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, /*parentTable*/ toDoList.GetAssistance().transform)
                    };

                    Gamma = new List<Assistances.GradationVisual.GradationVisual>
                    {
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Gamma-00", "", "Que pouvez-vous faire avec le chiffon?", "Je sais!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Gamma-01", "", "Vous devez �pousseter la table avec le chiffon jusqu'� que toute la surface de la table soit verte", "Ok!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, "J'ai besoin d'aide", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2NoButton("DustingTable-Gamma-02", "", "Je ne peux pas plus vous aider. Quelqu'un va venir vous voir", parentTable)
                    };

                    Delta = new List<Assistances.GradationVisual.GradationVisual>
                    {
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Delta-00", "", "Avez-vous fini votre activit�?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Delta-01", "", "Savez-vous ce que vous devez faire pour r�aliser votre activit�?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Delta-02", "", "Vous devez �pousseter la table avec un chiffon jusqu'� ce que la surface soit enti�rement verte. Vous �tes sur la bonne voie!", "Ok!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "J'ai besoin d'aide", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Delta-03", "", "�tes-vous s�r d'avoir termin� votre activit�?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Delta-04", "", "En r�alit� non. Vous devez �pousseter la table avec le chiffon jusqu'� que la surface soit enti�rement verte", "Ok!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, "J'ai besoin d'aide", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Delta-05", "", "Ok! Nous vous laissons faire", "Ok!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, parentTable)
                    };

                    Epsilon = new List<Assistances.GradationVisual.GradationVisual>
                    {
                        Assistances.GradationVisual.Factory.Instance.CreateExclamationMark("DustingTable-Epsilon-00", parentRag),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Epsilon-01", "", "L'objet dont vous avez besoin pour compl�ter votre activit� est ici", "Ok!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne comprends pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentRag),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Epsilon-02", "", "Vous devez utiliser le chiffon sous ce message pour nettoyer la table", "Ok!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne comprends pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentRag),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Epsilon-03", "", "Ok! Nous vous laissons faire", "Ok!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, parentRag),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2NoButton("DustingTable-Epsilon-04", "", "Je ne peux pas plus vous aider. Quelqu'un va venir vous voir", parentRag)
                    };

                    Iota = new List<Assistances.GradationVisual.GradationVisual>
                    {
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Iota-01", "", "�tes-vous s�r que vous utilisez le bon objet pour nettoyer la table?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne suis pas s�r", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Iota-02", "", "�tes-vous s�r que vous utilisez le chiffon pour nettoyer la table?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne suis pas s�r", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Iota-03", "", "Ce n'est pas le bon chiffon. L'avez-vous cherch�?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentTable),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Iota-04", "", "Non en effet. O� pouvez-vous regarder pour le trouver?", "Je sais", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parentTable)
                    };

                    Kappa = new List<Assistances.GradationVisual.GradationVisual>
                    {
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Kappa-00", "", "Savez-vous quelle activit� vous avez � faire dans cette pi�ce?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, MATCH.Assistances.InteractionSurfaceFollower.Instance.transform),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Kappa-01", "", "O� pouvez-vous trouver des d�tails sur l'activit� � faire?", "Je sais", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, MATCH.Assistances.InteractionSurfaceFollower.Instance.transform),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Kappa-02", "", "Avez-vous regard� sur les murs?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, MATCH.Assistances.InteractionSurfaceFollower.Instance.transform),
                        //Assistances.GradationVisual.Factory.Instance.CreateArch("DustingTable-Kappa-03", "Vous trouverez les d�tails de l'activit� au bout de cette arche", MATCH.Assistances.InteractionSurfaceFollower.Instance.transform, parentToDoList, MATCH.Assistances.InteractionSurfaceFollower.Instance.transform),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithLinePath("DustingTable-Kappa-03", "", "Vous trouverez les d�tails de l'activit� au bout de la ligne", toDoList, MATCH.Assistances.InteractionSurfaceFollower.Instance.transform),
                        Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Kappa-04", "", "Ok! Nous vous laissons faire", "Ok!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, MATCH.Assistances.InteractionSurfaceFollower.Instance.transform)
                    };

                    

                    /*Eta = new List<Assistances.GradationVisual.GradationVisual>
                    {

                    };*/

                    

                    
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
