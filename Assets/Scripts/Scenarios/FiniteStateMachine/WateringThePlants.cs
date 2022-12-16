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
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;
using System.Reflection;
using System.Linq;

namespace MATCH
{
    namespace Scenarios
    {
        namespace FiniteStateMachine
        {
            public class WateringThePlants : Scenario
            {
                public PathFinding.PathFinding m_pathFinderEngine;
                public Transform m_refInteractionSurface;
                public GameObject m_refCube;
                public MATCH.FiniteStateMachine.Display m_graphMain;
                public MATCH.FiniteStateMachine.Display m_graphHelp;
                public MATCH.FiniteStateMachine.Display m_graphPlants;
                public MATCH.Inferences.Manager m_inferenceManager;

                Assistances.Dialog m_dialogAssistanceWaterHelp; // Class variable as used in another function than the initialization of the scenario

                Transform m_pointOfReferenceForPaths;

                MATCH.FiniteStateMachine.Manager m_stateMachineMain;
                MATCH.FiniteStateMachine.Manager m_stateMachineAssistanceWateringPlants;

                EventHandler s_inference19h00;
                MATCH.Inferences.Time m_inference19h00;

                EventHandler s_dialogFirstUserFar;
                EventHandler s_dialogSecondUserFar;
                readonly EventHandler s_dialogThirdUserFar;
                EventHandler s_assistanceWaterNoPlantWatered;
                EventHandler s_assistanceWaterNeedHelp;
                EventHandler s_assistanceWaterNoNeedForHelp;

                int m_numberOfPlantsWatered = 0;

                PlantsManager m_plantsManager;

                MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistanceIntermediateState m_sIntermediateWateringPlants; // This particular state is a class variable as it is used in a callback

                private void Awake()
                {
                    // Initialize variables
                    SetId("Arroser les plantes");
                }

                // Start is called before the first frame update
                void Start()
                {
                    // Variables
                    m_stateMachineMain = new MATCH.FiniteStateMachine.Manager();
                    m_stateMachineAssistanceWateringPlants = new MATCH.FiniteStateMachine.Manager();

                    m_plantsManager = new PlantsManager(m_graphPlants, transform);

                    // Add buttons to admin menu
                    AdminMenu.Instance.AddButton("Add plant", CallbackAddPlant, AdminMenu.Panels.Left);
                    AdminMenu.Instance.AddButton("Clear plant paths", delegate ()
                    {
                        for (int i = 0; i < m_plantsManager.GetNbPlants(); i++)
                        {
                            m_plantsManager.GetLineRenderer(i).positionCount = 0;
                        }
                    }, AdminMenu.Panels.Left);


                    // Initialize scenario
                    InitializeScenario();
                }

                void CallbackAddPlant()
                {
                    int plantId = m_plantsManager.AddPlant();
                    Plant p = m_plantsManager.GetPlant(plantId);
                    p.s_moved += delegate (System.Object o, EventArgs e)
                    { // Maybe this part will have to be updated , in the case I add a functionality to remove plants (which is unlikely but we never know).
                    DrawLineForPlant(plantId);
                    };

                    // Adding the information to the state machine and the interactions
                    p.SetState(m_stateMachineMain.addNewAssistanceGradation(p.GetId()));
                    p.GetState().addFunctionShow(delegate (EventHandler e, bool animate)
                    { }, Utilities.Utility.GetEventHandlerEmpty(), true);
                    p.GetState().setFunctionHide(delegate (EventHandler e, bool animate)
                    {
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called for plant index " + plantId);

                        p.GetHighlight().Hide(Utilities.Utility.GetEventHandlerEmpty(), animate);

                    // Setting the color back to unwatered
                    p.GetHighlight().SetMaterial(Utilities.Materials.Colors.YellowGlowing);

                        e?.Invoke(this, EventArgs.Empty);
                    }, Utilities.Utility.GetEventHandlerEmpty(), true);

                    //EventHandler temp = m_sIntermediateWateringPlants.addState(p.getState());
                    p.GetInteractionSurface().EventInteractionSurfaceTableTouched += p.GotoStateHighlightWatered(); // If we remain in the default attention state, it will just not go there, so should be safe
                    p.GetInteractionSurface().EventInteractionSurfaceTableTouched += m_sIntermediateWateringPlants.addState(p.GetState());
                    p.GetInteractionSurface().EventInteractionSurfaceTableTouched += delegate (System.Object o, EventArgs e)
                    {
                        m_plantsManager.GetLineRenderer(plantId).positionCount = 0;
                    };

                    // Add plant to the GUI
                    m_dialogAssistanceWaterHelp.AddButton("Plante " + (plantId + 1), false);
                    m_dialogAssistanceWaterHelp.ButtonsController.Last().EventButtonClicked += delegate (System.Object o, EventArgs e)
                    {
                        if (m_dialogAssistanceWaterHelp.ButtonsController[plantId].IsChecked() == false)
                        {
                            DrawLineForPlant(plantId);
                        }
                    };
                }

