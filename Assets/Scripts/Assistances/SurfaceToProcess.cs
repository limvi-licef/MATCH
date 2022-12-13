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
using System.Reflection;
using System;
using System.Linq;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;

/**
 * Display a virtual surface to be "cleaned". Covers the surface of the given input gameobject m_hologramToUseToPopulateSurface. The surface is split in m_numberOfCubesToAddInRow * m_numberOfCubesToAddInColumn areas (number of elements per row and by column)
 * Emits a signal when the surface is fully cleaned
 * */
namespace MATCH
{
    namespace Assistances
    {
        public class SurfaceToProcess : Assistance, ISurface
        {
            public int NumberOfCubesToAddInRow = 5;
            public int NumberOfCubesToAddInColumn = 4;

            public ProcessingSurfaceElement HologramToUseToPopulateSurfaceController;

            public event EventHandler EventSurfaceCleaned;
            public event EventHandler EventNewPartCleaned;

            Dictionary<Tuple<float, float>, Tuple<ProcessingSurfaceElement, bool>> CubesTouched;

            private InteractionSurface SurfaceToPopulate;

            private Dialog Help;

            private void Awake()
            {
                // Initialize the variables
                CubesTouched = new Dictionary<Tuple<float, float>, Tuple<ProcessingSurfaceElement, bool>>();

                // Get child, i.e. the default hologram to use to populate the surface, in case the user did not provide one
                if (HologramToUseToPopulateSurfaceController == null)
                {
                    HologramToUseToPopulateSurfaceController = gameObject.transform.Find("DefaultHologramPopulateSurface").GetComponent<ProcessingSurfaceElement>();
                }

                // Help
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
                Help.GetTransform().localPosition = new Vector3(0, 0.2f, 0);

                // Sanity checks
                if (HologramToUseToPopulateSurfaceController.GetComponent<ProcessingSurfaceElement>() == null)
                {
                    MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Error, "The m_hologramToUseToPopulateSurface object should have a MouseChallengeCleanTableHologramForSurfaceToClean component");
                }
            }

            // Start is called before the first frame update
            void Start()
            {
                
            }

            public void SetColor(string colorName)
            {
                HologramToUseToPopulateSurfaceController.SetDefaultColor(colorName);

                foreach (KeyValuePair<Tuple<float, float>, Tuple<ProcessingSurfaceElement, bool>> cube in CubesTouched)
                {
                    cube.Value.Item1.SetDefaultColor(colorName);
                }
            }

            public void EnableWeavingHand(bool enable)
            {
                throw new NotImplementedException();
            }

