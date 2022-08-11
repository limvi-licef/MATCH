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
        public class ProcessingSurface : MonoBehaviour
        {
            public int m_numberOfCubesToAddInRow = 5;
            public int m_numberOfCubesToAddInColumn = 4;

            public Transform m_hologramToUseToPopulateSurface;

            public event EventHandler EventSurfaceCleaned;

            Dictionary<Tuple<float, float>, Tuple<GameObject, bool>> m_cubesTouched;

            // Start is called before the first frame update
            void Start()
            {
                // Initialize the variables
                m_cubesTouched = new Dictionary<Tuple<float, float>, Tuple<GameObject, bool>>();

                // Get child, i.e. the default hologram to use to populate the surface, in case the user did not provide one
                if (m_hologramToUseToPopulateSurface == null)
                {
                    m_hologramToUseToPopulateSurface = gameObject.transform.Find("DefaultHologramPopulateSurface");
                }

                // Sanity checks
                if (m_hologramToUseToPopulateSurface.GetComponent<ProcessingSurfaceElement>() == null)
                {
                    MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Error, "The m_hologramToUseToPopulateSurface object should have a MouseChallengeCleanTableHologramForSurfaceToClean component");
                }
            }

            public void ShowInteractionCubesTablePanel(EventHandler eventHandler)
            {
                bool populateTable = false;

                if (m_cubesTouched.Count > 0)
                {
                    KeyValuePair<Tuple<float, float>, Tuple<GameObject, bool>> cube = m_cubesTouched.First();
                    if (cube.Value.Item1.activeSelf == false)
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
                    Vector3 goLocalPosition = gameObject.transform.localPosition;

                    float goScaleX = 1.0f;
                    float goScaleZ = 1.0f;

                    //MouseDebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MouseDebugMessagesManager.MessageLevel.Info, "Scaling of surface to populate: x=" + goScaleX + " z=" + goScaleZ);

                    float posX = 0.0f;
                    float posZ = 0.0f;

                    float incrementX = goScaleX / m_numberOfCubesToAddInColumn;
                    float incrementZ = goScaleZ / m_numberOfCubesToAddInRow;

                    for (posX = 0.0f; posX < goScaleX; posX += incrementX)
                    {
                        for (posZ = 0.0f; posZ < goScaleZ; posZ += incrementZ)
                        {
                            GameObject temp = Instantiate(m_hologramToUseToPopulateSurface.gameObject);
                            temp.transform.SetParent(gameObject.transform, false);
                            temp.transform.localPosition = Vector3.zero;
                            temp.transform.localScale = new Vector3(incrementX, 0.01f, incrementZ);
                            float posXP = posX - goScaleX / 2.0f + temp.transform.localScale.x / 2.0f;
                            float posZP = posZ - goScaleZ / 2.0f + temp.transform.localScale.z / 2.0f;

                            temp.transform.localPosition = new Vector3(posXP, goLocalPosition.y + 1.0f, posZP);

                            BoxCollider box = temp.GetComponent<BoxCollider>();
                            box.size = new Vector3(box.size.x, 1000, box.size.z);

                            ProcessingSurfaceElement cubeInteractions = temp.GetComponent<ProcessingSurfaceElement>();
                            cubeInteractions.CubeTouchedEvent += CubeTouched;
                            m_cubesTouched.Add(new Tuple<float, float>(posXP, posZP), new Tuple<GameObject, bool>(temp, false));
                            temp.SetActive(true); // Hidden by default
                        }
                    }

                    eventHandler?.Invoke(this, EventArgs.Empty);
                }
            }

            void CubeTouched(object sender, EventArgs e)
            {
                ProcessingSurfaceElement tempCube = (ProcessingSurfaceElement)sender;

                MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Cube touched. Position: " + tempCube.transform.localPosition.x.ToString() + " " + tempCube.transform.localPosition.z.ToString());

                Tuple<float, float> tempTuple = new Tuple<float, float>(tempCube.transform.localPosition.x, tempCube.transform.localPosition.z);

                m_cubesTouched[tempTuple] = new Tuple<GameObject, bool>(m_cubesTouched[tempTuple].Item1, true);

                CheckIfSurfaceClean();
            }

            void CheckIfSurfaceClean()
            {
                bool allCubesTouched = true; // By default weconsidered all cubes are touched. then we browse the values of the dictionary, and if we find a cube that has not been touched, then we set this boolean to false.

                foreach (KeyValuePair<Tuple<float, float>, Tuple<GameObject, bool>> tempKeyValue in m_cubesTouched)
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

            public void Hide(EventHandler eventHandler)
            {
                // Here we will remove the cubes from the table and display an hologram in the center of the table
                foreach (KeyValuePair<Tuple<float, float>, Tuple<GameObject, bool>> tempKeyValue in m_cubesTouched)
                {
                    Destroy(tempKeyValue.Value.Item1);
                }

                m_cubesTouched.Clear();

                eventHandler?.Invoke(this, EventArgs.Empty);
            }
        }

    }
}