                /**
                 * The code of this function is a mess ... to be restructured
                 * */
                void InitializeScenario()
                {
                    Manager.Instance.addScenario(this);
                    // Object required
                    m_pointOfReferenceForPaths = Instantiate(m_refInteractionSurface, transform);
                    Assistances.InteractionSurface interactionSurfaceController = m_pointOfReferenceForPaths.GetComponent<Assistances.InteractionSurface>();
                    interactionSurfaceController.SetColor(Utilities.Materials.Colors.PurpleGlowing);
                    interactionSurfaceController.SetScaling(new Vector3(0.6f, 0.01f, 0.4f));
                    m_pointOfReferenceForPaths.position = new Vector3(0.846999645f, 0.542999983f, 5.22099972f);
                    interactionSurfaceController.SetAdminButtons("plants", AdminMenu.Panels.Left);
                    interactionSurfaceController.ShowInteractionSurfaceTable(true);
                    interactionSurfaceController.SetPreventResizeY(true);
                    interactionSurfaceController.SetObjectResizable(true);

                    Assistances.Basic successController = Assistances.Factory.Instance.CreateCube(Utilities.Materials.Textures.Congratulation, m_pointOfReferenceForPaths);

                    MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistance sSuccess = m_stateMachineMain.addNewAssistanceGradation("Success");
                    sSuccess.setFunctionHideAndShow(successController);
                    sSuccess.addFunctionShow(delegate (EventHandler e, bool animate)
                    {
                        OnChallengeSuccess();
                    }, Utilities.Utility.GetEventHandlerEmpty(), true);

                    m_sIntermediateWateringPlants = m_stateMachineMain.addIntermediateState("WateringPlants", sSuccess);

                    m_dialogAssistanceWaterHelp = Assistances.Factory.Instance.CreateCheckListNoButton("", "Voici les plantes qu'il vous reste à arroser. Si vous touchez une des plantes, un chemin au sol vous y guidera.", m_pointOfReferenceForPaths);

                    // Setting 3 cubes to evaluate if the intermediate state thing works
                    CallbackAddPlant();
                    CallbackAddPlant();
                    CallbackAddPlant();

                    // The next lines are to fit with the "virtual room" stuff
                    Plant plant1 = m_plantsManager.GetPlant(0);
                    Plant plant2 = m_plantsManager.GetPlant(1);
                    Plant plant3 = m_plantsManager.GetPlant(2);

                    plant1.GetInteractionSurface().SetScaling(new Vector3(0.3f, 0.02f, 0.3f));
                    plant1.GetHighlight().SetScale(plant1.GetInteractionSurface().GetInteractionSurface().localScale.x, plant1.GetHighlight().GetChildTransform().localScale.y, plant1.GetInteractionSurface().GetInteractionSurface().localScale.z);
                    plant1.GetInteractionSurface().transform.position = new Vector3(3.31499958f, 0.347000003f, 5.22099972f);
                    plant2.GetInteractionSurface().SetScaling(new Vector3(0.3f, 0.02f, 0.3f));
                    plant2.GetHighlight().SetScale(plant2.GetInteractionSurface().GetInteractionSurface().localScale.x, plant2.GetHighlight().GetChildTransform().localScale.y, plant2.GetInteractionSurface().GetInteractionSurface().localScale.z);
                    plant2.GetInteractionSurface().transform.position = new Vector3(-1.38000059f, 0.35800001f, 1.05400014f);
                    plant3.GetInteractionSurface().SetScaling(new Vector3(0.3f, 0.02f, 0.3f));
                    plant3.GetHighlight().SetScale(plant3.GetInteractionSurface().GetInteractionSurface().localScale.x, plant3.GetHighlight().GetChildTransform().localScale.y, plant3.GetInteractionSurface().GetInteractionSurface().localScale.z);
                    plant3.GetInteractionSurface().transform.position = new Vector3(3.35499954f, 0.331999987f, 1.5990001f);

                    Transform faucetView = Instantiate(m_refInteractionSurface, transform);
                    Assistances.InteractionSurface faucetController = faucetView.GetComponent<Assistances.InteractionSurface>();
                    faucetController.SetColor(Utilities.Materials.Colors.CyanGlowing);
                    faucetController.SetScaling(new Vector3(0.1f, 0.1f, 0.1f));
                    faucetView.position = new Vector3(0.948000014f, 0.592000008f, 5.36800003f);
                    faucetController.SetAdminButtons("faucet");
                    faucetController.ShowInteractionSurfaceTable(true);
                    faucetController.SetPreventResizeY(false);
                    faucetController.SetObjectResizable(true);

                    // First dialog
                    Assistances.Dialog dialogFirst = Assistances.Factory.Instance.CreateDialogNoButton("", "Qu'est-ce qu'il est conseillé de faire en fin de journée quand il fait moins chaud?", m_pointOfReferenceForPaths);

                    // Second dialog
                    Assistances.Dialog dialogSecond = Assistances.Factory.Instance.CreateDialogNoButton("", "Il n'y a pas que vous qui avez soif!", m_pointOfReferenceForPaths);

                    // Third dialog
                    Assistances.Dialog dialogThird = Assistances.Factory.Instance.CreateDialogNoButton("", "Il est temps d'arroser vos plantes", m_pointOfReferenceForPaths);

                    // Dialog to provide assistance to water the plants
                    Assistances.Dialog dialogAssistanceWaterProposeHelp = Assistances.Factory.Instance.CreateDialogTwoButtons("", "Besoin d'aide?", "Oui", delegate (System.Object o, EventArgs e) { s_assistanceWaterNeedHelp?.Invoke(o, e); }, Assistances.Buttons.Button.ButtonType.Yes, "Non", delegate (System.Object o, EventArgs e) { s_assistanceWaterNoNeedForHelp?.Invoke(o, e); }, Assistances.Buttons.Button.ButtonType.No, m_pointOfReferenceForPaths);

                    //// Inferences
                    DateTime tempTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 19, 0, 0);
                    m_inference19h00 = new MATCH.Inferences.Time("19h watering plants", tempTime, CallbackWateringThePlants);
                    m_inferenceManager.RegisterInference(m_inference19h00);