            public override void Show(EventHandler eventHandler)
            {
                Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();

                if (IsDisplayed == false)
                {
                    bool populateTable = false;

                    if (CubesTouched.Count > 0)
                    {
                        KeyValuePair<Tuple<float, float>, Tuple<ProcessingSurfaceElement, bool>> cube = CubesTouched.First();
                        if (cube.Value.Item1.gameObject.activeSelf == false)
                        {
                            populateTable = true;
                        }
                        else
                        {
                            MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Warning, "At leat one cube is already displayed, so nothing to do");
                            populateTable = false;
                        }
                    }
                    else
                    {
                        populateTable = true;
                    }

                    if (populateTable)
                    {
                        // Adjusting the scale and rotation to correctly match the surface to process to the interaction surface
                        transform.localScale = SurfaceToPopulate.GetLocalScale();
                        transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
                        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y + transform.localScale.y / 2.0f, transform.localPosition.z);

                        // Populating the surface
                        Vector3 goLocalPosition = gameObject.transform.localPosition;

                        float goScaleX = 1.0f /*SurfaceToPopulate.GetLocalScale().x*/;
                        float goScaleZ = 1.0f /*SurfaceToPopulate.GetLocalScale().z*/;

                        //MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Scaling of surface to populate: x=" + goScaleX + " z=" + goScaleZ);

                        float posX = 0.0f;
                        float posZ = 0.0f;

                        float incrementX = goScaleX / NumberOfCubesToAddInColumn;
                        float incrementZ = goScaleZ / NumberOfCubesToAddInRow;

                        for (posX = 0.0f; posX < goScaleX; posX += incrementX)
                        {
                            for (posZ = 0.0f; posZ < goScaleZ; posZ += incrementZ)
                            {
                                GameObject tempView = Instantiate(HologramToUseToPopulateSurfaceController.gameObject);
                                ProcessingSurfaceElement tempController = tempView.GetComponent<ProcessingSurfaceElement>();
                                tempView.transform.SetParent(gameObject.transform, false);
                                tempView.transform.localPosition = Vector3.zero;
                                tempView.transform.localScale = new Vector3(incrementX, 0.01f, incrementZ);
                                float posXP = posX - goScaleX / 2.0f + tempView.transform.localScale.x / 2.0f;
                                float posZP = posZ - goScaleZ / 2.0f + tempView.transform.localScale.z / 2.0f;

                                tempView.transform.localPosition = new Vector3(posXP, /*goLocalPosition.y + 1.0f*/0.0f, posZP);

                                BoxCollider box = tempView.GetComponent<BoxCollider>();
                                box.size = new Vector3(box.size.x, 1000, box.size.z);

                                ProcessingSurfaceElement cubeInteractions = tempView.GetComponent<ProcessingSurfaceElement>();
                                cubeInteractions.CubeTouchedEvent += CubeTouched;
                                CubesTouched.Add(new Tuple<float, float>(posXP, posZP), new Tuple<ProcessingSurfaceElement, bool>(tempController, false));
                                tempView.SetActive(true); // Hidden by default
                            }
                        }

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

            public void SetSurfaceToPopulate(InteractionSurface surfaceToPopulate)
            {
                SurfaceToPopulate = surfaceToPopulate;
            }

            private void CubeTouched(object sender, EventArgs e)
            {
                ProcessingSurfaceElement tempCube = (ProcessingSurfaceElement)sender;

                MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Cube touched. Position: " + tempCube.transform.localPosition.x.ToString() + " " + tempCube.transform.localPosition.z.ToString());

                Tuple<float, float> tempTuple = new Tuple<float, float>(tempCube.transform.localPosition.x, tempCube.transform.localPosition.z);

                CubesTouched[tempTuple] = new Tuple<ProcessingSurfaceElement, bool>(CubesTouched[tempTuple].Item1, true);

                EventNewPartCleaned?.Invoke(this, EventArgs.Empty);

                CheckIfSurfaceClean();
            }

            private void CheckIfSurfaceClean()
            {
                bool allCubesTouched = true; // By default weconsidered all cubes are touched. then we browse the values of the dictionary, and if we find a cube that has not been touched, then we set this boolean to false.

                foreach (KeyValuePair<Tuple<float, float>, Tuple<ProcessingSurfaceElement, bool>> tempKeyValue in CubesTouched)
                {
                    if (tempKeyValue.Value.Item2 == false)
                    {
                        allCubesTouched = false;
                    }
                }

                if (allCubesTouched)
                {
                    MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "All cubes touched !!!!");
                    EventSurfaceCleaned?.Invoke(this, EventArgs.Empty);

                }
                else
                {
                    MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Still some work to do ...");
                }
            }

            public void EnableLocationControls(bool status)
            {
                string materialName;

                if (status)
                {
                    materialName = Utilities.Materials.Colors.WhiteTransparent;
                }
                else
                {
                    materialName = Utilities.Materials.Colors.CyanGlowing;
                }

                GetComponent<Renderer>().material = Resources.Load(materialName, typeof(Material)) as Material;
                GetComponent<MeshRenderer>().enabled = status;
                GetComponent<ObjectManipulator>().enabled = status;
                GetComponent<BoundsControl>().enabled = status;
            }

            public override void Hide(EventHandler eventHandler)
            {
                Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();

                if (IsDisplayed)
                {
                    // Here we will remove the cubes from the table and display an hologram in the center of the table
                    foreach (KeyValuePair<Tuple<float, float>, Tuple<ProcessingSurfaceElement, bool>> tempKeyValue in CubesTouched)
                    {
                        tempKeyValue.Value.Item1.gameObject.SetActive(false);
                        Destroy(tempKeyValue.Value.Item1.gameObject);
                    }

                    CubesTouched.Clear();

                    IsDisplayed = false;

                    args.Success = true;
                    eventHandler?.Invoke(this, args);
                }
                else
                {
                    args.Success = false;
                    eventHandler?.Invoke(this, args);
                }
            }

            public override void ShowHelp(bool show, EventHandler callback)
            {
                if (show)
                {
                    if (Help.IsDisplayed == false)
                    {
                        Help.Show(callback);
                    }
                }
                else
                {
                    if(Help.IsDisplayed)
                    {
                        Help.Hide(callback);
                    }
                }
            }

            public override Transform GetTransform()
            {
                return transform;
            }

            public override bool IsDecorator()
            {
                return false;
            }

            Assistance IAssistance.GetAssistance()
            {
                return this;
            }
        }

    }
}
