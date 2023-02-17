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
                    Assistances.ExclamationMark assistanceBase = Assistances.Factory.Instance.CreateExclamationMark(true, new Vector3(0,0,0), true, parent);
                    assistanceBase.name = name + "_base";
                    assistance.AddAssistance(Assistances.Decorators.Factory.Instance.CreateMaterial(assistanceBase, Utilities.Materials.Colors.WhiteMetallic));
                    assistance.AddAssistance(Assistances.Decorators.Factory.Instance.CreateMaterial(assistanceBase, Utilities.Materials.Colors.YellowGlowing));
                    assistance.AddAssistance(Assistances.Decorators.Factory.Instance.CreateMaterial(assistanceBase, Utilities.Materials.Colors.RedGlowing));

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

                public GradationVisual CreateDialog2NoButton(string assistanceName, string title, string description, Transform parent)
                {
                    Assistances.Dialogs.Dialog2 dialog = Assistances.Factory.Instance.CreateDialog2NoButton(title, description, parent);

                    return AddDecoratorsToDialog2(assistanceName, dialog);
                }

                public GradationVisual CreateDialog2WithButtons(string assistanceName, string title, string description, string textButton1, EventHandler callbackButton1, Assistances.Buttons.Button.ButtonType type1, Transform parent)
                {
                    Assistances.Dialogs.Dialog2 dialog = Assistances.Factory.Instance.CreateDialog2WithButtons(title, description, textButton1, callbackButton1, type1, parent);

                    return AddDecoratorsToDialog2(assistanceName, dialog);
                }

                public GradationVisual CreateDialog2WithButtons(string assistanceName, string title, string description, string textButton1, EventHandler callbackButton1, Assistances.Buttons.Button.ButtonType type1, string textButton2, EventHandler callbackButton2, Assistances.Buttons.Button.ButtonType type2, Transform parent)
                {
                    Assistances.Dialogs.Dialog2 dialog = Assistances.Factory.Instance.CreateDialog2WithButtons(title, description, textButton1, callbackButton1, type1, textButton2, callbackButton2, type2, parent);

                    return AddDecoratorsToDialog2(assistanceName, dialog);
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

                    

                    Decorators.BackgroundColorFor2 decorator1 = (Decorators.BackgroundColorFor2)Assistances.Decorators.Factory.Instance.CreateBackground(dialog, Utilities.Materials.Colors.Cyan);
                    toReturn.AddAssistance(decorator1);

                    Decorators.BackgroundColorFor2 decorator2 = (Decorators.BackgroundColorFor2)Assistances.Decorators.Factory.Instance.CreateBackground(dialog, Utilities.Materials.Colors.Cyan, Utilities.Materials.Colors.Orange, decorator1);
                    toReturn.AddAssistance(decorator2);

                    //toReturn.AddAssistance(Assistances.Decorators.Factory.Instance.CreateEdge(dialog, Utilities.Materials.Colors.OrangeGlowing));
                    Decorators.BackgroundColorFor2 decorator3 = (Decorators.BackgroundColorFor2)Assistances.Decorators.Factory.Instance.CreateBackground(dialog, Utilities.Materials.Colors.Orange, decorator2);
                    toReturn.AddAssistance(decorator3);



                    return toReturn;
                }
            }
        }
    }
}