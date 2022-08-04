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
        public class Dialog : Assistance
        {
            Transform ButtonsParentView;
            Transform RefButtonView;
            Transform TitleView;
            Transform DescriptionView;
            Transform BackgroundView;
            List<Transform> ButtonsView;
            public List<Buttons.Basic> ButtonsController;

            Vector3 ButtonsParentScalingOriginal;
            Vector3 BackgoundScalingOriginal;
            Vector3 TitleScalingOriginal;
            Vector3 DescriptionScalingOriginal;
            List<Vector3> ButtonsScalingOriginal;

            public bool AdjustToHeight { get; set; } = true;

            private void Awake()
            {
                // Instantiate variables
                ButtonsView = new List<Transform>();
                ButtonsController = new List<Buttons.Basic>();
                ButtonsScalingOriginal = new List<Vector3>();
                //m_adjustToHeight = true;

                // Children
                ButtonsParentView = transform.Find("ButtonParent");
                RefButtonView = ButtonsParentView.Find("Button");
                TitleView = transform.Find("TitleText");
                DescriptionView = gameObject.transform.Find("DescriptionText");
                BackgroundView = gameObject.transform.Find("ContentBackPlate");

                // Initialize some values of the children
                ButtonsParentScalingOriginal = ButtonsParentView.localScale;
                BackgoundScalingOriginal = BackgroundView.localScale;
                TitleScalingOriginal = TitleView.localScale;
                DescriptionScalingOriginal = DescriptionView.localScale;
            }

            public void SetTitle(string text, float fontSize = -1.0f)
            {
                TextMeshPro tmp = TitleView.GetComponent<TextMeshPro>();

                SetTextToTextMeshProComponent(tmp, text, fontSize);
            }

            public void SetDescription(string text, float fontSize = -1.0f)
            {
                TextMeshPro tmp = DescriptionView.GetComponent<TextMeshPro>();

                SetTextToTextMeshProComponent(tmp, text, fontSize);
            }

            void SetTextToTextMeshProComponent(TextMeshPro component, string text, float fontSize)
            {
                if (fontSize > 0.0f)
                {
                    component.fontSize = fontSize;
                }

                component.SetText(text);
            }

            /**
             * If fontSize < 0.0f, means keep the default value of the button's size. Hence the default value.
             * */
            public Buttons.Basic AddButton(string text/*, EventHandler eventHandler*/, bool autoScaling, float fontSize = -1.0f)
            {
                // Instantiate the button
                Transform newButton = Instantiate(RefButtonView, ButtonsParentView);
                newButton.name = text;
                ButtonConfigHelper configHelper = newButton.GetComponent<ButtonConfigHelper>();
                configHelper.MainLabelText = text;
                TextMeshPro tmp = newButton.Find("IconAndText").Find("TextMeshPro").GetComponent<TextMeshPro>();

                // Get the text mesh pro component to set the fontsize
                if (fontSize > 0.0f)
                {
                    tmp.fontSize = fontSize;
                }

                // Store the button
                ButtonsView.Add(newButton);
                Buttons.Basic tempButtonController = newButton.GetComponent<Buttons.Basic>();
                ButtonsController.Add(tempButtonController); // Only for the ease of use, nothing special here.

                // Locate button
                float scalingx = 1.0f;
                if (autoScaling)
                {
                    scalingx = 1.0f / (float)(ButtonsView.Count());
                    tmp.margin = new Vector4(tmp.margin.x * scalingx, tmp.margin.y, tmp.margin.z * scalingx, tmp.margin.w);
                }


                foreach (Transform b in ButtonsView)
                {
                    b.localScale = new Vector3(scalingx, b.localScale.y, b.localScale.z);
                    Transform textButton = b.Find("IconAndText");
                    textButton.localScale = new Vector3(1.0f / scalingx, textButton.localScale.y, textButton.localScale.z);
                }

                // Store button scaling
                ButtonsScalingOriginal.Add(ButtonsView.Last().localScale);

                // Enable button
                ButtonsView.Last().gameObject.SetActive(true);

                ButtonsParentView.GetComponent<GridObjectCollection>().UpdateCollection();

                return tempButtonController;
            }

            bool m_mutexHide = false;
            public override void Hide(EventHandler eventHandler)
            {
                if (m_mutexHide == false)
                {
                    m_mutexHide = true;

                    MATCH.Utilities.Utility.AnimateDisappearInPlace(TitleView.gameObject, TitleScalingOriginal, delegate
                    {
                        m_mutexHide = false;
                        eventHandler?.Invoke(this, EventArgs.Empty);
                    });

                    Utilities.Utility.AnimateDisappearInPlace(DescriptionView.gameObject, DescriptionScalingOriginal);

                    Utilities.Utility.AnimateDisappearInPlace(ButtonsParentView.gameObject, ButtonsParentScalingOriginal);

                    Utilities.Utility.AnimateDisappearInPlace(BackgroundView.gameObject, BackgoundScalingOriginal);
                }
            }

            bool m_mutexShow = false;
            public override void Show(EventHandler eventHandler)
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called for object " + name);

                if (m_mutexShow == false)
                {
                    m_mutexShow = true;

                    if (AdjustToHeight)
                    {
                        Utilities.Utility.AdjustObjectHeightToHeadHeight(transform);
                    }

                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Going to show the background");

                    Utilities.Utility.AnimateAppearInPlace(BackgroundView.gameObject, BackgoundScalingOriginal, delegate {
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Background shown");

                        Utilities.Utility.AnimateAppearInPlace(TitleView.gameObject);
                        Utilities.Utility.AnimateAppearInPlace(ButtonsParentView.gameObject);
                        Utilities.Utility.AnimateAppearInPlace(DescriptionView.gameObject);

                        m_mutexShow = false;
                        eventHandler?.Invoke(this, EventArgs.Empty);

                    });
                }
                else
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Mutex locked, nothing will happen");
                }
            }

            public override void ShowHelp(bool show)
            {
                // Todo
            }

            public override Transform GetTransform()
            {
                return transform;
            }

            /**
             * This function changes the color - you still have the responsibility to disable the callback if required
             * */
            /*public void CheckButton(Buttons.Basic button, bool check)
            {
                //m_states[currentState.m_currentState.getId()].transform.Find("BackPlate").Find("Quad").GetComponent<Renderer>().material = Resources.Load("Mouse_Cyan_Glowing", typeof(Material)) as Material;
                button.CheckButton(check);
            }*/

            public void EnableBillboard(bool enable)
            {
                gameObject.GetComponent<Billboard>().enabled = enable;
            }

            public override bool IsDecorator()
            {
                return false;
            }
        }

    }
}


