using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using Microsoft.MixedReality.Toolkit.Input;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;
using System.Reflection;
using System.Linq;
using NPBehave;

namespace MATCH
{
    namespace Scenarios
    {
        namespace BehaviorTrees
        {
            public class WateringThePlants : Scenarios.BehaviorTrees.BehaviorTree
            {
                private Inferences.Manager InferenceManager;
                public Assistances.Surfaces.Manager SurfacesManager;

                string ConditionBeginning = "Beginning";
                string ConditionBottleFilled = "BottleFilled";
                string ConditionHelpNeeded = "HelpNeeded";
                string ConditionHelpRequested = "HelpRequested";
                string ConditionHelpRequestedAgain = "HelpRequestedAgain";
                string ConditionOnePlantWatered = "OnePlantWatered";
                string ConditionAllPlantsWatered = "AllPlantsWatered";


                string InferenceFarFromRag = "FarFromRag";
                float InferenceFarFromRagDistance = 4.0f;
                string InferenceDidNotStartWatering = "DidNotStartWatering";
                string InferenceInterruptWatering = "InterruptedWatering";

                // Interaction surface
                Assistances.InteractionSurface InteractionSurfaceDialogs;
                Assistances.InteractionSurface InteractionRag;

                Assistances.InteractionSurface InteractionSink;
                Assistances.InteractionSurface InteractionPlant1;
                Assistances.InteractionSurface InteractionPlant2;
                Assistances.InteractionSurface InteractionPlant3;

                Dictionary<Assistances.AssistanceGradationExplicit, bool> AssistancesWatering;

                public override void Awake()
                {
                    base.Awake();
                    SetId("Arroser les plantes");
                    AssistancesWatering = new Dictionary<Assistances.AssistanceGradationExplicit, bool>();
                }

                public override void Start()
                {
                    // Initialize assistances
                    InitializeAssistances();

                    // Initialize inference manager
                    InferenceManager = MATCH.Inferences.Factory.Instance.CreateManager(transform);

                    // Call base function
                    base.Start();

                    Init();

                    // Initialize debug buttons
                    InitializeDebugButtons();
                }

                protected override Root InitializeBehaviorTree()
                {
                    // Making the conditions update.
                    // SEE EXCEL SHEET TO GENERATE THE CODE BELOW
                    AddCondition(ConditionBeginning, false);
                    AddCondition(ConditionBottleFilled, false);
                    AddCondition(ConditionHelpNeeded, false);
                    AddCondition(ConditionHelpRequested, false);
                    AddCondition(ConditionHelpRequestedAgain, false);
                    AddCondition(ConditionOnePlantWatered, false);
                    AddCondition(ConditionAllPlantsWatered, false);


                    int nbConditions = GetNumberOfConditions();

                    AddConditionsUpdate(ConditionBeginning, new bool[]          { true, false, false, false, false, false, false });
                    AddConditionsUpdate(ConditionBottleFilled, new bool[]       { false, true, false, false, false, false, false });
                    AddConditionsUpdate(ConditionHelpNeeded, new bool[]         { false, false, true, false, false, false, false });
                    AddConditionsUpdate(ConditionHelpRequested, new bool[]      { false, false, false, true, false, false, false });
                    AddConditionsUpdate(ConditionHelpRequestedAgain, new bool[] { false, false, false, false, true, false, false });
                    AddConditionsUpdate(ConditionOnePlantWatered, new bool[]    { false, false, false, false, false, true, false });
                    AddConditionsUpdate(ConditionAllPlantsWatered, new bool[]   { false, false, false, false, false, false, true });


                    // End of code generation using the EXCEL file

                    UpdateConditionWithMatrix(ConditionBeginning);

                    //Defining the BT
                    //Selector srBottleNotTaken = new Selector(
                    //    new BlackboardCondition(ConditionBeginning, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceBTWP7())
                    //    );

                    //Selector srPlantsNotWatered = new Selector(
                    //    new BlackboardCondition(ConditionBeginning, Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART, srBottleNotTaken),
                    //    new BlackboardCondition(ConditionHelpNeeded, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceBTWP4()),
                    //    new BlackboardCondition(ConditionOnePlantWatered, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceBTWP2()),
                    //    new BlackboardCondition(ConditionTwoPlantWatered, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceBTWP2()),
                    //    new BlackboardCondition(ConditionThreePlantWatered, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceBTWP2()),
                    //    new BlackboardCondition(ConditionHelpRequestedAgain, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceBTWP3()),
                    //    new WaitUntilStopped()
                    //    );

                    //Selector srBegin = new Selector(
                    //    new BlackboardCondition(ConditionAllPlantsWatered, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceBTWP1()));

                    Selector srBegin = new Selector(
                        new BlackboardCondition(ConditionBeginning, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceBTWP7()),
                        new BlackboardCondition(ConditionBottleFilled, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceEta()),
                        new BlackboardCondition(ConditionHelpNeeded, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceBTWP4()),
                        new BlackboardCondition(ConditionHelpRequested, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceBTWP6()),
                        new BlackboardCondition(ConditionHelpRequestedAgain, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceBTWP3()),
                        new BlackboardCondition(ConditionOnePlantWatered, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceBTWP2()),
                        new BlackboardCondition(ConditionAllPlantsWatered, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceBTWP1()));

                    Root tree = new Root(/*Conditions*/Getconditions(), srBegin);

                    return tree;
                }