                    //// Asssitance to water the plants state machine
                    MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistance sAssistanceWaterPlantsStandBy = m_stateMachineAssistanceWateringPlants.addNewAssistanceGradation("standBy");
                    sAssistanceWaterPlantsStandBy.addFunctionShow(delegate (EventHandler e, bool animate)
                    {
                        OnChallengeStandBy();
                    }, Utilities.Utility.GetEventHandlerEmpty(), true);

                    sAssistanceWaterPlantsStandBy.setFunctionHide(delegate (EventHandler e, bool animate)
                    { e?.Invoke(this, EventArgs.Empty); }, Utilities.Utility.GetEventHandlerEmpty(), true);

                    MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistance sAssistanceWaterStart = m_stateMachineAssistanceWateringPlants.addNewAssistanceGradation("ready");

                    sAssistanceWaterStart.addFunctionShow(delegate (EventHandler e, bool animate)
                    {
                        Inferences.Factory.Instance.CreateDistanceLeavingAndComingInferenceOneShot(m_inferenceManager, "AssistanceWateringLeavingAndComing", CallbackAssistanceWateringCheckIfPlantWatered, m_pointOfReferenceForPaths.gameObject);

                    }, Utilities.Utility.GetEventHandlerEmpty(), true);

                    sAssistanceWaterStart.setFunctionHide(delegate (EventHandler e, bool animate)
                    { e?.Invoke(this, EventArgs.Empty); }, Utilities.Utility.GetEventHandlerEmpty(), true);

