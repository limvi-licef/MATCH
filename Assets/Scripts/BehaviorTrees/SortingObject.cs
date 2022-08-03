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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPBehave;
using System;
using System.Reflection;

namespace MATCH
{
    namespace BehaviorTrees
    {
        public class SortingObject : MonoBehaviour
        {
            private NPBehave.Root Tree;
            private Blackboard Conditions;
            public Inferences.Manager InferenceManager;

            Assistances.InteractionSurface AreaStorage;
            Assistances.InteractionSurface AreaObject;

            Assistances.Manager AssistancesManager;

            readonly string ObjectOfInterestName = "cup";

            //Inferences.ObjectInInteractionSurface InferenceObjectInStorage;
            Inferences.GameObjectInInteractionSurface InferenceGameObjectInStorage;
            Inferences.ObjectDetected InferenceObjectDetected;
            Inferences.GameObjectGrabbed InferenceGrabbedObject;
            Inferences.GameObjectReleased InferenceReleasedObject;
            Inferences.ObjectFocused InferenceFocusedOnObject;
            Inferences.TimeDidNotComeToObject InferenceTimeDidNotComeToObject;

            bool ObjectDetected;
            Utilities.PhysicalObjectInformation ObjectDetectedInformation;

            GameObject FakeObject;

            Assistances.InteractionSurface AssistancesAlphaInteractionSurface;
            Assistances.AssistanceGradationExplicit AssistancesAlphaGradation;
            //GameObject AssistancesAlphaGo;

            // Start is called before the first frame update
            void Start()
            {
                /*AssistancesAlphaGo = new GameObject("Assistances alpha");
                AssistancesAlphaGo.transform.parent = transform;
                AssistancesAlphaGo.name = "Assistances Alpha - interaction surface";*/

                FakeObject = transform.Find("FakeObject").gameObject;

                ObjectDetectedInformation = null;
                AssistancesManager = new Assistances.Manager();

                AreaStorage = Assistances.Factory.Instance.CreateInteractionSurface("Storage", default, new Vector3(0.2f, 0.8f, 0.3f), "Mouse_Green_Glowing", true, true, Utilities.Utility.GetEventHandlerEmpty(), transform);
                AreaStorage.GetInteractionSurface().position = new Vector3(3.38f, 0.22f, 3.19f); 
                AreaObject = Assistances.Factory.Instance.CreateInteractionSurface("Object", default, new Vector3(0.1f, 0.1f, 0.1f), "Mouse_Yellow_Glowing", true, true, Utilities.Utility.GetEventHandlerEmpty(), transform);
                AreaObject.GetInteractionSurface().gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                AreaObject.GetInteractionSurface().position = new Vector3(1000, 1000, 1000);//new Vector3(-0.61f, 0.46f, 5.18f);

                Assistances.Basic successController = Assistances.Factory.Instance.CreateCube("Mouse_Congratulation", AreaStorage.transform);
                //Assistances.Basic exclamationMark = Assistances.Factory.Instance.CreateCube("Mouse_Exclamation_Red", true, new Vector3(0.1f, 0.1f, 0.1f), new Vector3(0, 0, 0), true, m_object.GetInteractionSurface().transform);
                successController.Show(Utilities.Utility.GetEventHandlerEmpty());

                AssistancesManager.AddAssistance("Success", successController);

                InitializeScenario();
            }

            void ActionDisplaySuccess()
            {
                Debug.Log("Assistances Epsilon / Object stored");
                
            }