                void InitializeDebugButtons()
                {
                    // Debug buttons to check if the BT has been correctly modeled
                    AdminMenu.Instance.AddButton("BT - Watering - Trigger - All plants watered", delegate
                    {
                        UpdateConditionWithMatrix(ConditionAllPlantsWatered);
                    }, AdminMenu.Panels.Right);

                    AdminMenu.Instance.AddButton("BT - Watering - Trigger - one plant watered", delegate
                    {
                        UpdateConditionWithMatrix(ConditionOnePlantWatered);
                    }, AdminMenu.Panels.Right);

                    AdminMenu.Instance.AddButton("BT - Watering - Trigger - help needed", delegate
                    {
                        UpdateConditionWithMatrix(ConditionHelpNeeded);
                    }, AdminMenu.Panels.Right);

                    AdminMenu.Instance.AddButton("BT - Watering - Trigger - help requested", delegate
                    {
                        UpdateConditionWithMatrix(ConditionHelpRequested);
                    }, AdminMenu.Panels.Right);

                    AdminMenu.Instance.AddButton("BT - Watering - Trigger - help requested again", delegate
                    {
                        UpdateConditionWithMatrix(ConditionHelpRequestedAgain);
                    }, AdminMenu.Panels.Right);
                    AdminMenu.Instance.AddButton("BT - Watering - Trigger - Bottle filled", delegate
                    {
                        UpdateConditionWithMatrix(ConditionBottleFilled);
                    }, AdminMenu.Panels.Right);
                    AdminMenu.Instance.AddButton("BT - Watering - Trigger - all false", delegate
                    {
                        UpdateConditionWithMatrix(ConditionAllPlantsWatered);
                        UpdateCondition(ConditionAllPlantsWatered, false);
                    }, AdminMenu.Panels.Right);

                }

