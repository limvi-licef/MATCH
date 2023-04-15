/*Copyright 2022 Guillaume Spalla, Louis Marquet

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
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System;

/**
 * To be used with button having the "Interactable" MRTK component.
 * */
namespace MATCH
{
    namespace Assistances
    {
        namespace Buttons
        {
            public class Basic : Button
            {
                bool m_checked = false;

                public override void Hide(EventHandler e)
                {
                    throw new NotImplementedException();
                }

                public override void Show(EventHandler e)
                {
                    throw new NotImplementedException();
                }

                private void Awake()
                {
                    //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called for Basic");

                    Interactable interactions = gameObject.GetComponent<Interactable>();
                    interactions.AddReceiver<InteractableOnTouchReceiver>().OnTouchStart.AddListener(delegate () {
                        //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Button clicked");
                        OnButtonClicked();
                    });
                }

                public void CheckButton(bool check)
                {
                    m_checked = check;

                    if (m_checked)
                    {
                        transform.Find("BackPlate").Find("Quad").GetComponent<Renderer>().material = Resources.Load(Utilities.Materials.Colors.GreenGlowing, typeof(Material)) as Material;
                    }
                    else
                    {
                        transform.Find("BackPlate").Find("Quad").GetComponent<Renderer>().material = Resources.Load(Utilities.Materials.Colors.HolographicBackPlate, typeof(Material)) as Material;
                    }
                }

                public bool IsChecked()
                {
                    return m_checked;
                }

                public void CallbackSetButtonBackgroundCyan(System.Object o, EventArgs e)
                {
                    transform.Find("BackPlate").Find("Quad").GetComponent<Renderer>().material = Resources.Load(Utilities.Materials.Colors.CyanGlowing, typeof(Material)) as Material;
                }

                public void CallbackSetButtonBackgroundGreen(System.Object o, EventArgs e)
                {
                    transform.Find("BackPlate").Find("Quad").GetComponent<Renderer>().material = Resources.Load(Utilities.Materials.Colors.GreenGlowing, typeof(Material)) as Material;
                }

                public void CallbackSetButtonBackgroundDefault(System.Object o, EventArgs e)
                {
                    transform.Find("BackPlate").Find("Quad").GetComponent<Renderer>().material = Resources.Load(Utilities.Materials.Colors.HolographicBackPlate, typeof(Material)) as Material;
                }
            }

        }
    }
}

