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
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using Microsoft.MixedReality.Toolkit.Diagnostics;
using Microsoft.MixedReality.Toolkit.Utilities;
using TMPro;
using System.Reflection;
using System;
using System.Linq;

namespace MATCH
{
    public class AdminMenu : MonoBehaviour
    {
        public enum Panels
        {
            Default = 0,
            Obstacles = 1
        };

        bool MenuShown;

        public bool MenuStatic = false;

        //string m_hologramRagInteractionSurfaceMaterialName;

        private static AdminMenu InstanceInternal;

        public static AdminMenu Instance { get { return InstanceInternal; } }

        public GameObject RefButtonSwitch;
        public GameObject RefButton;

        List<GameObject> Buttons;
        Dictionary<Panels, Transform> PanelsStorage;

        private void Awake()
        {
            if (InstanceInternal != null && InstanceInternal != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                // Initialize variables
                Buttons = new List<GameObject>();
                PanelsStorage = new Dictionary<Panels, Transform>();

                // Remove the canvas renderer from the buttons, to avoid the warning message from unity
                DestroyImmediate(RefButton.transform.Find("IconAndText").Find("TextMeshPro").GetComponent<CanvasRenderer>(), true);
                DestroyImmediate(RefButton.transform.Find("SeeItSayItLabel").Find("TextMeshPro").GetComponent<CanvasRenderer>(), true);
                DestroyImmediate(RefButtonSwitch.transform.Find("IconAndText").Find("TextMeshPro").GetComponent<CanvasRenderer>(), true);
                DestroyImmediate(RefButtonSwitch.transform.Find("SeeItSayItLabel").Find("TextMeshPro").GetComponent<CanvasRenderer>(), true);

                // Get children
                PanelsStorage.Add(Panels.Default, gameObject.transform.Find("PanelDefault").Find("ButtonParent").transform);
                PanelsStorage.Add(Panels.Obstacles, gameObject.transform.Find("PanelObstacles").Find("ButtonParent").transform);

                InstanceInternal = this;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            // Variables
            MenuShown = false; // By default, the menu is hidden

            SwitchStaticOrMovingMenu();

            // Add the buttons to manage this menu
            AddSwitchButton("Static/Mobile menu", CallbackSwitchStaticOrMovingMenu);


        }

        public void AddSwitchButton(string text, UnityEngine.Events.UnityAction callback, Panels panel = Panels.Default)
        {
            Buttons.Add(Instantiate(RefButtonSwitch, PanelsStorage[panel]));
            Buttons.Last().GetComponent<Interactable>().GetReceiver<InteractableOnPressReceiver>().OnPress.AddListener(callback);
            Buttons.Last().GetComponent<Interactable>().GetReceiver<InteractableOnPressReceiver>().InteractionFilter = 0;
            Buttons.Last().transform.Find("IconAndText").Find("TextMeshPro").GetComponent<TextMeshPro>().SetText(text);
            PanelsStorage[panel].GetComponent<GridObjectCollection>().UpdateCollection();
        }

        public void AddButton(string text, UnityEngine.Events.UnityAction callback, Panels panel = Panels.Default)
        {
            Buttons.Add(Instantiate(RefButton, PanelsStorage[panel]));

            Buttons.Last().GetComponent<ButtonConfigHelper>().IconStyle = ButtonIconStyle.None;
            Buttons.Last().GetComponent<ButtonConfigHelper>().SeeItSayItLabelEnabled = false;
            Buttons.Last().GetComponent<Interactable>().GetReceiver<InteractableOnPressReceiver>().OnPress.AddListener(callback);
            Buttons.Last().GetComponent<Interactable>().GetReceiver<InteractableOnPressReceiver>().InteractionFilter = 0;
            Buttons.Last().transform.Find("IconAndText").Find("TextMeshPro").GetComponent<TextMeshPro>().SetText(text);
            PanelsStorage[panel].GetComponent<GridObjectCollection>().UpdateCollection();
        }

        public void CallbackCubeTouched()
        {
            MenuShown = !MenuShown;

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                gameObject.transform.GetChild(i).gameObject.SetActive(MenuShown);
            }
        }

        public void CallbackSwitchStaticOrMovingMenu()
        {
            MenuStatic = !MenuStatic;

            SwitchStaticOrMovingMenu();
        }

        public void SwitchStaticOrMovingMenu()
        {
            string materialName;

            gameObject.GetComponent<RadialView>().enabled = !MenuStatic; // Menu static == RadialView must be disabled

            if (gameObject.GetComponent<RadialView>().enabled)
            {
                materialName = Utilities.Materials.Colors.OrangeGlowing;
            }
            else
            {
                materialName = Utilities.Materials.Colors.PurpleGlowing;
            }

            gameObject.GetComponent<Renderer>().material = Resources.Load(materialName, typeof(Material)) as Material;
        }
    }

}