            void InitializeScenario()
            {
                Conditions = new Blackboard(UnityContext.GetClock());
                Conditions["ObjectStored"] = false;
                Conditions["PersonMovedAwayFromObject"] = false;
                Conditions["PersonCloseToObject"] = false;
                Conditions["PersonGrabbedObject"] = false;
                Conditions["PersonWatchedObject"] = false;
                Conditions["PersonDroppedObjectOutsideStoringArea"] = false;
                Conditions["PersonDidNotComeToObject"] = false;

                InferenceObjectDetected = new Inferences.ObjectDetected("SortingObjectDetectionObject", CallbackObjectDetected, ObjectOfInterestName);
                InferenceObjectDetected.EnableFakeObjectDetection(FakeObject);
                InferenceManager.RegisterInference(InferenceObjectDetected);

                //MATCH.Inferences.Time inferenceObjectStored = new Inferences.Time("Object sorted",
                //    new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 19, 35, 0));
                //inferenceObjectStored.AddCallback(CallbackObjectStored);
                //InferenceObjectInStorage = new Inferences.ObjectInInteractionSurface("Transport", CallbackDetectedInStorage, ObjectOfInterestName, AreaStorage);
                //InferenceManager.RegisterInference(InferenceObjectInStorage);
                InferenceGameObjectInStorage = new Inferences.GameObjectInInteractionSurface("GameObjectStorage", CallbackGameObjectDetectedInStorage, FakeObject, AreaStorage);
                InferenceManager.RegisterInference(InferenceGameObjectInStorage);

                RegisterInferenceComing();

                RegisterInferenceComingAndLeaving();

                InferenceGrabbedObject = new Inferences.GameObjectGrabbed("ObjectGrabbed", CallbackPersonGrabbedObject, FakeObject);
                InferenceManager.RegisterInference(InferenceGrabbedObject);

                InferenceReleasedObject = new Inferences.GameObjectReleased("ObjectReleased", CallbackPersonReleasedObject, FakeObject);
                InferenceManager.RegisterInference(InferenceReleasedObject);

                InferenceFocusedOnObject = new Inferences.ObjectFocused("FocusedOnObject", CallbackPersonWatchedObject, FakeObject, 3);
                InferenceManager.RegisterInference(InferenceFocusedOnObject);

                InferenceTimeDidNotComeToObject = new Inferences.TimeDidNotComeToObject("DidNotComeToObject", CallbackPersonDidNotComeToObject, FakeObject, 15);
                InferenceManager.RegisterInference(InferenceTimeDidNotComeToObject);

                /**
                 * The BT below corresponds to the iteration-9 of the BT in the diagram file
                 * */
                Sequence seObjectDroppedOutsideStorageArea = new Sequence(
                    new NPBehave.Action(() => Debug.Log("Assistances Delta")),
                    new WaitUntilStopped());

                BlackboardCondition cObjectDroppedOutsideStorageArea = new BlackboardCondition("PersonDroppedObjectOutsideStoringArea",
                    Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, seObjectDroppedOutsideStorageArea);

                Sequence sePersonTakeObject = new Sequence(
                    //cObjectDroppedOutsideStorageArea,
                    //new Sequence(
                        new NPBehave.Action(() => Debug.Log("Hide all assistances")),
                        new WaitUntilStopped());

                BlackboardCondition cDidPersonTakeObject = new BlackboardCondition("PersonGrabbedObject", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, sePersonTakeObject);

                Sequence sePersonMovedAwayFromObject = new Sequence(
                    /*new NPBehave.Action(() => Debug.Log("Assistances Alpha")*/ new NPBehave.Action(()=>RunAssistancesAlpha()),
                    new WaitUntilStopped());

                BlackboardCondition cDidPersonMoveAwayFromObject = new BlackboardCondition("PersonMovedAwayFromObject", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, sePersonMovedAwayFromObject);

                Sequence sePersonApproachedObject = new Sequence(
                    cDidPersonTakeObject
                    //cDidPersonMoveAwayFromObject
                    );

                BlackboardCondition cDidPersonApproachObject = new BlackboardCondition("PersonCloseToObject", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, new Sequence(sePersonApproachedObject));

                Sequence sePersonLookedAtObject = new Sequence(
                    new NPBehave.Action(() => Debug.Log("Assistances Beta")),
                    new WaitUntilStopped());

                BlackboardCondition cDidPersonLookAtObject = new BlackboardCondition("PersonWatchedObject", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, sePersonLookedAtObject);

                Sequence sePersonDidNotCameToObjectSinceAWhile = new Sequence(
                    new NPBehave.Action(() => Debug.Log("Assistances Gamma")),
                    new WaitUntilStopped());

                BlackboardCondition cDidPersonDidNotComeToTheObjectSinceAWhile = new BlackboardCondition("PersonDidNotComeToObject", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, sePersonDidNotCameToObjectSinceAWhile);

                /*Selector srDidPersonLookAtObject = new Selector(
                    cDidPersonLookAtObject,
                    cDidPersonDidNotComeToTheObjectSinceAWhile);*/

                Selector srObjectIsNotStored = new Selector(
                    cDidPersonApproachObject,
                    cObjectDroppedOutsideStorageArea,
                    cDidPersonMoveAwayFromObject,
                    cDidPersonLookAtObject,
                    cDidPersonDidNotComeToTheObjectSinceAWhile
                    //srDidPersonLookAtObject
                    );

                Sequence seObjectIsStored = new Sequence(
                    new NPBehave.Action(() => AssistancesManager.HideAllBut("Success")),
                    new WaitUntilStopped()
                 );

                BlackboardCondition cIsObjectStored = new BlackboardCondition("ObjectStored", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, seObjectIsStored);

                Selector srBegin = new Selector(
                    cIsObjectStored,
                    srObjectIsNotStored
                    );

                // Set assistances
                InitializeAssistancesAlpha();

                // Run tree
                Tree = new Root(Conditions, srBegin);

                //#if UNITY_EDITOR
                NPBehave.Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
                debugger.BehaviorTree = Tree;
                //#endif

                Tree.Start();
            }

