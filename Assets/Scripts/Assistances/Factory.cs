/*Copyright 2022 Guillaume Spalla, Louis Marquet

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
    namespace Assistances
    {
        public class Factory : MonoBehaviour
        {
            private static Factory InstanceInternal;
            public static Factory Instance { get { return InstanceInternal; } }

            //Todo: all of those should be prefabs that can be loaded dybanically
            //public Assistances.Dialog DialogAssistance;
            //public Basic RefCube;
            //public Assistances.Dialog RefCheckListAssistance;
            //public Assistances.InteractionSurface RefInteractionSurface;
            //public SurfaceToProcess RefSurfaceToProcess;
            //public Assistances.Dialog RefToDoListAssistance;
            //public AssistanceGradationExplicit RefAssistanceGradationExplicit;
            //public Assistances.Dialog RefButtons;
            //public GradationVisual.GradationVisual RefAssistanceGradationAttention;
            //public ArchWithTextAndHelp RefArch;
            public PathFinding.PathFinding PathFindingEngine;

            enum DialogsTypes
            {
                Assistance = 0,
                CheckList = 1,
                TodoList = 2,
                Buttons = 3
            }

            Dictionary<DialogsTypes, string> DialogsStore;

            private void Awake()
            {
                if (InstanceInternal != null && InstanceInternal != this)
                {
                    Destroy(this.gameObject);
                }
                else
                {
                    InstanceInternal = this;

                    // Initialize the dialogs types store
                    DialogsStore = new Dictionary<DialogsTypes, string>();
                    DialogsStore.Add(DialogsTypes.Assistance, /*DialogAssistance*/Utilities.Materials.Prefabs.AssistanceDialog);
                    DialogsStore.Add(DialogsTypes.CheckList, /*RefCheckListAssistance*/Utilities.Materials.Prefabs.AssistanceDialogCheckList);
                    DialogsStore.Add(DialogsTypes.TodoList, /*RefToDoListAssistance*/Utilities.Materials.Prefabs.AssistanceDialogToDoList);
                    DialogsStore.Add(DialogsTypes.Buttons, /*RefButtons*/Utilities.Materials.Prefabs.AssistanceDialogButtons);
                }
            }

            private Dialog InitializeDialog(DialogsTypes type, string title, string description, Transform parent)
            {
                Transform view = Instantiate(Utilities.Materials.Prefabs.Load(DialogsStore[type]).transform, parent);
                Dialog controller = view.GetComponent<Dialog>();
                controller.SetTitle(title, 0.15f);
                float sizeDescriptionText = -0.0005f * description.Length + 0.206f;
                controller.SetDescription(description, sizeDescriptionText);
                controller.EnableBillboard(true);

                return controller;
            }

            private void AddButton(ref Dialog dialog, EventHandler callback, string text, Assistances.Buttons.Button.ButtonType type, bool autoScaling, float fontSize = -1)
            {
                dialog.AddButton(text, autoScaling, fontSize);
                dialog.ButtonsController.Last().EventButtonClicked += callback;
                /*dialog.EventHelpButtonClicked += delegate (System.Object o, EventArgs e)
                {
                    
                };*/
                dialog.ButtonsController.Last().Type = type;
            }

            private void AddButton(ref Dialog dialog, EventHandler callback, string text, Assistances.Buttons.Button.ButtonType type, bool autoScaling, float fontSizeCoeffA, float fontSizeCoeffB)
            {
                AddButton(ref dialog, callback, text, type, autoScaling, fontSizeCoeffA * text.Length + fontSizeCoeffB);
            }

            public Dialog CreateDialogNoButton(string title, string description, Transform parent)
            {
                ///Transform dialogView = Instantiate(DialogAssistance.transform, parent);
                Dialog controller = InitializeDialog(DialogsTypes.Assistance, title, description, parent);

                ///Dialog dialogController = dialogView.GetComponent<Dialog>();
                ///dialogController.SetTitle(title);
                
                //float sizeDescriptionText = -0.002f * description.Length + 0.38f;
                
                ///float sizeDescriptionText = -0.00047619f * description.Length + 0.205714286f;
                ///dialogController.SetDescription(description, sizeDescriptionText);
                ///dialogController.EnableBillboard(true);

                return controller;//dialogController;
            }

            public Dialog CreateCheckListNoButton(string title, string description, Transform parent)
            {
                Dialog controller = InitializeDialog(DialogsTypes.CheckList, title, description, parent);

                //Transform dialogView = Instantiate(RefCheckListAssistance.transform, parent);

                //Dialog dialogController = dialogView.GetComponent<Dialog>();
                //dialogController.SetTitle(title);
                //float sizeDescriptionText = -0.00047619f * description.Length + 0.205714286f;
                //dialogController.SetDescription(description, sizeDescriptionText);
                //dialogController.EnableBillboard(true);

                return controller; //dialogController;
            }

            public Dialog CreateDialogOneButton(string title, string description, string textButton1, EventHandler callbackButton1, Assistances.Buttons.Button.ButtonType type1, Transform parent)
            {
                //Transform dialogView = Instantiate(m_refDialogAssistance.transform, parent);
                //MouseAssistanceDialog dialogController = dialogView.GetComponent<MouseAssistanceDialog>();
                //dialogController.setTitle(title);
                //float sizeDescriptionText = -0.002f * description.Length + 0.38f;
                //dialogController.setDescription(description, sizeDescriptionText);
                //dialogController.enableBillboard(true);
                Dialog controller = CreateDialogNoButton(title, description, parent);

                float fontSizeCoeffA = -0.017f;
                float fontSizeCoeffB = 0.37f;

                AddButton(ref controller, callbackButton1, textButton1, type1, true, fontSizeCoeffA, fontSizeCoeffB);

                //float sizeDescriptionText = -0.017f * textButton1.Length + 0.37f;
                //controller.AddButton(textButton1, true, sizeDescriptionText);
                //sizeDescriptionText = -0.017f * textButton2.Length + 0.37f;
                //controller.AddButton(textButton2, true, sizeDescriptionText);
                //controller.ButtonsController[0].s_buttonClicked += callbackButton1;
                //controller.ButtonsController[1].s_buttonClicked += callbackButton2;

                return controller;
            }

            public Dialog CreateDialogTwoButtons(string title, string description, string textButton1, EventHandler callbackButton1, Assistances.Buttons.Button.ButtonType type1, string textButton2, EventHandler callbackButton2, Assistances.Buttons.Button.ButtonType type2, Transform parent)
            {
                //Transform dialogView = Instantiate(m_refDialogAssistance.transform, parent);
                //MouseAssistanceDialog dialogController = dialogView.GetComponent<MouseAssistanceDialog>();
                //dialogController.setTitle(title);
                //float sizeDescriptionText = -0.002f * description.Length + 0.38f;
                //dialogController.setDescription(description, sizeDescriptionText);
                //dialogController.enableBillboard(true);
                Dialog controller = CreateDialogNoButton(title, description, parent);

                float fontSizeCoeffA = -0.017f;
                float fontSizeCoeffB = 0.37f;

                AddButton(ref controller, callbackButton1, textButton1, type1, true, fontSizeCoeffA, fontSizeCoeffB);
                AddButton(ref controller, callbackButton2, textButton2, type2, true, fontSizeCoeffA, fontSizeCoeffB);

                //float sizeDescriptionText = -0.017f * textButton1.Length + 0.37f;
                //controller.AddButton(textButton1, true, sizeDescriptionText);
                //sizeDescriptionText = -0.017f * textButton2.Length + 0.37f;
                //controller.AddButton(textButton2, true, sizeDescriptionText);
                //controller.ButtonsController[0].s_buttonClicked += callbackButton1;
                //controller.ButtonsController[1].s_buttonClicked += callbackButton2;

                return controller;
            }

            public Dialog CreateDialogThreeButtons(string title, string description, string textButton1, EventHandler callbackButton1, Assistances.Buttons.Button.ButtonType type1, string textButton2, EventHandler callbackButton2, Assistances.Buttons.Button.ButtonType type2, string textButton3, EventHandler callbackButton3, Assistances.Buttons.Button.ButtonType type3, Transform parent)
            {
                Dialog controller = CreateDialogNoButton(title, description, parent);

                AddButton(ref controller, callbackButton1, textButton1, type1, true);
                AddButton(ref controller, callbackButton2, textButton2, type2, true);
                AddButton(ref controller, callbackButton3, textButton3, type3, true);

                /*controller.AddButton(textButton1, true);
                controller.AddButton(textButton2, true);
                controller.AddButton(textButton3, true);
                controller.ButtonsController[0].s_buttonClicked += callbackButton1;
                controller.ButtonsController[1].s_buttonClicked += callbackButton2;
                controller.ButtonsController[2].s_buttonClicked += callbackButton3;*/

                return controller;
            }

            public Basic CreateCube(string texture, Transform parent)
            {
                Transform view = Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.AssistanceCube).transform;
                view.parent = parent;
                view.localPosition = new Vector3(0, 0, 0);
                //= Instantiate(RefCube.transform, parent);
                Basic cubeController = view.GetComponent<Basic>();
                cubeController.SetMaterial(texture);

                return cubeController;
            }

            public Basic CreateCube(string texture, bool adjustHeight, Vector3 scale, Vector3 localPosition, bool enableBillboard, Transform parent)
            {
                Basic cube = CreateCube(texture, parent);
                //cube.SetAdjustHeightOnShow(adjustHeight);
                cube.AdjustHeightOnShow = adjustHeight;
                cube.SetScale(scale);
                cube.SetLocalPosition(localPosition);
                cube.SetBillboard(enableBillboard);

                return cube;
            }

            public Basic CreateFlatSurface(string texture, Vector3 localPosition, Transform parent)
            {
                Basic cube = CreateCube(texture, false, new Vector3(1.0f, 0.01f, 1.0f), localPosition, false, parent);

                return cube;
            }

            public Assistances.InteractionSurface CreateInteractionSurface(string id, AdminMenu.Panels panel, Vector3 scaling, string texture, bool show, bool resizable, EventHandler onMove, Transform parent)
            {
                Transform interactionSurface = Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.AssistanceInteractionSurface); //Instantiate(RefInteractionSurface.transform, parent);
                interactionSurface.parent = parent;
                Assistances.InteractionSurface controller = interactionSurface.GetComponent<Assistances.InteractionSurface>();
                controller.SetAdminButtons(id, panel);
                controller.SetScaling(scaling);
                controller.SetColor(texture);
                controller.ShowInteractionSurfaceTable(show);
                controller.SetObjectResizable(resizable);
                controller.EventInteractionSurfaceMoved += onMove;

                return controller;
            }

            public SurfaceToProcess CreateSurfaceToProcess(Transform parent)
            {
                Transform view = Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.AssistanceSurfaceToProcess); //Instantiate(RefSurfaceToProcess.transform, parent);
                view.parent = parent;
                view.localPosition = new Vector3(0, 0, 0);

                SurfaceToProcess controller = view.GetComponent<SurfaceToProcess>();

                //MouseChallengeleanTableSurfaceToPopulateWithCubes
                return controller;
            }

            public Dialog CreateToDoList(string title, string description)
            {
                Dialog controller = InitializeDialog(DialogsTypes.TodoList, title, description, null);
                controller.EnableBillboard(false);
                return controller;
            }

            public Dialog CreateButtons(string title, string description, List<string> buttonsText, List<EventHandler> buttonsCallback, List<Assistances.Buttons.Button.ButtonType> buttonTypes, Transform parent)
            {
                Dialog controller = InitializeDialog(DialogsTypes.Buttons, title, description, parent);
                for (int i = 0; i < buttonsText.Count; i ++)
                {
                    AddButton(ref controller, buttonsCallback[i], buttonsText[i], buttonTypes[i], true);
                }

                return controller;
            }

            public AssistanceGradationExplicit CreateAssistanceGradationExplicit(string name)
            {
                Transform view = Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.AssistanceGradationExplicit);  //Instantiate(RefAssistanceGradationExplicit.transform);
                AssistanceGradationExplicit controller = view.GetComponent<AssistanceGradationExplicit>();
                controller.SetId(name);
                view.name = name;
                //view.gameObject.SetActive(true);
                //controller.gameObject.SetActive(true);
                //controller.Init();
                return controller;
            }

            public GradationVisual.GradationVisual CreateAssistanceGradationAttention(string name)
            {
                //Transform view = Instantiate(RefAssistanceGradationAttention.transform);
                Transform view = Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.AssistanceGradationAttention);
                GradationVisual.GradationVisual controller = view.GetComponent<GradationVisual.GradationVisual>();
                view.name = name;
                return controller;
            }    

            public ArchWithTextAndHelp CreateAssistanceArch(string name, Transform origin, Transform target, Transform parent)
            {
                Transform view = Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.AssistanceArch);
                view.parent = parent;
                //Transform view = Instantiate(RefArch.transform, parent);
                ArchWithTextAndHelp controller = view.GetComponent<ArchWithTextAndHelp>();
                view.name = name;
                controller.SetArchStartAndEndPoint(origin, target);

                return controller;
            }

            public ArchWithTextAndHelp CreateAssistanceArch(string name, Transform origin, Transform target, string description, float size, Transform parent)
            {
                ArchWithTextAndHelp controller = CreateAssistanceArch(name, origin, target, parent);

                //Transform view = Instantiate(RefArch.transform, parent);
                //view.name = name;
                //ArchWithTextAndHelp controller = view.GetComponent<ArchWithTextAndHelp>();
                controller.SetArchStartAndEndPoint(origin, target);
                controller.SetDescription(description, size);

                return controller;
            }

            public Assistances.LightedPath CreatePathFinding(string name, Transform objectBegin, Transform objectEnd, Transform parent)
            {
                Transform view = Instantiate(Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.AssistanceLightPath));
                view.parent = parent;

                Assistances.LightedPath controller = view.GetComponent<Assistances.LightedPath>();

                controller.PathFindingEngine = PathFindingEngine;
                controller.SetBeginAndEndObjects(objectBegin, objectEnd);

                return controller;
            }
        }

    }
}