                    MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistance sAssistanceWaterDialogFirst = m_stateMachineAssistanceWateringPlants.addNewAssistanceGradation("dialogFirst");
                    sAssistanceWaterDialogFirst.setFunctionHideAndShow(dialogAssistanceWaterProposeHelp);

                    MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistance sAssistanceWaterDialogSecond = m_stateMachineAssistanceWateringPlants.addNewAssistanceGradation("dialogSecond");
                    sAssistanceWaterDialogSecond.setFunctionHideAndShow(m_dialogAssistanceWaterHelp);

                    m_stateMachineAssistanceWateringPlants.setGradationInitial("standBy");

                    //// Main State machine
                    MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistance sStandBy = m_stateMachineMain.addNewAssistanceGradation("StandBy");
                    sStandBy.addFunctionShow(delegate (EventHandler e, bool animate)
                    {
                        m_inferenceManager.RegisterInference(m_inference19h00);
                    }, Utilities.Utility.GetEventHandlerEmpty(), true);

                    sStandBy.setFunctionHide(delegate (EventHandler e, bool animate)
                    {
                        e?.Invoke(this, EventArgs.Empty);
                    }, Utilities.Utility.GetEventHandlerEmpty(), true);

                    MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistance sDialogFirst = m_stateMachineMain.addNewAssistanceGradation("DialogFirst");
                    sDialogFirst.setFunctionHideAndShow(dialogFirst);
                    sDialogFirst.addFunctionShow(delegate (EventHandler e, bool animate)
                    {
                        Inferences.Factory.Instance.CreateDistanceComingAndLeavingInferenceOneShot(m_inferenceManager, "inferenceDistanceFirstDialog", s_dialogFirstUserFar, dialogSecond.gameObject);
                        OnChallengeStart();
                    }, Utilities.Utility.GetEventHandlerEmpty(), true);

                    MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistance sDialogSecond = m_stateMachineMain.addNewAssistanceGradation("DialogSecond");
                    sDialogSecond.setFunctionHideAndShow(dialogSecond);
                    sDialogSecond.addFunctionShow(delegate (EventHandler e, bool animate)
                    {
                        Inferences.Factory.Instance.CreateDistanceComingAndLeavingInferenceOneShot(m_inferenceManager, "inferenceDistanceSecondDialog", s_dialogSecondUserFar, dialogSecond.gameObject);
                    }, Utilities.Utility.GetEventHandlerEmpty(), true);

                    MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistance sDialogThird = m_stateMachineMain.addNewAssistanceGradation("DialogThird");
                    sDialogThird.setFunctionHideAndShow(dialogThird);
                    sDialogThird.addFunctionShow(delegate (EventHandler e, bool animate)
                    {
                        Inferences.Factory.Instance.CreateDistanceComingAndLeavingInferenceOneShot(m_inferenceManager, "inferenceDistanceThirdDialog", s_dialogThirdUserFar, dialogThird.gameObject);
                    }, Utilities.Utility.GetEventHandlerEmpty(), true);

                    // Set transitions and intermediate state
                    s_inference19h00 += sStandBy.goToState(sDialogFirst);
                    s_dialogFirstUserFar += sDialogFirst.goToState(sDialogSecond);
                    s_dialogSecondUserFar += sDialogSecond.goToState(sDialogThird);

                    faucetController.EventInteractionSurfaceTableTouched += sStandBy.goToState(m_sIntermediateWateringPlants);
                    faucetController.EventInteractionSurfaceTableTouched += sDialogFirst.goToState(m_sIntermediateWateringPlants);
                    faucetController.EventInteractionSurfaceTableTouched += sDialogSecond.goToState(m_sIntermediateWateringPlants);
                    faucetController.EventInteractionSurfaceTableTouched += sDialogThird.goToState(m_sIntermediateWateringPlants);
                    faucetController.EventInteractionSurfaceTableTouched += sAssistanceWaterPlantsStandBy.goToState(sAssistanceWaterStart); // Enabling the nested state machine

