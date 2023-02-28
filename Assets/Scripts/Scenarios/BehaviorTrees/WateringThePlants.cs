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

                // Interaction surface
                Assistances.InteractionSurface InteractionSurfaceDialogs;
                Assistances.InteractionSurface InteractionSink;
                Assistances.InteractionSurface InteractionPlant1;
                Assistances.InteractionSurface InteractionPlant2;

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
                    AddCondition(ConditionBeginning, false);
                    AddCondition(ConditionBottleFilled, false);
                    AddCondition(ConditionHelpNeeded, false);
                    AddCondition(ConditionHelpRequested, false);
                    AddCondition(ConditionHelpRequestedAgain, false);
                    AddCondition(ConditionOnePlantWatered, false);
                    AddCondition(ConditionAllPlantsWatered, false);

                    int nbConditions = GetNumberOfConditions();

                    AddConditionsUpdate(ConditionBeginning, new bool[] { true, false, false, false, false, false, false });
                    AddConditionsUpdate(ConditionBottleFilled, new bool[] { false, true, false, false, false, false, false });
                    AddConditionsUpdate(ConditionHelpNeeded, new bool[] { false, false, true, false, false, false, false });
                    AddConditionsUpdate(ConditionHelpRequested, new bool[] { false, false, false, true, false, false, false });
                    AddConditionsUpdate(ConditionHelpRequestedAgain, new bool[] { false, false, false, false, true, false, false });
                    AddConditionsUpdate(ConditionOnePlantWatered, new bool[] { false, false, false, false, false, true, false });
                    AddConditionsUpdate(ConditionAllPlantsWatered, new bool[] { false, false, false, false, false, false, true });

                    UpdateConditionWithMatrix(ConditionBeginning);

                    Selector srBegin = new Selector(
                        new BlackboardCondition(ConditionBeginning, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceBTWP7()),
                        new BlackboardCondition(ConditionBottleFilled, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceBTWP5()),
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
                    //Est-ce à modifier?

                }

                void InitializeAssistances()
                {
                    InteractionSurfaceDialogs = Assistances.Factory.Instance.CreateInteractionSurface("Practice-Dialogs", AdminMenu.Panels.Right, new Vector3(1.1f, 0.02f, 0.7f), new Vector3(0f, 0f, 0f), Utilities.Materials.Colors.CyanGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);

                    //� modifier les interactions
                    InteractionSink = Assistances.Factory.Instance.CreateInteractionSurface("Practice-Plant", AdminMenu.Panels.Right, new Vector3(1.1f, 0.02f, 0.7f), new Vector3(0f, 0f, 0f), Utilities.Materials.Colors.GreenGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);
                    InteractionPlant1 = Assistances.Factory.Instance.CreateInteractionSurface("Practice-Plant", AdminMenu.Panels.Right, new Vector3(1.1f, 0.02f, 0.7f), new Vector3(0f, 0f, 0f), Utilities.Materials.Colors.GreenGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);
                    InteractionPlant2 = Assistances.Factory.Instance.CreateInteractionSurface("Practice-Plant", AdminMenu.Panels.Right, new Vector3(1.1f, 0.02f, 0.7f), new Vector3(0f, 0f, 0f), Utilities.Materials.Colors.GreenGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);

                }

                //Is it 7pm?
                Sequence AssistanceBTWP7()
                {
                    MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Is it 7pm?");

                    Assistances.GradationVisual.GradationVisual alpha1 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("Practice-Alpha-1", "", "Qu'est-ce qu'il est conseilé de faire à la fin de la journée lorsqu'il ne fait moins chaud?", "Commencer !", delegate (System.Object o, EventArgs e)
                    {
                        UpdateConditionWithMatrix(ConditionAllPlantsWatered);
                    }, Assistances.Buttons.Button.ButtonType.ClosingButton, InteractionSurfaceDialogs.transform);


                    Assistances.AssistanceGradationExplicit alpha = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("ArroserLesPlantes-Alpha");
                    alpha.transform.parent = transform;

                    alpha.AddAssistance(alpha1, Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesWatering.Add(alpha, false);

                    alpha.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            ShowAssistanceHideOthers(alpha);
                            InferenceManager.UnregisterAllInferences();
                            UpdateTextAssistancesDebugWindow("Alpha");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "BTWP7");
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                //Did the person fill the bottle to water the plants?
                Sequence AssistanceBTWP5()
                {
                    return null;
                }

                //Does the person come back to the faucet without having watered any plant?
                Sequence AssistanceBTWP4()
                {
                    return null;
                }

                //Does the person request some help?
                Sequence AssistanceBTWP6()
                {
                    return null;
                }

                //Did the person already request help to find the location of the plants?
                Sequence AssistanceBTWP3()
                {
                    return null;
                }

                //Did the person water one of the plants?
                Sequence AssistanceBTWP2()
                {
                    return null;
                }

                //Are all plants watered?
                Sequence AssistanceBTWP1()
                {
                    return null;
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
            }

        }
    }
}
