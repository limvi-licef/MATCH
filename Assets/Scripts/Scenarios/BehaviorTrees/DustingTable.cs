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

                string InferenceDidNotStartDusting = "DidNotStartCleaning";
                string InferenceInterruptDusting = "InterruptedDusting";
                string InferenceFarFromRag = "FarFromRag";
                float InferenceFarFromRagDistance = 4.0f;

                // Interaction surface
                Assistances.InteractionSurface InteractionSurfaceTable;
                Assistances.InteractionSurface InteractionRag;

                public event EventHandler TableTouchedEvent;

                Dictionary<Assistances.AssistanceGradationExplicit, bool> AssistancesDusting;

                public override void Awake()
                {
                    base.Awake();
                    SetId("Nettoyer la table");
                    AssistancesDusting = new Dictionary<Assistances.AssistanceGradationExplicit, bool>();
                }

                public override void Start()
                {
                    Scenarios.Manager.Instance.addScenario(this);

                    // Initialize assistances
                    InitializeAssistances();

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
                    int nbConditions = GetNumberOfConditions();

                    AddConditionsUpdate(ConditionTableCleaned, new bool[] { true, false, false, false, false, false, false, false });
                    AddConditionsUpdate(ConditionRagNotTakenButHelpReceived, new bool[] { false, true, false, false, false, false, false, false });
                    AddConditionsUpdate(ConditionRagTaken, new bool[] { false, false, true, false, false, false, false, false });
                    AddConditionsUpdate(ConditionDidNotStartCleaning, new bool[] { false, false, true, true, false, false, false, false });
                    AddConditionsUpdate(ConditionCleaningInterrupted, new bool[] { false, false, true, false, true, false, false, false });
                    AddConditionsUpdate(ConditionNewPartCleaned, new bool[] { false, false, true, false, false, true, false, false });
                    AddConditionsUpdate(ConditionProcessRelatedToNewPartsCleanedDone, new bool[] { false, false, true, false, false, false, true, false });
                    AddConditionsUpdate(ConditionTableTouchedButNoRag, new bool[] { false, false, false, false, false, false, false, true });

                    // End of code generation using the EXCEL file

                    // Defining the BT
                    Selector srRagNotTaken = new Selector(
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

                Sequence AssistanceAlpha()
                {
                    /*Assistances.GradationVisual.GradationVisual alpha1 = Assistances.Factory.Instance.CreateAssistanceGradationAttention("DustingTable-Beta-1");*/

                    Assistances.GradationVisual.GradationVisual alpha1 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Alpha-1", "", "Vous avez terminé l'activité! Félicitations!", "Terminer", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, InteractionSurfaceTable.transform);

                    /*Assistances.Basic assistanceBase = Assistances.Factory.Instance.CreateCube(Utilities.Materials.Colors.PurpleGlowing, InteractionSurfaceTable.transform);
                    assistanceBase.name = name + "_base";
                    alpha1.AddAssistance(Assistances.Decorators.Factory.Instance.CreateMaterial(assistanceBase, Utilities.Materials.Textures.Congratulation));*/


                    Assistances.AssistanceGradationExplicit alpha = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("DustingTable-Alpha");
                    alpha.transform.parent = transform;

                    alpha.AddAssistance(alpha1, Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesDusting.Add(alpha, false);

                    alpha.Init();

                    Sequence temp = new Sequence(


                        new NPBehave.Action(() => {
                            ShowAssistanceHideOthers(alpha);
                            InferenceManager.UnregisterAllInferences();
                            UpdateTextAssistancesDebugWindow("Alpha");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Alpha");
                            OnChallengeSuccess();
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                void InitializeAssistances()
                {
                    InteractionSurfaceTable = Assistances.Factory.Instance.CreateInteractionSurface("DustingTable - Table", AdminMenu.Panels.Right, new Vector3(1.1f, 0.02f, 0.7f), new Vector3(-0.447f, -0.406f, 0.009f), Utilities.Materials.Colors.CyanGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);
                    //InteractionSurfaceTable.ShowInteractionSurfaceTable(false);
                    
                    InteractionRag = Assistances.Factory.Instance.CreateInteractionSurface("DustingTable - Rag", AdminMenu.Panels.Right, new Vector3(0.1f, 0.1f, 0.1f), new Vector3(-1.378f, -0.364f, 2.743f), Utilities.Materials.Colors.OrangeGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);

                    InteractionRag.EventUserTouched += CallbackInteractionSurfaceRagTouched;

                    InteractionSurfaceTable.EventUserTouched += CallbackInteractionSurfaceTableTouched;


                }


                Sequence AssistanceBeta()
                {
                    // For assistance beta
                    /*Assistances.GradationVisual.GradationVisual beta1 = Assistances.GradationVisual.Factory.Instance.CreateExclamationMarkFromCube("DustingTable-Beta-1", InteractionSurfaceTable.transform);*/

                    Assistances.GradationVisual.GradationVisual beta1 = Assistances.GradationVisual.Factory.Instance.CreateExclamationMark("DustingTable-Beta-1", InteractionSurfaceTable.transform);

                    Assistances.GradationVisual.GradationVisual beta2 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-2", "", "Il y a une activité à faire ici", "Je sais!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);
                    
                    Assistances.GradationVisual.GradationVisual beta3 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-3", "", "Vous devez nettoyer la table", "Ok!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "J'ai besoin d'aide", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);
                    
                    Assistances.GradationVisual.GradationVisual beta4 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-4", "", "Savez-vous de quel objet vous avez besoin pour nettoyer la table?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);

                    Assistances.GradationVisual.GradationVisual beta5 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-5", "", "Vous devez utiliser le chiffon. Savez-vous où il est?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);

                    Assistances.GradationVisual.GradationVisual beta6 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-6", "", "Où le trouvez-vous habituellement?", "Je sais!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);

                    /*Assistances.GradationVisual.GradationVisual beta7 = Assistances.GradationVisual.Factory.Instance.CreateLightPath("DustingTable-Beta-7", InteractionSurfaceTable.transform);*/
                    Assistances.GradationVisual.GradationVisual beta7 = Assistances.GradationVisual.Factory.Instance.CreateArch("DustingTable-Beta-7", "Vous trouverez le chiffon au bout de cette arche", InteractionSurfaceTable.transform, InteractionRag.transform, InteractionSurfaceTable.transform);

                    Assistances.GradationVisual.GradationVisual beta8 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-8", "", "Parfait! Nous vous laissons faire.", "Ok!", delegate(System.Object o, EventArgs e)
                    {
                        RegisterInferenceFarFromRag();
                    }, Assistances.Buttons.Button.ButtonType.Yes, InteractionSurfaceTable.transform);

                    Assistances.GradationVisual.GradationVisual beta9 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Beta-5", "", "Parfait! Savez-vous où il est?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);

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

                    assistanceBeta.Init();

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
                        else if (AssistancesDusting[key])
                        {
                            key.StopAssistance();
                            AssistancesDusting[key] = false;
                        }
                    }
                }

                Sequence AssistanceEpsilon()
                {
                    Assistances.GradationVisual.GradationVisual epsilon1 = Assistances.GradationVisual.Factory.Instance.CreateExclamationMark("DustingTable-Epsilon1", InteractionRag.transform);
                    Assistances.GradationVisual.GradationVisual epsilon2 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Epsilon2", "", "Le chiffon est ici!", "Ok!", delegate(System.Object o, EventArgs e)
                    {
                        RegisterInferenceFarFromRag();
                    }, Assistances.Buttons.Button.ButtonType.ClosingButton, "Je ne comprends pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionRag.transform);
                    Assistances.GradationVisual.GradationVisual epsilon3 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Epsilon3", "", "Parfait! Nous vous laissons faire", "Ok!", delegate (System.Object o, EventArgs e)
                    {
                        RegisterInferenceFarFromRag();
                    }, Assistances.Buttons.Button.ButtonType.ClosingButton, InteractionRag.transform);
                    Assistances.GradationVisual.GradationVisual epsilon4 = Assistances.GradationVisual.Factory.Instance.CreateAlreadyConfigured(Assistances.GradationVisual.Factory.AlreadyConfigured.SomeoneComingToHelpDialog2, "DustingTable-Epsilon4", InteractionRag.transform);

                    Assistances.AssistanceGradationExplicit epsilon = Assistances.Factory.Instance.CreateAssistanceGradationExplicit("DustingTable-Epsilon");
                    epsilon.AddAssistance(epsilon1, Assistances.Buttons.Button.ButtonType.No, epsilon3);
                    epsilon.AddAssistance(epsilon1, Assistances.Buttons.Button.ButtonType.Yes, epsilon2);
                    epsilon.AddAssistance(epsilon2, Assistances.Buttons.Button.ButtonType.ClosingButton, null);
                    epsilon.AddAssistance(epsilon2, Assistances.Buttons.Button.ButtonType.No, epsilon4);
                    epsilon.AddAssistance(epsilon4, Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesDusting.Add(epsilon, false);

                    epsilon.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() =>
                        {
                            ShowAssistanceHideOthers(epsilon);
                            epsilon.RunAssistance();
                            UpdateTextAssistancesDebugWindow("Epsilon");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Epsilon");
                        
                        }),
                        new WaitUntilStopped());

                    return temp;
                }
                    
                Sequence AssistanceGamma()
                {
                    Assistances.GradationVisual.GradationVisual gamma1 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Gamma-1", "", "Vous devez commencer à nettoyer la table avec le chiffon", "Ok", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne comprends pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);
                    Assistances.GradationVisual.GradationVisual gamma2 = Assistances.GradationVisual.Factory.Instance.CreateAlreadyConfigured(Assistances.GradationVisual.Factory.AlreadyConfigured.LetGoDialog2, "DustingTable-Gamma2", InteractionSurfaceTable.transform);
                    Assistances.GradationVisual.GradationVisual gamma3 = Assistances.GradationVisual.Factory.Instance.CreateAlreadyConfigured(Assistances.GradationVisual.Factory.AlreadyConfigured.SomeoneComingToHelpDialog2, "DustingTable-Gamma-3", InteractionSurfaceTable.transform);

                    Assistances.AssistanceGradationExplicit gamma = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("DustingTable-Gamma");
                    gamma.transform.parent = transform;
                    //alpha.InfManager = InferenceManager;


                    gamma.AddAssistance(gamma1, Assistances.Buttons.Button.ButtonType.Yes, gamma2);
                    gamma.AddAssistance(gamma1, Assistances.Buttons.Button.ButtonType.No, gamma3);
                    gamma.AddAssistance(gamma2, Assistances.Buttons.Button.ButtonType.ClosingButton, null);
                    gamma.AddAssistance(gamma3, Assistances.Buttons.Button.ButtonType.ClosingButton, null);
                    

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
                            UpdateTextAssistancesDebugWindow("Zeta");
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
                            UpdateTextAssistancesDebugWindow("Theta");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Theta");
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                Sequence AssistanceDelta()
                {
                    Assistances.GradationVisual.GradationVisual delta1 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Delta-1", "", "Avez-vous fini de nettoyer la table?", "Oui!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);
                    Assistances.GradationVisual.GradationVisual delta2 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Delta-2", "", "Êtes-vous sûr?", "Oui!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non ...", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);
                    Assistances.GradationVisual.GradationVisual delta3 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Delta-3", "", "En fait, la table n'est pas entièrement dépoussiérée. Avez-vous besoin d'aide pour continuer à effectuer cette tâche ?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);
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
                    delta.AddAssistance(delta5, Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesDusting.Add(delta, false);

                    delta.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            //ShowAssistanceHideOthers(delta);
                            delta.RunAssistance();
                            AssistancesDusting[delta] = true;
                            UpdateTextAssistancesDebugWindow("Delta");
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


                    /*Assistances.GradationVisual.GradationVisual beta2 = Assistances.GradationVisual.Factory.Instance.CreateDialogTwoButtons("DustingTable-Gamma-2", "", "Il y a une activité à faire ici", "Je sais!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);*/

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


                Sequence AssistanceIota()
                {
                    Assistances.GradationVisual.GradationVisual iota1 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Iota-1", "", "Vous devez d'abord prendre un chiffon pour nettoyer la table.", "Je sais!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);
                    Assistances.GradationVisual.GradationVisual iota2 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("DustingTable-Iota-3", "", "Où trouvez-vous le chiffon habituellement?", "Je sais!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);
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

                    iota.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            ShowAssistanceHideOthers(iota);
                            iota.RunAssistance();
                            AssistancesDusting[iota] = true;
                            UpdateTextAssistancesDebugWindow("Iota");
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

        }
    }
}
