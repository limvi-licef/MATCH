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
    namespace Assistances
    {
        public class AssistanceGradationExplicit : MonoBehaviour
        {
            private NPBehave.Root Tree;
            private NPBehave.Blackboard Conditions;

            private Inferences.ObjectFocused InfFocusedOnAssistance;
            private Inferences.Timer InfTimer2Minutes;
            public Inferences.Manager InfManager;
            private Inferences.ObjectLostFocused InfFocusLost;
            private Inferences.DistanceComing InfIsClose;
            private Inferences.Timer InfTimer30Seconds;
            private Inferences.DistanceLeaving InfIsFar;


            public enum AssistanceStatus
            {
                Stop = 0,
                Run = 1,
                Pause = 2
            }

            //private List<AssistanceGradationAttention> AssistancesGradation;
            private Assistances.StateMachine AssistancesGradation;
            int CurrentAssistanceIndex; // Index in the list AssistancesGradation

            AssistanceStatus Status;

            Utilities.EventHandlerArgs.ButtonAndAssistanceGradationAttention ArgsOnHelpButtonClicked;

            private void Awake()
            {
                AssistancesGradation = null;
                //AssistancesGradation = new List<AssistanceGradationAttention>();
                InfTimer2Minutes = new Inferences.Timer("AssistanceGradationExplicitTimer", 10, CallbackInterenceTimer);

                ArgsOnHelpButtonClicked = null;
            }

            void Start()
            {

                InfManager.RegisterInference(InfTimer2Minutes);
                InitializeInference30Seconds();

                Status = AssistanceStatus.Stop;
                CurrentAssistanceIndex = -1; // Means the process did not started yet

                InfFocusedOnAssistance = null; // Use InitializeInfereneFocusedAssistance to initialize it
                InfFocusLost = null;
                InfIsClose = null;
                //InfTimer30Seconds = null;

                // Initialize the BT
                Conditions = new NPBehave.Blackboard(UnityContext.GetClock());
                ResetConditions(); // Can also be used to initialize them

                // seIsWaitingSince30sec - begin
                Sequence seIsWaitingSince30sec = new Sequence(
                    new NPBehave.Action(() => DisplayHelpButtons()),
                    new NPBehave.WaitUntilStopped()
                    );
                BlackboardCondition cIsWaitingSince30sec = new BlackboardCondition("WaitingSince30Seconds", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, /*new NPBehave.Action(() => Debug.Log("Wait since 30 seconds"))*/ seIsWaitingSince30sec);

                // seIsWaitingSince30sec - end

                // seIsHelpClicked - begin
                Sequence seIsHelpClicked = new Sequence(
                    new NPBehave.Action(() => HideCurrentGradation()),
                    new NPBehave.WaitUntilStopped()
                    );
                BlackboardCondition cIsHelpClicked = new BlackboardCondition("HelpClicked", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, seIsHelpClicked);
                // seIsHelpClicked - end

                // seIsFar - begin
                Sequence seIsFar = new Sequence(
                    //cIsFar,
                    new NPBehave.Action(() => Debug.Log("Here should be displayed a symbol to inform the person the system understood he looked at the assistance")),
                    new NPBehave.WaitUntilStopped()
                    );

                BlackboardCondition cIsFar = new BlackboardCondition("IsFar", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, seIsFar);
                // seIsFar - end

                // srFocused - begin
                Sequence seFocusedInternal = new Sequence(
                    new NPBehave.Action(() => ShowAssistanceMinimalGradation(AssistancesGradation.AssistanceCurrent, CAssistanceDisplayed)),
                    new NPBehave.WaitUntilStopped()
                    );

                BlackboardCondition cIsNotMininal = new BlackboardCondition("MinimalConditionDisplayed", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART, seFocusedInternal);

                Selector srFocused = new Selector(
                    cIsHelpClicked,
                    cIsWaitingSince30sec,
                    cIsNotMininal,
                    cIsFar
                    );
                // srFocused - end

                // seIsFocused - begin
                BlackboardCondition cIsFocused = new BlackboardCondition("IsFocused", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, srFocused);
                // seIsFocused - end

                // srIsDisplayed - begin
                Sequence seInternalNotDisplayed = new Sequence(
                    new NPBehave.Action(() => ShowFirstAssistance()),
                    new NPBehave.WaitUntilStopped()
                    );

                BlackboardCondition cIsDisplayed = new BlackboardCondition("IsDisplayed", Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART, seInternalNotDisplayed);
                // srIsDisplayed - end

                // seIsDisplayedSince2Minutes - begin
                BlackboardCondition cIsDisplayedSince2Minutes = new BlackboardCondition("DisplayedSince2Minutes", Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, new NPBehave.Action(()=>Debug.Log("Displayed since 2 minutes")));

                Sequence seIsDisplayedSince2Minutes = new Sequence(
                    cIsDisplayedSince2Minutes,
                    new NPBehave.Action(() => IncreaseAttentionGrabbingGradation()),
                    new NPBehave.WaitUntilStopped());
                // seIsDisplayedSince2Minutes - end

                // seBegin - begin
                Selector srBegin = new Selector(
                    cIsDisplayed, //srIsDisplayed,
                    cIsFocused, //seIsFocused,
                    seIsDisplayedSince2Minutes
                    );
                // seBegin - end

                Tree = new Root(Conditions, srBegin);

                NPBehave.Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
                debugger.BehaviorTree = Tree;
            }

            private void ResetConditions()
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Resetting conditions");

                Conditions["IsDisplayed"] = false;
                Conditions["IsFocused"] = false;
                Conditions["HelpButtonDisplayed"] = false;
                Conditions["HelpClicked"] = false;
                Conditions["DisplayedSince2Minutes"] = false;
                Conditions["IsFar"] = true; // By default it is supposed the person is far from the object. If this is not the case, an inference will correct this.
                Conditions["WaitingSince30Seconds"] = false;
                Conditions["MinimalConditionDisplayed"] = true;
            }

            private void ShowAssistanceMinimalGradation(AssistanceGradationAttention assistance, EventHandler callback)
            {
                assistance.ShowMinimalGradation(callback);
                Conditions["MinimalConditionDisplayed"] = true;
            }

            private void HideCurrentGradation()
            {
                ArgsOnHelpButtonClicked.AssistanceCurrent.HideCurrentGradation(delegate (System.Object o, EventArgs e)
                {
                    //The next assistance will be shown automatically when resetting the conditions
                    ResetConditions();
                    //next.ShowMinimalGradation(Utilities.Utility.GetEventHandlerEmpty());
                });
            }

            private void ShowFirstAssistance()
            {
                CurrentAssistanceIndex = 0;
                ShowAssistanceMinimalGradation(AssistancesGradation.AssistanceCurrent, CAssistanceDisplayed);
                Conditions["IsDisplayed"] = true;

                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Display first assistance for " + AssistancesGradation.AssistanceCurrent.name);

                InitializeInferenceFocusedAssistance(CurrentAssistanceIndex);

                InfTimer2Minutes.StartCounter();
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Timer 2 minutes started");
            }

            private void CallbackInterenceTimer(System.Object o, EventArgs e)
            {
                Conditions["DisplayedSince2Minutes"] = true;
                
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Timer 2 minutes called");
            }

            /**
             * Is done for the current assistance
             * */
            private void IncreaseAttentionGrabbingGradation()
            {
                Conditions["DisplayedSince2Minutes"] = false; // The timer is going to be restarted, so the condition is not true anymore.

                if (AssistancesGradation.AssistanceCurrent.ShowNextGradation(Utilities.Utility.GetEventHandlerEmpty()))
                {
                    Conditions["MinimalConditionDisplayed"] = false;
                    InfTimer2Minutes.StartCounter(); // The counter is started again to continue increasing the attention gradation if necessary
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Next gradation for attention grabbing shown");
                }
                else
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Maximum gradation for attention grabbing shown");
                }


                InitializeInferenceFocusedAssistance(CurrentAssistanceIndex); // In any case, the timer for focus should be enabled
            }

            private void COnHelpButtonClicked(System.Object o, EventArgs e)
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Button clicked");

                ArgsOnHelpButtonClicked = (Utilities.EventHandlerArgs.ButtonAndAssistanceGradationAttention)e;

                if (ArgsOnHelpButtonClicked.AssistanceNext != null)
                {
                    Conditions["HelpClicked"] = true;
                }
            }

            /**
             * The order you add the assistances is important: it determines their order of appearance
             * */
            public void AddButton(AssistanceGradationAttention assistance, Buttons.Button.ButtonType type, AssistanceGradationAttention assistanceTarget)
            {
                if (AssistancesGradation == null)
                {
                    AssistancesGradation = new StateMachine(assistance);
                    AssistancesGradation.EventOnButtonClicked += COnHelpButtonClicked;
                }

                AssistancesGradation.AddButton(assistance, type, ref assistanceTarget);
                //AssistancesGradation.Add(assistance);
            }

            public void DisplayHelpButtons()
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Assistance focused for 30 seconds");
                Conditions["IsFocused"] = false;
                Conditions["WaitingSince30Seconds"] = false;

                AssistancesGradation.AssistanceCurrent.ShowHelpCurrentGradation(true, Utilities.Utility.GetEventHandlerEmpty());
            }

            /**
             * This function start the assistance from scratch, i.e. it will not remuse if it has been interrupted. If you want to resume the assistance after it has been interrupted, please use the ResumeAssistance function
             * The callback is called when the process is finished (finished, i.e. not interrupted)
             * */
            public void RunAssistance(EventHandler callback)
            {
                Status = AssistanceStatus.Run;

                // Initialize the BT
                Tree.Start();
            }

            public void PauseAssistance()
            {
                Status = AssistanceStatus.Pause;
            }

            public void StopAssistance()
            {
                Status = AssistanceStatus.Stop;
            }

            public AssistanceStatus GetAssistanceStatus()
            {
                return Status;
            }

            private void InitializeInferenceFocusedAssistance(int indexAssistanceToMonitor)
            {
                if (InfFocusedOnAssistance != null)
                {
                    InfManager.UnregisterInference(InfFocusedOnAssistance);
                    InfFocusedOnAssistance = null;
                }

                //Debug.Log("Object name: " + AssistancesGradation[indexAssistanceToMonitor].GetCurrentAssistance().GetTransform().gameObject.name);

                InfFocusedOnAssistance = new Inferences.ObjectFocused("AssistanceFocus", CallbackAssistanceFocused, AssistancesGradation.AssistanceCurrent.GetCurrentAssistance().GetTransform().gameObject, 3);
                InfManager.RegisterInference(InfFocusedOnAssistance);
            }

            private void InitializeInferenceLostFocus(int indexAssistanceToMonitor)
            {
                if (InfFocusLost != null)
                {
                    InfManager.UnregisterInference(InfFocusLost);
                    InfFocusLost = null;
                }

                InfFocusLost = new Inferences.ObjectLostFocused("InferenceGoLostFocus", CallbackInferenceLostFocus, AssistancesGradation.AssistanceCurrent.GetCurrentAssistance().GetTransform().gameObject);
                InfManager.RegisterInference(InfFocusLost);
            }

            private void InitializeInfDistance(int indexAssistanceToMonitor)
            {
                GameObject currentAssistance = AssistancesGradation.AssistanceCurrent.GetCurrentAssistance().GetTransform().gameObject;

                if (InfIsClose != null)
                {
                    InfManager.UnregisterInference(InfIsClose);
                    InfIsClose = null;
                }

                InfIsClose = new Inferences.DistanceComing("InferenceCloseToObject", CInfClose, currentAssistance, 1.5f);
                InfManager.RegisterInference(InfIsClose);

                if (InfIsFar != null)
                {
                    InfManager.UnregisterInference(InfIsFar);
                    InfIsFar = null;
                }

                InfIsFar = new Inferences.DistanceLeaving("AssistanceGradationExplicitIsFar", CInfIsFar, currentAssistance, 1.5f);
                InfManager.RegisterInference(InfIsFar);
            }

            private void InitializeInference30Seconds()
            {
                if (InfTimer30Seconds != null)
                {
                    InfManager.UnregisterInference(InfTimer30Seconds);
                    InfTimer30Seconds = null;
                }

                InfTimer30Seconds = new Inferences.Timer("Inf30Seconds", 5, CallbackInf30Seconds);
                InfManager.RegisterInference(InfTimer30Seconds);
            }

            private void CallbackInf30Seconds(System.Object o, EventArgs e)
            {
                Conditions["WaitingSince30Seconds"] = true;
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Timer 30 seconds called");
            }

            private void CInfClose (System.Object o, EventArgs e)
            {
                Conditions["IsFar"] = false;

                // Means the person is close to the object: consequently, some inferences are not necessary
                InfManager.UnregisterInference(InfTimer30Seconds);
            }

            private void CInfIsFar(System.Object o, EventArgs e)
            {
                Conditions["IsFar"] = true;

                // Far: initializing inferences
                //if (InfManager.Con)
                InitializeInference30Seconds();
            }

            private void CallbackInferenceLostFocus(System.Object o, EventArgs e)
            {
                Conditions["IsFocused"] = false;
                InfTimer2Minutes.StartCounter();
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Object lost focus");
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Timer 2 minutes called");
                InfManager.UnregisterInference(InfFocusLost);
                InfManager.UnregisterInference(InfIsClose);
                InfTimer30Seconds.StopCounter();
            }

            private void CallbackAssistanceFocused(System.Object o, EventArgs e)
            {
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Object focused for 3 seconds");

                Conditions["IsFocused"] = true;
                Conditions["DisplayedSince2Minutes"] = false;
                InfTimer2Minutes.StopCounter();
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Timer 2 minutes stopped");
                InfManager.UnregisterInference(InfFocusedOnAssistance);
                InfManager.UnregisterInference(InfFocusLost);
                InfManager.UnregisterInference(InfIsClose);
                InfManager.UnregisterInference(InfIsFar);
                //InitializeInferenceLostFocus(CurrentAssistanceIndex);
                //InitializeInfDistance(CurrentAssistanceIndex);
                
                InfTimer30Seconds.StartCounter();
            }

            private void CAssistanceDisplayed(System.Object o, EventArgs e)
            {
                InitializeInferenceFocusedAssistance(CurrentAssistanceIndex);
                InitializeInferenceLostFocus(CurrentAssistanceIndex);
                InitializeInfDistance(CurrentAssistanceIndex);
            }

            private void CallbackClickOnButton(System.Object o, EventArgs e)
            {
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "A button has been clicked");

                Conditions["HelpButtonDisplayed"] = true;

                InfTimer30Seconds.StopCounter();

            }
        }
    }
}
