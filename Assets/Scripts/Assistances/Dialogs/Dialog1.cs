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


            public class Dialog1 : Dialog, IPanel
            {
                Transform TitleView;
                Transform DescriptionView;
                Transform BackgroundView;

                Vector3 BackgroundScalingOriginal;
                Vector3 TitleScalingOriginal;
                Vector3 DescriptionScalingOriginal;
                protected Transform LinePath;

                public bool AdjustToHeight { get; set; } = true;

                Vector3 BoxColliderOriginalCenter;
                Vector3 BoxColliderOriginalSize;

                protected override void Awake()
                {
                    base.Awake();

                    // Children
                    TitleView = transform.Find("TitleText");
                    DescriptionView = gameObject.transform.Find("DescriptionText");
                    BackgroundView = gameObject.transform.Find("ContentBackPlate");

                    // Initialize some values of the children
                    BackgroundScalingOriginal = BackgroundView.localScale;
                    TitleScalingOriginal = TitleView.localScale;
                    DescriptionScalingOriginal = DescriptionView.localScale;

                    // Storing the original center and size of the box collider
                    BoxCollider box = transform.GetComponent<BoxCollider>();
                    BoxColliderOriginalCenter = box.center;
                    BoxColliderOriginalSize = box.size;

                    LinePath = gameObject.transform.Find("LinePath");
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
                                OnIsHidden(this, args);
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
                            OnIsHidden(this, args);
                        }
                    }
                    else
                    {
                        args.Success = false;
                        eventHandler?.Invoke(this, args);
                        OnIsHidden(this, args);
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
                            Utilities.Utility.AnimateAppearInPlace(BackgroundView.gameObject, BackgroundScalingOriginal, delegate
                            {

                                Utilities.Utility.AnimateAppearInPlace(TitleView.gameObject);
                                Utilities.Utility.AnimateAppearInPlace(DescriptionView.gameObject);

                                IsDisplayed = true;

                                args.Success = true;
                                eventHandler?.Invoke(this, args);
                                OnIsShown(this, args);
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
                            OnIsShown(this, args);
                        }


                    }
                    else
                    {
                        args.Success = false;
                        eventHandler?.Invoke(this, args);
                        OnIsShown(this, args);
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
                    { // Modify the box collider size and position so that the buttons can still be clicked
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

                public void EnableBillboard(bool enable, float offsetRotation = 0.0f)
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

                public Assistance GetAssistance()
                {
                    return this;
                }

                public Assistance GetRootDecoratedAssistance()
                {
                    return this;
                }

                public Assistance GetDecoratedAssistance()
                {
                    return this;
                }

                public Transform GetBackground()
                {
                    return BackgroundView;
                }

                public Transform GetSound()
                {
                    return null;
                }

                public Transform GetArch()
                {
                    return null;
                }

                public Assistances.Icon GetIcon()
                {
                    return null;
                }

                public Transform GetLinePath()
                {
                    return LinePath;
                }

                public override void Emphasize(bool enable)
                {
                    if (enable)
                    {
                        Utilities.Emphasize emphasize = gameObject.AddComponent<Utilities.Emphasize>();

                        emphasize.AddMaterial(BackgroundView);
                        emphasize.EnableEmphasize(true);

                    }
                    else
                    {
                        Utilities.Emphasize emphasize = null;

                        if (gameObject.TryGetComponent<Utilities.Emphasize>(out emphasize))
                        {
                            emphasize.EnableEmphasize(false);

                            Destroy(emphasize);
                        }
                    }
                }
            }
        }
    }
}