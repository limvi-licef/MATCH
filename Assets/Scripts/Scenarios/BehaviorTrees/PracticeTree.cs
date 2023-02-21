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
            public class PracticeTree : Scenarios.BehaviorTrees.BehaviorTree
            {
                private Inferences.Manager InferenceManager;
                public Assistances.Surfaces.Manager SurfacesManager;

                string ConditionBeginning = "Beginning";
                string ConditionSinkTouched = "SinkTouched";
                string ConditionPlantWatered = "PlantWatered";

                // Interaction surface
                Assistances.InteractionSurface InteractionSurfaceDialogs;
                Assistances.InteractionSurface InteractionCube;
                Assistances.InteractionSurface InteractionPen;

                Dictionary<Assistances.AssistanceGradationExplicit, bool> AssistancesDusting;

                // Cube
                public Utilities.EyeTrackerMRTKTest Cube;

                public override void Awake()
                {
                    base.Awake();
                    SetId("Arroser les plantes");
                    AssistancesDusting = new Dictionary<Assistances.AssistanceGradationExplicit, bool>();
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
                    AddCondition(ConditionSinkTouched, false);
                    AddCondition(ConditionPlantWatered, false);

                    int nbConditions = GetNumberOfConditions();

                    AddConditionsUpdate(ConditionBeginning, new bool[] { true, false, false });
                    AddConditionsUpdate(ConditionSinkTouched, new bool[] { false, true, false });
                    AddConditionsUpdate(ConditionPlantWatered, new bool[] { false, false, true });

                    UpdateConditionWithMatrix(ConditionBeginning);

                    Selector srBegin = new Selector(
                        new BlackboardCondition(ConditionBeginning, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceAlpha()),
                        new BlackboardCondition(ConditionSinkTouched, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceBeta()),
                        new BlackboardCondition(ConditionPlantWatered, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceGamma()));

                    Root tree = new Root(/*Conditions*/Getconditions(), srBegin);

                    return tree;
                }

                void InitializeDebugButtons()
                {


                }

                void InitializeAssistances()
                {
                    InteractionSurfaceDialogs = Assistances.Factory.Instance.CreateInteractionSurface("Practice-Dialogs", AdminMenu.Panels.Right, new Vector3(1.1f, 0.02f, 0.7f), new Vector3(0f, 0f, 0f), Utilities.Materials.Colors.CyanGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);

                    InteractionCube = Assistances.Factory.Instance.CreateInteractionSurface("Practice-Cube", AdminMenu.Panels.Right, new Vector3(0.1f, 0.02f, 0.1f), new Vector3(-0.9f, -0.406f, -0.7f), Utilities.Materials.Colors.OrangeGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);

                    InteractionPen = Assistances.Factory.Instance.CreateInteractionSurface("Practice-Pen", AdminMenu.Panels.Right, new Vector3(0.1f, 0.1f, 0.1f), new Vector3(-0.1f, -0.406f, -0.7f), Utilities.Materials.Colors.PurpleGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);
                }

                //À MODIFIER
                Sequence AssistanceAlpha()
                {
                    Assistances.GradationVisual.GradationVisual alpha1 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("Practice-Alpha-1", "", "Qu'est-ce qu'il est conseilé de faire à la fin de la journée lorsqu'il ne fait moins chaud?", "Commencer !", delegate (System.Object o, EventArgs e)
                    {
                        UpdateConditionWithMatrix(ConditionSinkTouched);
                    }, Assistances.Buttons.Button.ButtonType.ClosingButton, InteractionSurfaceDialogs.transform);


                    Assistances.AssistanceGradationExplicit alpha = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("ArroserLesPlantes-Alpha");
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
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                Sequence AssistanceBeta()
                {
                    Assistances.GradationVisual.GradationVisual betaBase = Assistances.GradationVisual.Factory.Instance.CreateDialog2NoButton("Practice-Alpha-1", "", "Maintenant que vous avez rempli la bouteille d'eau. Vous pouvez aller arroser votre plante!", InteractionSurfaceDialogs.transform);

                    Assistances.AssistanceGradationExplicit beta = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("Practice-Beta");
                    beta.transform.parent = transform;

                    beta.AddAssistance(betaBase, Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesDusting.Add(beta, false);

                    beta.Init();

                    Cube.gameObject.transform.parent = InteractionCube.transform;
                    Cube.transform.localPosition = new Vector3(0, 0f, 0);
                    Utilities.Utility.AdjustObjectHeightToHeadHeight(Cube.transform);
                    Cube.CubeFocused += delegate (System.Object o, EventArgs e)
                    {
                        UpdateConditionWithMatrix(ConditionPlantWatered);
                    };

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            ShowAssistanceHideOthers(beta);
                            Cube.gameObject.SetActive(true);

                            InferenceManager.UnregisterAllInferences();
                            UpdateTextAssistancesDebugWindow("Beta");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Beta");
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                Sequence AssistanceGamma()
                {
                    Assistances.GradationVisual.GradationVisual gamma1 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("Practice-Gamma-1", "", "Bravo! Votre plante est arrosée.", "Suivant", delegate (System.Object o, EventArgs e)
                    {
                        UpdateConditionWithMatrix(ConditionPlantWatered);
                    }, Assistances.Buttons.Button.ButtonType.ClosingButton, InteractionCube.transform);

                    Assistances.AssistanceGradationExplicit gamma = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("Practice-Gamma");
                    gamma.transform.parent = transform;

                    gamma.AddAssistance(gamma1, Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesDusting.Add(gamma, false);

                    gamma.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            ShowAssistanceHideOthers(gamma);
                            Utilities.Utility.AnimateDisappearInPlace(Cube.gameObject, Cube.transform.localScale);
                            InferenceManager.UnregisterAllInferences();
                            UpdateTextAssistancesDebugWindow("Gamma");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Gamma");
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                void ShowAssistanceHideOthers(Assistances.AssistanceGradationExplicit assistance)
                {
                    List<Assistances.AssistanceGradationExplicit> keys = new List<Assistances.AssistanceGradationExplicit>(AssistancesDusting.Keys);

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
            }

        }
    }
}
