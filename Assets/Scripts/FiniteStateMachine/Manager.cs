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

/**
 * State machine to handle the assistance gradation that contains more advanced functionalities that the basic one (who said that sounds logic? You won a candy).
 * The basic idea behing this is that each assistance as a life that start from a moment where it is "showed", and ends when it is "hidden". 
 * So first you create states with the addNewAssistanceGradation function.
 * Then, for each state, you have to add a pointer to one or several functions that should be called when the state is enabled and disabled. See the class below "MouseUtilitiesGradationAssistance" for more details. 
 * The state changes are done asynchrounously. In other words, you do not call the functions to move the states yourself. Rather, for a given state, you attach each next possible states to an event. This is not managed by this class but by the "MouseUtilitiesGradationAssistance" class below. See there for more information. This class registers automatically to each state change so that it knows when to trigger the hide / show functions. The hide function of the current state is first called. When it is finished, it calls all the show functions sequencially. It does not wait for a show function to finish to call the next one.
 * Handles a one level undo.
 * */
namespace MATCH
{
    namespace FiniteStateMachine
    {
        public class Manager
        {
            Dictionary<string, MouseUtilitiesGradationAssistanceAbstract> m_assistanceGradation; // id of the gradation, actual assistance.
                                                                                                 //Dictionary<string, MouseUtilitiesGradationAssistanceIntermediateState> m_assistanceIntermediateStates;

            string m_gradationPrevious;
            string m_gradationCurrent;
            string m_gradationNext;
            string m_gradationInitial;

            public event EventHandler s_newStateSelected; // Emitted with a MouseUtilisiesGradationAssistanceArgCurrentState argument

            public string getGradationCurrent()
            {
                return m_gradationCurrent;
            }

            public string getGradationPrevious()
            {
                return m_gradationPrevious;
            }

            public List<MouseUtilitiesGradationAssistanceAbstract> getListOfStates()
            {
                return m_assistanceGradation.Values.ToList();
            }

            /**
             * Do not forget to call this function at the end of the setup, otherwise, the object will most likely not work properly
             * */
            public void setGradationInitial(string id)
            {
                m_gradationInitial = id;

                // When setting the initial gradation, if there is no current state, then it also becomes the current state
                if (m_gradationCurrent == "")
                {
                    m_gradationCurrent = id;
                }
            }

            public MouseUtilitiesGradationAssistanceAbstract getAssistance(string id)
            {
                MouseUtilitiesGradationAssistanceAbstract toReturn = null;

                if (m_assistanceGradation.ContainsKey(id))
                {
                    toReturn = m_assistanceGradation[id];
                }

                return toReturn;
            }

            public MouseUtilitiesGradationAssistanceAbstract getInitialAssistance()
            {
                MouseUtilitiesGradationAssistanceAbstract toReturn;

                if (m_gradationInitial == "")
                {
                    toReturn = null;
                }
                else
                {
                    toReturn = m_assistanceGradation[m_gradationInitial];
                }

                return toReturn;
            }

            public Manager()
            {
                m_assistanceGradation = new Dictionary<string, MouseUtilitiesGradationAssistanceAbstract>();
                //m_assistanceIntermediateStates = new Dictionary<string, MouseUtilitiesGradationAssistanceIntermediateState>();
                m_gradationPrevious = "";
                m_gradationCurrent = "";
                m_gradationNext = "";
                m_gradationInitial = "";
            }

