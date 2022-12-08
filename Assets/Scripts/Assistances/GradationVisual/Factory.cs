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
                    SomeoneComingToHelp = 0,
                    LetGo = 1,
                    DoYouNeedHelp = 2
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

                public GradationVisual CreateTypeExclamationMark(string name, Transform parent)
                {
                    GradationVisual assistance = Assistances.Factory.Instance.CreateAssistanceGradationAttention(name);
                    Assistances.Basic assistanceBase = Assistances.Factory.Instance.CreateCube(Utilities.Materials.Colors.PurpleGlowing,parent);
                    assistanceBase.name = name+"_base";
                    assistance.AddAssistance(Assistances.Decorators.Factory.Instance.CreateMaterial(assistanceBase, Utilities.Materials.Textures.Exclamation));
                    assistance.AddAssistance(Assistances.Decorators.Factory.Instance.CreateMaterial(assistanceBase, Utilities.Materials.Textures.ExclamationRed));

                    return assistance;
                }

                public GradationVisual CreateDialogNoButton(string assistanceName, string title, string description, Transform parent)
                {
                    Assistances.Dialog dialog = Assistances.Factory.Instance.CreateDialogNoButton(title, description,  parent);

                    Assistances.GradationVisual.GradationVisual toReturn = AddDecoratorsToDialog(assistanceName, dialog);

                    return toReturn;
                }

                public GradationVisual CreateDialogOneButton(string assistanceName, string title, string description, string textButton1, EventHandler callbackButton1, Assistances.Buttons.Button.ButtonType type1, Transform parent)
                {
                    Assistances.Dialog dialog = Assistances.Factory.Instance.CreateDialogOneButton(title, description, textButton1, callbackButton1, type1, parent);

                    Assistances.GradationVisual.GradationVisual toReturn = AddDecoratorsToDialog(assistanceName, dialog);

                    return toReturn;
                }

                public GradationVisual CreateDialogTwoButtons(string assistanceName, string title, string description, string textButton1, EventHandler callbackButton1, Assistances.Buttons.Button.ButtonType type1, string textButton2, EventHandler callbackButton2, Assistances.Buttons.Button.ButtonType type2, Transform parent)
                {

                    //GradationVisual toReturn = Assistances.Factory.Instance.CreateAssistanceGradationAttention(name);
                    //toReturn.name = assistanceName;

                    Assistances.Dialog dialog = Assistances.Factory.Instance.CreateDialogTwoButtons(title, description, textButton1, callbackButton1, type1, textButton2, callbackButton2, type2, parent);
                    //toReturn.AddAssistance(Assistances.Decorators.Factory.Instance.CreateBackground(assistanceBase, Utilities.Materials.Colors.CyanGlowing));
                    //toReturn.AddAssistance(Assistances.Decorators.Factory.Instance.CreateBackground(assistanceBase, Utilities.Materials.Colors.OrangeGlowing));

                    Assistances.GradationVisual.GradationVisual toReturn = AddDecoratorsToDialog(assistanceName, dialog);

                    return toReturn;
                }

                public GradationVisual CreateSurfaceToProcess(string assistanceName, EventHandler callbackNewPartCleaned, EventHandler callbackWhenCleaned, Transform parent)
                {
                    Assistances.SurfaceToProcess surface = Assistances.Factory.Instance.CreateSurfaceToProcess(parent);
                    surface.EventSurfaceCleaned += callbackWhenCleaned;
                    surface.EventNewPartCleaned += callbackNewPartCleaned;
                    //surface.Event

                    Assistances.GradationVisual.GradationVisual toReturn = Assistances.Factory.Instance.CreateAssistanceGradationAttention(assistanceName);
                    toReturn.AddAssistance(surface);

                    //Todo: add visual gradation, which is not the case for now

                    return toReturn;
                }

                public GradationVisual CreateLightPath(string assistanceName, Transform parent)
                {
                    //Assistances.Pa
                    Assistances.GradationVisual.GradationVisual toReturn = Assistances.Factory.Instance.CreateAssistanceGradationAttention(assistanceName);

                    // Todo: assistance not yet implemented

                    return toReturn;
                }

                public GradationVisual CreateLightPathWithInformation(string assistanceName, Transform parent)
                {
                    //Assistances.Pa
                    Assistances.GradationVisual.GradationVisual toReturn = Assistances.Factory.Instance.CreateAssistanceGradationAttention(assistanceName);

                    // Todo: assistance not yet implemented

                    return toReturn;
                }

                public GradationVisual CreateAlreadyConfigured(AlreadyConfigured type, string assistanceName, Transform parent)
                {
                    GradationVisual toReturn = null;

                    switch (type)
                    {
                        case AlreadyConfigured.SomeoneComingToHelp:
                            toReturn = CreateDialogOneButton(assistanceName, "", "Je ne peux continuer à vous aider. Une personne arrive pour vous aider", "Ok", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, parent);
                            break;
                        case AlreadyConfigured.LetGo:
                            toReturn = CreateDialogOneButton(assistanceName, "", "Parfait! Je vous laisse aller", "Ok", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.ClosingButton, parent);
                            break;
                        case AlreadyConfigured.DoYouNeedHelp:
                            toReturn = CreateDialogTwoButtons(assistanceName, "", "Avez-vous besoin d'aide?", "Oui", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.Yes, "Non", Utilities.Utility.GetEventHandlerEmpty(), Assistances.Buttons.Button.ButtonType.No, parent);
                            break;

                        default:
                            break;
                    }


                    return toReturn;
                }

                private GradationVisual AddDecoratorsToDialog(string assistanceName, Assistances.Dialog dialog)
                {
                    GradationVisual toReturn = Assistances.Factory.Instance.CreateAssistanceGradationAttention(assistanceName);
                    toReturn.name = assistanceName;

                    toReturn.AddAssistance(Assistances.Decorators.Factory.Instance.CreateBackground(dialog, Utilities.Materials.Colors.CyanGlowing));
                    toReturn.AddAssistance(Assistances.Decorators.Factory.Instance.CreateEdge(dialog, Utilities.Materials.Colors.OrangeGlowing));
                    toReturn.AddAssistance(Assistances.Decorators.Factory.Instance.CreateBackground(dialog, Utilities.Materials.Colors.OrangeGlowing)); //Todo: won't work for now, because only one level of visual gradation is supported

                    return toReturn;
                }
            }
        }
    }
}