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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPBehave;
using System;
using System.Reflection;
using System.Linq;

namespace MATCH
{
    namespace Assistances
    {
        namespace GradationVisual
        {
            public class Factory:MonoBehaviour
            {
                private static Factory InstanceInternal;
                public static Factory Instance { get { return InstanceInternal; } }

                public enum AlreadyConfigured
                {
                    SomeoneComingToHelpDialog1 = 0,
                    LetGoDialog1 = 1,
                    DoYouNeedHelpDialog1 = 2,
                    SomeoneComingToHelpDialog2 = 3,
                    LetGoDialog2 = 4,
                    DoYouNeedHelpDialog2 = 5,
                }

                private void Awake()
                {
                    if (InstanceInternal != null && InstanceInternal != this)
                    {
                        Destroy(this.gameObject);
                    }
                    else
                    {
                        InstanceInternal = this;
                    }
                }

                public GradationVisual CreateExclamationMarkFromCube(string name, Transform parent)
                {
                    GradationVisual assistance = Assistances.Factory.Instance.CreateAssistanceGradationAttention(name);
                    Assistances.Basic assistanceBase = Assistances.Factory.Instance.CreateCube(Utilities.Materials.Colors.PurpleGlowing,parent);
                    assistanceBase.name = name+"_base";
                    assistance.AddAssistance(Assistances.Decorators.Factory.Instance.CreateMaterial(assistanceBase, Utilities.Materials.Textures.Exclamation));
                    assistance.AddAssistance(Assistances.Decorators.Factory.Instance.CreateMaterial(assistanceBase, Utilities.Materials.Textures.ExclamationRed));

                    return assistance;
                }

                public GradationVisual CreateExclamationMark(string name, Transform parent)
                {
                    GradationVisual assistance = Assistances.Factory.Instance.CreateAssistanceGradationAttention(name);
                    Assistances.Icon assistanceBase = Assistances.Factory.Instance.CreateIcon(true, new Vector3(0,0,0), new Vector3(0.3f,0.3f,0.3f), true, parent, /*null*/Utilities.Materials.Icon.ExclamationMark, Utilities.Materials.Colors.WhiteMetallicAdjustHSV);
                    assistanceBase.name = name + "_base";

                    assistance.AddAssistance(Assistances.Decorators.Factory.Instance.CreateMaterial(assistanceBase, Utilities.Materials.Colors.CyanAdjustHSV));
                    assistance.AddAssistance(Assistances.Decorators.Factory.Instance.CreateMaterial(assistanceBase, Utilities.Materials.Colors.YellowAdjustHSV));
                    assistance.AddAssistance(Assistances.Decorators.Factory.Instance.CreateMaterial(assistanceBase, Utilities.Materials.Colors.RedAdjustHSV));
                    assistance.AddAssistance(Assistances.Decorators.Factory.Instance.CreateLinePathWithTexture(assistanceBase, Utilities.Materials.Colors.CyanAdjustHSV, 0.039f, true));

                    assistance.transform.localPosition = new Vector3(0, 0, 0);

                    return assistance;
                }

                public GradationVisual CreateDialogNoButton(string assistanceName, string title, string description, Transform parent)
                {
                    MATCH.Assistances.Dialogs.Dialog1 dialog = Assistances.Factory.Instance.CreateDialogNoButton(title, description,  parent);

                    Assistances.GradationVisual.GradationVisual toReturn = AddDecoratorsToDialog1(assistanceName, dialog);

                    return toReturn;
                }

                public GradationVisual CreateDialogOneButton(string assistanceName, string title, string description, string textButton1, EventHandler callbackButton1, Assistances.Buttons.Button.ButtonType type1, Transform parent)
                {
                    MATCH.Assistances.Dialogs.Dialog1 dialog = Assistances.Factory.Instance.CreateDialogOneButton(title, description, textButton1, callbackButton1, type1, parent);

                    Assistances.GradationVisual.GradationVisual toReturn = AddDecoratorsToDialog1(assistanceName, dialog);

                    return toReturn;
                }