            void RunAssistancesAlpha()
            {
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Assistances alpha");

                AssistancesAlphaGradation.RunAssistance(Utilities.Utility.GetEventHandlerEmpty());
            }

            void InitializeAssistancesAlpha()
            {
                AssistancesAlphaInteractionSurface = Assistances.Factory.Instance.CreateInteractionSurface("AssistancesAlpha", AdminMenu.Panels.Obstacles, new Vector3(0.5f, 0.05f, 0.5f), "Mouse_Orange_Glowing", true, true, Utilities.Utility.GetEventHandlerEmpty(), transform);
                AssistancesAlphaInteractionSurface.transform.localPosition = new Vector3(0, 0.464f, 1.632f); //new Vector3(0.85f, 0.46f, 2.18f);
                AssistancesAlphaInteractionSurface.name = "Assistances Alpha - Interaction surface";


                //Assistances.Basic cube = ;
                Assistances.AssistanceGradationAttention exclamationMark = new Assistances.AssistanceGradationAttention();

                Assistances.Basic cube = Assistances.Factory.Instance.CreateCube("Mouse_Purple_Glowing", AssistancesAlphaInteractionSurface.transform);
                //exclamationMark.AddAssistance(cube);
                /*Assistances.IAssistance exclamationMarkPale = */exclamationMark.AddAssistance(new Assistances.Decorators.Material(cube, "Mouse_Exclamation"));
                //exclamationMark.AddAssistance(Assistances.Decorators.)
                exclamationMark.AddAssistance(new Assistances.Decorators.Material(cube, "Mouse_Exclamation_Red"));

                Assistances.AssistanceGradationAttention end = new Assistances.AssistanceGradationAttention();
                end.AddAssistance(Assistances.Factory.Instance.CreateDialogNoButton("", "Ok! We let you do.", AssistancesAlphaInteractionSurface.transform));

                Assistances.AssistanceGradationAttention yesNo1 = new Assistances.AssistanceGradationAttention();
                yesNo1.AddAssistance(Assistances.Factory.Instance.CreateDialogNoButton("", "Don't you think there is something to do here?", AssistancesAlphaInteractionSurface.transform));

                AssistancesAlphaGradation = Assistances.Factory.Instance.CreateAssistanceGradationExplicit();
                AssistancesAlphaGradation.InfManager = InferenceManager;

                AssistancesAlphaGradation.AddButton(exclamationMark, Assistances.Buttons.Button.ButtonType.Yes, yesNo1);
                AssistancesAlphaGradation.AddButton(exclamationMark, Assistances.Buttons.Button.ButtonType.No, end);
                //AssistancesAlphaGradation.AddButton(yesNo1, Assistances.Buttons.Button.ButtonType.Undefined, null);
            }

