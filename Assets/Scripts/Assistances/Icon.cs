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
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Reflection;
using System;

/**
 * This class manages a basic show / hide for many icon that should be named IconObj (that 3D Object .obj) 
 * An event is emitted if the nested hologram is touched.
 * Appears / disappears in place. I told you it was basic.
 * */

namespace MATCH
{
    namespace Assistances
    {
        public class Icon : Assistance, IBasic
        {
            //Transform Mat;
            Transform IconObj;

            Vector3 IconObjScaleOrigin;

            public EventHandler s_touched;
            public string IconType ;
            MATCH.Assistances.Dialogs.Dialog1 Help;

            public bool AdjustHeightOnShow { private get; set; }

            private void Awake()
            {
                // Initialize variables
                IconObj = gameObject.transform.Find(IconType);

            }

            private void Start()
            {
                
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
                Help.gameObject.name = "Icon_help";
                Help.GetTransform().localPosition = new Vector3(0, -0.4f, 0);
                Help.GetComponent<BoxCollider>().enabled = false;
                                
            }

            public override void Show(EventHandler eventHandler, bool withAnimation)
            {
                Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();

                if (IsDisplayed == false)
                {
                    gameObject.GetComponent<BoxCollider>().enabled = true;
                    Help.GetComponent<BoxCollider>().enabled = true;

                    if (AdjustHeightOnShow)
                    {
                        MATCH.Utilities.Utility.AdjustObjectHeightToHeadHeight(transform);
                    }
                    IconObj.gameObject.SetActive(true);
                    DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Help buttons instanciated for " + gameObject.name);

                    IsDisplayed = true;
                    args.Success = true;
                    eventHandler?.Invoke(this, args);

                }
                else
                {
                    args.Success = false;
                    eventHandler?.Invoke(this, args);
                }
                
            }

            public override void Hide(EventHandler eventHandler, bool withAnimation)
            {
                Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();

                if (IsDisplayed)
                {
                    gameObject.GetComponent<BoxCollider>().enabled = false;
                    Help.GetComponent<BoxCollider>().enabled = false;

                    if (withAnimation)
                    {
                        MATCH.Utilities.Utility.AnimateDisappearInPlace(IconObj.gameObject, IconObjScaleOrigin, delegate (System.Object o, EventArgs e)
                        {
                            IconObj.gameObject.transform.localScale = IconObjScaleOrigin;
                            IsDisplayed = false;
                            //this.gameObject.SetActive(false);
                            IconObj.gameObject.SetActive(false);
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
                        //this.gameObject.SetActive(false);
                        IconObj.gameObject.SetActive(false);

                        IconObj.gameObject.transform.localScale = IconObjScaleOrigin;
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

            }

            public override void ShowHelp(bool show, EventHandler callback, bool withAnimation)
            {
                DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "ShowHelp function called for " + Help.name + " show=" + show.ToString());
                if (show)
                {
                    Help.Show(delegate (System.Object o, EventArgs e)
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
                return /*IconObj*/ transform;
            }

            public void SetMaterial(string materialName)
            {
                for(int i=0; i < IconObj.childCount; i++)
                {
                    Transform child = IconObj.GetChild(i);
                    Renderer renderer = child.GetComponent<Renderer>();

                    if (renderer != null)
                    {
                        //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Material set to " + materialName);
                        renderer.material = Resources.Load(materialName, typeof(Material)) as Material;
                    }
                    else
                    {
                        DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "No renderer component for the IconObj - no action will be done");
                    }
                }                
            }

            public string GetIconType()
            {
                return IconType;
            }

            public void SetIconType(string newIcon)
            {
                IconObj = transform.Find("Child").Find(newIcon);
            }
            public void SetScale(float x, float y, float z)
            {
                SetScale(new Vector3(x, y, z));
            }

            public void SetScale(Vector3 scale)
            {
                this.transform.localScale = scale;
                IconObjScaleOrigin = IconObj.transform.localScale;
            }

            public Vector3 GetScale()
            {
                return this.transform.localScale;
            }

            public void SetLocalPositionObject(float x, float y, float z)
            {
                SetLocalPositionObject(new Vector3(x, y, z));
            }

            public void SetLocalPositionObject(Vector3 localPosition)
            {
                transform.localPosition = localPosition;
            }

            public void SetLocalPositionIcon(float x, float y, float z)
            {
                SetLocalPositionIcon(new Vector3(x, y, z));
            }

            public void SetLocalPositionIcon(Vector3 localPosition)
            {
                IconObj.transform.localPosition = localPosition;
            }

            public Vector3 GetLocalPositionIcon()
            {
                return IconObj.transform.localPosition;
            }

            public Vector3 GetLocalPositionObject()
            {
                return transform.localPosition;
            }

            public void SetBillboard(bool enable)
            {
                this.GetComponent<Billboard>().enabled = enable;
            }

            public void TriggerTouch()
            {
                s_touched?.Invoke(this, EventArgs.Empty);
            }

            public Transform GetIconObjTransform()
            {
                return IconObj.transform;
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

            public Transform GetSound()
            {
                return null;
            }

            public Transform GetArch()
            {
                return null;
            }

            public Icon GetIcon()
            {
                return this;
            }

            public Transform GetLinePath()
            {
                return null;
            }

            public override void Emphasize(bool enable)
            {
                if (enable)
                {
                    Utilities.Emphasize emphasize = gameObject.AddComponent<Utilities.Emphasize>();

                    emphasize.AddMaterial(IconObj.Find("default"));
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