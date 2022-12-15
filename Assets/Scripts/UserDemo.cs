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
    public class UserDemo : MonoBehaviour
    {
        public Scenarios.FiniteStateMachine.TakeOutGarbage m_challengeGarbage;
        bool m_challengeGarbageFirstLevelCalled = false;

        public Scenarios.FiniteStateMachine.WateringThePlants m_challengeWatering;

        public Scenarios.FiniteStateMachine.DustingTheTable m_challengeTable;

        Assistances.Basic m_triggerGarbage;
        Assistances.Basic m_triggerWateringPlants;
        Assistances.Basic m_triggerCleanTable;

        // Start is called before the first frame update
        void Start()
        {
            initializeScenario();

            AdminMenu.Instance.AddSwitchButton("User demo - Hide", delegate ()
            {
                gameObject.SetActive(!gameObject.activeSelf);
            }, AdminMenu.Panels.Left, AdminMenu.ButtonType.Hide);
        }

        // Update is called once per frame
        void Update()
        {

        }

        void initializeScenario()
        {
            Assistances.InteractionSurface demo = Assistances.Factory.Instance.CreateInteractionSurface("User Demos", AdminMenu.Panels.Left, new Vector3(1.1f, 0.02f, 0.7f), new Vector3(0,0,0), Utilities.Materials.Colors.PurpleGlowing, true, false, Utilities.Utility.GetEventHandlerEmpty(), true, transform);

            Assistances.Basic demoSurface = Assistances.Factory.Instance.CreateFlatSurface(Utilities.Materials.Colors.CyanGlowing, new Vector3(demo.GetLocalPosition().x, demo.GetLocalPosition().y + 0.02f, demo.GetLocalPosition().z), demo.transform);
            demoSurface.SetScale(new Vector3(demo.GetLocalScale().x, demo.GetLocalScale().y, demo.GetLocalScale().z));
            demoSurface.Show(Utilities.Utility.GetEventHandlerEmpty(), false);

            m_triggerGarbage = Assistances.Factory.Instance.CreateCube(Utilities.Materials.Textures.GarbageLevel1, demo.transform);
            m_triggerGarbage.SetLocalPosition(new Vector3(-0.2f, m_triggerGarbage.GetLocalPosition().y, m_triggerGarbage.GetLocalPosition().z));
            m_triggerGarbage.Show(Utilities.Utility.GetEventHandlerEmpty(), false);
            m_triggerGarbage.s_touched += delegate (System.Object o, EventArgs e)
            {
                if (m_challengeGarbageFirstLevelCalled == false)
                {
                    m_challengeGarbage.GetInference19h().CallbackOneMinuteTrigger();
                    m_challengeGarbageFirstLevelCalled = true;
                    m_triggerGarbage.SetMaterial(Utilities.Materials.Textures.GarbageLevel2);
                }
                else
                {
                    m_challengeGarbage.GetInference19h30().CallbackOneMinuteTrigger();
                    m_challengeGarbageFirstLevelCalled = false;
                    m_triggerGarbage.SetMaterial(Utilities.Materials.Textures.GarbageLevel2Pressed);
                }
            };

            m_triggerWateringPlants = Assistances.Factory.Instance.CreateCube(Utilities.Materials.Textures.Flower, demo.transform);
            m_triggerWateringPlants.SetLocalPosition(new Vector3(0f, m_triggerWateringPlants.GetLocalPosition().y, m_triggerWateringPlants.GetLocalPosition().z));
            m_triggerWateringPlants.Show(Utilities.Utility.GetEventHandlerEmpty(), false);
            m_triggerWateringPlants.s_touched += delegate (System.Object o, EventArgs e)
            {
                m_challengeWatering.GetInference().CallbackOneMinuteTrigger();
                m_triggerWateringPlants.SetMaterial(Utilities.Materials.Textures.FlowerPressed);
            };


            m_triggerCleanTable = Assistances.Factory.Instance.CreateCube(Utilities.Materials.Textures.CleanTable, demo.transform);
            m_triggerCleanTable.SetLocalPosition(new Vector3(0.2f, m_triggerCleanTable.GetLocalPosition().y, m_triggerCleanTable.GetLocalPosition().z));
            m_triggerCleanTable.Show(Utilities.Utility.GetEventHandlerEmpty(), false);
            m_triggerCleanTable.s_touched += delegate (System.Object o, EventArgs e)
            {
                m_challengeTable.GetInference().CallbackOneMinuteTrigger();
                m_triggerCleanTable.SetMaterial(Utilities.Materials.Textures.CleanTablePressed);
            };

            Assistances.Dialog dialogInstructions = Assistances.Factory.Instance.CreateDialogNoButton("", "Touchez un des boutons pour commencer un scénario. Le scénario commence 10 secondes après avoir touché le bouton.", demo.transform);
            //MouseAssistanceDialog dialogInstructions = MouseUtilitiesAssistancesFactory.Instance.createDialogTwoButtons("", "Touchez un des boutons pour commencer un scénario. Le scénario commence 10 secondes après avoir touché le bouton.", "Test bouton 1", MouseUtilities.getEventHandlerEmpty(), "Test button 2", MouseUtilities.getEventHandlerEmpty(), demo.transform);
            //dialogInstructions.setDescription("Touchez un des boutons pour commencer un scénario: sortir les poubelles, arroser les plantes, nettoyer la table.Le scénario commence 10 secondes après avoir touché le bouton.", 0.12f);
            //dialogInstructions.setDescription("Touchez un des boutons pour commencer un scénario. Le scénario commence 10 secondes après avoir touché le bouton.", 0.15f);
            dialogInstructions.AdjustToHeight = false;
            dialogInstructions.transform.localPosition = new Vector3(0f, 0.5f, 0);
            dialogInstructions.Show(Utilities.Utility.GetEventHandlerEmpty(), false);


            m_challengeGarbage.EventChallengeOnStandBy += callbackChallengeGarbageStandBy;
            m_challengeWatering.EventChallengeOnStandBy += callbackChallengeWateringPlants;
            m_challengeTable.EventChallengeOnStandBy += callbackChallengeCleanTable;
        }

        void callbackChallengeGarbageStandBy(System.Object o, EventArgs e)
        {
            m_triggerGarbage.SetMaterial(Utilities.Materials.Textures.GarbageLevel1);
            m_challengeGarbageFirstLevelCalled = false;
        }

        void callbackChallengeCleanTable(System.Object o, EventArgs e)
        {
            m_triggerCleanTable.SetMaterial(Utilities.Materials.Textures.CleanTable);
        }

        void callbackChallengeWateringPlants(System.Object o, EventArgs e)
        {
            m_triggerWateringPlants.SetMaterial(Utilities.Materials.Textures.Flower);
        }
    }

}