                public GradationVisual CreateDialogTwoButtons(string assistanceName, string title, string description, string textButton1, EventHandler callbackButton1, Assistances.Buttons.Button.ButtonType type1, string textButton2, EventHandler callbackButton2, Assistances.Buttons.Button.ButtonType type2, Transform parent)
                {

                    //GradationVisual toReturn = Assistances.Factory.Instance.CreateAssistanceGradationAttention(name);
                    //toReturn.name = assistanceName;

                    MATCH.Assistances.Dialogs.Dialog1 dialog = Assistances.Factory.Instance.CreateDialogTwoButtons(title, description, textButton1, callbackButton1, type1, textButton2, callbackButton2, type2, parent);
                    //toReturn.AddAssistance(Assistances.Decorators.Factory.Instance.CreateBackground(assistanceBase, Utilities.Materials.Colors.CyanGlowing));
                    //toReturn.AddAssistance(Assistances.Decorators.Factory.Instance.CreateBackground(assistanceBase, Utilities.Materials.Colors.OrangeGlowing));

                    Assistances.GradationVisual.GradationVisual toReturn = AddDecoratorsToDialog1(assistanceName, dialog);

                    return toReturn;
                }

                public GradationVisual CreateSurfaceToProcess(string assistanceName, EventHandler callbackNewPartCleaned, EventHandler callbackWhenCleaned, InteractionSurface surfaceToPopulate, Transform parent)
                {
                    Assistances.SurfaceToProcess surface = Assistances.Factory.Instance.CreateSurfaceToProcess(parent, surfaceToPopulate);
                    surface.EventSurfaceCleaned += callbackWhenCleaned;
                    surface.EventNewPartCleaned += callbackNewPartCleaned;
                    //surface.Event

                    Assistances.GradationVisual.GradationVisual toReturn = Assistances.Factory.Instance.CreateAssistanceGradationAttention(assistanceName);
                    toReturn.AddAssistance(surface);

                    //Todo: add visual gradation, which is not the case for now

                    return toReturn;
                }

                //Todo: finalize the implementation of the assistance below
                /*public GradationVisual CreateLightPath(string assistanceName, Transform parent)
                {
                    //Assistances.Pa
                    Assistances.GradationVisual.GradationVisual toReturn = Assistances.Factory.Instance.CreateAssistanceGradationAttention(assistanceName);

                    // Todo: assistance not yet implemented

                    return toReturn;
                }*/

                // Todo: finalize the implementation of the assistance below
                /*public GradationVisual CreateLightPathWithInformation(string assistanceName, Transform parent)
                {
                    //Assistances.Pa
                    Assistances.GradationVisual.GradationVisual toReturn = Assistances.Factory.Instance.CreateAssistanceGradationAttention(assistanceName);

                    // Todo: assistance not yet implemented

                    return toReturn;
                }*/

                public GradationVisual CreateArch(string assistanceName, string assistance, Transform start, Transform end, Transform parent)
                {
                    ArchWithTextAndHelp controller = Assistances.Factory.Instance.CreateAssistanceArch(assistanceName, start, end, parent);
                    controller.SetDescription(assistance);

                    Assistances.GradationVisual.GradationVisual toReturn = Assistances.Factory.Instance.CreateAssistanceGradationAttention(assistanceName);
                    toReturn.AddAssistance(controller);

                    return toReturn;
                }

                /*public GradationVisual CreatePath(string assistanceName, string assistance, Transform start, Transform end, Transform parent)
                {
                    PathWithTextAndHelp controller = Assistances.Factory.Instance.CreateAssistancePath(assistanceName, start, end, parent);
                    controller.SetDescription(assistance);

                    GradationVisual toReturn = Assistances.Factory.Instance.CreateAssistanceGradationAttention(assistanceName);
                    toReturn.AddAssistance(controller);

                    return toReturn;
                }*/