            public MouseUtilitiesGradationAssistance addNewAssistanceGradation(string id)
            {
                MouseUtilitiesGradationAssistance newItem = new MouseUtilitiesGradationAssistance(id);

                m_assistanceGradation.Add(newItem.getId(), newItem);

                /*if (m_gradationCurrent == "")
                {
                    m_gradationCurrent = id;
                }*/

                // Setting the internal callbacks to actually go to the next state
                List<Action<EventHandler>> fShows = newItem.getFunctionsShow();
                List<EventHandler> fShowsEventHandlers = newItem.getFunctionsShowEventHandlers();

                fShows.Add(delegate (EventHandler e)
                {
                    //MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Setting the new current: current>previous: " + m_gradationCurrent + " next>current: " + m_gradationNext);


                    m_gradationPrevious = m_gradationCurrent;
                    m_gradationCurrent = m_gradationNext;

                    MouseUtilisiesGradationAssistanceArgCurrentState args = new MouseUtilisiesGradationAssistanceArgCurrentState(m_assistanceGradation[m_gradationCurrent]);
                    //args.m_currentState

                    s_newStateSelected?.Invoke(this, args);
                });

                List<EventHandler> actionsEventHandler = newItem.getFunctionsShowEventHandlers();
                actionsEventHandler.Add(Utilities.Utility.GetEventHandlerEmpty());

                newItem.m_triggerNext += new EventHandler(delegate (System.Object o, EventArgs e)
                {
                    MouseUtilitiesGradationAssistance caller = (MouseUtilitiesGradationAssistance)o;
                    MouseUtilisiesGradationAssistanceArgNextState args = (MouseUtilisiesGradationAssistanceArgNextState)e;

                    if (caller.getId() == m_gradationCurrent)
                    { // A same trigger can call several time the same event for different objects (typically, the reminder one for instance, which is present at different stages of the scenario). So here we take into account only the trigger sent from the current gradation level.
                      //MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Caller: " + caller.getId() + " | next state: " + args.m_nextState);

                        goToNextState(args.m_nextState);
                    }
                    else
                    {
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Caller: " + caller.getId() + " is different from current state (" + m_gradationCurrent + ") so nothing will happen. This is labelled as a warning, by most likely this is a good safety thing.");
                    }
                });

                newItem.m_triggerPrevious += new EventHandler(delegate (System.Object o, EventArgs e)
                {
                    MouseUtilitiesGradationAssistance caller = (MouseUtilitiesGradationAssistance)o;

                    goToPreviousState();
                });

                return newItem;
            }

            public MouseUtilitiesGradationAssistanceIntermediateState addIntermediateState(string id, MouseUtilitiesGradationAssistance nextState)
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called");

                if (/*m_assistanceIntermediateStates*/m_assistanceGradation.ContainsKey(id) == false)
                {
                    MouseUtilitiesGradationAssistanceIntermediateState state = new MouseUtilitiesGradationAssistanceIntermediateState(id, nextState);
                    /*m_assistanceIntermediateStates*/
                    m_assistanceGradation.Add(id, state);


                    List<Action<EventHandler>> fShows = state.getFunctionsShow();
                    List<EventHandler> fShowsEventHandlers = state.getFunctionsShowEventHandlers();

                    state.setFunctionShow(delegate (EventHandler e)
                    {
                        //MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Called - intermediate show function trigerring the event for the graph display");

                        m_gradationPrevious = m_gradationCurrent;
                        m_gradationCurrent = m_gradationNext;

                        MouseUtilisiesGradationAssistanceArgCurrentState args = new MouseUtilisiesGradationAssistanceArgCurrentState(m_assistanceGradation[m_gradationCurrent]);
                        //args.m_currentState

                        s_newStateSelected?.Invoke(this, args);
                    });
                }
                else
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Intermediate state already registered - returning the existing state");
                }

                /*m_assistanceIntermediateStates*/
                ((MouseUtilitiesGradationAssistanceIntermediateState)m_assistanceGradation[id]).s_eventNextState += delegate (System.Object o, EventArgs e)
                {
                    MouseUtilisiesGradationAssistanceArgNextState next = (MouseUtilisiesGradationAssistanceArgNextState)e;

                    goToNextState(next.m_nextState);
                };


                return /*m_assistanceIntermediateStates*/(MouseUtilitiesGradationAssistanceIntermediateState)m_assistanceGradation[id];
            }

