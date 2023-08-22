/*Copyright 2022 Guillaume Spalla, Louis Marquet, Léri Lamour

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
using MATCH.Assistances.GradationVisual;

namespace MATCH
{
    namespace Assistances
    {
        public class Factory : MonoBehaviour
        {
            private static Factory InstanceInternal;
            public static Factory Instance { get { return InstanceInternal; } }

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

            private MATCH.Assistances.Dialogs.Dialog1 InitializeDialog(DialogsTypes type, string title, string description, Transform parent)
            {
                Transform view = /*Instantiate(*/Utilities.Materials.Prefabs.Load(DialogsStore[type]).transform;
                view.parent = parent;
                view.localPosition = new Vector3(0, 0, 0);

                MATCH.Assistances.Dialogs.Dialog1 controller = view.GetComponent<MATCH.Assistances.Dialogs.Dialog1>();
                controller.SetTitle(title, 0.15f);
                float sizeDescriptionText = -0.0005f * description.Length + 0.206f;
                controller.SetDescription(description, sizeDescriptionText);
                controller.EnableBillboard(true);

                return controller;
            }

            private void AddButton(ref MATCH.Assistances.Dialogs.Dialog dialog, EventHandler callback, string text, Assistances.Buttons.Button.ButtonType type, bool autoScaling, float fontSize = -1)
            {
                dialog.AddButton(text, autoScaling, fontSize);
                dialog.ButtonsController.Last().EventButtonClicked += callback;
  
                dialog.ButtonsController.Last().Type = type;
            }

            /*private void AddButton(ref Dialog dialog, EventHandler callback, string text, Assistances.Buttons.Button.ButtonType type, bool autoScaling, float fontSizeCoeffA, float fontSizeCoeffB)
            {
                AddButton(ref dialog, callback, text, type, autoScaling, fontSizeCoeffA * text.Length + fontSizeCoeffB);
            }*/

            public MATCH.Assistances.Dialogs.Dialog1 CreateDialogNoButton(string title, string description, Transform parent)
            {
                MATCH.Assistances.Dialogs.Dialog1 controller = InitializeDialog(DialogsTypes.Assistance, title, description, parent);

                return controller;;
            }

            public MATCH.Assistances.Dialogs.Dialog1 CreateCheckListNoButton(string title, string description, Transform parent)
            {
                MATCH.Assistances.Dialogs.Dialog1 controller = InitializeDialog(DialogsTypes.CheckList, title, description, parent);

                return controller;
            }

            public MATCH.Assistances.Dialogs.Dialog1 CreateDialogOneButton(string title, string description, string textButton1, EventHandler callbackButton1, Assistances.Buttons.Button.ButtonType type1, Transform parent)
            {
                MATCH.Assistances.Dialogs.Dialog controller = CreateDialogNoButton(title, description, parent);

                /*float fontSizeCoeffA = -0.017f;
                float fontSizeCoeffB = 0.37f;*/

                //AddButton(ref controller, callbackButton1, textButton1, type1, true, FontSizeCoeffA, FontSizeCoeffB);
                AddButton(ref controller, callbackButton1, textButton1, type1, true, ComputeFontSizeExp(textButton1.Length));

                return (Dialogs.Dialog1)controller;
            }

            public MATCH.Assistances.Dialogs.Dialog1 CreateDialogTwoButtons(string title, string description, string textButton1, EventHandler callbackButton1, Assistances.Buttons.Button.ButtonType type1, string textButton2, EventHandler callbackButton2, Assistances.Buttons.Button.ButtonType type2, Transform parent)
            {
                MATCH.Assistances.Dialogs.Dialog controller = CreateDialogNoButton(title, description, parent);



                //AddButton(ref controller, callbackButton1, textButton1, type1, true, FontSizeCoeffA, FontSizeCoeffB);
                //AddButton(ref controller, callbackButton2, textButton2, type2, true, FontSizeCoeffA, FontSizeCoeffB);
                AddButton(ref controller, callbackButton1, textButton1, type1, true, ComputeFontSizeExp(textButton1.Length));
                AddButton(ref controller, callbackButton2, textButton2, type2, true, ComputeFontSizeExp(textButton2.Length));

                return (Dialogs.Dialog1)controller;
            }

            public MATCH.Assistances.Dialogs.Dialog1 CreateDialogThreeButtons(string title, string description, string textButton1, EventHandler callbackButton1, Assistances.Buttons.Button.ButtonType type1, string textButton2, EventHandler callbackButton2, Assistances.Buttons.Button.ButtonType type2, string textButton3, EventHandler callbackButton3, Assistances.Buttons.Button.ButtonType type3, Transform parent)
            {
                MATCH.Assistances.Dialogs.Dialog controller = CreateDialogNoButton(title, description, parent);

                AddButton(ref controller, callbackButton1, textButton1, type1, true, ComputeFontSizeExp(textButton1.Length));
                AddButton(ref controller, callbackButton2, textButton2, type2, true, ComputeFontSizeExp(textButton2.Length));
                AddButton(ref controller, callbackButton3, textButton3, type3, true, ComputeFontSizeExp(textButton3.Length));

                return (Dialogs.Dialog1)controller;
            }

            public Dialogs.Dialog2 CreateDialog2NoButton(string title, string description, Transform parent)
            {
                Transform view = Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.AssistanceDialog2);
                view.parent = parent;
                view.localPosition = new Vector3(0, 0, 0);

                Dialogs.Dialog2 controller = view.GetComponent<Dialogs.Dialog2>();
                controller.SetTitle(title, 0.15f);
                //float sizeDescriptionText = -0.0005f * description.Length + 0.206f;
                float sizeDescriptionText = -0.0003f * description.Length + 0.17f;
                controller.SetDescription(description, sizeDescriptionText);
                controller.EnableBillboard(true);

                return controller;
            }

            public Dialogs.Dialog2 CreateDialog2WithButtons(string title, string description, string textButton1, EventHandler callbackButton1, Assistances.Buttons.Button.ButtonType type1, Transform parent)
            {
                Dialogs.Dialog controller = CreateDialog2NoButton(title, description, parent);

                AddButton(ref controller, callbackButton1, textButton1, type1, true, ComputeFontSizeExp(textButton1.Length));

                return (Dialogs.Dialog2)controller;
            }

            public Dialogs.Dialog2 CreateDialog2WithButtons(string title, string description, string textButton1, EventHandler callbackButton1, Assistances.Buttons.Button.ButtonType type1, string textButton2, EventHandler callbackButton2, Assistances.Buttons.Button.ButtonType type2, Transform parent)
            {
                Dialogs.Dialog controller = CreateDialog2NoButton(title, description, parent);

                AddButton(ref controller, callbackButton1, textButton1, type1, true, ComputeFontSizeExp(textButton1.Length));
                AddButton(ref controller, callbackButton2, textButton2, type2, true, ComputeFontSizeExp(textButton2.Length));

                return (Dialogs.Dialog2)controller;
            }

            public Dialogs.Dialog2Contextualized CreateDialog2WithButtonsContextualized(string assistanceName, string title, string description, Transform contextObject, string textButton1, EventHandler callbackButton1, Assistances.Buttons.Button.ButtonType type1, string textButton2, EventHandler callbackButton2, Assistances.Buttons.Button.ButtonType type2, Transform parent)
            {
                Transform view = Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.AssistanceDialog2);
                view.parent = parent;
                view.localPosition = new Vector3(0, 0, 0);

                Dialogs.Dialog2 controllerToRemove = view.GetComponent<Dialogs.Dialog2>();
                DestroyImmediate(controllerToRemove);

                Dialogs.Dialog2Contextualized controllerContextualized = view.gameObject.AddComponent<Dialogs.Dialog2Contextualized>();

                float sizeDescriptionText = -0.0003f * description.Length + 0.17f;
                controllerContextualized.SetDescription(description, contextObject, sizeDescriptionText);

                Dialogs.Dialog2 controllerDialog2 = (Dialogs.Dialog2)controllerContextualized;

                controllerDialog2.SetTitle(title, 0.15f);
                //float sizeDescriptionText = -0.0005f * description.Length + 0.206f;
                
                //controllerDialog2.SetDescription(description, sizeDescriptionText);
                controllerDialog2.EnableBillboard(true);

                

                Dialogs.Dialog controllerDialog = (Dialogs.Dialog)controllerDialog2;

                Assistances.Factory.Instance.AddButton(ref controllerDialog, callbackButton1, textButton1, type1, true, ComputeFontSizeExp(textButton1.Length));
                AddButton(ref controllerDialog, callbackButton2, textButton2, type2, true, ComputeFontSizeExp(textButton2.Length));

                return controllerContextualized;
            }

            public Basic CreateCube(string texture, Transform parent)
            {
                Transform view = Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.AssistanceCube).transform;
                view.parent = parent;
                view.localPosition = new Vector3(0, 0, 0);

                Basic cubeController = view.GetComponent<Basic>();
                cubeController.SetMaterial(texture);

                return cubeController;
            }

            public Basic CreateCube(string texture, bool adjustHeight, Vector3 scale, Vector3 localPosition, bool enableBillboard, Transform parent)
            {
                Basic cube = CreateCube(texture, parent);
                cube.AdjustHeightOnShow = adjustHeight;
                cube.SetScale(scale);
                cube.SetLocalPosition(localPosition);
                cube.SetBillboard(enableBillboard);

                return cube;
            }

            public Icon CreateIcon(bool adjustHeight, Vector3 localPosition, Vector3 scale, bool enableBillboard, Transform parent, string iconType, string material)
            {
                Transform view = Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.AssistanceIcon).transform;

                view.parent = parent;
                view.transform.localPosition = localPosition;

                Icon controller = view.GetComponent<Icon>();
                controller.AdjustHeightOnShow = adjustHeight;
                controller.SetIconType(iconType);
                controller.SetMaterial(material);
                controller.SetLocalPositionObject(localPosition);
                controller.SetBillboard(enableBillboard);
                controller.SetScale(scale);

                return controller;
            }

            public Basic CreateFlatSurface(string texture, Vector3 localPosition, Transform parent)
            {
                Basic cube = CreateCube(texture, false, new Vector3(1.0f, 0.01f, 1.0f), localPosition, false, parent);

                return cube;
            }

            public Assistances.InteractionSurface CreateInteractionSurface(string id, AdminMenu.Panels panel, Vector3 scaling, Vector3 localPosition, string texture, bool show, bool resizable, EventHandler onMove, bool registerToWLT, Transform parent)
            {
                Transform view = Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.AssistanceInteractionSurface); //Instantiate(RefInteractionSurface.transform, parent);
                view.parent = parent;
                view.name = id;
                Assistances.InteractionSurface controller = view.GetComponent<Assistances.InteractionSurface>();
                controller.SetAdminButtons(id, panel);
                controller.SetScaling(scaling);
                controller.SetColor(texture);
                controller.SetObjectResizable(resizable);
                controller.EventConfigMoved += onMove;

                view.localPosition = localPosition;

                if (registerToWLT)
                {
                     Utilities.WorldLockingToolsManager.Instance.RegisterObject(id, view, controller.GetInteractionSurface());
                }

                controller.ShowInteractionSurfaceTable(true);
                controller.ShowInteractionSurfaceTable(show);

                return controller;
            }

            public SurfaceToProcess CreateSurfaceToProcess(Transform parent, InteractionSurface surfaceToPopulate, int NumberOfRows = 5, int NumberOfColumns = 4)
            {
                Transform view = Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.AssistanceSurfaceToProcess); //Instantiate(RefSurfaceToProcess.transform, parent);
                view.parent = parent;
                view.localPosition = new Vector3(0, 0, 0);

                SurfaceToProcess controller = view.GetComponent<SurfaceToProcess>();
                controller.SetSurfaceToPopulate(surfaceToPopulate);
                controller.SetNumberOfSquares(NumberOfRows, NumberOfColumns);

                return controller;
            }

            public MATCH.Assistances.Dialogs.ToDoList CreateToDoList(string title, string description, Transform parent)
            {
                InteractionSurface temp = CreateInteractionSurface("TodoList", AdminMenu.Panels.Left, new Vector3(0.7f, 0.02f, 0.05f), new Vector3(-1.949f, -0.523f, -2.753f), Utilities.Materials.Colors.Red2, false, true, Utilities.Utility.GetEventHandlerEmpty(), true, parent);

                //MATCH.Assistances.Dialogs.Dialog1 controller = InitializeDialog(DialogsTypes.TodoList, title, description, temp.transform);

                Transform view = /*Instantiate(*/Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.AssistanceDialogToDoList).transform;
                view.parent = temp.transform;
                view.localPosition = new Vector3(0, 0, 0);

                MATCH.Assistances.Dialogs.ToDoList controller = view.GetComponent<MATCH.Assistances.Dialogs.ToDoList>();
                controller.SetTitle(title, 0.15f);
                float sizeDescriptionText = -0.0005f * description.Length + 0.206f;
                controller.SetDescription(description, sizeDescriptionText);
                //controller.EnableBillboard(true);

                controller.EnableBillboard(false);
                return controller;
            }

            public MATCH.Assistances.Dialogs.Dialog1 CreateButtons(string title, string description, List<string> buttonsText, List<EventHandler> buttonsCallback, List<Assistances.Buttons.Button.ButtonType> buttonTypes, Transform parent)
            {
                MATCH.Assistances.Dialogs.Dialog controller = InitializeDialog(DialogsTypes.Buttons, title, description, parent);
                for (int i = 0; i < buttonsText.Count; i ++)
                {
                    AddButton(ref controller, buttonsCallback[i], buttonsText[i], buttonTypes[i], true, ComputeFontSizeExp(buttonsText[i].Length));
                }

                return (Dialogs.Dialog1)controller;
            }           

            private float ComputeFontSizeLinear(int textLength)
            {
                float coeffA = -0.017f;
                float coeffB = 0.37f;

                return coeffA * textLength + coeffB;
            }

            private float ComputeFontSizeExp(int textLength)
            {
                float coeffA = 0.3813f;
                float coeffB = -0.065f;

                return coeffA * (float)Math.Exp(coeffB * textLength);
            }

            public AssistanceGradationExplicit CreateAssistanceGradationExplicit(string name)
            {
                Transform view = Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.AssistanceGradationExplicit);  //Instantiate(RefAssistanceGradationExplicit.transform);
                AssistanceGradationExplicit controller = view.GetComponent<AssistanceGradationExplicit>();
                controller.SetId(name);
                view.name = name;

                return controller;
            }

            public GradationVisual.GradationVisual CreateAssistanceGradationAttention(string name)
            {
                Transform view = Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.AssistanceGradationAttention);
                GradationVisual.GradationVisual controller = view.GetComponent<GradationVisual.GradationVisual>();
                view.name = name;
                return controller;
            }    

            public ArchWithTextAndHelp CreateAssistanceArch(string name, Transform origin, Transform target, Transform parent)
            {
                Transform view = Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.AssistanceArch);
                view.parent = parent;

                ArchWithTextAndHelp controller = view.GetComponent<ArchWithTextAndHelp>();
                view.name = name;
                controller.SetArchStartAndEndPoint(origin, target);

                return controller;
            }

            /**
             * parent: is considered as the "origin", i.e. is where the text will be located and the beginning of the lighted path
             * */
            public PathWithTextAndHelp CreateAssistancePath(string name, string title, string description, Transform target, Transform parent)
            {
                Transform view = Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.AssistanceLightPath);
                view.parent = parent;
                view.transform.localPosition = new Vector3(0, 0, 0);

                PathWithTextAndHelp controller = view.GetComponent<PathWithTextAndHelp>();
                view.name = name;
                controller.InitializeAssistance(title, description, target, parent);
                //controller.SetPathStartAndEndPoint(origin, target);

                return controller;
            }

            public ArchWithTextAndHelp CreateAssistanceArch(string name, Transform origin, Transform target, string description, float size, Transform parent)
            {
                ArchWithTextAndHelp controller = CreateAssistanceArch(name, origin, target, parent);

                controller.SetArchStartAndEndPoint(origin, target);
                controller.SetDescription(description, size);

                return controller;
            }

            public Assistances.LightedPath CreatePathFinding(string name, Transform objectBegin, Transform objectEnd, Transform parent)
            {
                Transform view = /*Instantiate(*/Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.AssistanceLightPath);
                view.parent = parent;

                Assistances.LightedPath controller = view.GetComponent<Assistances.LightedPath>();

                controller.PathFindingEngine = PathFindingEngine;
                controller.SetBeginAndEndObjects(objectBegin, objectEnd);

                return controller;
            }

            /**
             * In this case the follower is used as the origin object
             * */
            public Assistances.LightedPath CreatePathFindingWithFollowerBegin(string name, Transform objectEnd, Transform parent)
            {
                Transform view = /*Instantiate(*/Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.AssistanceLightPath);
                view.parent = parent;

                Assistances.LightedPath controller = view.GetComponent<Assistances.LightedPath>();

                controller.PathFindingEngine = PathFindingEngine;
                controller.SetEndObject(objectEnd);

                return controller;
            }
        }

    }
}