                    s_assistanceWaterNoPlantWatered += sAssistanceWaterStart.goToState(sAssistanceWaterDialogFirst);
                    foreach (EventHandler handler in m_plantsManager.GotoStateHighlight(/*s_assistanceWaterNeedHelp*/))
                    {
                        s_assistanceWaterNeedHelp += handler;
                    }

                    s_assistanceWaterNeedHelp += delegate (System.Object o, EventArgs e)
                    {
                    //MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Called - preparing lines display for the plants");

                    int indexPlant = 0;
                        for (indexPlant = 0; indexPlant < m_plantsManager.GetNbPlants(); indexPlant++)
                        {
                            LineRenderer line = m_plantsManager.GetLineRenderer(indexPlant);
                            Plant plant = m_plantsManager.GetPlant(indexPlant);

                            Assistances.Buttons.Basic plantButton = m_dialogAssistanceWaterHelp.ButtonsController[indexPlant];

                            bool plantAlreadyWatered = m_sIntermediateWateringPlants.checkStateCalled(plant.GetState());

                            plantButton.CheckButton(plantAlreadyWatered);

                        //plant.getInteractionSurface().m_eventInteractionSurfaceTableTouched += plant.gotoStateHighlightWatered();

                        // If the plant has already been watered, artifically triger the event so that the state of the plant goes to "highlightWatered"
                        if (plantAlreadyWatered)
                            {
                                plant.GetInteractionSurface().TriggerTouchEvent();
                            }

                            plant.GetInteractionSurface().EventInteractionSurfaceTableTouched += delegate (System.Object oo, EventArgs ee)
                            {
                            //MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Called for plant index " + indexPlant);

                            line.positionCount = 0;
                                plantButton.CheckButton(true);
                            };
                        }
                    };
                    s_assistanceWaterNeedHelp += sAssistanceWaterDialogFirst.goToState(sAssistanceWaterDialogSecond);
                    s_assistanceWaterNoNeedForHelp += sAssistanceWaterDialogFirst.goToState(sAssistanceWaterPlantsStandBy);

                    m_sIntermediateWateringPlants.s_eventNextState += sAssistanceWaterDialogFirst.goToState(sAssistanceWaterPlantsStandBy);
                    m_sIntermediateWateringPlants.s_eventNextState += sAssistanceWaterDialogSecond.goToState(sAssistanceWaterPlantsStandBy);
                    foreach (EventHandler handler in m_plantsManager.GotoStateDefaultFromHighlightWatered())
                    {
                        m_sIntermediateWateringPlants.s_eventNextState += handler;
                    }
                    m_sIntermediateWateringPlants.s_eventNextState += sAssistanceWaterStart.goToState(sAssistanceWaterPlantsStandBy);
                    successController.s_touched += sSuccess.goToState(sStandBy);

                    // Initial state
                    m_stateMachineMain.setGradationInitial("StandBy");

                    // Display graphs
                    m_graphMain.SetManager(m_stateMachineMain);
                    m_graphHelp.SetManager(m_stateMachineAssistanceWateringPlants);
                    m_plantsManager.DisplayGraphLastPlant();

                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Current state: " + m_stateMachineMain.getGradationCurrent());
                }

                void DrawLineForPlant(int index)
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called");

                    Plant p = m_plantsManager.GetPlant(index);

                    Vector3[] corners = m_pathFinderEngine.ComputePath(m_pointOfReferenceForPaths, p.GetInteractionSurface().transform);

                    LineRenderer lineRenderer = m_plantsManager.GetLineRenderer(index);
                    lineRenderer.positionCount = corners.Length;

                    for (int i = 0; i < corners.Length; i++)
                    {
                        Vector3 corner = corners[i];

                        lineRenderer.SetPosition(i, corner);

                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Corner : " + corner);
                    }
                }