            /*
             * id of the next state to reach
             * Return true if max gradation is reached, false otherwise
             * */
            bool goToNextState(string idNext)
            {
                bool toReturn = false;

                MouseUtilitiesGradationAssistanceAbstract stateCurrent = m_assistanceGradation[m_gradationCurrent];
                MouseUtilitiesGradationAssistanceAbstract stateNext = m_assistanceGradation[m_gradationCurrent].getGradationNext(idNext);

                if (stateNext == null)
                { // Means we have reached the last state of the state machine. So nothing to do excepted informing the user
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "The required next state " + idNext + " is not defined as a potentiel next state for the current state " + stateCurrent.getId() + ". Nothing will happen. This can be a normal behavior.");

                    toReturn = true;
                }
                else
                {
                    m_gradationNext = stateNext.getId();
                    //MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Transitions from " + stateCurrent.getId() + " to " + stateNext.getId() + ". Number of show functions to call: " + stateNext.getFunctionsShow().Count);

                    goToState(stateCurrent, stateNext);

                }

                return toReturn;
            }

            void goToPreviousState()
            { // No arguments as previous state is known internally
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Transitions from " + m_assistanceGradation[m_gradationCurrent].getId() + " to " + m_assistanceGradation[m_gradationPrevious].getId());

                m_gradationNext = m_gradationPrevious; // Little trick to handle correctly the transition to the previous state, as next becomes current and current becomes previous.

                goToState(m_assistanceGradation[m_gradationCurrent], m_assistanceGradation[m_gradationPrevious]);
            }

            // Be careful with this function ! Should not be called directly if you do not know what your are doing. It is a short function but can mess up many things.
            void goToState(MouseUtilitiesGradationAssistanceAbstract current, MouseUtilitiesGradationAssistanceAbstract next)
            { // Current: will call hide function; Next: will call show functions
              //MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Called. Going from " + current.getId() + " to " + next.getId());

                raiseEventsGradation(current.getFunctionHide(), current.getFunctionHideEventHandler(), next.getFunctionsShow(), next.getFunctionsShowEventHandlers());
            }

            /**
             * Reponsible to call the hide event of the current state, and the shows functions of the next state.
             * */
            void raiseEventsGradation(Action<EventHandler> fHide, EventHandler fHideEventHandler, List<Action<EventHandler>> fShows, List<EventHandler> fShowsEventHandler)
            {
                fHide(new EventHandler(delegate (System.Object o, EventArgs e)
                {
                    for (int i = 0; i < fShows.Count; i++)
                    {
                        //MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Function show " + i + " is going to be called");

                        fShows[i](fShowsEventHandler[i]);
                    }

                    fHideEventHandler?.Invoke(this, EventArgs.Empty);
                }));
            }

            public void goBackToOriginalState()
            {
                if (m_gradationInitial == "")
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Initial state not set - however required. So nothing will happen.");
                }
                else
                {
                    m_gradationNext = m_gradationInitial;

                    goToState(m_assistanceGradation[m_gradationCurrent], m_assistanceGradation[m_gradationNext]);
                }
            }
        }

        public abstract class MouseUtilitiesGradationAssistanceAbstract
        {
            public abstract string getId();
            public abstract List<Action<EventHandler>> getFunctionsShow();
            public abstract List<EventHandler> getFunctionsShowEventHandlers();
            public abstract Action<EventHandler> getFunctionHide();
            public abstract EventHandler getFunctionHideEventHandler();
            public abstract MouseUtilitiesGradationAssistanceAbstract getGradationNext(string id);
            public abstract Dictionary<string, MouseUtilitiesGradationAssistanceAbstract> getNextStates();
        }

        /*
         * This class managed the states for the above state machine manager. 
         * Once you have created a state with the constructor, you have to add a pointer to one or several functions that should be called when the state is enabled, thanks to the "addFunctionShow" function. The only requirement of these functions is to have a EvantHandler as input parameter. If your function does not have such parameter, you can easily encapsulate it in a delegate. The "show" functions are called sequentially.
         * Then do a similar process by setting a hide function using the "setHideFunction" parameter. As you can notice, you can set several "show" function and only one "hide" function. The reason is that it can happen that a "show" is composed of several steps, whereas the "hide" concerns only the object targeted by the state. Moreover, the "hide" function is used as a entry point to call the show functions. But you can also use a delegate to encapsulate this and do several processes. I would not advice that, because my feeling would be that something should be changed in the architecture of the code. However, if you do that, DO NOT FORGET to invoke the EventHandler given as input parameter at the end of your delegate. The reason is that due to the internal process, it won't be called automatically called.
         * */
        public class MouseUtilitiesGradationAssistance : MouseUtilitiesGradationAssistanceAbstract
        {
            string m_id;

            Dictionary<string, MouseUtilitiesGradationAssistanceAbstract> m_nextStates; // Id of the state, <next state corresponding to this ID, list of hide / show pair>. Use of this way because a same state can go to different next states following the provided interaction

            List<Action<EventHandler>> m_functionsShow;
            List<EventHandler> m_functionsShowEventHandlers;
            Action<EventHandler> m_functionHide;
            EventHandler m_functionHideEventHandler;

            public event EventHandler m_triggerNext;
            public event EventHandler m_triggerPrevious;

            public MouseUtilitiesGradationAssistance(string id)
            {
                m_id = id;
                m_nextStates = new Dictionary<string, MouseUtilitiesGradationAssistanceAbstract>();

                m_functionsShow = new List<Action<EventHandler>>();
                m_functionsShowEventHandlers = new List<EventHandler>();
                m_functionHide = null;
                m_functionHideEventHandler = null;
            }

            public override string getId()
            {
                return m_id;
            }

            public EventHandler setGradationPrevious()
            {
                return delegate
                {
                    m_triggerPrevious?.Invoke(this, EventArgs.Empty);
                };
            }

            public EventHandler goToState(MouseUtilitiesGradationAssistanceAbstract nextState)
            {
                if (m_nextStates.ContainsKey(nextState.getId()) == false)
                {
                    m_nextStates.Add(nextState.getId(), nextState);
                }


                return new EventHandler(delegate (System.Object o, EventArgs e)
                {
                    MouseUtilisiesGradationAssistanceArgNextState temp = new MouseUtilisiesGradationAssistanceArgNextState();
                    temp.m_nextState = nextState.getId();

                    m_triggerNext?.Invoke(this, temp);
                });
            }

            // Returns null if key does not exist. Most likely means that you have reached the last state of the state machine. So sad.
            public override MouseUtilitiesGradationAssistanceAbstract getGradationNext(string id)
            {
                MouseUtilitiesGradationAssistanceAbstract toReturn = null;

                if (m_nextStates.ContainsKey(id))
                {
                    toReturn = m_nextStates[id];
                }

                return toReturn;
            }

            public void addFunctionShow(Action<EventHandler> fShow, EventHandler handler)
            {
                m_functionsShow.Add(fShow);
                m_functionsShowEventHandlers.Add(handler);
            }

            public void addFunctionShow(Assistances.Assistance assistance, EventHandler callback)
            {
                addFunctionShow(assistance.Show, callback);
            }

            /**
             * Then no callback trigerred when processed finished
             * */
            public void addFunctionShow(Assistances.Assistance assistance)
            {
                addFunctionShow(assistance.Show, Utilities.Utility.GetEventHandlerEmpty());
            }

            public override List<Action<EventHandler>> getFunctionsShow()
            {
                return m_functionsShow;
            }

            public override List<EventHandler> getFunctionsShowEventHandlers()
            {
                return m_functionsShowEventHandlers;
            }

            public void setFunctionHide(Action<EventHandler> fHide, EventHandler handler)
            {
                m_functionHide = fHide;
                m_functionHideEventHandler = handler;
            }

            public void setFunctionHide(Assistances.Assistance assistance, EventHandler callback)
            {
                setFunctionHide(assistance.Hide, callback);
            }

            public void setFunctionHide(Assistances.Assistance assistance)
            {
                setFunctionHide(assistance.Hide, Utilities.Utility.GetEventHandlerEmpty());
            }

            public void setFunctionHideAndShow(Assistances.Assistance assistance)
            {
                setFunctionHide(assistance);
                addFunctionShow(assistance);
            }

            public override Action<EventHandler> getFunctionHide()
            {
                return m_functionHide;
            }

            public override EventHandler getFunctionHideEventHandler()
            {
                return m_functionHideEventHandler;
            }

            public override Dictionary<string, MouseUtilitiesGradationAssistanceAbstract> getNextStates()
            {
                return m_nextStates;
            }
        }

        public class MouseUtilitiesGradationAssistanceIntermediateState : MouseUtilitiesGradationAssistanceAbstract
        {
            Dictionary<string, MouseUtilitiesGradationAssistanceAbstract> m_statesThatHaveToCall;
            Dictionary<string, bool> m_statesWhoCalled; // if the bool it true, means the state trigerred the callback. All boolean have to be true to triger the request for the next state
            Dictionary<string, EventHandler> m_statesCallbacks;
            MouseUtilitiesGradationAssistanceAbstract m_nextState; // Called once all the state from the previous dictionary have shown up
            public EventHandler s_eventNextState;
            string m_id;
            int m_nbOfStatesCalled = 0; // Variable storing the number of intermediate states that have called so far

            Action<EventHandler> m_showFunction;

            public MouseUtilitiesGradationAssistanceIntermediateState(string id, MouseUtilitiesGradationAssistance nextState)
            {
                m_id = id;
                m_statesThatHaveToCall = new Dictionary<string, MouseUtilitiesGradationAssistanceAbstract>();
                m_statesWhoCalled = new Dictionary<string, bool>();
                m_statesCallbacks = new Dictionary<string, EventHandler>();
                setNextState(nextState);
            }

            /**
             * For internal use. Please do not call this function, although it is public on purpose
             * */
            public void setFunctionShow(Action<EventHandler> fShow)
            {
                m_showFunction = fShow;
            }

            /**
             * State associated with the trigger that will call the returned EventHandler
             * */
            public EventHandler addState(MouseUtilitiesGradationAssistance state)
            {
                EventHandler callback;

                if (m_statesThatHaveToCall.ContainsKey(state.getId()) == false)
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "New state added: " + state.getId());

                    m_statesThatHaveToCall.Add(state.getId(), state);
                    m_statesWhoCalled.Add(state.getId(), false);

                    callback = new EventHandler(delegate (System.Object o, EventArgs e)
                    {
                        m_statesWhoCalled[state.getId()] = true;

                        // Check if all states are true. If yes, then trigger the request to show the next state
                        tryTriggerNextState();
                    });

                    state.goToState(m_nextState);

                    m_statesCallbacks.Add(state.getId(), callback);
                }
                else
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "State already registered - returning the callback already registered for this state");
                    callback = m_statesCallbacks[state.getId()];
                }

                return callback;
            }

            void tryTriggerNextState()
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Trying to trigger the next state");

                bool triggerNextState = true;

                int totalNbStates = 0;
                int nbStatesCalled = 0;

                foreach (KeyValuePair<string, bool> pair in m_statesWhoCalled)
                {
                    totalNbStates++;

                    if (pair.Value == false)
                    {
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "State " + pair.Key + " NOT called");

                        triggerNextState = false;
                        //break; // No need to continue the loop
                    }
                    else
                    {
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "State " + pair.Key + " called");

                        nbStatesCalled++;
                    }
                }

                m_nbOfStatesCalled = nbStatesCalled;

                if (triggerNextState)
                {
                    MouseUtilisiesGradationAssistanceArgNextState temp = new MouseUtilisiesGradationAssistanceArgNextState();
                    temp.m_nextState = m_nextState.getId();

                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "All intermediate states have called, going to the next state!");

                    s_eventNextState?.Invoke(this, temp);
                }
                else
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Cannot trigger the next state yet - Total number of registered states: " + totalNbStates + " number of states called so far : " + m_nbOfStatesCalled);
                }
            }

            public bool checkStateCalled(MouseUtilitiesGradationAssistance state)
            {
                return m_statesWhoCalled[state.getId()];
            }

            void setNextState(MouseUtilitiesGradationAssistance next)
            {
                m_nextState = next;
            }

            public List<MouseUtilitiesGradationAssistanceAbstract> getStatesCaller()
            {
                return m_statesThatHaveToCall.Values.ToList();
            }

            public MouseUtilitiesGradationAssistanceAbstract getStateNext()
            {
                return m_nextState;
            }

            public override string getId()
            {
                return m_id;
            }

            public override List<Action<EventHandler>> getFunctionsShow()
            {
                List<Action<EventHandler>> toReturn = new List<Action<EventHandler>>();

                // Stacking in the return list all the show functions available
                foreach (KeyValuePair<string, MouseUtilitiesGradationAssistanceAbstract> assistance in m_statesThatHaveToCall)
                {
                    toReturn.AddRange(assistance.Value.getFunctionsShow());
                }

                toReturn.Insert(0, m_showFunction); // For internal purpose, to add extra process from the gradationmanager

                return toReturn;
            }

            public override List<EventHandler> getFunctionsShowEventHandlers()
            {
                List<EventHandler> toReturn = new List<EventHandler>();

                // Stacking in the return list all the show functions available
                foreach (KeyValuePair<string, MouseUtilitiesGradationAssistanceAbstract> assistance in m_statesThatHaveToCall)
                {
                    toReturn.AddRange(assistance.Value.getFunctionsShowEventHandlers());
                }

                toReturn.Insert(0, Utilities.Utility.GetEventHandlerEmpty()); // For internal purpose, to add extra process from the gradationmanager, although this is empty here. This is to remain consistant with the bunch of show functions, where the process is relevant.

                return toReturn;
            }

            public override Action<EventHandler> getFunctionHide()
            {
                return hide;
            }

            void hide(EventHandler eventHandler)
            {
                // Calling hide function of all registered assistances

                foreach (KeyValuePair<string, MouseUtilitiesGradationAssistanceAbstract> assistance in m_statesThatHaveToCall)
                {
                    assistance.Value.getFunctionHide()(assistance.Value.getFunctionHideEventHandler());
                }

                // And resetting the object
                foreach (KeyValuePair<string, MouseUtilitiesGradationAssistanceAbstract> state in m_statesThatHaveToCall)
                {
                    m_statesWhoCalled[state.Key] = false;
                }

                eventHandler?.Invoke(this, EventArgs.Empty);
            }

            public override EventHandler getFunctionHideEventHandler()
            {
                return delegate (System.Object o, EventArgs e)
                {

                };
            }

            public override MouseUtilitiesGradationAssistanceAbstract getGradationNext(string id)
            {
                MouseUtilitiesGradationAssistanceAbstract toReturn = null;

                if (m_nextState.getId() == id)
                {
                    toReturn = m_nextState;
                }

                return toReturn;
            }

            public override Dictionary<string, MouseUtilitiesGradationAssistanceAbstract> getNextStates()
            {
                // Only one "next state" supported at the moment - no real technical issue to support more than one I think, but I have to make sure it works for one before going further with this.
                // Consequently, this function is here only to implement the bastract class and encapsulate the "next state" in the required dictionary.
                Dictionary<string, MouseUtilitiesGradationAssistanceAbstract> toReturn = new Dictionary<string, MouseUtilitiesGradationAssistanceAbstract>();

                toReturn.Add(m_nextState.getId(), m_nextState);

                return toReturn;
            }

            public int getNbOfStatesWhoCalled()
            {
                return m_nbOfStatesCalled;
            }
        }

        /**
         * Simple encapsulation to inform the main class about the ID of the next state
         * */
        public class MouseUtilisiesGradationAssistanceArgNextState : EventArgs
        {
            public string m_nextState;
        }
        public class MouseUtilisiesGradationAssistanceArgCurrentState : EventArgs
        {
            public MouseUtilitiesGradationAssistanceAbstract m_currentState;

            public MouseUtilisiesGradationAssistanceArgCurrentState(MouseUtilitiesGradationAssistanceAbstract currentState)
            {
                m_currentState = currentState;
            }
        }

    }
}

