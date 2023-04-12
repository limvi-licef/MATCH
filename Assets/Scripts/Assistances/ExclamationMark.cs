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
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Reflection;
using System;

/**
 * This class manages a basic show / hide for a child that should be named ... "child" (I am a creative person)
 * An event is emitted if the nested hologram is touched.
 * Appears / disappears in place. I told you it was basic.
 * */

namespace MATCH
{
    namespace Assistances
    {
        public class ExclamationMark : Assistance, IBasic
        {
            Transform Top;
            //Transform Bottom;
            Transform Child;

            Vector3 ChildScaleOrigin;

            public EventHandler s_touched;

            MATCH.Assistances.Dialogs.Dialog1 Help;

            public bool AdjustHeightOnShow { private get; set; }

            private void Awake()
            {
                // Initialize variables

                // Children
                Child = gameObject.transform.Find("Child");
                Top = Child.transform.Find("default");
                //Bottom = Child.transform.Find("Bottom");

                // Scale origin
                ChildScaleOrigin = Child.localScale;

                // Adding the touch event
                Utilities.HologramInteractions interactions = Child.GetComponent<Utilities.HologramInteractions>();
                if (interactions == null)
                {
                    interactions = Child.gameObject.AddComponent<Utilities.HologramInteractions>();
                }
                interactions.EventTouched += delegate (System.Object sender, EventArgs args)
                {
                    s_touched?.Invoke(sender, args);
                };
                //interactions.s

                AdjustHeightOnShow = true;

               
            }

            private void Start()
            {
                // Help buttons
                //if (!transform.Find("ExclamationMarkButtons"))
                //{
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Help buttons instanciated for " + gameObject.name);

                List<string> buttonsText = new List<string>();
                buttonsText.Add("Oui");
                buttonsText.Add("Non");
                List<EventHandler> buttonsCallback = new List<EventHandler>();
                buttonsCallback.Add(CButtonHelp);
                buttonsCallback.Add(CButtonHelp);
                List<Assistances.Buttons.Button.ButtonType> buttonsType = new List<Buttons.Button.ButtonType>();
                buttonsType.Add(Buttons.Button.ButtonType.Yes);
                buttonsType.Add(Buttons.Button.ButtonType.No);
                Help = Assistances.Factory.Instance.CreateButtons("", "Besoin d'aide?", buttonsText, buttonsCallback, buttonsType, transform);
                Help.AdjustToHeight = false;
                Help.gameObject.name = "ExclamationMarkButtons";
                //Help.GetTransform().localPosition = new Vector3(ChildView.localPosition.x, ChildView.localPosition.y - 0.3f, ChildView.localPosition.z);
                Help.GetTransform().localPosition = new Vector3(0, -0.1f, 0);
                //Help.Hide(Utilities.Utility.GetEventHandlerEmpty());
                //}


            }

            //bool MutexShow = false;
            public override void Show(EventHandler eventHandler, bool withAnimation)
            {
                Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();

                if (IsDisplayed == false)
                {
                    if (AdjustHeightOnShow)
                    {
                        MATCH.Utilities.Utility.AdjustObjectHeightToHeadHeight(transform);
                    }

                    if (withAnimation)
                    {
                        MATCH.Utilities.Utility.AnimateAppearInPlace(Child.gameObject, ChildScaleOrigin, delegate (System.Object o, EventArgs e)
                        {
                            IsDisplayed = true;
                            //MutexShow = false;
                            args.Success = true;
                            eventHandler?.Invoke(this, args);
                            //Help.Show(Utilities.Utility.GetEventHandlerEmpty());
                        });
                    }
                    else
                    {
                        Child.gameObject.SetActive(true);
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
                //}
            }

            //bool MutexHide = false;
            public override void Hide(EventHandler eventHandler, bool withAnimation)
            {
                Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();

                if (IsDisplayed)
                {
                    if ( withAnimation )
                    {
                        MATCH.Utilities.Utility.AnimateDisappearInPlace(Child.gameObject, ChildScaleOrigin, delegate (System.Object o, EventArgs e)
                        {
                            Child.gameObject.transform.localScale = ChildScaleOrigin;
                            IsDisplayed = false;

                            if (Help.IsDisplayed)
                            {
                                ShowHelp(false, eventHandler, withAnimation);
                            }
                            else
                            {
                                args.Success = true;
                                eventHandler?.Invoke(this, args);
                            }
                        });
                    }
                    else
                    {
                        Child.gameObject.SetActive(false);

                        Child.gameObject.transform.localScale = ChildScaleOrigin;
                        IsDisplayed = false;

                        if (Help.IsDisplayed)
                        {
                            ShowHelp(false, eventHandler, withAnimation);
                        }
                        else
                        {
                            args.Success = true;
                            eventHandler?.Invoke(this, args);
                        }
                    }
                }
                else
                {
                    args.Success = false;
                    eventHandler?.Invoke(this, args);
                }
                    
                //}
            }

            public override void ShowHelp(bool show, EventHandler callback, bool withAnimation)
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "ShowHelp function called for " + Help.name + " show=" + show.ToString());
                if (show)
                {
                    Help.Show(delegate(System.Object o, EventArgs e)
                    {
                        Help.ShowHelp(true, callback, withAnimation);
                    }, withAnimation);
                }
                else
                {
                    Help.Hide(callback, withAnimation);
                }
            }

            public override Transform GetTransform()
            {
                return Child;
            }

            public void SetMaterial(string materialName)
            {
                Renderer renderer = Top.GetComponent<Renderer>();
                if (renderer != null)
                {
                    //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Material set to " + materialName);
                    renderer.material = Resources.Load(materialName, typeof(Material)) as Material;
                }
                else
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "No renderer component for the child - no action will be done");
                }
            }

            /*public void SetAdjustHeightOnShow(bool enable)
            {
                m_adjustHeight = enable;
            }*/

            public void SetScale(float x, float y, float z)
            {
                SetScale(new Vector3(x, y, z));
                //m_childView.transform.localScale = new Vector3(x, y, z);
                //m_childScaleOrigin = m_childView.transform.localScale;
            }

            public void SetScale(Vector3 scale)
            {
                Child.transform.localScale = scale;
                ChildScaleOrigin = Child.transform.localScale;
            }

            public Vector3 GetScale()
            {
                return Child.transform.localScale;
            }

            public void SetLocalPositionObject(float x, float y, float z)
            {
                //m_childView.transform.localPosition = new Vector3(x, y, z);
                SetLocalPositionObject(new Vector3(x, y, z));
            }

            public void SetLocalPositionObject(Vector3 localPosition)
            {
                transform.localPosition = localPosition;
            }

            public void SetLocalPositionExclamationMark(Vector3 localPosition)
            {
                Child.transform.localPosition = localPosition;
            }

            public Vector3 GetLocalPosition()
            {
                return Child.transform.localPosition;
            }

            public void SetBillboard(bool enable)
            {
                Child.GetComponent<Billboard>().enabled = enable;
            }

            public void TriggerTouch()
            {
                s_touched?.Invoke(this, EventArgs.Empty);
            }

            public Transform GetChildTransform()
            {
                return Child.transform;
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

            public override bool IsDecorator()
            {
                return false;
            }

            /*public override bool IsActive()
            {
                return ChildView.gameObject.activeSelf;
            }*/
            public Transform GetSound()
            {
                return null;
            }

            public Transform GetArch()
            {
                return null;
            }

            public Transform GetIcon()
            {
                return null;
            }
        }
    }
}