                public GradationVisual CreateAlreadyConfigured(AlreadyConfigured type, string assistanceName, Transform parent)
                {
                    GradationVisual toReturn = null;

                    switch (type)
                    {
                        case AlreadyConfigured.SomeoneComingToHelpDialog1:
                            toReturn = CreateDialogOneButton(assistanceName, "", "Je ne peux continuer à vous aider. Une personne arrive pour vous aider", "Ok", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, parent);
                            break;
                        case AlreadyConfigured.LetGoDialog1:
                            toReturn = CreateDialogOneButton(assistanceName, "", "Parfait! Je vous laisse aller", "Ok", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, parent);
                            break;
                        case AlreadyConfigured.DoYouNeedHelpDialog1:
                            toReturn = CreateDialogTwoButtons(assistanceName, "", "Avez-vous besoin d'aide?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parent);
                            break;
                        case AlreadyConfigured.SomeoneComingToHelpDialog2:
                            toReturn = CreateDialog2WithButtons(assistanceName, "", "Je ne peux continuer à vous aider. Une personne arrive pour vous aider", "Ok", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, parent);
                            break;
                        case AlreadyConfigured.LetGoDialog2:
                            toReturn = CreateDialog2WithButtons(assistanceName, "", "Parfait! Je vous laisse aller", "Ok", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, parent);
                            break;
                        case AlreadyConfigured.DoYouNeedHelpDialog2:
                            toReturn = CreateDialog2WithButtons(assistanceName, "", "Avez-vous besoin d'aide?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parent);
                            break;
                        default:
                            break;
                    }


                    return toReturn;
                }

                public GradationVisual CreateDialog2WithLinePath(string assistanceName, string title, string description, bool adjustToHeight, Transform lineDestination, Transform parent)
                {
                    GradationVisual toReturn = CreateDialog2WithLinePath(assistanceName, title, description, lineDestination, parent);

                    Dialogs.Dialog2 dialog = ((Assistances.PathWithTextAndHelp)((Assistances.IAssistance)toReturn.GetCurrentAssistance()).GetRootDecoratedAssistance()).GetDialog2();
                    dialog.AdjustToHeight = adjustToHeight;

                    return toReturn;
                }

                public GradationVisual CreateDialog2WithLinePath(string assistanceName, string title, string description, Transform lineDestination, Transform parent)
                {
                    GradationVisual toReturn = Assistances.Factory.Instance.CreateAssistanceGradationAttention(assistanceName);
                    toReturn.name = assistanceName;

                    PathWithTextAndHelp assistance = Assistances.Factory.Instance.CreateAssistancePath(assistanceName + "Path", title, description, lineDestination, parent);

                    toReturn.AddAssistance(assistance);

                    return toReturn;
                }

                public GradationVisual CreateDialog2NoButton(string assistanceName, string title, string description, bool adjustToHeight, Transform parent)
                {
                    GradationVisual toReturn = CreateDialog2NoButton(assistanceName, title, description, parent);

                    Dialogs.Dialog2 dialog = ((Dialogs.Dialog2)((Assistances.IAssistance)toReturn.GetCurrentAssistance()).GetRootDecoratedAssistance());
                    dialog.AdjustToHeight = adjustToHeight;

                    return toReturn;
                }

                public GradationVisual CreateDialog2NoButton(string assistanceName, string title, string description, Transform parent)
                {
                    Assistances.Dialogs.Dialog2 dialog = Assistances.Factory.Instance.CreateDialog2NoButton(title, description, parent);

                    return AddDecoratorsToDialog2(assistanceName, dialog);
                }

                public GradationVisual CreateDialog2WithButtons(string assistanceName, string title, string description, bool adjustToHeight, string textButton1, EventHandler callbackButton1, Assistances.Buttons.Button.ButtonType type1, Transform parent)
                {
                    GradationVisual toReturn = CreateDialog2WithButtons(assistanceName, title, description, textButton1, callbackButton1, type1, parent);

                    Dialogs.Dialog2 dialog = ((Dialogs.Dialog2)((Assistances.IAssistance)toReturn.GetCurrentAssistance()).GetRootDecoratedAssistance());
                    dialog.AdjustToHeight = adjustToHeight;

                    return toReturn;
                }

                public GradationVisual CreateDialog2WithButtons(string assistanceName, string title, string description, string textButton1, EventHandler callbackButton1, Assistances.Buttons.Button.ButtonType type1, Transform parent)
                {
                    Assistances.Dialogs.Dialog2 dialog = Assistances.Factory.Instance.CreateDialog2WithButtons(title, description, textButton1, callbackButton1, type1, parent);

                    return AddDecoratorsToDialog2(assistanceName, dialog);
                }

                public GradationVisual CreateDialog2WithButtons(string assistanceName, string title, string description, bool adjustToHeight, string textButton1, EventHandler callbackButton1, Assistances.Buttons.Button.ButtonType type1, string textButton2, EventHandler callbackButton2, Assistances.Buttons.Button.ButtonType type2, Transform parent)
                {
                    GradationVisual toReturn = CreateDialog2WithButtons(assistanceName, title, description, textButton1, callbackButton1, type1, textButton2, callbackButton2, type2, parent);

                    Dialogs.Dialog2 temp = ((Dialogs.Dialog2)((Assistances.IAssistance)toReturn.GetCurrentAssistance()).GetRootDecoratedAssistance());
                    temp.AdjustToHeight = adjustToHeight;

                    return toReturn;
                }

                public GradationVisual CreateDialog2WithButtons(string assistanceName, string title, string description, string textButton1, EventHandler callbackButton1, Assistances.Buttons.Button.ButtonType type1, string textButton2, EventHandler callbackButton2, Assistances.Buttons.Button.ButtonType type2, Transform parent)
                {
                    Assistances.Dialogs.Dialog2 dialog = Assistances.Factory.Instance.CreateDialog2WithButtons(title, description, textButton1, callbackButton1, type1, textButton2, callbackButton2, type2, parent);

                    return AddDecoratorsToDialog2(assistanceName, dialog);
                }

                public GradationVisual CreateDialog2WithButtonsContextualized(string assistanceName, string title, string description, Transform contextObject, string textButton1, EventHandler callbackButton1, Assistances.Buttons.Button.ButtonType type1, string textButton2, EventHandler callbackButton2, Assistances.Buttons.Button.ButtonType type2, Transform parent)
                {
                    Dialogs.Dialog2 dialog = (Dialogs.Dialog2) Assistances.Factory.Instance.CreateDialog2WithButtonsContextualized(assistanceName, title, description, contextObject, textButton1, callbackButton1, type1, textButton2, callbackButton2, type2, parent);

                    return AddDecoratorsToDialog2(assistanceName, dialog);
                }

                public GradationVisual CreateAssistanceDialog(string assistanceName, Assistances.Dialogs.Dialog1 dialog)
                {
                    return AddDecoratorsToDialog1(assistanceName, dialog);
                }

                private GradationVisual AddDecoratorsToDialog1(string assistanceName, MATCH.Assistances.Dialogs.Dialog1 dialog)
                {
                    GradationVisual toReturn = Assistances.Factory.Instance.CreateAssistanceGradationAttention(assistanceName);
                    toReturn.name = assistanceName;

                    toReturn.AddAssistance(Assistances.Decorators.Factory.Instance.CreateBackground(dialog, Utilities.Materials.Colors.CyanGlowing));
                    toReturn.AddAssistance(Assistances.Decorators.Factory.Instance.CreateEdge(dialog, Utilities.Materials.Colors.OrangeGlowing));
                    toReturn.AddAssistance(Assistances.Decorators.Factory.Instance.CreateBackground(dialog, Utilities.Materials.Colors.OrangeGlowing)); //Todo: won't work for now, because only one level of visual gradation is supported

                    return toReturn;
                }

                private GradationVisual AddDecoratorsToDialog2(string assistanceName, MATCH.Assistances.Dialogs.Dialog2 dialog)
                {
                    GradationVisual toReturn = Assistances.Factory.Instance.CreateAssistanceGradationAttention(assistanceName);
                    toReturn.name = assistanceName;

                    //Premier niveau de gradation avec 2 décorateurs
                    Decorators.BackgroundColorIcon2 decorator1a = (Decorators.BackgroundColorIcon2)Assistances.Decorators.Factory.Instance.CreateBackgroundIcon(dialog, Utilities.Materials.Colors.CyanAdjustHSV,true);
                    Decorators.BackgroundColorMessage2 decorator1 = (Decorators.BackgroundColorMessage2)Assistances.Decorators.Factory.Instance.CreateBackgroundMessage(decorator1a, Utilities.Materials.Colors.CyanAdjustHSV);
                    toReturn.AddAssistance(decorator1);

                    //Deuxieme niveau de gradation
                    Decorators.BackgroundColorIcon2 decorator2 = (Decorators.BackgroundColorIcon2)Assistances.Decorators.Factory.Instance.CreateBackgroundIcon(decorator1, Utilities.Materials.Colors.OrangeAdjustHSV,true);
                    toReturn.AddAssistance(decorator2);

                    //Troisieme niveau de gradation
                    //toReturn.AddAssistance(Assistances.Decorators.Factory.Instance.CreateEdge(dialog, Utilities.Materials.Colors.OrangeGlowing));
                    Decorators.BackgroundColorMessage2 decorator3 = (Decorators.BackgroundColorMessage2)Assistances.Decorators.Factory.Instance.CreateBackgroundMessage(decorator2, Utilities.Materials.Colors.OrangeAdjustHSV);
                    toReturn.AddAssistance(decorator3);

                    //Qualitrème niveau de gradation
                    Decorators.LinePath decorator4 = (Decorators.LinePath)Decorators.Factory.Instance.CreateLinePathWithTexture(decorator3, /*Utilities.Materials.Colors.Orange*/ /*Utilities.Materials.Textures.ArrowProgressive*/ Utilities.Materials.Colors.CyanAdjustHSV, /*0.1f*/ 0.039f, true);
                    toReturn.AddAssistance(decorator4);

                    return toReturn;
                }

                public GradationVisual CreateDialog2SandBoxDecorators(string assistanceName, string title, string description, string textButton1, EventHandler callbackButton1, Assistances.Buttons.Button.ButtonType type1, Transform parent)
                {
                    Assistances.Dialogs.Dialog2 dialog = Assistances.Factory.Instance.CreateDialog2WithButtons(title, description, textButton1, callbackButton1, type1, parent);

                    return AddDecoratorsToDialog2SandBox(assistanceName, dialog);
                }

                private GradationVisual AddDecoratorsToDialog2SandBox(string assistanceName, MATCH.Assistances.Dialogs.Dialog2 dialog)
                {
                    GradationVisual toReturn = Assistances.Factory.Instance.CreateAssistanceGradationAttention(assistanceName);
                    toReturn.name = assistanceName;

                    Decorators.BackgroundColorIcon2 decorator1a = (Decorators.BackgroundColorIcon2)Assistances.Decorators.Factory.Instance.CreateBackgroundIcon(dialog, Utilities.Materials.Colors.Cyan, true);
                    Decorators.Icon decoratorIcon1 = (Decorators.Icon)Assistances.Decorators.Factory.Instance.CreateIcon(decorator1a, Utilities.Materials.Icon.ExclamationMark, Utilities.Materials.Colors.WhiteMetallic);
                    IPanel2 adapterIcon1 = Decorators.Adapters.Factory.Instance.CreateAdapterForIPanel2(decoratorIcon1, (IPanel2)decoratorIcon1.GetDecoratedAssistance());
                    Decorators.BackgroundColorMessage2 decorator1 = (Decorators.BackgroundColorMessage2)Assistances.Decorators.Factory.Instance.CreateBackgroundMessage(adapterIcon1, Utilities.Materials.Colors.Cyan);
                    toReturn.AddAssistance(decorator1);

                    //Decorators.Icon decoratorIcon2 = (Decorators.Icon)Assistances.Decorators.Factory.Instance.CreateIcon(decorator1, MATCH.Utilities.Materials.Icon.Heart, Utilities.Materials.Colors.WhiteMetallic);
                    //IPanel2 adapterIcon2 = Decorators.Adapters.Factory.Instance.CreateAdapterForIPanel2(decoratorIcon2, (IPanel2)decoratorIcon1.GetDecoratedAssistance());
                    //Decorators.Sound decoratorSound2 = (Decorators.Sound)Assistances.Decorators.Factory.Instance.CreateSound(adapterIcon2, MATCH.Utilities.Materials.Sounds.Debug, 3.0f);
                    //IPanel2 adapterSound2 = Decorators.Adapters.Factory.Instance.CreateAdapterForIPanel2(decoratorSound2, (IPanel2)decoratorSound2.GetDecoratedAssistance());
                    //Decorators.BackgroundColorIcon2 decorator2 = (Decorators.BackgroundColorIcon2)Assistances.Decorators.Factory.Instance.CreateBackgroundIcon(adapterSound2, Utilities.Materials.Colors.Orange, true);
                    //toReturn.AddAssistance(decorator2);

                    /*Decorators.BackgroundColorIcon2 decorator2 = (Decorators.BackgroundColorIcon2)Assistances.Decorators.Factory.Instance.CreateBackgroundIcon(decorator1, Utilities.Materials.Colors.Orange, true);
                    toReturn.AddAssistance(decorator2);

                    Decorators.BackgroundColorMessage2 decorator3 = (Decorators.BackgroundColorMessage2)Decorators.Factory.Instance.CreateBackgroundMessage(decorator2, Utilities.Materials.Colors.Orange);
                    toReturn.AddAssistance(decorator3);

                    Decorators.LinePath decorator4 = (Decorators.LinePath)Decorators.Factory.Instance.CreateLinePath(decorator3, Utilities.Materials.Colors.Cyan, false);
                    toReturn.AddAssistance(decorator4);*/

                    Decorators.LinePath decorator5 = (Decorators.LinePath)Decorators.Factory.Instance.CreateLinePathWithTexture(decorator1, /*Utilities.Materials.Colors.Orange*/ Utilities.Materials.Textures.ArrowProgressive, 0.1f, true);
                    toReturn.AddAssistance(decorator5);

                    //Premier niveau de gradation avec 2 décorateurs
                    /*Decorators.BackgroundColorIcon2 decorator1a = (Decorators.BackgroundColorIcon2)Assistances.Decorators.Factory.Instance.CreateBackgroundIcon(dialog, Utilities.Materials.Colors.Cyan, false);
                    Decorators.Icon decoratorIcon1 = (Decorators.Icon)Assistances.Decorators.Factory.Instance.CreateIcon(decorator1a, null, null);
                    IPanel2 adapterIcon1 = Decorators.Adapters.Factory.Instance.CreateAdapterForIPanel2(decoratorIcon1, (IPanel2)decoratorIcon1.GetDecoratedAssistance());
                    Decorators.BackgroundColorMessage2 decorator1 = (Decorators.BackgroundColorMessage2)Assistances.Decorators.Factory.Instance.CreateBackgroundMessage(adapterIcon1, Utilities.Materials.Colors.Cyan);
                    toReturn.AddAssistance(decorator1);

                    Decorators.Icon decoratorIcon2 = (Decorators.Icon)Assistances.Decorators.Factory.Instance.CreateIcon(decorator1, MATCH.Utilities.Materials.Icon.Heart, Utilities.Materials.Colors.WhiteMetallic);
                    IPanel2 adapterIcon2 = Decorators.Adapters.Factory.Instance.CreateAdapterForIPanel2(decoratorIcon2, (IPanel2)decoratorIcon1.GetDecoratedAssistance());
                    Decorators.Sound decoratorSound2 = (Decorators.Sound)Assistances.Decorators.Factory.Instance.CreateSound(adapterIcon2, MATCH.Utilities.Materials.Sounds.Debug, 3.0f);
                    IPanel2 adapterSound2 = Decorators.Adapters.Factory.Instance.CreateAdapterForIPanel2(decoratorSound2, (IPanel2)decoratorSound2.GetDecoratedAssistance());
                    Decorators.BackgroundColorIcon2 decorator2 = (Decorators.BackgroundColorIcon2)Assistances.Decorators.Factory.Instance.CreateBackgroundIcon(adapterSound2, Utilities.Materials.Colors.Orange, true);
                    toReturn.AddAssistance(decorator2);

                    Decorators.Icon decoratorIcon3 = (Decorators.Icon)Assistances.Decorators.Factory.Instance.CreateIcon(decorator2, MATCH.Utilities.Materials.Icon.Diamond, Utilities.Materials.Colors.Red);
                    IPanel2 adapterIcon3 = Decorators.Adapters.Factory.Instance.CreateAdapterForIPanel2(decoratorIcon3, (IPanel2)decoratorIcon3.GetDecoratedAssistance());
                    Decorators.LinePath decoratorLine3 = (Decorators.LinePath)Assistances.Decorators.Factory.Instance.CreateLinePath(adapterIcon3, Utilities.Materials.Colors.CyanGlowing);
                    toReturn.AddAssistance(decoratorLine3);

                    IPanel2 adapterLine3 = Decorators.Adapters.Factory.Instance.CreateAdapterForIPanel2(decoratorLine3, (IPanel2)decoratorLine3.GetDecoratedAssistance());
                    Decorators.Sound decoratorSound4 = (Decorators.Sound)Assistances.Decorators.Factory.Instance.CreateSound(adapterLine3, MATCH.Utilities.Materials.Sounds.Debug, 1.0f);
                    IPanel2 adapterSound4 = Decorators.Adapters.Factory.Instance.CreateAdapterForIPanel2(decoratorSound4, (IPanel2)decoratorSound4.GetDecoratedAssistance());
                    Decorators.LinePath decoratorLine4 = (Decorators.LinePath)Assistances.Decorators.Factory.Instance.CreateLinePath(adapterSound4, Utilities.Materials.Colors.GreenGlowing);
                    IPanel2 adapterLine4 = Decorators.Adapters.Factory.Instance.CreateAdapterForIPanel2(decoratorLine4, (IPanel2)decoratorLine4.GetDecoratedAssistance());
                    Decorators.Icon decoratorIcon4 = (Decorators.Icon)Assistances.Decorators.Factory.Instance.CreateIcon(adapterLine4, MATCH.Utilities.Materials.Icon.Club, Utilities.Materials.Colors.GreenGlowing);
                    toReturn.AddAssistance(decoratorIcon4);

                    IPanel2 adapterIcon4 = Decorators.Adapters.Factory.Instance.CreateAdapterForIPanel2(decoratorIcon4, (IPanel2)decoratorIcon4.GetDecoratedAssistance());
                    Decorators.Sound decoratorSound5 = (Decorators.Sound)Assistances.Decorators.Factory.Instance.CreateSound(adapterIcon4, null, 1.0f);
                    IPanel2 adapterSound5 = Decorators.Adapters.Factory.Instance.CreateAdapterForIPanel2(decoratorSound5, (IPanel2)decoratorSound5.GetDecoratedAssistance());
                    Decorators.LinePath decoratorLine5 = (Decorators.LinePath)Assistances.Decorators.Factory.Instance.CreateLinePath(adapterSound5, null);
                    IPanel2 adapterLine5 = Decorators.Adapters.Factory.Instance.CreateAdapterForIPanel2(decoratorLine5, (IPanel2)decoratorLine5.GetDecoratedAssistance());
                    Decorators.Arch decoratorArch5 = (Decorators.Arch)Assistances.Decorators.Factory.Instance.CreateArch(adapterLine5, Utilities.Materials.Colors.Cyan);
                    toReturn.AddAssistance(decoratorArch5);

                    IPanel2 adapterArch5 = Decorators.Adapters.Factory.Instance.CreateAdapterForIPanel2(decoratorArch5, (IPanel2)decoratorArch5.GetDecoratedAssistance());
                    Decorators.Arch decoratorArch6 = (Decorators.Arch)Assistances.Decorators.Factory.Instance.CreateArch(adapterArch5, Utilities.Materials.Colors.Red);
                    //IPanel2 adapterArch6 = Decorators.Adapters.Factory.Instance.CreateAdapterForIPanel2(decoratorArch6, (IPanel2)decoratorArch6.GetDecoratedAssistance());
                    //Decorators.BackgroundColorMessage2 decorator6 = (Decorators.BackgroundColorMessage2)Assistances.Decorators.Factory.Instance.CreateBackgroundMessage(adapterArch6, Utilities.Materials.Colors.Orange);
                    toReturn.AddAssistance(decoratorArch6);*/

                    return toReturn;
                }
            }
        }
    }
}