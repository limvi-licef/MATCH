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
            public class Tutorial : Scenarios.BehaviorTrees.BehaviorTree
            {
                private Inferences.Manager InferenceManager;
                public Assistances.Surfaces.Manager SurfacesManager;

                // Below: generated by the Excel file
                string ConditionBeginning = "Beginning";
                string ConditionBeginningClicked = "BeginningClicked";
                string ConditionCubeFocused = "CubeFocused";
                string ConditionNextClicked = "NextClicked";
                string ConditionPenGrabbed = "PenGrabbed";

                // End of Excel file generation

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
                    SetId("Nettoyer la table");
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
                    // Making the conditions update. SEE EXCEL SHEET TO GENERATE THE CODE BELOW
                    AddCondition(ConditionBeginning, false);
                    AddCondition(ConditionBeginningClicked, false);
                    AddCondition(ConditionCubeFocused, false);
                    AddCondition(ConditionNextClicked, false);
                    AddCondition(ConditionPenGrabbed, false);

                    int nbConditions = GetNumberOfConditions();

                    AddConditionsUpdate(ConditionBeginning, new bool[] { true, false, false, false, false });
                    AddConditionsUpdate(ConditionBeginningClicked, new bool[] { false, true, false, false, false });
                    AddConditionsUpdate(ConditionCubeFocused, new bool[] { false, false, true, false, false });
                    AddConditionsUpdate(ConditionNextClicked, new bool[] { false, false, false, true, false });
                    AddConditionsUpdate(ConditionPenGrabbed, new bool[] { false, false, false, false, true });


                    UpdateConditionWithMatrix(ConditionBeginning);

                    // End of code generation using the EXCEL file

                    // Defining the BT
                    Selector srBegin = new Selector(
                        new BlackboardCondition(ConditionBeginning, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceAlpha()),
                        new BlackboardCondition(ConditionBeginningClicked, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceBeta()),
                        new BlackboardCondition(ConditionCubeFocused, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceGamma()),
                        new BlackboardCondition(ConditionNextClicked, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceDelta()),
                        new BlackboardCondition(ConditionPenGrabbed, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceEpsilon()));

                    Root tree = new Root(/*Conditions*/Getconditions(), srBegin);

                    return tree;
                }

                void InitializeDebugButtons()
                {
                    
                    
                }

                void InitializeAssistances()
                {
                    InteractionSurfaceDialogs = Assistances.Factory.Instance.CreateInteractionSurface("Tutorial-Dialogs", AdminMenu.Panels.Right, new Vector3(1.1f, 0.02f, 0.7f), new Vector3(-0.447f, -0.406f, 0.009f), Utilities.Materials.Colors.CyanGlowing, true, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);
                    
                    InteractionCube = Assistances.Factory.Instance.CreateInteractionSurface("Tutorial-Cube", AdminMenu.Panels.Right, new Vector3(0.1f, 0.02f, 0.1f), new Vector3(-1.378f, -0.364f, 2.743f), Utilities.Materials.Colors.OrangeGlowing, true, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);

                    InteractionPen = Assistances.Factory.Instance.CreateInteractionSurface("Tutorial-Pen", AdminMenu.Panels.Right, new Vector3(0.1f, 0.1f, 0.1f), new Vector3(-1.378f, -0.364f, 2.743f), Utilities.Materials.Colors.PurpleGlowing, true, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);
                }

                Sequence AssistanceAlpha()
                {
                    Assistances.GradationVisual.GradationVisual alpha1 = Assistances.GradationVisual.Factory.Instance.CreateDialogOneButton("Tutorial-Alpha-1", "", "Bienvenue! Voici un tutoriel pour vous familiariser avec cette assistance. Pour commencer, avec un doigt, touchez le bouton \"commencer\" ci-dessous", "Commencer !", delegate(System.Object o, EventArgs e)
                    {
                        UpdateConditionWithMatrix(ConditionBeginningClicked);
                    }, Assistances.Buttons.Button.ButtonType.ClosingButton, InteractionSurfaceDialogs.transform);

                    Assistances.AssistanceGradationExplicit alpha = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("DustingTable-Alpha");
                    alpha.transform.parent = transform;

                    alpha.AddAssistance(alpha1, Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesDusting.Add(alpha, false);

                    alpha.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            ShowAssistanceHideOthers(alpha);
                            InferenceManager.UnregisterAllInferences();
                            AssistancesDebugWindow.SetDescription("Alpha");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Alpha");
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                Sequence AssistanceBeta()
                {
                    Assistances.GradationVisual.GradationVisual betaBase = Assistances.GradationVisual.Factory.Instance.CreateDialogNoButton("Tutorial-Alpha-1", "", "Vous venez de d�couvrir la premi�re interaction: toucher un bouton!\nMaintenant, vous allez voir un cube sur votre droite. Regardez le pendant quelques secondes pour passer � la suite", InteractionSurfaceDialogs.transform);

                    Assistances.AssistanceGradationExplicit beta = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("Tutorial-Beta");
                    beta.transform.parent = transform;

                    beta.AddAssistance(betaBase, Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesDusting.Add(beta, false);

                    beta.Init();

                    Cube.gameObject.transform.parent = InteractionCube.transform;
                    Cube.transform.localPosition = new Vector3(0, 0, 0);
                    Cube.CubeFocused += delegate (System.Object o, EventArgs e)
                    {
                        UpdateConditionWithMatrix(ConditionCubeFocused);
                    };

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            ShowAssistanceHideOthers(beta);
                            Cube.gameObject.SetActive(true);

                            InferenceManager.UnregisterAllInferences();
                            AssistancesDebugWindow.SetDescription("Beta");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Beta");
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }
                    
                Sequence AssistanceGamma()
                {
                    Assistances.GradationVisual.GradationVisual gamma1 = Assistances.GradationVisual.Factory.Instance.CreateDialogOneButton("Tutorial-Gamma-1", "", "Super! Comme vous le voyez, le syst�me comprend ce que vous regardez. Il pourra donc faire certaines action si vous regardez un objet.\nTouchez le bouton ci-dessous pour continuer", "Suivant", delegate(System.Object o, EventArgs e)
                    {
                        UpdateConditionWithMatrix(ConditionNextClicked);
                    }, Assistances.Buttons.Button.ButtonType.ClosingButton, InteractionCube.transform);

                    Assistances.AssistanceGradationExplicit gamma = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("Tutorial-Gamma");
                    gamma.transform.parent = transform;

                    gamma.AddAssistance(gamma1, Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesDusting.Add(gamma, false);

                    gamma.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            ShowAssistanceHideOthers(gamma);
                            Cube.gameObject.SetActive(false);
                            InferenceManager.UnregisterAllInferences();
                            AssistancesDebugWindow.SetDescription("Gamma");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Gamma");
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                Sequence AssistanceDelta()
                {
                    Assistances.GradationVisual.GradationVisual delta1 = Assistances.GradationVisual.Factory.Instance.CreateDialogOneButton("Tutorial-Delta-1", "", "Une derni�re chose! Il est important que vous regardiez votre lorsque vous faites une action. Cea est d� � une limite du casque que vous portez. Touchez le bouton ci-dessous pour continuer", "Suivant", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, InteractionCube.transform);

                    Assistances.GradationVisual.GradationVisual delta2 = Assistances.GradationVisual.Factory.Instance.CreateDialogNoButton("Tutorial-Delta-1", "", "Pour tester cela, prenez le stylo sur votre gauche. Si vous l'avez regard� correctement en le prenant, alors vous verrez un nouveau texte appara�tre ici. Sinon, reposez le au m�me endroit et recommencez.", InteractionCube.transform);

                    Assistances.AssistanceGradationExplicit delta = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("Tutorial-Delta");
                    delta.transform.parent = transform;

                    delta.AddAssistance(delta1, Assistances.Buttons.Button.ButtonType.Yes, delta2);
                    delta.AddAssistance(delta2, Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesDusting.Add(delta, false);

                    delta.Init();

                    InteractionPen.EventInteractionSurfaceTableTouched += delegate (System.Object o, EventArgs e)
                    {
                        UpdateConditionWithMatrix(ConditionPenGrabbed);
                    };

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            ShowAssistanceHideOthers(delta);
                            InferenceManager.UnregisterAllInferences();
                            AssistancesDebugWindow.SetDescription("Delta");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Delta");
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                Sequence AssistanceEpsilon()
                {
                    Assistances.GradationVisual.GradationVisual epsilon1 = Assistances.GradationVisual.Factory.Instance.CreateDialogOneButton("Tutorial-Epsilon-1", "", "F�licitations! Le tutoriel est maintenant termin�. Touchez le bouton ci-dessous pour commencer le test de l'application!", "Commencer le test", delegate(System.Object o, EventArgs e)
                    {
                        gameObject.transform.parent.Find("DustingTable").gameObject.SetActive(true);
                        gameObject.SetActive(false);
                    }, Assistances.Buttons.Button.ButtonType.ClosingButton, InteractionCube.transform);

                    Assistances.AssistanceGradationExplicit epsilon = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("Tutorial-Epsilon");
                    epsilon.transform.parent = transform;

                    epsilon.AddAssistance(epsilon1, Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesDusting.Add(epsilon, false);

                    epsilon.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            ShowAssistanceHideOthers(epsilon);
                            InferenceManager.UnregisterAllInferences();
                            AssistancesDebugWindow.SetDescription("Epsilon");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Epsilon");
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                void CallbackInteractionSurfaceRagTouched(System.Object o, EventArgs e)
                {
                    //UpdateConditionWithMatrix(ConditionRagTaken);
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
            }

        }
    }
}
