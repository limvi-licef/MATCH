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

using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;
using System.Reflection;
using System.Linq;

namespace MATCH
{
    namespace Assistances
    {
        namespace Decorators
        {
            public class Edge : Assistance, IPanel
            {
                IPanel PanelToDecorate;
                string EdgeColor;
                float EdgeThickness;

                Transform EdgeTop;
                Transform EdgeLeft;
                Transform EdgeRight;
                Transform EdgeBottom;

                private void Awake()
                {
                    EdgeTop = Instantiate<Transform>(transform.Find("Edge"), transform);
                    EdgeLeft = Instantiate<Transform>(transform.Find("Edge"), transform);
                    EdgeRight = Instantiate<Transform>(transform.Find("Edge"), transform);
                    EdgeBottom = Instantiate<Transform>(transform.Find("Edge"), transform);
                }

                public override Transform GetTransform()
                {
                    return PanelToDecorate.GetAssistance().GetTransform();
                }

                /*public override bool IsActive()
                {
                    return PanelToDecorate.GetAssistance().IsActive();
                }*/

                public override void Hide(EventHandler callback, bool withAnimation)
                {
                    PanelToDecorate.GetAssistance().Hide(delegate(System.Object o, EventArgs e)
                    {
                        EdgeTop.gameObject.SetActive(false);
                        EdgeBottom.gameObject.SetActive(false);
                        EdgeLeft.gameObject.SetActive(false);
                        EdgeRight.gameObject.SetActive(false);

                        callback?.Invoke(this, e);

                        IsDisplayed = false;
                    }, withAnimation);
                }

                public override bool IsDecorator()
                {
                    return true;
                }

                public void SetAssistanceToDecorate(IPanel toDecorate)
                {
                    PanelToDecorate = toDecorate;

                    // Relaying the eventhandler
                    Assistance temp = (Assistance)PanelToDecorate;
                    temp.EventHelpButtonClicked += delegate (System.Object o, EventArgs e)
                    {
                        MATCH.Utilities.EventHandlerArgs.Button args = (MATCH.Utilities.EventHandlerArgs.Button)e;
                        OnHelpButtonClicked(args.ButtonType);
                    };

                    // Make the decorator a child of the panel to decorate so that it will be in the same coordinate system
                    Transform transformPanelToDecorate = PanelToDecorate.GetAssistance().GetTransform();
                    transform.SetParent(transformPanelToDecorate);

                    // Adjust the edges to the size of the background plate
                    Transform backPlate = transformPanelToDecorate.Find("ContentBackPlate");
                    EdgeTop.localScale = new Vector3(backPlate.localScale.x+0.02f, 0.01f, 0.01f);
                    EdgeTop.position = new Vector3(backPlate.localPosition.x, backPlate.localPosition.y + backPlate.localScale.y / 2.0f + EdgeTop.localScale.y/2.0f, backPlate.localPosition.z);
                    
                    EdgeBottom.localScale = new Vector3(backPlate.localScale.x+0.02f, 0.01f, 0.01f);
                    EdgeBottom.position = new Vector3(backPlate.localPosition.x, backPlate.localPosition.y - (backPlate.localScale.y / 2.0f + EdgeBottom.localScale.y / 2.0f), backPlate.localPosition.z);
                    
                    EdgeLeft.localScale = new Vector3(0.01f, backPlate.localScale.y, 0.01f);
                    EdgeLeft.position = new Vector3(backPlate.localPosition.x - (backPlate.localScale.x / 2.0f + EdgeLeft.localScale.x / 2.0f), backPlate.localPosition.y, backPlate.localPosition.z);
                    
                    EdgeRight.localScale = new Vector3(0.01f, backPlate.localScale.y, 0.01f);
                    EdgeRight.position = new Vector3(backPlate.localPosition.x + (backPlate.localScale.x / 2.0f + EdgeRight.localScale.x / 2.0f), backPlate.localPosition.y, backPlate.localPosition.z);
                    
#if UNITY_EDITOR
                    EdgeTop.Rotate(new Vector3(0, 180, 0));
                    EdgeBottom.Rotate(new Vector3(0, 180, 0));
                    EdgeLeft.Rotate(new Vector3(0, 180, 0));
                    EdgeRight.Rotate(new Vector3(0, 180, 0));
#endif
                }

                public override void Show(EventHandler callback, bool withAnimation)
                {
                    PanelToDecorate.GetAssistance().Show(delegate (System.Object o, EventArgs e)
                    {
                        transform.localPosition = PanelToDecorate.GetBackground().localPosition;

                        transform.GetComponent<Billboard>().gameObject.SetActive(true);

                        EdgeTop.gameObject.SetActive(true);
                        EdgeBottom.gameObject.SetActive(true);
                        EdgeLeft.gameObject.SetActive(true);
                        EdgeRight.gameObject.SetActive(true);

                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Edges shown");

                        callback?.Invoke(this, e);
                    }, withAnimation);
                }

                public override void ShowHelp(bool show, EventHandler callback, bool withAnimation)
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Not implemented yet");
                    throw new NotImplementedException();
                }

                public void EnableWeavingHand(bool enable)
                {
                    PanelToDecorate.EnableWeavingHand(enable);
                }

                public Assistance GetAssistance()
                {
                    return this;//PanelToDecorate.GetAssistance();
                }

                public Assistance GetRootDecoratedAssistance()
                {
                    return PanelToDecorate.GetRootDecoratedAssistance();
                }

                public Assistance GetDecoratedAssistance()
                {
                    return PanelToDecorate.GetAssistance();
                }

                /*public void SetBackgroundColor(string colorName)
                {
                    PanelToDecorate.SetBackgroundColor(colorName);
                }*/

                public void SetEdgeColor(string colorName)
                {
                    EdgeColor = colorName;

                    UnityEngine.Material color = Resources.Load(EdgeColor, typeof(UnityEngine.Material)) as UnityEngine.Material;

                    EdgeTop.GetComponent<Renderer>().material = color;
                    EdgeBottom.GetComponent<Renderer>().material = color;
                    EdgeLeft.GetComponent<Renderer>().material = color;
                    EdgeRight.GetComponent<Renderer>().material = color;
                }

                public void SetEdgeThickness(float thickness)
                {
                    EdgeThickness = thickness;
                }

                public Transform GetBackground()
                {
                    return PanelToDecorate.GetBackground();
                }

                public Transform GetSound()
                {
                    return PanelToDecorate.GetSound();
                }

                public Transform GetArch()
                {
                    return PanelToDecorate.GetArch();
                }

                public Transform GetIcon()
                {
                    return PanelToDecorate.GetIcon();
                }
            }
        }
    }
}