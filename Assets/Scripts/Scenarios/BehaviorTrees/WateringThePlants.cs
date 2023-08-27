/*Copyright 2023 Rémi Létourneau, Pierre-Daniel Godfrey, Brian Biswas

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using NPBehave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

namespace MATCH
{
    namespace Scenarios
    {
        namespace BehaviorTrees
        {
            public class WateringThePlants : BehaviorTree
            {

                private Inferences.Manager InferenceManager;
                public Assistances.Surfaces.Manager SurfacesManager;

                string ConditionBeginning = "Beginning";
                string ConditionBottleFilled = "BottleFilled";
                string ConditionHelpNeeded = "HelpNeeded";
                string ConditionHelpRequestedAgain = "HelpRequestedAgain";
                string ConditionPlantWatered = "PlantWatered";
                string ConditionAllPlantsWatered = "AllPlantsWatered";

                string InferenceFarFromBottle = "FarFromBottle";
                float InferenceFarFromBottleDistance = 4.0f;
                string InferenceDidNotStartWatering = "DidNotStartWatering";
                string InferenceInterruptWatering = "InterruptedWatering";

                // Interaction surface
                Assistances.InteractionSurface InteractionSurfaceDialogs;
                Assistances.InteractionSurface InteractionBottle;

                Assistances.InteractionSurface InteractionSink;
                Assistances.InteractionSurface InteractionPlant1;
                Assistances.InteractionSurface InteractionPlant2;
                Assistances.InteractionSurface InteractionPlant3;

                List<GameObject> Cubes;
                public EventHandler EventResized;
                public EventHandler EventMoved;
                List<Assistances.InteractionSurface> InteractionPlants = new List<Assistances.InteractionSurface>();

                bool[] LightPathsShown = new bool[3];
                float NextTimeCheck = 0f;

                Dictionary<Assistances.AssistanceGradationExplicit, bool> AssistancesWatering;

                Inferences.Timer inf1;

                Assistances.InteractionSurface FollowObject;

                MATCH.Assistances.Dialogs.Dialog1 DialogAssistanceWaterHelp;
                Assistances.GradationVisual.GradationVisual MenuPlant;
                PathFinding.PathFinding PathFinderEngine;
                
                Utilities.ObjectPositioningStorage PlantsPositioningStorage;


                public override void Awake()
                {
                    base.Awake();
                    SetId("Arroser les plantes");
                    AssistancesWatering = new Dictionary<Assistances.AssistanceGradationExplicit, bool>();

                    Cubes = new List<GameObject>();
                    PlantsPositioningStorage = new Utilities.ObjectPositioningStorage("PlantsStorage.txt");
                }

                public override void Start()
                {
                    Scenarios.Manager.Instance.AddScenario(this);
                    
                    List<String> registeredObjectsIds = PlantsPositioningStorage.GetObjetsRegisteredNames();
                    foreach (string id in registeredObjectsIds)
                    {
                        Utilities.ObjectPositioningStorage.ObjectsInformation objectsInformation = PlantsPositioningStorage.GetRegisteredObjectInformation(id);

                        AddPlant(id, objectsInformation.Scale, objectsInformation.Position,  Utilities.Materials.Colors.WhiteTransparent, true, false, true, transform);
                    }

                    // Initialize assistances
                    InitializeAssistances();

                    // Initialize inference manager
                    InferenceManager = MATCH.Inferences.Factory.Instance.CreateManager(transform);

                    // Call base function
                    base.Start();

                    Init();

                    PathFinderEngine = GameObject.Find("PathFinderEngine").GetComponent<PathFinding.PathFinding>();

                    // Add button to restart scenario
                    MATCH.AdminMenu.Instance.AddButton("Watering the plants - restart scenario", delegate
                    {
                        inf1.StopCounter();
                        SetConditionsTo(false);
                        UpdateConditionWithMatrix(ConditionBeginning);
                        for (int i = 0; i < InteractionPlants.Count(); i++)
                        {
                            NextTimeCheck = 0f;
                            if (LightPathsShown[i])
                            {
                                DialogAssistanceWaterHelp.ButtonsController[i].CheckButton(false);
                                InteractionPlants[i].CallbackShow();
                                LightPathsShown[i] = false;
                                GameObject gameObjectForLine = GameObject.Find("Line for " + InteractionPlants[i].name);
                                Destroy(gameObjectForLine);
                            }
                            if (InteractionPlants[i].CompareTag("Watered"))
                            {
                                InteractionPlants[i].tag = "Untagged";
                                InteractionPlants[i].SetColor(Utilities.Materials.Colors.GreenGlowing);
                            }
                        }
                        MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "----------Scenario restarted----------");
                    }, AdminMenu.Panels.Right);

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
                    AddCondition(ConditionHelpRequestedAgain, false);
                    AddCondition(ConditionPlantWatered, false);
                    AddCondition(ConditionAllPlantsWatered, false);


                    int nbConditions = GetNumberOfConditions();

                    AddConditionsUpdate(ConditionBeginning, new bool[] { true, false, false, false, false, false });
                    AddConditionsUpdate(ConditionBottleFilled, new bool[] { false, true, false, false, false, false });
                    AddConditionsUpdate(ConditionHelpNeeded, new bool[] { false, false, true, false, false, false });
                    AddConditionsUpdate(ConditionHelpRequestedAgain, new bool[] { false, false, false, true, false, false });
                    AddConditionsUpdate(ConditionPlantWatered, new bool[] { false, false, false, false, true, false });
                    AddConditionsUpdate(ConditionAllPlantsWatered, new bool[] { false, false, false, false, false, true });
                    // End of code generation using the EXCEL file

                    UpdateConditionWithMatrix(ConditionBeginning);

                    //Defining the BT
                    Selector srPlantsNotWatered = new Selector(
                        new BlackboardCondition(ConditionBeginning, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceBTWP7()),
                        new BlackboardCondition(ConditionBottleFilled, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceBTWP5()),
                        new BlackboardCondition(ConditionHelpNeeded, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceBTWP4()),
                        new BlackboardCondition(ConditionHelpRequestedAgain, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceBTWP3()),
                        new BlackboardCondition(ConditionPlantWatered, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceBTWP2()),
                        new WaitUntilStopped()
                        );

                    //
                    Selector srBegin = new Selector(
                        new BlackboardCondition(ConditionAllPlantsWatered, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceBTWP1()),
                        srPlantsNotWatered);

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
                        UpdateConditionWithMatrix(ConditionPlantWatered);
                    }, AdminMenu.Panels.Right);

                    AdminMenu.Instance.AddButton("BT - Watering - Trigger - help needed", delegate
                    {
                        UpdateConditionWithMatrix(ConditionHelpNeeded);
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
                    
                    //Add a button to the admin panel
                    MATCH.AdminMenu.Instance.AddInputWithButton("Nom", "Ajouter une plante", delegate (System.Object o, EventArgs e)
                    {
                        Utilities.EventHandlerArgs.String arg = (Utilities.EventHandlerArgs.String)e;
                        
                        AddPlant(arg.m_text, new Vector3(0.3f, 0.5f, 0.3f),
                            new Vector3(-2.309f, 0.263f, 2.031f), Utilities.Materials.Colors.GreenGlowing, true, false,
                            true, transform);
                    }, AdminMenu.Panels.Middle);

                }

                void InitializeAssistances()
                {
                    InteractionSurfaceDialogs = Assistances.Factory.Instance.CreateInteractionSurface("Practice-Dialogs", AdminMenu.Panels.Right, new Vector3(0f, 0.02f, 0.7f),
                        new Vector3(0f, 0f, 0.009f), Utilities.Materials.Colors.CyanGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);

                    FollowObject = Assistances.InteractionSurfaceFollower.Instance.GetInteractionSurface();

                    //à modifier les interactions
                    InteractionSink = Assistances.Factory.Instance.CreateInteractionSurface("Practice-Sink", AdminMenu.Panels.Right, new Vector3(0.6f, 0.1f, 0.4f),
                        new Vector3(-4.777f, 0.659f, 2.031f), Utilities.Materials.Colors.GreenGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);
                    /*
                    InteractionPlant1 = Assistances.Factory.Instance.CreateInteractionSurface("Practice-Plant1", AdminMenu.Panels.Right, new Vector3(0.3f, 0.5f, 0.3f),
                        new Vector3(-2.309f, 0.263f, 2.031f), Utilities.Materials.Colors.GreenGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);
                    InteractionPlant2 = Assistances.Factory.Instance.CreateInteractionSurface("Practice-Plant2", AdminMenu.Panels.Right, new Vector3(0.3f, 0.5f, 0.3f),
                        new Vector3(2f, 0f, 0f), Utilities.Materials.Colors.GreenGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);
                    InteractionPlant3 = Assistances.Factory.Instance.CreateInteractionSurface("Practice-Plant3", AdminMenu.Panels.Right, new Vector3(0.3f, 0.5f, 0.3f),
                         new Vector3(-7f, 0f, 0f), Utilities.Materials.Colors.GreenGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform);
                    

                    InteractionPlants = new List<Assistances.InteractionSurface> { InteractionPlant1, InteractionPlant2, InteractionPlant3 };*/

                    InteractionSink.EventUserTouched += CallbackInteractionSurfaceSinkTouched;

                    DialogAssistanceWaterHelp = Assistances.Factory.Instance.CreateCheckListNoButton("", "Voici les plantes qu'il vous reste à arroser. Si vous touchez une des plantes, un chemin au sol vous y guidera.", FollowObject.transform);

                    /*
                    DialogAssistanceWaterHelp.AddButton("Plante " + (1), false, 0.12f);
                    DialogAssistanceWaterHelp.AddButton("Plante " + (2), false, 0.12f);
                    DialogAssistanceWaterHelp.AddButton("Plante " + (3), false, 0.12f);*/

                    for (int i = 0; i < DialogAssistanceWaterHelp.ButtonsController.Count; i++)
                    {

                        int plantId = i;

                        DialogAssistanceWaterHelp.ButtonsController[plantId].EventButtonClicked += delegate (System.Object o, EventArgs e)
                        {
                            UpdateTextAssistancesDebugWindow("i is : " + plantId);
                            if (DialogAssistanceWaterHelp.ButtonsController[plantId].IsChecked() == false)
                            {
                                InteractionPlants[plantId].CallbackShow();
                                ShowLightpathToPlant(InteractionPlants[plantId]);
                                DialogAssistanceWaterHelp.ButtonsController[plantId].CheckButton(true);
                                LightPathsShown[plantId] = true;
                                NextTimeCheck = Time.time + 5f;
                            }
                        };
                    }

                    MenuPlant = Assistances.GradationVisual.Factory.Instance.CreateAssistanceDialog("WateringThePlants-BTWP4-1", DialogAssistanceWaterHelp);
                }

                private void Update()
                {
                    if (Array.Exists<bool>(LightPathsShown, element => element) && Time.time > NextTimeCheck)
                    {
                        for (int i = 0; i < InteractionPlants.Count(); i++)
                        {
                            if (InteractionPlants[i].GetComponentInChildren<BoundsControl>().enabled && InteractionPlants[i].tag != "Watered")
                            {
                                updateLightPath(InteractionPlants[i]);
                            }
                        }
                        NextTimeCheck += 5f;
                    }
                }

                //Is it 7pm?
                Sequence AssistanceBTWP7()
                {

                    Assistances.GradationVisual.GradationVisual alpha1 = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("Practice-Alpha-1", "",
                        "Qu'est-ce qu'il est conseilé de faire à la fin de la journée lorsqu'il ne fait moins chaud?", "Commencer !",
                        Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, FollowObject.transform);

                    Assistances.AssistanceGradationExplicit BTWP7 = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("ArroserLesPlantes-Alpha");
                    BTWP7.transform.parent = transform;

                    BTWP7.AddAssistance(alpha1, Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesWatering.Add(BTWP7, false);

                    BTWP7.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            ShowAssistanceHideOthers(BTWP7);
                            UpdateTextAssistancesDebugWindow("Is it 7pm?");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "BTWP7");
                        }),
                        new WaitUntilStopped()
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
                            foreach (Assistances.InteractionSurface interactionPlant in InteractionPlants)
                            {
                                interactionPlant.EventUserTouched += CallbackInteractionSurfacePlantWatered;
                            }

                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "BTWP5");

                            InferenceManager.UnregisterInference(InferenceDidNotStartWatering);

                            inf1 = new Inferences.Timer(InferenceDidNotStartWatering, 15, delegate (System.Object oo, EventArgs ee)
                            {
                                if (!InteractionPlants.Any(p => p.CompareTag("Watered")))
                                {
                                    //Faire attention si on a 2 inférences avec le même nom (pour la même condition par exemple)
                                    UpdateConditionWithMatrix(ConditionHelpNeeded);
                                    InferenceManager.UnregisterInference(InferenceDidNotStartWatering);
                                }
                            });
                            InferenceManager.RegisterInference(inf1);
                            inf1.StartCounter();
                            UpdateTextAssistancesDebugWindow("Timer started");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "Timer started");
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }


                //Does the person come back to the faucet without having watered any plant?
                Sequence AssistanceBTWP4()
                {


                    Assistances.GradationVisual.GradationVisual requestHelp = Assistances.GradationVisual.Factory.Instance.CreateAlreadyConfigured(Assistances.GradationVisual.Factory.AlreadyConfigured.DoYouNeedHelpDialog1,
                         "WateringThePlants-BTWP4", FollowObject.transform);


                    Assistances.GradationVisual.GradationVisual dontKnow = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("WateringThePlants-BTWP4-1", "",
                        "Voulez vous savoir où sont vos plantes?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes,
                        "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, FollowObject.transform);


                    Assistances.GradationVisual.GradationVisual letItGo = Assistances.GradationVisual.Factory.Instance.CreateAlreadyConfigured(Assistances.GradationVisual.Factory.AlreadyConfigured.LetGoDialog2,
                        "WateringThePlants-BTWP4-1", FollowObject.transform);

                    Assistances.GradationVisual.GradationVisual sayWhatToDo = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("WateringThePlants-BTWP4-1", "",
                        "Vos plantes sont indiquées en vert, Arroser-les maintenant", "Ok", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne comprends pas",
                        Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, FollowObject.transform);

                    Assistances.GradationVisual.GradationVisual someoneComing = Assistances.GradationVisual.Factory.Instance.CreateAlreadyConfigured(Assistances.GradationVisual.Factory.AlreadyConfigured.SomeoneComingToHelpDialog2, "Watering-BTWP4-3", FollowObject.transform);

                    Assistances.AssistanceGradationExplicit BTWP4 = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("WateringThePlants-BTWP4");

                    BTWP4.transform.parent = transform;

                    BTWP4.AddAssistance(requestHelp, Assistances.Buttons.Button.ButtonType.Yes, MenuPlant);
                    BTWP4.AddAssistance(requestHelp, Assistances.Buttons.Button.ButtonType.No, letItGo);

                    BTWP4.AddAssistance(dontKnow, Assistances.Buttons.Button.ButtonType.Yes, sayWhatToDo);
                    BTWP4.AddAssistance(dontKnow, Assistances.Buttons.Button.ButtonType.No, letItGo);

                    BTWP4.AddAssistance(sayWhatToDo, Assistances.Buttons.Button.ButtonType.Yes, letItGo);
                    BTWP4.AddAssistance(sayWhatToDo, Assistances.Buttons.Button.ButtonType.No, someoneComing);

                    BTWP4.AddAssistance(letItGo, Assistances.Buttons.Button.ButtonType.ClosingButton, null);
                    BTWP4.AddAssistance(someoneComing, Assistances.Buttons.Button.ButtonType.ClosingButton, null);
                    BTWP4.AddAssistance(MenuPlant, Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesWatering.Add(BTWP4, false);

                    BTWP4.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() =>
                        {
                            ShowAssistanceHideOthers(BTWP4);
                            BTWP4.RunAssistance();
                            //AssistancesWatering[BTWP4] = true;
                            InferenceManager.UnregisterAllInferences();
                            UpdateTextAssistancesDebugWindow("BTWP4");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, "BTWP4");
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                //Did the person already request help to find the location of the plants?
                Sequence AssistanceBTWP3()
                {
                    Assistances.GradationVisual.GradationVisual helpNeeded = Assistances.GradationVisual.Factory.Instance.CreateAlreadyConfigured(Assistances.GradationVisual.Factory.AlreadyConfigured.DoYouNeedHelpDialog1,
                       "WateringThePlants-BTWP3", FollowObject.transform);

                    Assistances.GradationVisual.GradationVisual dontKnow = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("WateringThePlants-BTWP3-1", "",
                        "Saviez ce que vous êtier en train de faire?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes,
                        "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, FollowObject.transform);

                    Assistances.GradationVisual.GradationVisual letItGo = Assistances.GradationVisual.Factory.Instance.CreateAlreadyConfigured(Assistances.GradationVisual.Factory.AlreadyConfigured.LetGoDialog2,
                        "WateringThePlants-BTWP3-1", FollowObject.transform);

                    Assistances.GradationVisual.GradationVisual sayWhatToDo = Assistances.GradationVisual.Factory.Instance.CreateDialog2WithButtons("WateringThePlants-BTWP3-1", "",
                        "Vous devez continuer à arroser les plantes", "Ok", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Je ne comprends pas",
                        Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, FollowObject.transform);

                    Assistances.GradationVisual.GradationVisual someoneComing = Assistances.GradationVisual.Factory.Instance.CreateAlreadyConfigured(Assistances.GradationVisual.Factory.AlreadyConfigured.SomeoneComingToHelpDialog2, "WateringThePlants-BTWP8-3", FollowObject.transform);

                    Assistances.AssistanceGradationExplicit BTWP3 = MATCH.Assistances.Factory.Instance.CreateAssistanceGradationExplicit("WateringThePlants-BTWP3");
                    BTWP3.transform.parent = transform;

                    BTWP3.AddAssistance(helpNeeded, Assistances.Buttons.Button.ButtonType.Yes, MenuPlant);
                    BTWP3.AddAssistance(helpNeeded, Assistances.Buttons.Button.ButtonType.No, letItGo);
                    BTWP3.AddAssistance(dontKnow, Assistances.Buttons.Button.ButtonType.Yes, letItGo);
                    BTWP3.AddAssistance(dontKnow, Assistances.Buttons.Button.ButtonType.No, sayWhatToDo);
                    BTWP3.AddAssistance(sayWhatToDo, Assistances.Buttons.Button.ButtonType.Yes, letItGo);
                    BTWP3.AddAssistance(sayWhatToDo, Assistances.Buttons.Button.ButtonType.No, someoneComing);

                    BTWP3.AddAssistance(letItGo, Assistances.Buttons.Button.ButtonType.ClosingButton, null);
                    BTWP3.AddAssistance(someoneComing, Assistances.Buttons.Button.ButtonType.ClosingButton, null);
                    BTWP3.AddAssistance(MenuPlant, Assistances.Buttons.Button.ButtonType.ClosingButton, null);

                    AssistancesWatering.Add(BTWP3, false);
                    BTWP3.Init();

                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => {
                            ShowAssistanceHideOthers(BTWP3);
                            //BTWP3.RunAssistance();
                            AssistancesWatering[BTWP3] = true;
                            InferenceManager.UnregisterAllInferences();
                            UpdateTextAssistancesDebugWindow("Are you finished?");
                            MATCH.Utilities.Logger.Instance.Log(this.GetId(), MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, " BTWP3 : Help need again");
                        }),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                //Did the person water one of the plants?
                Sequence AssistanceBTWP2()
                {
                    Sequence temp = new Sequence(
                        new NPBehave.Action(() =>
                        {

                            //Arrêter l'inférence du timer ici
                            UpdateTextAssistancesDebugWindow("On Arrête l'inférence du timer ici");
                            inf1.StopCounter();

                            UpdateTextAssistancesDebugWindow("One plant watered");
                            InferenceManager.UnregisterInference(InferenceInterruptWatering);

                            //Démarrage du timer
                            inf1 = new Inferences.Timer(InferenceInterruptWatering, 15, delegate (System.Object oo, EventArgs ee)
                            {
                                UpdateConditionWithMatrix(ConditionHelpRequestedAgain);
                                InferenceManager.UnregisterInference(InferenceInterruptWatering);
                            });
                            InferenceManager.RegisterInference(inf1);
                            inf1.StartCounter();
                            UpdateTextAssistancesDebugWindow("Interupt water : Timer started");

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
                        "Vous avez terminé l'activité! Félicitations!", "Terminer", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, FollowObject.transform);


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
                    UpdateConditionWithMatrix(ConditionPlantWatered);
                    foreach (Assistances.InteractionSurface interactionPlant in InteractionPlants)
                    {
                        if (o.Equals(interactionPlant))
                        {
                            interactionPlant.tag = "Watered";
                            interactionPlant.SetColor(Utilities.Materials.Colors.CyanGlowing);
                            //interactionPlant.ShowInteractionSurfaceTable(false);
                            GameObject gameObjectForLine = GameObject.Find("Line for " + interactionPlant.name);
                            Destroy(gameObjectForLine);
                        }
                    }

                    if (InteractionPlants.All(interactionPlant => interactionPlant.tag == "Watered"))
                    {
                        UpdateConditionWithMatrix(ConditionAllPlantsWatered);
                    }
                }

                void ShowLightpathToPlant(Assistances.InteractionSurface plant)
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name,
                        DebugMessagesManager.MessageLevel.Info, "Called");

                    Vector3[] corners = PathFinderEngine.ComputePath(FollowObject.transform, plant.transform);

                    GameObject gameObjectForLine = new GameObject("Line for " + plant.name);
                    LineRenderer lineRenderer = gameObjectForLine.AddComponent<LineRenderer>();
                    lineRenderer.startWidth = 0.017f;
                    lineRenderer.endWidth = 0.017f;
                    lineRenderer.material = Resources.Load(Utilities.Materials.Colors.GreenGlowing, typeof(Material)) as Material;
                    lineRenderer.positionCount = 0;

                    lineRenderer.positionCount = corners.Length;

                    for (int i = 0; i < corners.Length; i++)
                    {
                        Vector3 corner = corners[i];

                        lineRenderer.SetPosition(i, corner);

                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Corner : " + corner);
                    }
                }

                void updateLightPath(Assistances.InteractionSurface plant)
                {
                    GameObject gameObjectForLine = GameObject.Find("Line for " + plant.name);
                    Destroy(gameObjectForLine);
                    ShowLightpathToPlant(plant);
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

                public void RemovePlant()
                {
                    if (InteractionPlants.Count() != 0)
                        InteractionPlants.RemoveAt(InteractionPlants.Count() - 1);
                }

                public void AddPlant(string name, Vector3 scaling, Vector3 position, string color, bool navMeshTag,
                    bool callbackOnTouch, bool registerObject, Transform parent)
                {
                    Assistances.InteractionSurface plant = new Assistances.InteractionSurface();
                    
                    // Set parent
                    plant = Assistances.Factory.Instance.CreateInteractionSurface(name, AdminMenu.Panels.Right, new Vector3(1.1f, 0.02f, 0.7f), new Vector3(-0.447f, -0.406f, 0.009f), Utilities.Materials.Colors.CyanGlowing, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, transform); ;
                    plant.gameObject.name = name;
                    // Add buttons to interface
                    AdminMenu.Instance.AddButton("Plante " + name + " - Bring",
                        delegate() { MATCH.Utilities.Utility.BringObject(plant.transform); }, AdminMenu.Panels.Left);
                    AdminMenu.Instance.AddSwitchButton("Plante " + name + " - Hide",
                        delegate()
                        {
                            MATCH.Utilities.Utility.ShowInteractionSurface(plant.transform,
                                !plant.gameObject.GetComponent<Renderer>().enabled);
                        }, AdminMenu.Panels.Left, AdminMenu.ButtonType.Hide);

                    // Set color
                    MATCH.Utilities.Utility.SetColor(plant.transform.transform, color);

                    // Set scaling and position
                    plant.transform.position = position;
                    plant.transform.localScale = scaling;

                    // Set the manipulation features
                    ObjectManipulator objectManipulator = plant.gameObject.AddComponent<ObjectManipulator>();
                    plant.gameObject.AddComponent<RotationAxisConstraint>().ConstraintOnRotation =
                        Microsoft.MixedReality.Toolkit.Utilities.AxisFlags.XAxis |
                        Microsoft.MixedReality.Toolkit.Utilities.AxisFlags.ZAxis;
                    BoundsControl boundsControl = plant.gameObject.AddComponent<BoundsControl>();
                    boundsControl.ScaleHandlesConfig.ScaleBehavior = Microsoft.MixedReality.Toolkit.UI
                        .BoundsControlTypes.HandleScaleMode.NonUniform;
                    boundsControl.TranslationHandlesConfig.ShowHandleForX = true;
                    boundsControl.TranslationHandlesConfig.ShowHandleForY = true;
                    boundsControl.TranslationHandlesConfig.ShowHandleForZ = true;

                    // Set optional features
                    if (navMeshTag)
                    {
                        plant.gameObject.AddComponent<NavMeshSourceTag>();
                    }

                    if (callbackOnTouch)
                    {
                        // As we will be adding the MouseAssistanceBasic, it requires to encapsulate the cube in an empty gameobject, and to rename the cube "Child"
                        Assistances.InteractionSurface child = plant;
                        child.name = "Child";
                        plant = new Assistances.InteractionSurface();
                        plant.gameObject.name = name;
                        child.transform.parent = plant.transform;

                        plant.gameObject.AddComponent<MATCH.Assistances.Basic>();
                    }

                    // Add the callbacks
                    boundsControl.ScaleStopped.AddListener(delegate { EventResized?.Invoke(plant, EventArgs.Empty); });

                    objectManipulator.OnManipulationEnded.AddListener(delegate(ManipulationEventData data)
                    {
                        EventMoved?.Invoke(plant, EventArgs.Empty);
                    });

                    InteractionPlants.Add(plant);

                    if (registerObject)
                    {
                        PlantsPositioningStorage.RegisterObject(name, plant.transform, plant.transform);
                    }

                    DialogAssistanceWaterHelp.AddButton(name, false, 0.12f);

                    int currentIndex = DialogAssistanceWaterHelp.ButtonsController.Count - 1;
                        
                    DialogAssistanceWaterHelp.ButtonsController[currentIndex].EventButtonClicked += delegate (System.Object o, EventArgs e)
                    {
                        UpdateTextAssistancesDebugWindow("i is : " + currentIndex);
                        if (DialogAssistanceWaterHelp.ButtonsController[currentIndex].IsChecked() == false)
                        {
                            InteractionPlants[currentIndex].CallbackShow();
                            ShowLightpathToPlant(InteractionPlants[currentIndex]);
                            DialogAssistanceWaterHelp.ButtonsController[currentIndex].CheckButton(true);
                            LightPathsShown[currentIndex] = true;
                            NextTimeCheck = Time.time + 5f;
                        }
                    };
                }
            }
        }
    }
}