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
    namespace Utilities
    {
        public class Emphasize : MonoBehaviour
        {
            List<Material> Materials;
            float Hue;
            float ToAdd;

            bool EmphasizeEnabled;

            private void Awake()
            {
                Hue = 0.0f;
                if (Utilities.Utility.IsEditorSimulator() || Utilities.Utility.IsEditorGameView())
                {
                    ToAdd = 0.5f;
                }
                else
                { // Means running in the Hololens: the frame rate being lower, the value is higher, to have a faster color change
                    ToAdd = 3.0f;
                }
                EmphasizeEnabled = false;
                Materials = new List<Material>();
            }

            // Start is called before the first frame update
            void Start()
            {
                //ObjectShader = gameObject.GetComponent<Renderer>().material;
                //HueCurrent = ObjectShader.GetFloat("_Hue");
            }

            /**
             * To be added, the shader attached to the material must be "MATCH/Shaders/AdjustHSV". If yes, returns true; otherwise, returns false and does not add the material to the list.
             */
            public bool AddMaterial(Transform transform)
            {
                bool toReturn = false;

                Renderer renderer = null;

                if (transform.gameObject.TryGetComponent<Renderer>(out renderer))
                {
                    if (renderer.material.shader.name == "MATCH/Shaders/AdjustHSV")
                    {
                        Materials.Add(renderer.material);
                        //Hues.Add(material.GetFloat("_Hue"));

                        toReturn = true;
                    }
                }

                return toReturn;
            }

            public void EnableEmphasize (bool enable)
            {
                EmphasizeEnabled = enable;

                if (EmphasizeEnabled == false)
                {
                    foreach (Material material in Materials)
                    {
                        material.SetFloat("_Hue", 0.0f);
                    }
                }
            }

            // Update is called once per frame
            void Update()
            {
                if (EmphasizeEnabled)
                {
                    if (Hue >= 360)
                    {
                        Hue = -360;
                    }

                    foreach (Material material in Materials)
                    {
                        Hue += ToAdd;
                        material.SetFloat("_Hue", Hue);
                    }
                }

                //ObjectShader.SetFloat("_Hue", Hues);

            }
        }
    }
}