            void RegisterInferenceComing()
            {
                //Conditions["PersonCloseToObject"] = false;
                Inferences.Factory.Instance.CreateDistanceComingInferenceOneShot(InferenceManager, "CloseToObject", CallbackPersonCloseToObject, FakeObject);
            }

            void RegisterInferenceComingAndLeaving()
            {
                Conditions["PersonMovedAwayFromObject"] = false;
                Inferences.Factory.Instance.CreateDistanceComingAndLeavingInferenceOneShot(InferenceManager, "PassingByObject", CallbackPersonMovedAwayFromObject, FakeObject);
            }

            void CallbackObjectDetected(System.Object o, EventArgs e)
            {
                //InferenceManager.UnregisterInference(InferenceObjectDetected);
                ObjectDetected = true;
                ObjectDetectedInformation = ((Utilities.EventHandlerArgs.EventHandlerArgObject)e).ObjectDetected;
            }

            void CallbackGameObjectDetectedInStorage(System.Object o, EventArgs e)
            {
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "FakeObject stored!");

                InferenceManager.UnregisterInference(InferenceGameObjectInStorage);
                Conditions["ObjectStored"] = true;
                Conditions["PersonDroppedObjectOutsideStoringArea"] = false;
            }

            void CallbackObjectStored(System.Object o, EventArgs e)
            {
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called");
                Conditions["ObjectStored"] = !(bool)Conditions["ObjectStored"];
            }

            void CallbackPersonCloseToObject(System.Object o, EventArgs e)
            {
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Person close to object");
                Conditions["PersonCloseToObject"] = true;
                Conditions["PersonDidNotComeToObject"] = false;
                Conditions["PersonMovedAwayFromObject"] = false;
                RegisterInferenceComingAndLeaving();
            }

            void CallbackPersonMovedAwayFromObject(System.Object o, EventArgs e)
            {
                if (ObjectDetected)
                {
                    RegisterInferenceComing();
                    //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "User passed by object");
                    Conditions["PersonMovedAwayFromObject"] = true;
                    Conditions["PersonCloseToObject"] = false;
                    Conditions["PersonDroppedObjectOutsideStoringArea"] = false;
                }
            }

            void CallbackPersonGrabbedObject(System.Object o, EventArgs e)
            {
                InferenceManager.UnregisterInference(InferenceGrabbedObject);
                InferenceManager.RegisterInference(InferenceReleasedObject);
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Object grabbed");
                Conditions["PersonGrabbedObject"] = true;
            }

            void CallbackPersonReleasedObject(System.Object o, EventArgs e)
            {
                InferenceManager.UnregisterInference(InferenceReleasedObject);
                InferenceManager.RegisterInference(InferenceGrabbedObject);
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Object released");
                Conditions["PersonDroppedObjectOutsideStoringArea"] = true;
                Conditions["PersonGrabbedObject"] = false;
            }

            void CallbackPersonWatchedObject(System.Object o, EventArgs e)
            {
                InferenceManager.UnregisterInference(InferenceFocusedOnObject);
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Person focused on object for at 3 seconds");
                Conditions["PersonWatchedObject"] = true;
            }

            void CallbackPersonDidNotComeToObject(System.Object o, EventArgs e)
            {
                InferenceManager.UnregisterInference(InferenceTimeDidNotComeToObject);
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called");
                Conditions["PersonDidNotComeToObject"] = true;//!(bool)Conditions["PersonDidNotComeToObject"];
                Conditions["PersonCloseToObject"] = false;
            }

            void CallbackPersonDroppedObjectOutsideStoringArea(System.Object o, EventArgs e)
            {
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called");
                Conditions["PersonDroppedObjectOutsideStoringArea"] = !(bool)Conditions["PersonDroppedObjectOutsideStoringArea"];
            }
        }

        
    }
}
