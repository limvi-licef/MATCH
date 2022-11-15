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
using TMPro;
using Microsoft.MixedReality.Toolkit.UI;

namespace MATCH
{
    namespace Scenarios
    {
        namespace BehaviorTrees
        {
            public class DustingTable : Scenarios.BehaviorTrees.BehaviorTree
            {
                //private NPBehave.Root Tree;
                public Inferences.Manager InferenceManager;
                public Assistances.Surfaces.Manager SurfacesManager;

                string ConditionTableCleaned = "TableCleaned",
                    ConditionRagTaken = "RagTaken",
                    ConditionCleaningWithoutRag = "CleaningWithoutRag",
                    ConditionCleaningWithRag = "CleaningWithRag",
                    ConditionNewPartsCleaned = "NewPartsCleaned",
                    ConditionCleaningInterrupted = "CleaningInterrupted";

                public override void Awake()
                {
                    base.Awake();
                    SetId("Nettoyer la table");
                }

                public override void Start()
                {
                    base.Start();

                    // Creating the conditions
                    Conditions[ConditionTableCleaned] = false;
                    Conditions[ConditionRagTaken] = false;
                    Conditions[ConditionCleaningWithoutRag] = false;
                    Conditions[ConditionCleaningWithRag] = false;
                    Conditions[ConditionNewPartsCleaned] = false;
                    Conditions[ConditionCleaningInterrupted] = false;

                    // Making the conditions update
                    int nbConditions = Conditions.Keys.Count;
                    AddConditionsUpdate(ConditionTableCleaned, new bool[] { true, false, false, false, false, false });
                    AddConditionsUpdate(ConditionRagTaken, new bool[] { false, true, false, false, false, false });
                    AddConditionsUpdate(ConditionCleaningWithoutRag, new bool[] { false, false, true, false, false, false });
                    AddConditionsUpdate(ConditionCleaningWithRag, new bool[] { false, true, false, true, false, false });
                    AddConditionsUpdate(ConditionNewPartsCleaned, new bool[] { false, true, false, true, true, false });
                    AddConditionsUpdate(ConditionCleaningInterrupted, new bool[] { false, true, false, true, false, true });

                    // Defining the BT
                    BlackboardCondition cTableCleaned = new BlackboardCondition(ConditionTableCleaned, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceAlpha());


                    BlackboardCondition cCleaningWithoutRag = new BlackboardCondition(ConditionCleaningWithoutRag, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceEpsilon());

                    Selector srRagNotTaken = new Selector(
                        cCleaningWithoutRag,
                        AssistanceBeta()
                        );

                    BlackboardCondition cRagNotTaken = new BlackboardCondition(ConditionRagTaken, Operator.IS_EQUAL,
                        false, Stops.IMMEDIATE_RESTART, srRagNotTaken);

                    BlackboardCondition cBegingCleanTable = new BlackboardCondition(ConditionCleaningWithRag, Operator.IS_EQUAL, false, Stops.IMMEDIATE_RESTART, AssistanceGamma());

                    BlackboardCondition cCleanNewPart = new BlackboardCondition(ConditionNewPartsCleaned, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceZeta());

                    BlackboardCondition cInterruptCleaning = new BlackboardCondition(ConditionCleaningInterrupted, Operator.IS_EQUAL, true, Stops.IMMEDIATE_RESTART, AssistanceDelta());

                    Selector srTableNotCleaned = new Selector(
                        cRagNotTaken,
                        cBegingCleanTable,
                        cCleanNewPart,
                        cInterruptCleaning
                        );

                    Selector srBegin = new Selector(
                        cTableCleaned,
                        srTableNotCleaned);

                    Tree = new Root(Conditions, srBegin);

                    // NP debugger
                    NPBehave.Debugger debugger = (Debugger)this.gameObject.AddComponent(typeof(Debugger));
                    debugger.BehaviorTree = Tree;

                    // Start BT
                    Tree.Start();

                    // Debug buttons to check if the BT has been correctly modeled
                    AdminMenu.Instance.AddButton("BT - Dusting - Trigger - table cleaned", delegate 
                    {
                        UpdateConditionWithMatrix(ConditionTableCleaned);
                    }, AdminMenu.Panels.Right);

                    AdminMenu.Instance.AddButton("BT - Dusting - Trigger - rag taken", delegate
                    {
                        UpdateConditionWithMatrix(ConditionRagTaken);
                    }, AdminMenu.Panels.Right);

                    AdminMenu.Instance.AddButton("BT - Dusting - Trigger - clean without rag", delegate
                    {
                        UpdateConditionWithMatrix(ConditionCleaningWithoutRag);
                    }, AdminMenu.Panels.Right);

                    AdminMenu.Instance.AddButton("BT - Dusting - Trigger - clean with rag", delegate
                    {
                        UpdateConditionWithMatrix(ConditionCleaningWithRag);
                    }, AdminMenu.Panels.Right);
                    AdminMenu.Instance.AddButton("BT - Dusting - Trigger - new parts cleaned", delegate
                    {
                        UpdateConditionWithMatrix(ConditionNewPartsCleaned);
                    }, AdminMenu.Panels.Right);
                    AdminMenu.Instance.AddButton("BT - Dusting - Trigger - cleaning interrupted", delegate
                    {
                        UpdateConditionWithMatrix(ConditionCleaningInterrupted);
                    }, AdminMenu.Panels.Right);
                    AdminMenu.Instance.AddButton("BT - Dusting - Trigger - all false", delegate
                    {
                        UpdateConditionWithMatrix(ConditionTableCleaned);
                        UpdateCondition(ConditionTableCleaned, false);
                    }, AdminMenu.Panels.Right);
                }

                Sequence AssistanceAlpha()
                {
                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => AssistancesDebugWindow.SetDescription("Alpha")),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                Sequence AssistanceBeta()
                {
                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => AssistancesDebugWindow.SetDescription("Beta")),
                        new WaitUntilStopped()
                        );

                    return temp;
                }

                Sequence AssistanceEpsilon()
                {
                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => AssistancesDebugWindow.SetDescription("Epsilon")),
                        new WaitUntilStopped());

                    return temp;
                }
                    
                Sequence AssistanceGamma()
                {
                    Sequence temp = new Sequence (
                        new NPBehave.Action(() => AssistancesDebugWindow.SetDescription("Gamma")),
                        new WaitUntilStopped());

                    return temp;
                }

                Sequence AssistanceTest()
                {
                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => AssistancesDebugWindow.SetDescription("Test")),
                        new WaitUntilStopped());

                    return temp;
                }

                Sequence AssistanceZeta()
                {
                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => AssistancesDebugWindow.SetDescription("Zeta")),
                        new WaitUntilStopped());

                    return temp;
                }

                Sequence AssistanceDelta()
                {
                    Sequence temp = new Sequence(
                        new NPBehave.Action(() => AssistancesDebugWindow.SetDescription("Delta")),
                        new WaitUntilStopped());

                    return temp;
                }
            }

        }
    }
}
