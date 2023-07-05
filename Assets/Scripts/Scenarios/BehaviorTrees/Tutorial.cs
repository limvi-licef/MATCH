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
                string ConditionShowingExclamation = "ConditionShowingExclamation";
                string ConditionBeginningClicked = "BeginningClicked";
                string ConditionCubeFocused = "CubeFocused";
                string ConditionNextClicked = "NextClicked";
                string ConditionPenGrabbed = "PenGrabbed";

                // End of Excel file generation

                // Interaction surface
                Assistances.InteractionSurface InteractionSurfaceDialogs;
                Assistances.InteractionSurface InteractionCube;
                Assistances.InteractionSurface InteractionPen;
                Assistances.InteractionSurface InteractionExclamationMark;
                Assistances.InteractionSurface InteractionLinePointB;

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
                    AddCondition(ConditionShowingExclamation, false);
                    AddCondition(ConditionBeginningClicked, false);
                    AddCondition(ConditionCubeFocused, false);
                    AddCondition(ConditionNextClicked, false);
                    AddCondition(ConditionPenGrabbed, false);

                    int nbConditions = GetNumberOfConditions();

                    AddConditionsUpdate(ConditionBeginning, new bool[] { true, false, false, false, false, false });
                    AddConditionsUpdate(ConditionShowingExclamation, new bool[] { false, true, false, false, false, false });
                    AddConditionsUpdate(ConditionBeginningClicked, new bool[] { false, false, true, false, false, false }); ;
                    AddConditionsUpdate(ConditionCubeFocused, new bool[] { false, false, false, true, false, false });
                    AddConditionsUpdate(ConditionNextClicked, new bool[] { false, false, false, false, true, false });
                    AddConditionsUpdate(ConditionPenGrabbed, new bool[] { false, false, false, false, false, true });


                    UpdateConditionWithMatrix(ConditionBeginning);

                    // End of code generation using the EXCEL file

                    // Defining the BT
                    Selector srBegin = new Selector(
                        new BlackboardCondition(ConditionBeginning, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceAlpha()),
                        new BlackboardCondition(ConditionShowingExclamation, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceBeta()),
                        new BlackboardCondition(ConditionBeginningClicked, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceGamma()),
                        new BlackboardCondition(ConditionCubeFocused, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceDelta()),
                        new BlackboardCondition(ConditionNextClicked, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceEpsilon()),
                        new BlackboardCondition(ConditionPenGrabbed, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceZeta()));

                    Root tree = new Root(/*Conditions*/Getconditions(), srBegin);

                    return tree;
                }

                void InitializeDebugButtons()
                {
                    
                    
                }

                void InitializeAssistances()
                {
                    InteractionSurfaceDialogs = Assistances.Factory.Instance.CreateInteractionSurface("Tutorial-Dialogs", AdminMenu.Panels.Right, new Vector3(1.1f, 0.02f, 0.7f), new Vector3(-0.447f, -0.406f, 0.009f), Utilities.Materials.Colors.CyanGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);
                    
                    InteractionCube = Assistances.Factory.Instance.CreateInteractionSurface("Tutorial-Cube", AdminMenu.Panels.Right, new Vector3(0.1f, 0.02f, 0.1f), new Vector3(-0.9f, -0.406f, -0.7f), Utilities.Materials.Colors.OrangeGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);

                    InteractionPen = Assistances.Factory.Instance.CreateInteractionSurface("Tutorial-Pen", AdminMenu.Panels.Right, new Vector3(0.1f, 0.1f, 0.1f), new Vector3(-0.1f, -0.406f, -0.7f), Utilities.Materials.Colors.PurpleGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);

                    InteractionExclamationMark = Assistances.Factory.Instance.CreateInteractionSurface("Tutorial-ExclamationMark", AdminMenu.Panels.Right, new Vector3(0.1f, 0.1f, 0.1f), new Vector3(0.4f, -0.406f, -0.7f), Utilities.Materials.Colors.RedGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);
                    
                    InteractionLinePointB = Assistances.Factory.Instance.CreateInteractionSurface("Tutorial-Line", AdminMenu.Panels.Right, new Vector3(0.1f, 0.1f, 0.1f), new Vector3(0.4f, -0.406f, -0.7f), Utilities.Materials.Colors.GreenGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);

                }

                Sequence AssistanceAlpha()
                {
                    /*Assistances.GradationVisual.GradationVisual alpha1 = Assistances.GradationVisual.Factory.Instance.CreateDialogOneButton("Tutorial-Alpha-1", "", "Bienvenue! Voici un tutoriel pour vous familiariser avec cette assistance. Pour commencer, avec un doigt, touchez le bouton \"Commencer !\" ci-dessous, comme si vous touchiez un interrupteur.", "Commencer !", delegate(System.Object o, EventArgs e)
                    {
                        UpdateConditionWithMatrix(ConditionBeginningClicked);
                    }, Assistances.Buttons.Button.ButtonType.ClosingButton, InteractionSurfaceDialogs.transform);*/

                    Assistances.GradationVisual.GradationVisual alpha1 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("Tutorial-Alpha-1", "", "Bienvenue! Voici un tutoriel pour vous familiariser avec cette assistance. Pour commencer, avec un doigt, touchez le bouton \"Commencer !\" ci-dessous, comme si vous touchiez un interrupteur.", "Commencer !", delegate (System.Object o, EventArgs e)
                    {
                        //UpdateConditionWithMatrix(ConditionBeginningClicked);
                        UpdateConditionWithMatrix(ConditionShowingExclamation);
                    }, Assistances.Buttons.Button.ButtonType.ClosingButton, InteractionSurfaceDialogs.transform);


                    Assistances.AssistanceGradationExplicit alpha = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("DustingTable-Alpha");
                    alpha.transform.parent = transform;

                    alpha.AddAssistance(alpha1, Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    //////////Line to delete [FOR TEST ONLY]
                    alpha.AddAssistance(alpha1, Assistances.Buttons.Button.ButtonType.CustomChoice1, null);

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
                    Assistances.GradationVisual.GradationVisual beta1 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("Tutorial-ExclamationMarkDialog", "", "Vous vous appretez à voir un point d'exclamation. Celui-ci a pour but de vous assister dans votre quotidien en vous guidant vers certaines taches à réaliser, appuyez sur SUIVANT pour le faire apparaître !", "SUIVANT", null, Assistances.Buttons.Button.ButtonType.Yes, InteractionSurfaceDialogs.transform);
                    Assistances.GradationVisual.GradationVisual beta2 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("Tutorial-ExclamationMarkDialog-2", "", "Il vient d'apparaitre sur ma droite ! Si vous n'y prêtez pas attention, le point d'exclamation va changer de couleur pour attirer un peu plus votre attention ! Appuyez sur SUIVANT pour voir.", "SUIVANT", null, Assistances.Buttons.Button.ButtonType.Yes, InteractionSurfaceDialogs.transform);
                    Assistances.GradationVisual.GradationVisual beta3 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("Tutorial-ExclamationMarkDialog-3", "", "Le point d'exclamation est maintenant orange. Si vous n'y prêtez toujours pas attention, il deviendra rouge. Appuyez sur SUIVANT pour voir.", "SUIVANT", null, Assistances.Buttons.Button.ButtonType.Yes, InteractionSurfaceDialogs.transform);
                    Assistances.GradationVisual.GradationVisual beta4 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("Tutorial-ExclamationMarkDialog-4", "", "Le point d'exclamation est maintenant rouge. Si une tâche que vous avez l'habitude de réaliser se situe dans une autre pièce, une flèche vous indiquera le chemin vers la pièce et la tâche en question ! Appuyez sur SUIVANT pour voir le chemin en surbrillance et suivez le !", "SUIVANT", null, Assistances.Buttons.Button.ButtonType.Yes, InteractionSurfaceDialogs.transform);
                    Assistances.GradationVisual.GradationVisual beta5 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("Tutorial-FlecheDialog-5", "", "Bravo ! Vous êtes arrivé à la tâche pointée par la flèche en la suivant !", "SUIVANT", null, Assistances.Buttons.Button.ButtonType.Yes, InteractionLinePointB.transform);


                    Assistances.Icon assistanceExclamation = Assistances.Factory.Instance.CreateIcon(true, new Vector3(0, 0, 0), new Vector3(0.15f, 0.15f, 0.15f), true, InteractionExclamationMark.transform, /*null*/Utilities.Materials.Icon.ExclamationMark, Utilities.Materials.Colors.WhiteMetallic);
                    
                    Assistances.AssistanceGradationExplicit beta = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("Tutorial-Beta");

                    //decorator4.Show(Utilities.Utility.GetEventHandlerEmpty(), false);
                    /*TODO*/

                    beta.transform.parent = transform;
                    beta.AddAssistance(beta1, Assistances.Buttons.Button.ButtonType.Yes, beta2);
                    beta.AddAssistance(beta2, Assistances.Buttons.Button.ButtonType.Yes, beta3);
                    beta.AddAssistance(beta3, Assistances.Buttons.Button.ButtonType.Yes, beta4);
                    beta.AddAssistance(beta4, Assistances.Buttons.Button.ButtonType.Yes, beta5);


                    AssistancesDusting.Add(beta, false);

                    Assistances.IAssistance beta1IAssistance = (Assistances.IAssistance)beta1.GetCurrentAssistance();
                    Assistances.Assistance beta1Assistance = (Assistances.Assistance)beta1IAssistance.GetRootDecoratedAssistance();

                    Assistances.IAssistance beta2IAssistance = (Assistances.IAssistance)beta2.GetCurrentAssistance();
                    Assistances.Assistance beta2Assistance = (Assistances.Assistance)beta2IAssistance.GetRootDecoratedAssistance();

                    Assistances.IAssistance beta3IAssistance = (Assistances.IAssistance)beta3.GetCurrentAssistance();
                    Assistances.Assistance beta3Assistance = (Assistances.Assistance)beta3IAssistance.GetRootDecoratedAssistance();

                    Assistances.IAssistance beta4IAssistance = (Assistances.IAssistance)beta4.GetCurrentAssistance();
                    Assistances.Assistance beta4Assistance = (Assistances.Assistance)beta4IAssistance.GetRootDecoratedAssistance();
                    
                    Assistances.IAssistance beta5IAssistance = (Assistances.IAssistance)beta5.GetCurrentAssistance();
                    Assistances.Assistance beta5Assistance = (Assistances.Assistance)beta5IAssistance.GetRootDecoratedAssistance();
                    
                    beta.Init();
                    beta1Assistance.EventHelpButtonClicked += delegate (System.Object o, EventArgs e)
                    {
                        assistanceExclamation.Show(Utilities.Utility.GetEventHandlerEmpty(), false);
                    };

                    beta2Assistance.EventHelpButtonClicked += delegate (System.Object o, EventArgs e)
                    {
                        assistanceExclamation.SetMaterial(Utilities.Materials.Colors.OrangeGlowing);
                    };

                    beta3Assistance.EventHelpButtonClicked += delegate (System.Object o, EventArgs e)
                    {
                        assistanceExclamation.SetMaterial(Utilities.Materials.Colors.RedGlowing);
                    };

                    beta4Assistance.EventHelpButtonClicked += delegate(System.Object o, EventArgs e)
                    {
                        Assistances.Decorators.LinePath decorator4 =
                            (Assistances.Decorators.LinePath)Assistances.Decorators.Factory.Instance
                                .CreateLinePathWithTexture(beta5IAssistance, /*Utilities.Materials.Colors.Orange*/
                                    Utilities.Materials.Textures.ArrowProgressive, 0.1f, true);
                        
                        decorator4.Show(Utilities.Utility.GetEventHandlerEmpty(), false);
                        
                        assistanceExclamation.Hide(Utilities.Utility.GetEventHandlerEmpty(), false);
                    };

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            ShowAssistanceHideOthers(beta);
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
                    Assistances.GradationVisual.GradationVisual gammaBase = Assistances.GradationVisual.Factory.Instance.CreateDialog2NoButton("Tutorial-Gamma-1", "", "Vous venez de découvrir la première interaction: toucher un bouton!\nMaintenant, vous allez voir un cube sur votre droite. Regardez le pendant quelques secondes pour passer à la suite.", InteractionSurfaceDialogs.transform);

                    Assistances.AssistanceGradationExplicit gamma = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("Tutorial-Gamma");
                    gamma.transform.parent = transform;

                    gamma.AddAssistance(gammaBase, Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesDusting.Add(gamma, false);

                    gamma.Init();

                    Cube.gameObject.transform.parent = InteractionCube.transform;
                    Cube.transform.localPosition = new Vector3(0, 0f, 0);
                    Utilities.Utility.AdjustObjectHeightToHeadHeight(Cube.transform);
                    Cube.CubeFocused += delegate (System.Object o, EventArgs e)
                    {
                        UpdateConditionWithMatrix(ConditionCubeFocused);
                    };

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            ShowAssistanceHideOthers(gamma);
                            Cube.gameObject.SetActive(true);

                            InferenceManager.UnregisterAllInferences();
                            UpdateTextAssistancesDebugWindow("Gamma");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Gamma");
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }
                    
                Sequence AssistanceDelta()
                {
                    /*Assistances.GradationVisual.GradationVisual gamma1 = Assistances.GradationVisual.Factory.Instance.CreateDialogOneButton("Tutorial-Gamma-1", "", "Super! Comme vous le voyez, le système comprend ce que vous regardez. Il pourra donc faire certaines actions si vous regardez un objet.\nTouchez le bouton ci-dessous pour continuer.", "Suivant", delegate(System.Object o, EventArgs e)
                    {
                        UpdateConditionWithMatrix(ConditionNextClicked);
                    }, Assistances.Buttons.Button.ButtonType.ClosingButton, InteractionCube.transform);*/

                    Assistances.GradationVisual.GradationVisual delta1 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("Tutorial-Delta-1", "", "Super! Comme vous le voyez, le système comprend ce que vous regardez. Il pourra donc faire certaines actions si vous regardez un objet.Touchez le bouton ci-dessous pour continuer.", "Suivant", delegate (System.Object o, EventArgs e)
                    {
                        UpdateConditionWithMatrix(ConditionNextClicked);
                    }, Assistances.Buttons.Button.ButtonType.ClosingButton, InteractionCube.transform);

                    Assistances.AssistanceGradationExplicit delta = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("Tutorial-Delta");
                    delta.transform.parent = transform;

                    delta.AddAssistance(delta1, Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesDusting.Add(delta, false);

                    delta.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            ShowAssistanceHideOthers(delta);
                            Utilities.Utility.AnimateDisappearInPlace(Cube.gameObject, Cube.transform.localScale);
                            //Cube.gameObject.SetActive(false);
                            InferenceManager.UnregisterAllInferences();
                            UpdateTextAssistancesDebugWindow("Delta");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Delta");
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                Sequence AssistanceEpsilon()
                {
                    /*Assistances.GradationVisual.GradationVisual delta1 = Assistances.GradationVisual.Factory.Instance.CreateDialogOneButton("Tutorial-Delta-1", "", "Une dernière chose! Il est important que vous regardiez votre main lorsque vous faites une action. Cela est dû à une limite technique du casque que vous portez. Un bouton va apparaître ci-dessous dans quelques secondes. Touchez le pour continuer.", "Suivant", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, InteractionCube.transform);*/

                    Assistances.GradationVisual.GradationVisual epsilon1 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("Tutorial-Epsilon-1", "", "Une dernière chose! Il est important que vous regardiez votre main lorsque vous faites une action. Cela est dû à une limite technique du casque que vous portez. Un bouton va apparaître ci-dessous dans quelques secondes. Touchez le pour continuer.", "Suivant", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, InteractionCube.transform);

                    /*Assistances.GradationVisual.GradationVisual delta2 = Assistances.GradationVisual.Factory.Instance.CreateDialogNoButton("Tutorial-Delta-1", "", "Pour tester cela, prenez le stylo sur votre droite. Si vous l'avez regardé correctement en le prenant, alors vous verrez un nouveau texte apparaître ici. Sinon, reposez-le au même endroit et recommencez.", InteractionCube.transform);*/

                    Assistances.GradationVisual.GradationVisual epsilon2 = Assistances.GradationVisual.Factory.Instance.CreateDialog2NoButton("Tutorial-Epsilon-1", "", "Pour tester cela, prenez le stylo sur votre droite. Si vous l'avez regardé correctement en le prenant, alors vous verrez un nouveau texte apparaître ici. Sinon, reposez-le au même endroit et recommencez.", InteractionCube.transform);

                    Assistances.AssistanceGradationExplicit epsilon = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("Tutorial-Epsilon");
                    epsilon.transform.parent = transform;

                    epsilon.AddAssistance(epsilon1, Assistances.Buttons.Button.ButtonType.Yes, epsilon2);
                    epsilon.AddAssistance(epsilon2, Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesDusting.Add(epsilon, false);

                    epsilon.Init();

                    InteractionPen.EventUserTouched += delegate (System.Object o, EventArgs e)
                    {
                        UpdateConditionWithMatrix(ConditionPenGrabbed);
                    };

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            ShowAssistanceHideOthers(epsilon);
                            InferenceManager.UnregisterAllInferences();
                            UpdateTextAssistancesDebugWindow("Epsilon");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Epsilon");
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                Sequence AssistanceZeta()
                {
                    /*Assistances.GradationVisual.GradationVisual epsilon1 = Assistances.GradationVisual.Factory.Instance.CreateDialogOneButton("Tutorial-Epsilon-1", "", "Félicitations! Le tutoriel est maintenant terminé. Touchez le bouton ci-dessous pour commencer le test de l'application!", "Commencer le test", delegate(System.Object o, EventArgs e)
                    {
                        gameObject.transform.parent.Find("DustingTable").gameObject.SetActive(true);
                        gameObject.SetActive(false);
                    }, Assistances.Buttons.Button.ButtonType.ClosingButton, InteractionCube.transform);*/

                    Assistances.GradationVisual.GradationVisual zeta1 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("Tutorial-Zeta-1", "", "Félicitations! Le tutoriel est maintenant terminé. Touchez le bouton ci-dessous pour commencer le test de l'application!", "Commencer le test", delegate (System.Object o, EventArgs e)
                    {
                        gameObject.transform.parent.Find("DustingTable").gameObject.SetActive(true);
                        gameObject.SetActive(false);
                    }, Assistances.Buttons.Button.ButtonType.ClosingButton, InteractionCube.transform);

                    Assistances.AssistanceGradationExplicit zeta = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("Tutorial-Zeta");
                    zeta.transform.parent = transform;

                    zeta.AddAssistance(zeta1, Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesDusting.Add(zeta, false);

                    zeta.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            ShowAssistanceHideOthers(zeta);
                            InferenceManager.UnregisterAllInferences();
                            UpdateTextAssistancesDebugWindow("Zeta");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Zeta");
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
