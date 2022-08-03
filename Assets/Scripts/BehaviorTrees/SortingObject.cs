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
            Assistances.InteractionSurface AreaObjectPosition;

            Assistances.Manager AssistancesManager;

            Assistances.Basic successController;

            readonly string ObjectOfInterestName = "cup";

            Inferences.ObjectInInteractionSurface InferenceObjectInStorage;
            Inferences.ObjectOutInteractionSurface InferenceObjectOutAreaObject;
            Inferences.GameObjectInInteractionSurface InferenceGameObjectInStorage;
            Inferences.ObjectDetected InferenceObjectDetected;
            Inferences.GameObjectGrabbed InferenceGrabbedObject;
            Inferences.GameObjectReleased InferenceReleasedObject;
            Inferences.ObjectFocused InferenceFocusedOnObject;
            Inferences.TimeDidNotComeToObject InferenceTimeDidNotComeToObject;
            


            bool ObjectDetected = false;
            bool ObjectSet = false;
            Utilities.PhysicalObjectInformation ObjectDetectedInformation;

            GameObject FakeObject;

			Assistances.InteractionSurface AssistancesAlphaInteractionSurface;
            Assistances.AssistanceGradationExplicit AssistancesAlphaGradation;																  
            // Start is called before the first frame update
            void Start()
            {
                FakeObject = transform.Find("FakeObject").gameObject;

                ObjectDetectedInformation = null;
                AssistancesManager = new Assistances.Manager();

                AreaStorage = Assistances.Factory.Instance.CreateInteractionSurface("Storage", default, new Vector3(0.2f, 0.8f, 0.3f), "Mouse_Green_Glowing", true, true, Utilities.Utility.GetEventHandlerEmpty(), transform);
                if (Utilities.Utility.IsEditorSimulator() || Utilities.Utility.IsEditorGameView())
                {
                    AreaStorage.transform.position = new Vector3(3.38f, 0.22f, 3.19f);
                }
                else
                {
                    AreaStorage.SetLocalPosition(new Vector3(0f, 0f, 0.2f));
                }

                AreaObject = Assistances.Factory.Instance.CreateInteractionSurface("Object", default, new Vector3(0.4f, 0.4f, 0.4f), "Mouse_Yellow_Glowing", true, true, Utilities.Utility.GetEventHandlerEmpty(), transform);       
                //AreaObject.GetInteractionSurface().gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                AreaObject.transform.position = new Vector3(1000, 1000, 1000);//new Vector3(-0.61f, 0.46f, 5.18f);
                //AreaObject.GetInteractionSurface().transform.position = new Vector3(1000, 1000, 1000);

                AreaObjectPosition = Assistances.Factory.Instance.CreateInteractionSurface("Object_Position", default, new Vector3(0.05f, 0.05f, 0.05f), "Mouse_Yellow_Glowing", true, true, Utilities.Utility.GetEventHandlerEmpty(), transform);
                AreaObjectPosition.GetInteractionSurface().gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                //AreaObjectPosition.GetInteractionSurface().transform.position = new Vector3(1000, 1000, 1000);
                AreaObjectPosition.transform.position = new Vector3(1000, 1000, 1000);
                
                AdminMenu.Instance.addButton("Sorting Object - See boolean states", CallbackSeeStates);

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
                if (Utilities.Utility.IsEditorSimulator() || Utilities.Utility.IsEditorGameView())
                {
                    InferenceObjectDetected.EnableFakeObjectDetection(FakeObject);
                }
                InferenceManager.RegisterInference(InferenceObjectDetected);

                //MATCH.Inferences.Time inferenceObjectStored = new Inferences.Time("Object sorted",
                //    new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 19, 35, 0));
                //inferenceObjectStored.AddCallback(CallbackObjectStored);
                //InferenceObjectInStorage = new Inferences.ObjectInInteractionSurface("Transport", CallbackDetectedInStorage, ObjectOfInterestName, AreaStorage);
                //InferenceManager.RegisterInference(InferenceObjectInStorage);
                InferenceGameObjectInStorage = new Inferences.GameObjectInInteractionSurface("GameObjectStorage", CallbackGameObjectDetectedInStorage, FakeObject, AreaStorage);
                InferenceManager.RegisterInference(InferenceGameObjectInStorage);
                InferenceObjectInStorage = new Inferences.ObjectInInteractionSurface("ObjectStorage", CallbackGameObjectDetectedInStorage, ObjectOfInterestName, AreaStorage);
                InferenceManager.RegisterInference(InferenceObjectInStorage);


                //AreaObject.GetComponent<Interactable>().GetReceiver<InteractableOnTouchReceiver>().OnTouchStart.AddListener.CallbackPersonGrabbedObject;
                AreaObject.TriggerTouchEvent();
                AreaObject.EventInteractionSurfaceTableTouched += CallbackPersonGrabbedObject;

                RegisterInferenceComing();
                //RegisterInferenceLeaving();
                RegisterInferenceComingAndLeaving();

                if (Utilities.Utility.IsEditorSimulator() || Utilities.Utility.IsEditorGameView())
                {
                    InferenceGrabbedObject = new Inferences.GameObjectGrabbed("ObjectGrabbed", CallbackPersonGrabbedObject, FakeObject);
                    InferenceManager.RegisterInference(InferenceGrabbedObject);

                    InferenceReleasedObject = new Inferences.GameObjectReleased("ObjectReleased", CallbackPersonReleasedObject, FakeObject);
                    InferenceManager.RegisterInference(InferenceReleasedObject);

                    InferenceFocusedOnObject = new Inferences.ObjectFocused("FocusedOnObject", CallbackPersonWatchedObject, FakeObject, 3);
                    InferenceManager.RegisterInference(InferenceFocusedOnObject);

                    InferenceTimeDidNotComeToObject = new Inferences.TimeDidNotComeToObject("DidNotComeToObject", CallbackPersonDidNotComeToObject, FakeObject, 15);
                    InferenceManager.RegisterInference(InferenceTimeDidNotComeToObject);
                }
                else
                {
                    //InferenceObjectOutAreaObject = new Inferences.ObjectOutInteractionSurface("ObjectGrabbed", CallbackPersonGrabbedObject, ObjectOfInterestName, AreaObject);
                    //InferenceManager.RegisterInference(InferenceObjectOutAreaObject);

                    //InferenceReleasedObject = new Inferences.GameObjectReleased("ObjectReleased", CallbackPersonReleasedObject, FakeObject); //CHANGE THIS INFERENCE
                    //InferenceManager.RegisterInference(InferenceReleasedObject);

                    InferenceFocusedOnObject = new Inferences.ObjectFocused("FocusedOnObject", CallbackPersonWatchedObject, AreaObject.GetInteractionSurface().gameObject, 3);
                    InferenceManager.RegisterInference(InferenceFocusedOnObject);

                    InferenceTimeDidNotComeToObject = new Inferences.TimeDidNotComeToObject("DidNotComeToObject", CallbackPersonDidNotComeToObject, AreaObject.GetInteractionSurface().gameObject, 15);
                    //InferenceManager.RegisterInference(InferenceTimeDidNotComeToObject);
                }

                /*
                 * Assistances
                 */
                Assistances.QandDAssistances AssistancesGradation = new Assistances.QandDAssistances();

                Assistances.Basic AssistancesAlpha = Assistances.Factory.Instance.CreateCube("Mouse_Congratulation", AreaStorage.transform);
                //Assistances.QandDAssistances AssistancesAlphaGradation = new Assistances.QandDAssistances();
                AssistancesGradation.AddAssistance(Assistances.QandDAssistances.Gradation.Alpha, AssistancesAlpha);

                Assistances.Dialog AssistancesBeta = Assistances.Factory.Instance.CreateDialogNoButton("Information", "Cet objet doit être rangé.", AreaObject.transform);
                //Assistances.QandDAssistances AssistancesBetaGradation = new Assistances.QandDAssistances();
                AssistancesGradation.AddAssistance(Assistances.QandDAssistances.Gradation.Beta, AssistancesBeta);

                Assistances.Basic AssistancesGamma = Assistances.Factory.Instance.CreateCube("Mouse_Exclamation", AreaObject.transform);
                //Assistances.QandDAssistances AssistancesGammaGradation = new Assistances.QandDAssistances();
                AssistancesGradation.AddAssistance(Assistances.QandDAssistances.Gradation.Gamma, AssistancesGamma);

                Assistances.Dialog AssistancesDelta = Assistances.Factory.Instance.CreateDialogNoButton("Information","L'objet n'est pas rangé au bon endroit.", AreaObject.transform);
                //Assistances.QandDAssistances AssistancesDeltaGradation = new Assistances.QandDAssistances();
                AssistancesGradation.AddAssistance(Assistances.QandDAssistances.Gradation.Delta, AssistancesDelta);

                Assistances.Basic AssistancesEpsilon = Assistances.Factory.Instance.CreateCube("Mouse_Exclamation_Red", AreaObject.transform);
                //Assistances.QandDAssistances AssistancesEpsilonGradation = new Assistances.QandDAssistances();
                AssistancesGradation.AddAssistance(Assistances.QandDAssistances.Gradation.Epsilon, AssistancesEpsilon);

                /**
                 * The BT below corresponds to the iteration-9 of the BT in the diagram file
                 * */
                Sequence seObjectDroppedOutsideStorageArea = new Sequence(
                    new NPBehave.Action(() => AssistancesGradation.ShowOneHideOthers(Assistances.QandDAssistances.Gradation.Delta, Utilities.Utility.GetEventHandlerEmpty())),
                    new WaitUntilStopped());

                BlackboardCondition cObjectDroppedOutsideStorageArea = new BlackboardCondition("PersonDroppedObjectOutsideStoringArea",
                    Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, seObjectDroppedOutsideStorageArea);

                Sequence sePersonTakeObject = new Sequence(
                        //cObjectDroppedOutsideStorageArea,
                        //new Sequence(
                        new NPBehave.Action(() => AssistancesGradation.HideAll()),
                        new WaitUntilStopped());

                BlackboardCondition cDidPersonTakeObject = new BlackboardCondition("PersonGrabbedObject", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, sePersonTakeObject);

                Sequence sePersonMovedAwayFromObject = new Sequence(
                    new NPBehave.Action(() => AssistancesGradation.ShowOneHideOthers(Assistances.QandDAssistances.Gradation.Epsilon, Utilities.Utility.GetEventHandlerEmpty())),
                    new WaitUntilStopped());

                BlackboardCondition cDidPersonMoveAwayFromObject = new BlackboardCondition("PersonMovedAwayFromObject", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, sePersonMovedAwayFromObject);

                Sequence sePersonApproachedObject = new Sequence(
                    cDidPersonTakeObject
                    //cDidPersonMoveAwayFromObject
                    );

                BlackboardCondition cDidPersonApproachObject = new BlackboardCondition("PersonCloseToObject", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, new Sequence(sePersonApproachedObject));

                Sequence sePersonLookedAtObject = new Sequence(
                    new NPBehave.Action(() => AssistancesGradation.ShowOneHideOthers(Assistances.QandDAssistances.Gradation.Beta, Utilities.Utility.GetEventHandlerEmpty())),
                    new WaitUntilStopped());

                BlackboardCondition cDidPersonLookAtObject = new BlackboardCondition("PersonWatchedObject", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, sePersonLookedAtObject);

                Sequence sePersonDidNotCameToObjectSinceAWhile = new Sequence(
                    new NPBehave.Action(() => AssistancesGradation.ShowOneHideOthers(Assistances.QandDAssistances.Gradation.Gamma, Utilities.Utility.GetEventHandlerEmpty())),
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
                    new NPBehave.Action(() => AssistancesGradation.ShowOneHideOthers(Assistances.QandDAssistances.Gradation.Alpha, Utilities.Utility.GetEventHandlerEmpty())),
                    new WaitUntilStopped()
                 );

                BlackboardCondition cIsObjectStored = new BlackboardCondition("ObjectStored", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, seObjectIsStored);

                Selector srBegin = new Selector(
                    cIsObjectStored,
                    srObjectIsNotStored
                    );

				// Set assistances
				// For now we use the assistances from the QandDAssistances class. Uncomment the following line when going to the BT to manage the assistances
                //InitializeAssistancesAlpha();
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
                if (Utilities.Utility.IsEditorSimulator() || Utilities.Utility.IsEditorGameView())
                {
                    Inferences.Factory.Instance.CreateDistanceComingInferenceOneShot(InferenceManager, "CloseToObject", CallbackPersonCloseToObject, FakeObject);
                }
                else
                {
                    Inferences.Factory.Instance.CreateDistanceComingInferenceOneShot(InferenceManager, "CloseToObject", CallbackPersonCloseToObject, AreaObject.GetInteractionSurface().gameObject);
                }
            }



            void RegisterInferenceLeaving()
            {
                //Conditions["PersonGrabbedObject"] = false;
                if (!Utilities.Utility.IsEditorSimulator() && !Utilities.Utility.IsEditorGameView())
                {
                    Inferences.Factory.Instance.CreateDistanceLeavingInferenceOneShot(InferenceManager, "DistantToObject", CallbackPersonReleasedObject, AreaObjectPosition.GetInteractionSurface().gameObject);
                }
            }

            void RegisterInferenceComingAndLeaving()
            {
                Conditions["PersonMovedAwayFromObject"] = false;
                if (Utilities.Utility.IsEditorSimulator() || Utilities.Utility.IsEditorGameView())
                {
                    Inferences.Factory.Instance.CreateDistanceComingAndLeavingInferenceOneShot(InferenceManager, "PassingByObject", CallbackPersonMovedAwayFromObject, FakeObject);
                }
                else
                {
                    Inferences.Factory.Instance.CreateDistanceComingAndLeavingInferenceOneShot(InferenceManager, "PassingByObject", CallbackPersonMovedAwayFromObject, AreaObject.GetInteractionSurface().gameObject);
                }
            }

            void CallbackObjectDetected(System.Object o, EventArgs e)
            {
                //InferenceManager.UnregisterInference(InferenceObjectDetected);
                
                ObjectDetectedInformation = ((Utilities.EventHandlerArgObject)e).ObjectDetected;
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Object detected : " + ObjectDetectedInformation.GetObjectName());

                if (!Utilities.Utility.IsEditorSimulator() && !Utilities.Utility.IsEditorGameView())
                {
                    AreaObjectPosition.transform.position = ObjectDetectedInformation.GetCenter();
                }

                if (ObjectSet == false)
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Object detected and set!");
                    ObjectSet = true;
                    CallBackSetAreaObject();
                    InferenceManager.RegisterInference(InferenceTimeDidNotComeToObject);
                }
                ObjectDetected = true;
              //AreaObject.SetLocalPosition(ObjectDetectedInformation.GetCenter());
            }

            void CallbackGameObjectDetectedInStorage(System.Object o, EventArgs e)
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Object stored!");
                if (Utilities.Utility.IsEditorSimulator() || Utilities.Utility.IsEditorGameView())
                {
                    InferenceManager.UnregisterInference(InferenceGameObjectInStorage);
                }
                else
                {
                    InferenceManager.UnregisterInference(InferenceObjectInStorage);
                    //ObjectDetectedInformation = ((Utilities.EventHandlerArgObject)e).ObjectDetected;
                }
                InferenceManager.UnregisterInference(InferenceObjectDetected);
                Conditions["ObjectStored"] = true;// !(bool)Conditions["ObjectStored"];
				Conditions["PersonDroppedObjectOutsideStoringArea"] = false;

                CallBackRemoveAreaObject();
                AreaObjectPosition.transform.position = new Vector3(1000, 1000, 1000);
                

                //successController.Show(Utilities.Utility.GetEventHandlerEmpty());//Move in assitance epsilon
                //AssistancesManager.AddAssistance("Success", successController);
            }

            void CallbackObjectStored(System.Object o, EventArgs e)
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Object stored");
                Conditions["ObjectStored"] = !(bool)Conditions["ObjectStored"];
            }

            void CallbackPersonCloseToObject(System.Object o, EventArgs e)
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Person close to object");
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
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "User passed by object");
                    Conditions["PersonMovedAwayFromObject"] = true;
					Conditions["PersonCloseToObject"] = false;

                    ObjectDetected = false;
                    //Conditions["PersonGrabbedObject"] = false;
                }
            }

            void CallbackPersonGrabbedObject(System.Object o, EventArgs e)
            {
                if (Utilities.Utility.IsEditorSimulator() || Utilities.Utility.IsEditorGameView())
                {
                    InferenceManager.UnregisterInference(InferenceGrabbedObject);
                    InferenceManager.RegisterInference(InferenceReleasedObject);
                }
                else
                {
                    CallBackRemoveAreaObject();
                    RegisterInferenceLeaving();
                    //InferenceManager.UnregisterInference(InferenceObjectOutAreaObject);
                }
               
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Object grabbed");
                Conditions["PersonGrabbedObject"] = true;

            }

            void CallbackPersonReleasedObject(System.Object o, EventArgs e)
            {
                if (Utilities.Utility.IsEditorSimulator() || Utilities.Utility.IsEditorGameView())
                {
                    InferenceManager.UnregisterInference(InferenceReleasedObject);
                    InferenceManager.RegisterInference(InferenceGrabbedObject);
                }
                
                if (ObjectDetected)
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Object released");
                    CallBackSetAreaObject();
                }
                else
                { 
                    RegisterInferenceLeaving();
                }
                Conditions["PersonDroppedObjectOutsideStoringArea"] = true;
				Conditions["PersonGrabbedObject"] = false;
                ObjectDetected = false;
            }

            void CallbackPersonWatchedObject(System.Object o, EventArgs e)
            {
                InferenceManager.UnregisterInference(InferenceFocusedOnObject);
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Person focused on object for at 3 seconds");
                Conditions["PersonWatchedObject"] = true;
            }

            void CallbackPersonDidNotComeToObject(System.Object o, EventArgs e)
            {
                InferenceManager.UnregisterInference(InferenceTimeDidNotComeToObject);
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Person didn't come to the object");
                Conditions["PersonDidNotComeToObject"] = true;//!(bool)Conditions["PersonDidNotComeToObject"];
				Conditions["PersonCloseToObject"] = false;
            }

            void CallbackPersonDroppedObjectOutsideStoringArea(System.Object o, EventArgs e)
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Person drop object outside storing area");
                Conditions["PersonDroppedObjectOutsideStoringArea"] = !(bool)Conditions["PersonDroppedObjectOutsideStoringArea"];
            }



            void CallbackSeeStates()
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "List of booleans :");
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "ObjectStored - " + Conditions["ObjectStored"]);
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "PersonMovedAwayFromObject - " + Conditions["PersonMovedAwayFromObject"]);
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "PersonCloseToObject - " + Conditions["PersonCloseToObject"]);
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "PersonGrabbedObject - " + Conditions["PersonGrabbedObject"]);
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "PersonWatchedObject - " + Conditions["PersonWatchedObject"]);
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "PersonDroppedObjectOutsideStoringArea - " + Conditions["PersonDroppedObjectOutsideStoringArea"]);
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "PersonDidNotComeToObject - " + Conditions["PersonDidNotComeToObject"]);
             }

            void CallBackSetAreaObject()
            {
                if (!Utilities.Utility.IsEditorSimulator() && !Utilities.Utility.IsEditorGameView())
                {
                    AreaObject.transform.position = AreaObjectPosition.transform.position;//ObjectDetectedInformation.GetCenter();
                    //AreaObject.GetInteractionSurface().transform.position = ObjectDetectedInformation.GetCenter();
                }
            }

            void CallBackRemoveAreaObject()
            {
                if (!Utilities.Utility.IsEditorSimulator() && !Utilities.Utility.IsEditorGameView())
                {
                    AreaObject.transform.position = new Vector3(1000, 1000, 1000);
                }
            }
        }
    }
}
