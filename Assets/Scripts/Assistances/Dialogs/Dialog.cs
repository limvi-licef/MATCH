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

using System.Collections.Generic;
using UnityEngine;
using System.Timers;
using System.Reflection;
using TMPro;
using System.Linq;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System;

namespace MATCH
{
    namespace Assistances
    {
        namespace Dialogs
        {
            public abstract class Dialog : Assistance
            {
                protected Transform ButtonsParentView;
                Transform RefButtonView;
                List<Transform> ButtonsView;
                public List<Buttons.Basic> ButtonsController;
                protected Vector3 ButtonsParentScalingOriginal;
                List<Vector3> ButtonsScalingOriginal;

                protected virtual void Awake()
                {
                    // Instantiate variables
                    ButtonsView = new List<Transform>();
                    ButtonsController = new List<Buttons.Basic>();
                    ButtonsScalingOriginal = new List<Vector3>();

                    // Children
                    ButtonsParentView = transform.Find("ButtonParent");
                    RefButtonView = ButtonsParentView.Find("Button");

                    // Initialize some values of the children
                    ButtonsParentScalingOriginal = ButtonsParentView.localScale;
                }

                /**
                 * If fontSize < 0.0f, means keep the default value of the button's size. Hence the default value.
                 * */
                public Buttons.Basic AddButton(string text/*, EventHandler eventHandler*/, bool autoScaling, float fontSize = -1.0f)
                {
                    // Instantiate the button
                    Transform view = Instantiate(RefButtonView, ButtonsParentView);
                    view.name = text;
                    ButtonConfigHelper configHelper = view.GetComponent<ButtonConfigHelper>();
                    configHelper.MainLabelText = text;
                    TextMeshPro tmp = view.Find("IconAndText").Find("TextMeshPro").GetComponent<TextMeshPro>();

                    // Get the text mesh pro component to set the fontsize
                    if (fontSize > 0.0f)
                    {
                        tmp.fontSize = fontSize;
                    }

                    // Store the button
                    ButtonsView.Add(view);
                    Buttons.Basic controller = view.GetComponent<Buttons.Basic>();
                    ButtonsController.Add(controller); // Only for the ease of use, nothing special here.

                    // Locate button
                    float scalingx = 1.0f;
                    if (autoScaling)
                    {
                        //scalingx = 1.0f / (float)(ButtonsView.Count());
                        scalingx = (1.0f- (0.1f*((float)(ButtonsView.Count()-1.0f)))) / (float)(ButtonsView.Count()); // New formula to have a small space between button
                        tmp.margin = new Vector4(tmp.margin.x * scalingx, tmp.margin.y, tmp.margin.z * scalingx, tmp.margin.w);
                    }


                    foreach (Transform b in ButtonsView)
                    {
                        b.localScale = new Vector3(scalingx, b.localScale.y, b.localScale.z);
                        Transform textButton = b.Find("IconAndText");
                        textButton.localScale = new Vector3(1.0f / scalingx, textButton.localScale.y, textButton.localScale.z);
                        Transform tmp2 = textButton.Find("TextMeshPro");
                        RectTransform rectTransform = tmp2.GetComponent<RectTransform>();
                        //rectTransform.rect.Set(rectTransform.rect.x, rectTransform.rect.y, rectTransform.rect.width * scalingx, rectTransform.rect.height);
                        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectTransform.rect.width * scalingx);


                        // Update the boxcollider
                        BoxCollider collider = b.GetComponent<BoxCollider>();
                        collider.size = new Vector3(0.18f, collider.size.y, 0.05f);
                    }

                    // Store button scaling
                    ButtonsScalingOriginal.Add(ButtonsView.Last().localScale);

                    // Enable button
                    ButtonsView.Last().gameObject.SetActive(true);

                    ButtonsParentView.GetComponent<GridObjectCollection>().UpdateCollection();

                    // Calling the assistancecallback
                    controller.EventButtonClicked += CButtonHelp;

                    // Return
                    return controller;
                }
            }
        }
    }
}