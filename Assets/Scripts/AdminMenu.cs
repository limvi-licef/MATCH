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
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using UnityEngine.UI;
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
            Middle = 0,
            Left = 1,
            Right = 2
        };

        public enum ButtonType
        {
            Default = 0,
            Hide = 1
        }

        bool MenuShown;

        public bool MenuStatic = false;

        //string m_hologramRagInteractionSurfaceMaterialName;

        private static AdminMenu InstanceInternal;

        public static AdminMenu Instance { get { return InstanceInternal; } }

        //public GameObject RefButtonSwitch;
        //public GameObject RefButton;
        public GameObject RefInput;

        List<GameObject> Buttons;
        Dictionary<Panels, Transform> PanelsStorage;

        List<UnityEngine.Events.UnityAction> HideAllCallbacks;

        TouchScreenKeyboard Keyboard;
        GameObject ModifiedByKeyboard;

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
                HideAllCallbacks = new List<UnityEngine.Events.UnityAction>();

                // Remove the canvas renderer from the buttons, to avoid the warning message from unity
                //DestroyImmediate(RefButton.transform.Find("IconAndText").Find("TextMeshPro").GetComponent<CanvasRenderer>(), true);
                //DestroyImmediate(RefButton.transform.Find("SeeItSayItLabel").Find("TextMeshPro").GetComponent<CanvasRenderer>(), true);
                //DestroyImmediate(RefButtonSwitch.transform.Find("IconAndText").Find("TextMeshPro").GetComponent<CanvasRenderer>(), true);
                //DestroyImmediate(RefButtonSwitch.transform.Find("SeeItSayItLabel").Find("TextMeshPro").GetComponent<CanvasRenderer>(), true);

                // Get children
                PanelsStorage.Add(Panels.Middle, gameObject.transform.Find("PanelMiddle").Find("ButtonParent").transform);
                PanelsStorage.Add(Panels.Left, gameObject.transform.Find("PanelLeft").Find("ButtonParent").transform);
                PanelsStorage.Add(Panels.Right, gameObject.transform.Find("PanelRight").Find("ButtonParent").transform);

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

            // Add button to hide/show all elements that are registered with a button type "hide"
            AddSwitchButton("Hide/show all", CallbackHideShowAll);

            //gameObject.AddComponent<ObjectManipulator>();

            // Hiding spongy
            AddSwitchButton("Spongy - hide", CallbackHideSpongy, Panels.Left, ButtonType.Hide);
            

            //AddText("Test", delegate() { });
        }

        private void Update()
        {
            if (Keyboard != null)
            {
                ModifiedByKeyboard.GetComponent<TextMesh>().text = Keyboard.text;
            }
        }

        public void CallbackHideSpongy()
        {
            BoxCollider box = transform.GetComponent<BoxCollider>();
            Transform wltAdjustment = box.transform.root.Find("WLT_Adjustment");
            Transform spongy = null;
            Transform f1 = null;

            foreach (GameObject go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (go.name == "WLT_Adjustment")
                {
                    spongy = go.transform.Find("Spongy");
                }
                if (go.name == "WorldLockingViz")
                {
                    f1 = go.transform.Find("F1");
                }
            }
            //Transform spongy = wltAdjustment.Find("Spongy");
            if (spongy != null)
            {
                spongy.gameObject.SetActive(!spongy.gameObject.activeSelf);
            }
            if (f1 != null)
            {
                f1.gameObject.SetActive(!f1.gameObject.activeSelf);
            }
        }

        public void CallbackHideShowAll()
        {
            foreach (UnityEngine.Events.UnityAction e in HideAllCallbacks)
            {
                e?.Invoke();
            }
        }

        /**
         * buttonType: used to manager internal functionalities. For instance, if "HideShow" type is selected, then the callback will also be added to a general "hide all" button.
         * */
        public MATCH.Assistances.Buttons.Basic AddSwitchButton(string text, UnityEngine.Events.UnityAction callback, Panels panel = Panels.Middle, ButtonType buttonType = ButtonType.Default)
        {
            //Buttons.Add(Instantiate(RefButtonSwitch, PanelsStorage[panel]));
            Transform button = Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.ButtonSwitch, PanelsStorage[panel]);
            Buttons.Add(button.gameObject);

            Buttons.Last().GetComponent<Interactable>().GetReceiver<InteractableOnPressReceiver>().OnPress.AddListener(callback);
            Buttons.Last().GetComponent<Interactable>().GetReceiver<InteractableOnPressReceiver>().InteractionFilter = 0;
            Buttons.Last().transform.Find("IconAndText").Find("TextMeshPro").GetComponent<TextMeshPro>().SetText(text);
            PanelsStorage[panel].GetComponent<GridObjectCollection>().UpdateCollection();

            if (buttonType == ButtonType.Hide)
            {
                HideAllCallbacks.Add(callback);
            }

            return Buttons.Last().GetComponent<MATCH.Assistances.Buttons.Basic>();
        }

        public MATCH.Assistances.Buttons.Basic AddButton(string text, UnityEngine.Events.UnityAction callback, Panels panel = Panels.Middle)
        {
            //Buttons.Add(Instantiate(RefButton, PanelsStorage[panel]));
            Transform button = Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.Button, PanelsStorage[panel]);
            //button.SetParent(PanelsStorage[panel]);
            Buttons.Add(button.gameObject);

            Buttons.Last().GetComponent<ButtonConfigHelper>().IconStyle = ButtonIconStyle.None;
            Buttons.Last().GetComponent<ButtonConfigHelper>().SeeItSayItLabelEnabled = false;
            Buttons.Last().GetComponent<Interactable>().GetReceiver<InteractableOnPressReceiver>().OnPress.AddListener(callback);
            Buttons.Last().GetComponent<Interactable>().GetReceiver<InteractableOnPressReceiver>().InteractionFilter = 0;
            Buttons.Last().transform.Find("IconAndText").Find("TextMeshPro").GetComponent<TextMeshPro>().SetText(text);
            PanelsStorage[panel].GetComponent<GridObjectCollection>().UpdateCollection();

            return Buttons.Last().GetComponent<MATCH.Assistances.Buttons.Basic>();
        }

        public MATCH.Assistances.Buttons.Basic AddButtonIcon(string text, Material icon, UnityEngine.Events.UnityAction callback, Panels panel = Panels.Middle)
        {
            //Buttons.Add(Instantiate(RefButton, PanelsStorage[panel]));
            Transform button = Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.ButtonIcon, PanelsStorage[panel]);
            //button.SetParent(PanelsStorage[panel]);
            Buttons.Add(button.gameObject);

            //Buttons.Last().GetComponent<ButtonConfigHelper>().IconQ = ButtonIconStyle.None;
            MeshRenderer renderer = Buttons.Last().transform.Find("IconAndText").Find("UIButtonSquareIcon").GetComponent<MeshRenderer>();
            //MeshRenderer renderer = Buttons.Last().transform.Find("Icon").GetComponent<MeshRenderer>();
            renderer.material = icon;
            Buttons.Last().GetComponent<ButtonConfigHelper>().SeeItSayItLabelEnabled = false;
            Buttons.Last().GetComponent<Interactable>().GetReceiver<InteractableOnPressReceiver>().OnPress.AddListener(callback);
            Buttons.Last().GetComponent<Interactable>().GetReceiver<InteractableOnPressReceiver>().InteractionFilter = 0;
            Buttons.Last().transform.Find("IconAndText").Find("TextMeshPro").GetComponent<TextMeshPro>().SetText(text);
            PanelsStorage[panel].GetComponent<GridObjectCollection>().UpdateCollection();

            return Buttons.Last().GetComponent<MATCH.Assistances.Buttons.Basic>();
        }

            public MATCH.Assistances.Buttons.Basic AddButtonIcon(string text, string pathToIcon, UnityEngine.Events.UnityAction callback, Panels panel = Panels.Middle)
        {
            return AddButtonIcon(text, Resources.Load(pathToIcon, typeof(Material)) as Material, callback, panel);
        }

        /**
         * Callback contains a EventHandlerArgs.String arg, the value being the new value of the text field.
         */
        public void AddInputWithButton(string textInput, string textButton, EventHandler callbackButton, Panels panel = Panels.Middle)
        {
            // Manage the input text
            GameObject input = Instantiate(RefInput, PanelsStorage[panel]);
            Buttons.Add(input);
            Buttons.Last().AddComponent<BoxCollider>();
            Interactable interactions = Buttons.Last().AddComponent<Interactable>();
            interactions.OnClick.AddListener(delegate ()
            {
                ModifiedByKeyboard = input;
                Keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NumberPad, false);
            });

            TextMesh textMesh = Buttons.Last().GetComponent<TextMesh>();
            textMesh.text = textInput;
            PanelsStorage[panel].GetComponent<GridObjectCollection>().UpdateCollection();

            // Manage the update button
            //Buttons.Add(Instantiate(RefButton, PanelsStorage[panel]));
            Transform button = Utilities.Materials.Prefabs.Load(Utilities.Materials.Prefabs.Button, PanelsStorage[panel]);
            //button.SetParent(PanelsStorage[panel]);
            Buttons.Add(button.gameObject);

            Buttons.Last().GetComponent<ButtonConfigHelper>().IconStyle = ButtonIconStyle.None;
            Buttons.Last().GetComponent<ButtonConfigHelper>().SeeItSayItLabelEnabled = false;
            Buttons.Last().GetComponent<Interactable>().GetReceiver<InteractableOnPressReceiver>().OnPress.AddListener(delegate()
            {
                Utilities.EventHandlerArgs.String arg = new Utilities.EventHandlerArgs.String(textMesh.text);
                callbackButton?.Invoke(this, arg);
            });
            Buttons.Last().GetComponent<Interactable>().GetReceiver<InteractableOnPressReceiver>().InteractionFilter = 0;
            Buttons.Last().transform.Find("IconAndText").Find("TextMeshPro").GetComponent<TextMeshPro>().SetText(textButton);
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