                void InitializeAssistances()
                {
                    InteractionSurfaceDialogs = Assistances.Factory.Instance.CreateInteractionSurface("Practice-Dialogs", AdminMenu.Panels.Right, new Vector3(1.1f, 0.02f, 0.7f), new Vector3(-0.447f, -0.406f, 0.009f), Utilities.Materials.Colors.CyanGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);

                    //à modifier les interactions
                    InteractionSink = Assistances.Factory.Instance.CreateInteractionSurface("Practice-Sink", AdminMenu.Panels.Right, new Vector3(1.1f, 0.02f, 0.7f),
                        new Vector3(-4.777f, 0.659f, 2.031f), Utilities.Materials.Colors.GreenGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);

                    InteractionPlant1 = Assistances.Factory.Instance.CreateInteractionSurface("Practice-Plant1", AdminMenu.Panels.Right, new Vector3(0.3f, 0.5f, 0.3f),
                        new Vector3(-2.309f, 0.263f, 2.031f), Utilities.Materials.Colors.GreenGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);
                    InteractionPlant2 = Assistances.Factory.Instance.CreateInteractionSurface("Practice-Plant2", AdminMenu.Panels.Right, new Vector3(0.3f, 0.5f, 0.3f),
                        new Vector3(2f, 0f, 0f), Utilities.Materials.Colors.GreenGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);
                    InteractionPlant3 = Assistances.Factory.Instance.CreateInteractionSurface("Practice-Plant3", AdminMenu.Panels.Right, new Vector3(0.3f, 0.5f, 0.3f),
                         new Vector3(-7f, 0f, 0f), Utilities.Materials.Colors.GreenGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);

                    
                    InteractionSink.EventInteractionSurfaceTableTouched += CallbackInteractionSurfaceSinkTouched;
                    InteractionPlant1.EventInteractionSurfaceTableTouched += CallbackInteractionSurfacePlantWatered;
                    InteractionPlant2.EventInteractionSurfaceTableTouched += CallbackInteractionSurfacePlantWatered;
                    InteractionPlant3.EventInteractionSurfaceTableTouched += CallbackInteractionSurfacePlantWatered;

                }

                //Is it 7pm?
                Sequence AssistanceBTWP7()
                {
                    Assistances.GradationVisual.GradationVisual alpha1 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("Practice-Alpha-1", "",
                        "Qu'est-ce qu'il est conseilé de faire à la fin de la journée lorsqu'il ne fait moins chaud?", "Commencer !", delegate (System.Object o, EventArgs e)
                    {
                        UpdateConditionWithMatrix(ConditionBottleFilled);
                    }, Assistances.Buttons.Button.ButtonType.ClosingButton, InteractionSurfaceDialogs.transform);


                    Assistances.AssistanceGradationExplicit alpha = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("ArroserLesPlantes-Alpha");
                    alpha.transform.parent = transform;

                    alpha.AddAssistance(alpha1, Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesWatering.Add(alpha, false);

                    alpha.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            ShowAssistanceHideOthers(alpha);
                            UpdateTextAssistancesDebugWindow("Is it 7pm?");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "BTWP7");
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                Sequence AssistanceEta()
                {
                    Assistances.AssistanceGradationExplicit eta = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("Dusting - Eta");
                    eta.transform.parent = transform;
                    //eta.InfManager = InferenceManager;

                    Assistances.GradationVisual.GradationVisual eta1 = Assistances.GradationVisual.Factory.Instance.CreateSurfaceToProcess("DustingTable-Eta-1", delegate (System.Object o, EventArgs e)
                    {
                        ShowAssistanceHideOthers(eta);
                        UpdateConditionWithMatrix(ConditionOnePlantWatered);
                    }, delegate (System.Object o, EventArgs e)
                    {
                        UpdateConditionWithMatrix(ConditionAllPlantsWatered);
                    }, InteractionPlant1, InteractionPlant1.transform);
                    //eta1


                    /*Assistances.GradationVisual.GradationVisual beta2 = Assistances.GradationVisual.Factory.Instance.CreateDialogTwoButtons("DustingTable-Gamma-2", "", "Il y a une activité à faire ici", "Je sais!", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne sais pas", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, InteractionSurfaceTable.transform);*/

                    eta.AddAssistance(eta1, Assistances.Buttons.Button.ButtonType.Undefined, null);

                    AssistancesWatering.Add(eta, false);

                    eta.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() =>
                        {
                            ShowAssistanceHideOthers(eta);
                            OnChallengeStart();

                            UpdateTextAssistancesDebugWindow("Eta");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Eta");
                        })
                        );

                    return temp;
                }

