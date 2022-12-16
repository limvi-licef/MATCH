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
        public class Dialog : Assistance, IPanel
        {
            Transform ButtonsParentView;
            Transform RefButtonView;
            Transform TitleView;
            Transform DescriptionView;
            Transform BackgroundView;
            List<Transform> ButtonsView;
            public List<Buttons.Basic> ButtonsController;

            Vector3 ButtonsParentScalingOriginal;
            Vector3 BackgroundScalingOriginal;
            Vector3 TitleScalingOriginal;
            Vector3 DescriptionScalingOriginal;
            List<Vector3> ButtonsScalingOriginal;

            public bool AdjustToHeight { get; set; } = true;

            Vector3 BoxColliderOriginalCenter;
            Vector3 BoxColliderOriginalSize;

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
                BackgroundScalingOriginal = BackgroundView.localScale;
                TitleScalingOriginal = TitleView.localScale;
                DescriptionScalingOriginal = DescriptionView.localScale;

                // Storing the original center and size of the box collider
                BoxCollider box = transform.GetComponent<BoxCollider>();
                BoxColliderOriginalCenter = box.center;
                BoxColliderOriginalSize = box.size;
            }

            public void SetTitle(string text, float fontSize = -1.0f)
            {
                TextMeshPro tmp = TitleView.GetComponent<TextMeshPro>();

                SetTextToTextMeshProComponent(tmp, text, fontSize);
            }

            public void SetDescription(string text, float fontSize = -1.0f)
            {
                TextMeshPro tmp = DescriptionView.GetComponent<TextMeshPro>();

                if (text != tmp.text)
                { // To avoid to update if the text did not change
                    SetTextToTextMeshProComponent(tmp, text, fontSize);
                }
            }

            public string GetDescription()
            {
                TextMeshPro tmp = DescriptionView.GetComponent<TextMeshPro>();

                return tmp.text;
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
                    scalingx = 1.0f / (float)(ButtonsView.Count());
                    tmp.margin = new Vector4(tmp.margin.x * scalingx, tmp.margin.y, tmp.margin.z * scalingx, tmp.margin.w);
                }


                foreach (Transform b in ButtonsView)
                {
                    b.localScale = new Vector3(scalingx, b.localScale.y, b.localScale.z);
                    Transform textButton = b.Find("IconAndText");
                    textButton.localScale = new Vector3(1.0f / scalingx, textButton.localScale.y, textButton.localScale.z);

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

            public override void Hide(EventHandler eventHandler, bool withAnimation)
            {
                Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();

                if (IsDisplayed)
                {
                    gameObject.GetComponent<BoxCollider>().enabled = false;

                    if (withAnimation)
                    {
                        MATCH.Utilities.Utility.AnimateDisappearInPlace(TitleView.gameObject, TitleScalingOriginal, delegate
                        {
                            IsDisplayed = false;
                            args.Success = true;
                            eventHandler?.Invoke(this, args);
                        });

                        Utilities.Utility.AnimateDisappearInPlace(DescriptionView.gameObject, DescriptionScalingOriginal);

                        Utilities.Utility.AnimateDisappearInPlace(ButtonsParentView.gameObject, ButtonsParentScalingOriginal);

                        Utilities.Utility.AnimateDisappearInPlace(BackgroundView.gameObject, BackgroundScalingOriginal);
                    }
                    else
                    {
                        TitleView.gameObject.SetActive(false);
                        DescriptionView.gameObject.SetActive(false);
                        ButtonsParentView.gameObject.SetActive(false);
                        BackgroundView.gameObject.SetActive(false);
                        IsDisplayed = false;
                        args.Success = true;
                        eventHandler?.Invoke(this, args);
                    }
                }
                else
                {
                    args.Success = false;
                    eventHandler?.Invoke(this, args);
                }
            }

            public override void Show(EventHandler eventHandler, bool withAnimation)
            {
                Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();

                if (IsDisplayed == false)
                {
                    gameObject.GetComponent<BoxCollider>().enabled = true;

                    if (AdjustToHeight)
                    {
                        Utilities.Utility.AdjustObjectHeightToHeadHeight(transform);
                    }

                    if (withAnimation)
                    {
                        Utilities.Utility.AnimateAppearInPlace(BackgroundView.gameObject, BackgroundScalingOriginal, delegate {

                            Utilities.Utility.AnimateAppearInPlace(TitleView.gameObject);
                            Utilities.Utility.AnimateAppearInPlace(DescriptionView.gameObject);

                            IsDisplayed = true;

                            args.Success = true;
                            eventHandler?.Invoke(this, args);

                        });
                    }
                    else
                    {
                        BackgroundView.gameObject.SetActive(true);
                        BackgroundView.transform.localScale = BackgroundScalingOriginal;

                        TitleView.gameObject.SetActive(true);
                        DescriptionView.gameObject.SetActive(true);
                        IsDisplayed = true;

                        args.Success = true;
                        eventHandler?.Invoke(this, args);
                    }

                    
                }
                else
                {
                    args.Success = false;
                    eventHandler?.Invoke(this, args);
                }   
            }

            /**
             * Todo: withAnimation ignored for now. Never uses animations to display help here
             */
            public override void ShowHelp(bool show, EventHandler callback, bool withAnimation)
            {
                ButtonsParentView.gameObject.SetActive(show);

                Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();
                args.Success = true; //Todo: Always a success ... at least until I find a situation where it is not ok ... so double check this?
                callback?.Invoke(this, args);

                BoxCollider box = transform.GetComponent<BoxCollider>();

                if (show)
                { // Modify the box cillider size and position so that the buttons can still be clicked
                    GridObjectCollection buttonsLayout = ButtonsParentView.GetComponent<GridObjectCollection>();
                    box.center = new Vector3(BackgroundView.localPosition.x, BackgroundView.localPosition.y + buttonsLayout.CellHeight, BackgroundView.localPosition.z);
                    box.size = new Vector3(box.size.x, BackgroundView.localScale.y - buttonsLayout.CellHeight, box.size.z);
                }
                else
                { // Set the box collider to it original position and size
                    box.size = BoxColliderOriginalSize;
                    box.center = BoxColliderOriginalCenter;
                }
            }

            public override Transform GetTransform()
            {
                return transform;
                //return DescriptionView;
            }

            public void EnableBillboard(bool enable, float offsetRotation=0.0f)
            {
                gameObject.GetComponent<Billboard>().enabled = false;

                gameObject.transform.rotation = new Quaternion(0, 0, 0, 0);

                Vector3 newPos = TitleView.localPosition;
                newPos.z += offsetRotation;
                TitleView.localPosition = newPos;

                newPos = DescriptionView.localPosition;
                newPos.z += offsetRotation;
                DescriptionView.localPosition = newPos;

                newPos = ButtonsParentView.localPosition;
                newPos.z += offsetRotation;
                ButtonsParentView.localPosition = newPos;

                newPos = BackgroundView.localPosition;
                newPos.z += offsetRotation;
                BackgroundView.localPosition = newPos;

                gameObject.GetComponent<Billboard>().enabled = enable;
            }

            public override bool IsDecorator()
            {
                return false;
            }

            /*public void SetBackgroundColor(string colorName)
            {
                BackgroundView.GetComponent<Renderer>().material = Resources.Load(colorName, typeof(Material)) as Material;
            }*/

            /*public void SetEdgeColor(string colorName)
            {
                throw new NotImplementedException();
            }*/

            /*public void SetEdgeThickness(float thickness)
            {
                throw new NotImplementedException();
            }*/

            public void EnableWeavingHand(bool enable)
            {
                throw new NotImplementedException();
            }

            Assistance IAssistance.GetAssistance()
            {
                return this;
            }

            public Transform GetBackground()
            {
                return BackgroundView;
            }
        }

    }
}


