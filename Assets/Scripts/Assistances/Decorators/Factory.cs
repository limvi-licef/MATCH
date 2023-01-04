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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;


namespace MATCH
{
    namespace Assistances
    {
        namespace Decorators
        {
            public class Factory : MonoBehaviour
            {
                private static Factory InstanceInternal;
                public static Factory Instance { get { return InstanceInternal; } }

                public MATCH.Assistances.Decorators.Material RefMaterial;
                //public MATCH.Assistances.Decorators.BackgroundColor RefBackgroundColor;

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

                public MATCH.Assistances.Assistance CreateBackground (MATCH.Assistances.IPanel panelToDecorate, string backgroundColor)
                {
                    Transform view = Instantiate(Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.DecoratorBackgroundColor));
                    view.name = ((Assistance)panelToDecorate).name;

                    Assistances.Decorators.BackgroundColor controller = view.gameObject.GetComponent<Assistances.Decorators.BackgroundColor>();

                    controller.SetAssistanceToDecorate(panelToDecorate);
                    //controller.SetBackgroundColor(background);
                    controller.GetBackground().GetComponent<Renderer>().material = Utilities.Utility.LoadMaterial(backgroundColor);

                    return controller;
                }

                public MATCH.Assistances.Assistance CreateBackground(MATCH.Assistances.IPanel2 panelToDecorate, string backgroundColor, BackgroundColorFor2 toHideOnShow=null)
                {

                    return CreateBackground(panelToDecorate, backgroundColor, backgroundColor, toHideOnShow);
                }

                    public MATCH.Assistances.Assistance CreateBackground(MATCH.Assistances.IPanel2 panelToDecorate, string backgroundMessageColor, string backgroundIconColor, BackgroundColorFor2 toHideOnShow=null)
                {
                    Transform view = Instantiate(Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.DecoratorBackgroundColor2));
                    view.name = ((Assistance)panelToDecorate).name;

                    Assistances.Decorators.BackgroundColorFor2 controller = view.gameObject.GetComponent<Assistances.Decorators.BackgroundColorFor2>();

                    controller.SetAssistanceToDecorate(panelToDecorate);
                    //controller.SetBackgroundColor(background);
                    controller.GetBackgroundMessage().GetComponent<Renderer>().material = Utilities.Utility.LoadMaterial(backgroundMessageColor);
                    controller.GetBackgroundIcon().GetComponent<Renderer>().material = Utilities.Utility.LoadMaterial(backgroundIconColor);

                    if (toHideOnShow != null)
                    {
                        controller.AddToHideOnShow(toHideOnShow);
                    }

                    return controller;
                }

                public MATCH.Assistances.Assistance CreateMaterial(MATCH.Assistances.IBasic toDecorate, string material)
                {
                    Transform view = Instantiate(RefMaterial.transform);
                    view.name = ((Assistance)toDecorate).name;

                    MATCH.Assistances.Decorators.Material controller = view.gameObject.GetComponent<MATCH.Assistances.Decorators.Material>();
                    controller.SetAssistanceToDecorate(toDecorate);
                    controller.SetMaterial(material);

                    return controller;
                }

                public MATCH.Assistances.Assistance CreateEdge(MATCH.Assistances.IPanel toDecorate, string edgeMaterial)
                {
                    Transform view = Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.DecoratorEdge);
                    Assistances.Decorators.Edge controller = view.GetComponent<Assistances.Decorators.Edge>();

                    controller.SetAssistanceToDecorate((Assistances.IPanel)toDecorate);
                    controller.SetEdgeColor(edgeMaterial);

                    //controller.Show(Utilities.Utility.GetEventHandlerEmpty());

                    return controller;
                }
            }
        }
    }
}