                //Did the person fill the bottle to water the plants?
                Sequence AssistanceBTWP5()
                {
                    
                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            //ShowAssistanceHideOthers(btwp5);
        
                            UpdateTextAssistancesDebugWindow("Bottle Filled");

                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "BTWP5");
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                //Does the person come back to the faucet without having watered any plant?
                Sequence AssistanceBTWP4()
                {
                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            //ShowAssistanceHideOthers(alpha);
        
                            UpdateTextAssistancesDebugWindow("BTWP4");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "BTWP4");
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                //Does the person request some help?
                Sequence AssistanceBTWP6()
                {
                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            //ShowAssistanceHideOthers(alpha);
      
                            UpdateTextAssistancesDebugWindow("BTWP6");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "BTWP6");
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                //Did the person already request help to find the location of the plants?
                Sequence AssistanceBTWP3()
                {
                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            //ShowAssistanceHideOthers(alpha);
        
                            UpdateTextAssistancesDebugWindow("BTWP3");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "BTWP3");
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                //Did the person water one of the plants?
                Sequence AssistanceBTWP2()
                {
                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            //ShowAssistanceHideOthers(alpha);
        
                            UpdateTextAssistancesDebugWindow("One plant watered");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, " BTWP2 : One plant watered");
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                //Are all plants watered?
                Sequence AssistanceBTWP1()
                {
                    Assistances.GradationVisual.GradationVisual gradationVisual_BTWP1 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("Watering-BTWP-1", "",
                        "Vous avez terminé l'activité! Félicitations!", "Terminer", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, InteractionSurfaceDialogs.transform);


                    Assistances.AssistanceGradationExplicit gradationExplicit_BTWP1 = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("Watering-BTWP1");
                    gradationExplicit_BTWP1.transform.parent = transform;

                    gradationExplicit_BTWP1.AddAssistance(gradationVisual_BTWP1, Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesWatering.Add(gradationExplicit_BTWP1, false);

                    gradationExplicit_BTWP1.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            ShowAssistanceHideOthers(gradationExplicit_BTWP1);
                            InferenceManager.UnregisterAllInferences();
                            UpdateTextAssistancesDebugWindow("BTWP1");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "BTWP1");
                            OnChallengeSuccess();
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                void CallbackInteractionSurfaceSinkTouched(System.Object o, EventArgs e)
                {
                    UpdateConditionWithMatrix(ConditionBottleFilled);
                }

                void CallbackInteractionSurfacePlantWatered(System.Object o, EventArgs e)
                {
                    UpdateConditionWithMatrix(ConditionOnePlantWatered);
                }

                void ShowAssistanceHideOthers(Assistances.AssistanceGradationExplicit assistance)
                {
                    List<Assistances.AssistanceGradationExplicit> keys = new List<Assistances.AssistanceGradationExplicit>(AssistancesWatering.Keys);

                    foreach (Assistances.AssistanceGradationExplicit key in keys)
                    {
                        if (assistance == key)
                        {
                            if (AssistancesWatering[assistance] == false)
                            {
                                assistance.RunAssistance();
                                AssistancesWatering[assistance] = true;
                            }
                        }
                        else if (AssistancesWatering[key])
                        {
                            key.StopAssistance();
                            AssistancesWatering[key] = false;
                        }
                    }
                }

                //void CallbackInteractionSurfaceRagTouched(System.Object o, EventArgs e)
                //{
                //    InferenceManager.UnregisterInference(InferenceFarFromRag);
                //    UpdateConditionWithMatrix(ConditionBeginning);
                //}

                //void RegisterInferenceFarFromRag()
                //{
                //    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Registering inference distance from rag with distance value: " + InferenceFarFromRagDistance);

                //    InferenceManager.UnregisterInference(InferenceFarFromRag);

                //    MATCH.Inferences.Factory.Instance.CreateDistanceLeavingInferenceOneShot(InferenceManager, InferenceFarFromRag, delegate (System.Object oo, EventArgs ee)
                //    {
                //        UpdateConditionWithMatrix(ConditionRagNotTakenButHelpReceived);
                //    }, InteractionRag.gameObject, InferenceFarFromRagDistance);
                //}
            }

        }
    }
}