                /*
                 * This callback is aimed to be called when the person left to water plants and comes back.
                 * If the person did not water plants in the meantime, then an event is trigerred
                 * */
                void CallbackAssistanceWateringCheckIfPlantWatered(System.Object o, EventArgs e)
                {
                    if (m_sIntermediateWateringPlants.getNbOfStatesWhoCalled() == m_numberOfPlantsWatered)
                    { // Then no plants have been watered > trigger event to request additional assistance
                        s_assistanceWaterNoPlantWatered?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    { // At least one plant has been watered > update the number of watered plants
                        m_numberOfPlantsWatered = m_sIntermediateWateringPlants.getNbOfStatesWhoCalled();
                        Inferences.Factory.Instance.CreateDistanceLeavingAndComingInferenceOneShot(m_inferenceManager, "AssistanceWateringLeavingAndComing", CallbackAssistanceWateringCheckIfPlantWatered, m_pointOfReferenceForPaths.gameObject);
                    }
                }

                void CallbackWateringThePlants(System.Object o, EventArgs e)
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called");

                    m_inferenceManager.UnregisterInference("19h watering plants");
                    s_inference19h00?.Invoke(o, e);
                }

                public MATCH.Inferences.Time GetInference()
                {
                    return (MATCH.Inferences.Time)m_inferenceManager.GetInference("19h watering plants");
                }
            }

            public class PlantsManager
            {
                private readonly List<Plant> m_plants;
                private readonly List<LineRenderer> m_plantsLines;
                private readonly Transform m_parent;
                private readonly MATCH.FiniteStateMachine.Display m_graph;

                public PlantsManager(MATCH.FiniteStateMachine.Display graph, Transform parent)
                {
                    m_plants = new List<Plant>();
                    m_plantsLines = new List<LineRenderer>();

                    m_graph = graph;

                    m_parent = parent;
                }

                /**
                 * Returns the id of the plant
                 * */
                public int AddPlant()
                {

                    string plantId = "Plante " + (m_plants.Count + 1).ToString(); //+1 because the new plant is not yet added to the list of plants
                    Plant plant = new Plant(plantId, m_parent);
                    m_plants.Add(plant);
                    int indexPlant = m_plants.Count - 1;

                    GameObject gameObjectForLine = new GameObject("Line for " + plantId);
                    gameObjectForLine.transform.parent = m_parent;
                    LineRenderer lineRenderer = gameObjectForLine.AddComponent<LineRenderer>();
                    lineRenderer.startWidth = 0.017f;
                    lineRenderer.endWidth = 0.017f;
                    lineRenderer.material = Resources.Load(Utilities.Materials.Colors.GreenGlowing, typeof(Material)) as Material;
                    lineRenderer.positionCount = 0;

                    m_plantsLines.Add(lineRenderer);

                    return indexPlant;
                }

                public Plant GetPlant(int index)
                {
                    return m_plants[index];
                }

                public LineRenderer GetLineRenderer(int index)
                {
                    return m_plantsLines[index];
                }

                public int GetNbPlants()
                {
                    return m_plants.Count;
                }

                public List<EventHandler> GotoStateHighlight()
                {
                    List<EventHandler> handlers = new List<EventHandler>();

                    foreach (Plant p in m_plants)
                    {
                        handlers.Add(p.GotoStateHighlight());
                    }

                    return handlers;
                }

                public List<EventHandler> GotoStateDefaultFromHighlightWatered()
                {
                    List<EventHandler> handlers = new List<EventHandler>();

                    foreach (Plant p in m_plants)
                    {
                        handlers.Add(p.GotoStateDefaultFromHighlightWatered());
                    }

                    return handlers;
                }

                public void DisplayGraphLastPlant()
                {
                    m_plants.Last().DisplayGraph(m_graph);
                }
            }

            public class Plant
            {
                readonly MATCH.FiniteStateMachine.Manager m_grabAttentionManager;

                readonly Assistances.InteractionSurface InteractionSurface;
                readonly Assistances.Basic Highlight;

                MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistance State; // State to be used in the main state machine

                readonly string Id;

                public EventHandler s_moved;
                public EventHandler s_scaled;

