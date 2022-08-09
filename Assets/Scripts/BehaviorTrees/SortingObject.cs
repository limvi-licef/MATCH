/*Copyright 2022 Louis Marquet, Guillaume Spalla

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

            Assistances.InteractionSurface AreaStorage;         // Place where the object is supposed to be stored.

            /**
             * Comment to explain the purpose of the two variables below
             * AreaObject is mainly used to detect when the object is grabbed, i.e. when the hand of the user crosses this area. It is also used to display fixed assistances.
             * The AreaObjectPosition area is used to know where the object is detected and set the AreaObject position. Two area are used (i.e. AreaObject and AreaObjectPosition) instead of one, because AreaObjectPosition will move every time the object is detected, and due to some inacurracies, the position of the object changes even if the object did not move. And if the assistances would belong to AreaObjectPosition, then their position would continuously be changing. To deal with this, the AreaObject interaction surface is dedicated to the display of the assistances. AreaObject moves only when the obejct is grabbed and the first time the object is detected.
             * */
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
            bool ObjectSet = false; // Used for the first time the object is detected, to add an interaction surface around it.
            Utilities.PhysicalObjectInformation ObjectDetectedInformation;

            GameObject FakeObject;

            /**
             * The next two lines are to be used with the assistance behavior tree.
             * Currently, in order to test the code, the class QandDAssistances is used. This is meant to be a temporary solution, i.e. all assistances should in the end be based on the behavior tree.
             * */
			Assistances.InteractionSurface AssistancesAlphaInteractionSurface;
            Assistances.AssistanceGradationExplicit AssistancesAlphaGradation;
            Assistances.AssistanceGradationAttention ExclamationMark;

            // Start is called before the first frame update
            void Start()
            {
                FakeObject = transform.Find("FakeObject").gameObject;

                ObjectDetectedInformation = null;
                AssistancesManager = new Assistances.Manager();

                AreaStorage = Assistances.Factory.Instance.CreateInteractionSurface("Storage", default, new Vector3(0.2f, 0.8f, 0.3f), "Mouse_Green_Glowing", true, true, Utilities.Utility.GetEventHandlerEmpty(), transform);
                
                /*if (Utilities.Utility.IsEditorSimulator() || Utilities.Utility.IsEditorGameView())
                {*/
                    AreaStorage.transform.position = new Vector3(3.38f, 0.22f, 3.19f);
                /*}
                else
                {
                    AreaStorage.SetLocalPosition(new Vector3(0f, 0f, 0.2f));
                }*/

                /* 
                 * AreaObject is the interaction surface that is meant to be around the detected object. 
                 * However, it is worth noticing that some inferences are based on the distance between the user and this interaction surface. Hence, to avoid that those inferences are triggered at the beginning of the scenario, the position of the area is set to (1000, 1000, 1000) on purpose.
                */
                AreaObject = Assistances.Factory.Instance.CreateInteractionSurface("Object", default, new Vector3(0.4f, 0.4f, 0.4f), "Mouse_Yellow_Glowing", true, true, Utilities.Utility.GetEventHandlerEmpty(), transform);       
                AreaObject.transform.position = new Vector3(1000, 1000, 1000);


                AreaObjectPosition = Assistances.Factory.Instance.CreateInteractionSurface("Object_Position", default, new Vector3(0.05f, 0.05f, 0.05f), "Mouse_Yellow_Glowing", true, true, Utilities.Utility.GetEventHandlerEmpty(), transform);
                AreaObjectPosition.GetInteractionSurface().gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                AreaObjectPosition.transform.position = new Vector3(1000, 1000, 1000);
                
                AdminMenu.Instance.addButton("Sorting Object - See boolean states", CallbackSeeStates);

                InitializeScenario();
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

                InferenceGameObjectInStorage = new Inferences.GameObjectInInteractionSurface("GameObjectStorage", CallbackGameObjectDetectedInStorage, FakeObject, AreaStorage);
                InferenceManager.RegisterInference(InferenceGameObjectInStorage);
                InferenceObjectInStorage = new Inferences.ObjectInInteractionSurface("ObjectStorage", CallbackGameObjectDetectedInStorage, ObjectOfInterestName, AreaStorage);
                InferenceManager.RegisterInference(InferenceObjectInStorage);

                AreaObject.TriggerTouchEvent();
                AreaObject.EventInteractionSurfaceTableTouched += CallbackPersonGrabbedObject;

                RegisterInferenceComing();
                RegisterInferenceComingAndLeaving();

                /**
                 * Be careful when testing: some inferences are available only in the editor and others only in the hololens. Thus, some situations can only be tested in the hololens.
                 **/
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
                    //Inference ObjectGrabbed is replaced by the detection of the hand in the AreaObject
                    //Inference ObjectReleased is created and registered in CallbackPersonGrabbedObject

                    InferenceFocusedOnObject = new Inferences.ObjectFocused("FocusedOnObject", CallbackPersonWatchedObject, AreaObject.GetInteractionSurface().gameObject, 3);
                    InferenceManager.RegisterInference(InferenceFocusedOnObject);

                    InferenceTimeDidNotComeToObject = new Inferences.TimeDidNotComeToObject("DidNotComeToObject", CallbackPersonDidNotComeToObject, AreaObject.GetInteractionSurface().gameObject, 15);
                    //Inference DidNotComeToObject is register in CallbackObjectDetected
                }

                /*
                 * Temporary assistances built with QandDAssistances
                 * To be removed when the behavior tree handling the assistances is reliable.
                 */
                Assistances.QandDAssistances AssistancesGradation = new Assistances.QandDAssistances();

                Assistances.Basic AssistancesAlpha = Assistances.Factory.Instance.CreateCube("Mouse_Congratulation", AreaStorage.transform);
                AssistancesGradation.AddAssistance(Assistances.QandDAssistances.Gradation.Alpha, AssistancesAlpha);

                Assistances.Dialog AssistancesBeta = Assistances.Factory.Instance.CreateDialogNoButton("Information", "Cet objet doit être rangé.", AreaObject.transform);
                AssistancesGradation.AddAssistance(Assistances.QandDAssistances.Gradation.Beta, AssistancesBeta);

                Assistances.Basic AssistancesGamma = Assistances.Factory.Instance.CreateCube("Mouse_Exclamation", AreaObject.transform);
                AssistancesGradation.AddAssistance(Assistances.QandDAssistances.Gradation.Gamma, AssistancesGamma);

                Assistances.Dialog AssistancesDelta = Assistances.Factory.Instance.CreateDialogNoButton("Information","L'objet n'est pas rangé au bon endroit.", AreaObject.transform);
                AssistancesGradation.AddAssistance(Assistances.QandDAssistances.Gradation.Delta, AssistancesDelta);

                Assistances.Basic AssistancesEpsilon = Assistances.Factory.Instance.CreateCube("Mouse_Exclamation_Red", AreaObject.transform);
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
                        new NPBehave.Action(() => AssistancesGradation.HideAll()), // When the object is grabbed, no assistances should be displayed, so they are all hidden.
                        new WaitUntilStopped());

                BlackboardCondition cDidPersonTakeObject = new BlackboardCondition("PersonGrabbedObject", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, sePersonTakeObject);

                Sequence sePersonMovedAwayFromObject = new Sequence(
                    new NPBehave.Action(() => AssistancesAlphaGradation.RunAssistance(Utilities.Utility.GetEventHandlerEmpty()) /*AssistancesGradation.ShowOneHideOthers(Assistances.QandDAssistances.Gradation.Epsilon, Utilities.Utility.GetEventHandlerEmpty())*/),
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
                InitializeAssistancesAlpha();
                Tree = new Root(Conditions, srBegin);

                //#if UNITY_EDITOR
                NPBehave.Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
                debugger.BehaviorTree = Tree;
                //#endif

                Tree.Start();
            }
            
			void InitializeAssistancesAlpha()
            {
                AssistancesAlphaInteractionSurface = Assistances.Factory.Instance.CreateInteractionSurface("AssistancesAlpha", AdminMenu.Panels.Obstacles, new Vector3(0.5f, 0.05f, 0.5f), "Mouse_Orange_Glowing", true, true, Utilities.Utility.GetEventHandlerEmpty(), transform);
                AssistancesAlphaInteractionSurface.transform.localPosition = new Vector3(0, 0.464f, 1.632f); //new Vector3(0.85f, 0.46f, 2.18f);
                AssistancesAlphaInteractionSurface.name = "Assistances Alpha - Interaction surface";


                //Assistances.Basic cube = ;
                //Assistances.AssistanceGradationAttention exclamationMark = new Assistances.AssistanceGradationAttention();
                /*Assistances.AssistanceGradationAttention exclamationMark*/ ExclamationMark = Assistances.Factory.Instance.CreateAssistanceGradationAttention();
                //exclamationMark.gameObject.SetActive(true);

                Assistances.Basic cube = Assistances.Factory.Instance.CreateCube("Mouse_Purple_Glowing", AssistancesAlphaInteractionSurface.transform);
                cube.name = "Alpha_Cube";
                //exclamationMark.AddAssistance(cube);
                /*Assistances.IAssistance exclamationMarkPale = */
                //ExclamationMark.AddAssistance(new Assistances.Decorators.Material(cube, "Mouse_Exclamation"));
                //exclamationMark.AddAssistance(Assistances.Decorators.)
                //ExclamationMark.AddAssistance(new Assistances.Decorators.Material(cube, "Mouse_Exclamation_Red"));
                ExclamationMark.AddAssistance(Assistances.Decorators.Factory.Instance.CreateMaterial(cube, "Mouse_Exclamation"));

                /*cube.EventHelpButtonClicked += delegate (System.Object o, EventArgs e)
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called 1");
                };*/

                //Assistances.AssistanceGradationAttention end = new Assistances.AssistanceGradationAttention();
                Assistances.AssistanceGradationAttention end = Assistances.Factory.Instance.CreateAssistanceGradationAttention();
                end.AddAssistance(Assistances.Factory.Instance.CreateDialogNoButton("", "Ok! We let you do.", AssistancesAlphaInteractionSurface.transform));

                //Assistances.AssistanceGradationAttention yesNo1 = new Assistances.AssistanceGradationAttention();
                Assistances.AssistanceGradationAttention yesNo1 =  Assistances.Factory.Instance.CreateAssistanceGradationAttention();
                yesNo1.AddAssistance(Assistances.Factory.Instance.CreateDialogNoButton("", "Don't you think there is something to do here?", AssistancesAlphaInteractionSurface.transform));

                AssistancesAlphaGradation = Assistances.Factory.Instance.CreateAssistanceGradationExplicit();
                AssistancesAlphaGradation.InfManager = InferenceManager;

                AssistancesAlphaGradation.AddButton(ExclamationMark, Assistances.Buttons.Button.ButtonType.Yes, yesNo1);
                AssistancesAlphaGradation.AddButton(ExclamationMark, Assistances.Buttons.Button.ButtonType.No, end);
                //AssistancesAlphaGradation.AddButton(yesNo1, Assistances.Buttons.Button.ButtonType.Undefined, null);

                // Callbacks to have the state machine working
                /*cube.EventHelpButtonClicked += delegate (System.Object o, EventArgs e)
                {
                    MATCH.Utilities.EventHandlerArgs.Button args = (MATCH.Utilities.EventHandlerArgs.Button)e;
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Button " + args.ButtonType.ToString() + " Clicked");

                    if (args.ButtonType == Assistances.Buttons.Button.ButtonType.No)
                    {
                        cube.Hide(delegate (System.Object oo, EventArgs ee)
                        {
                            end.ShowMinimalGradation(MATCH.Utilities.Utility.GetEventHandlerEmpty());
                        });
                    }
                    else
                    {
                        cube.Hide(delegate (System.Object oo, EventArgs ee)
                        {
                            yesNo1.ShowMinimalGradation(MATCH.Utilities.Utility.GetEventHandlerEmpty());
                        });

                    }
                };*/

                /*if (exclamationMark.gameObject.activeSelf)
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Exclamation mark object active");
                }
                else
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Exclamation mark object NOT active");
                }*/
            }

			void RegisterInferenceComing()
            {
                /**
                 * If the code runs in the editor, then the fake object is used as reference instead of the "real" object
                 **/
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
                /**
                 * Only used in the Hololens, because used when the "real" object is realeased, which is not necessary in the editor. Indeed, in the editor (see the inference InferenceReleasedObject) the "release" of the fake object is detected when the "fake hand" releases it, which is not possible in the case of the detection of the real object.
                 * */
                if (!Utilities.Utility.IsEditorSimulator() && !Utilities.Utility.IsEditorGameView())
                {
                    Inferences.Factory.Instance.CreateDistanceLeavingInferenceOneShot(InferenceManager, "DistantToObject", CallbackPersonReleasedObject, AreaObjectPosition.GetInteractionSurface().gameObject);
                }
            }

            void RegisterInferenceComingAndLeaving()
            {
                Conditions["PersonMovedAwayFromObject"] = false;

                /**
                 * Difference process when the code runs in the editor and in the hololens
                 * */
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
                ObjectDetectedInformation = ((Utilities.EventHandlerArgs.PhysicalObject)e).ObjectDetected;
               
                /**
                 * If the code runs in the hololens, this is the place where the AreaObjectPosition is moved when the real object is detected.
                 * */
                if (!Utilities.Utility.IsEditorSimulator() && !Utilities.Utility.IsEditorGameView())
                {
                    AreaObjectPosition.transform.position = ObjectDetectedInformation.GetCenter();

                    /**
                    * If the code runs in the Hololens, then the AreaObject is moved to the position of the real object the first time and only the first time the object is detected
                    */
                    if (ObjectSet == false) //If the AreaObject is not placed on the object
                    {
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Object detected and set!");
                        ObjectSet = true;
                        CallBackSetAreaObject();
                        InferenceManager.RegisterInference(InferenceTimeDidNotComeToObject);
                    }
                }

                ObjectDetected = true;
            }

            void CallbackGameObjectDetectedInStorage(System.Object o, EventArgs e)
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Object stored!");
                
                /**
                 * Different code following if executed on the hololens or the editor
                 */
                if (Utilities.Utility.IsEditorSimulator() || Utilities.Utility.IsEditorGameView())
                {
                    InferenceManager.UnregisterInference(InferenceGameObjectInStorage);
                }
                else
                {
                    InferenceManager.UnregisterInference(InferenceObjectInStorage);
                }

                InferenceManager.UnregisterInference(InferenceObjectDetected);
                Conditions["ObjectStored"] = true;// !(bool)Conditions["ObjectStored"];
				Conditions["PersonDroppedObjectOutsideStoringArea"] = false;

                CallBackRemoveAreaObject(); //If the object is stored, the both areas are "removed", by being moved back in (1000, 1000, 1000)
                AreaObjectPosition.transform.position = new Vector3(1000, 1000, 1000); // Same here, the area is moved back in (1000, 1000, 1000)
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

                    ObjectDetected = false;
                    //Conditions["PersonGrabbedObject"] = false;
                }
            }

            void CallbackPersonGrabbedObject(System.Object o, EventArgs e)
            {
                /**
                 * Different code between the hololens and the editor
                 */
                if (Utilities.Utility.IsEditorSimulator() || Utilities.Utility.IsEditorGameView())
                {
                    InferenceManager.UnregisterInference(InferenceGrabbedObject);
                    InferenceManager.RegisterInference(InferenceReleasedObject);
                }
                else
                {
                    CallBackRemoveAreaObject(); // When the person has grabbed the object, there will never be an assistance displayed. This is maybe a bit overkill, but the AreaObject is moved to (1000, 1000, 1000).
                    ObjectDetected = false; //set to false to detect in the callback CallbackPersonReleasedObject if the object is away from the user and after, if the object is detected
                    RegisterInferenceLeaving();
                    //InferenceManager.UnregisterInference(InferenceObjectOutAreaObject);
                }
               
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Object grabbed");
                Conditions["PersonGrabbedObject"] = true;

            }

            void CallbackPersonReleasedObject(System.Object o, EventArgs e)
            {
                /**
                 * Code different between the hololens and the editor
                 */
                if (Utilities.Utility.IsEditorSimulator() || Utilities.Utility.IsEditorGameView())
                {
                    InferenceManager.UnregisterInference(InferenceReleasedObject);
                    InferenceManager.RegisterInference(InferenceGrabbedObject);
                }
                else
                {
                    if (ObjectDetected)
                    {
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Object released");
                        CallBackSetAreaObject(); // If this point is reached, that means the person released the object AND that the object has been detected, so the AreaObject is brought to the object's position.
                    }
                    else
                    {
                        RegisterInferenceLeaving(); // Yes this is the correct inference to register here: in fact we cannot detect when the real object is actually released. This is deduced when the following conditions are met: (1) the person is "far" from the AreaObjectPosition (which will call this callback); (2) the object is detected (which is known by the ObjectDetected boolean, which is updated thanks to the CallbackObjectDetected function); 
                    }
                }
                
                Conditions["PersonDroppedObjectOutsideStoringArea"] = true;
				Conditions["PersonGrabbedObject"] = false;
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

            void CallbackSeeStates() //A callback to see the state of the booleans of the BT
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