                public Plant(string id, Transform parent)
                {
                    // Variables
                    m_grabAttentionManager = new MATCH.FiniteStateMachine.Manager();

                    Id = id;

                    //Todo: here the initial position is set to 0,0,0. But this is not comptaible with WLT, because the local position is modified elsewhere. So if this scenario has to be compatible with WLT, you will have to update this part
                    InteractionSurface = Assistances.Factory.Instance.CreateInteractionSurface(id, AdminMenu.Panels.Left, new Vector3(0.1f, 0.1f, 0.1f), new Vector3(0,0,0), Utilities.Materials.Colors.YellowGlowing, true, true, delegate (System.Object o, EventArgs e)
                    {
                        s_moved?.Invoke(this, EventArgs.Empty);
                    }, true, parent);

                    Highlight = Assistances.Factory.Instance.CreateCube(Utilities.Materials.Colors.YellowGlowing, false, new Vector3(InteractionSurface.GetInteractionSurface().localScale.x, 0.6f, InteractionSurface.GetInteractionSurface().localScale.z), new Vector3(0, -0.35f, 0), false, InteractionSurface.transform);

                    InteractionSurface.EventInteractionSurfaceScaled += delegate
                    {
                        Highlight.SetScale(InteractionSurface.GetInteractionSurface().localScale.x,
                            Highlight.GetChildTransform().localScale.y,
                            InteractionSurface.GetInteractionSurface().localScale.z);
                    };

                    MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistance sDefault = m_grabAttentionManager.addNewAssistanceGradation("default");
                    MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistance sHighlight = m_grabAttentionManager.addNewAssistanceGradation("highlight");
                    MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistance sHighlightWatered = m_grabAttentionManager.addNewAssistanceGradation("highlightWatered");

                    sDefault.addFunctionShow(delegate (EventHandler e, bool animate)
                    {
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Plant " + Id + " is going to default state");
                    }, Utilities.Utility.GetEventHandlerEmpty(), true);

                    sDefault.setFunctionHide(delegate (EventHandler e, bool animate)
                    {
                        e?.Invoke(this, EventArgs.Empty);
                    }, Utilities.Utility.GetEventHandlerEmpty(), true);

                    sHighlight.addFunctionShow(delegate (EventHandler e, bool animate)
                    {
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Plant " + Id + " is going to be highlighted");

                        Highlight.Show(Utilities.Utility.GetEventHandlerEmpty(), animate);
                    }, Utilities.Utility.GetEventHandlerEmpty(), true);

                    sHighlight.setFunctionHide(delegate (EventHandler e, bool animate)
                    {
                        e?.Invoke(this, EventArgs.Empty);
                    }, Utilities.Utility.GetEventHandlerEmpty(), true);

                    sHighlightWatered.addFunctionShow(delegate (EventHandler e, bool animate)
                    {
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Plant " + Id + " has been watered");

                        Highlight.SetMaterial(Utilities.Materials.Colors.GreenGlowing);
                    }, Utilities.Utility.GetEventHandlerEmpty(), true);

                    sHighlightWatered.setFunctionHide(delegate (EventHandler e, bool animate)
                    {
                        Highlight.SetMaterial(Utilities.Materials.Colors.YellowGlowing);
                        Highlight.Hide(Utilities.Utility.GetEventHandlerEmpty(), animate);

                        e?.Invoke(this, EventArgs.Empty);
                    }, Utilities.Utility.GetEventHandlerEmpty(), true);

                    m_grabAttentionManager.setGradationInitial("default");
                }

                public string GetId()
                {
                    return Id;
                }

                public Assistances.InteractionSurface GetInteractionSurface()
                {
                    return InteractionSurface;
                }

                public Assistances.Basic GetHighlight()
                {
                    return Highlight;
                }

                public EventHandler GotoStateHighlight()
                {
                    return GetState("default").goToState(GetState("highlight"));
                }

                public EventHandler GotoStateHighlightWatered()
                {
                    MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistance state = GetState("highlightWatered");

                    return GetState("highlight").goToState(state);
                }

                public EventHandler GotoStateDefaultFromHighlight()
                {
                    return GetState("default").goToState(GetState("highlight"));
                }

                public EventHandler GotoStateDefaultFromHighlightWatered()
                {
                    return GetState("highlightWatered").goToState(GetState("default"));
                }

                MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistance GetState(string id)
                {
                    return (MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistance)m_grabAttentionManager.getAssistance(id);
                }

                public void SetState(MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistance state)
                {
                    State = state;
                }

                public MATCH.FiniteStateMachine.MouseUtilitiesGradationAssistance GetState()
                {
                    return State;
                }

                public void DisplayGraph(MATCH.FiniteStateMachine.Display graph)
                {
                    graph.SetManager(m_grabAttentionManager);
                }
            }
        }
    }
}
