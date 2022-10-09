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

namespace MATCH
{
    namespace Assistances
    {
        namespace Surfaces
        {
            public class Manager : MonoBehaviour
            {
                List<InteractionSurface> Surfaces;

                void Start()
                {
                    // Add admin button to create new surface
                    AdminMenu.Instance.AddButton("Add surface", CallbackAddSurface, AdminMenu.Panels.Obstacles);
                }


                void CallbackAddSurface()
                {
                    Assistances.Factory.Instance.CreateInteractionSurface("Surface " + Surfaces.Count, AdminMenu.Panels.Obstacles, new Vector3(0.4f, 0.2f, 0.4f), Utilities.Materials.Colors.PurpleGlowing, true, true, Utilities.Utility.GetEventHandlerEmpty(), transform);

                }

                public bool IsObjectInteractingWithSurface(Vector3 positionDetected)
                {
                    // Todo - if hit surface and distance between origin and hit is < xxx 
                    foreach(InteractionSurface surface in Surfaces)
                    {
                        //Vector3.Distance(positionDetected, )
                    }
                    

                    return false;
                }
            }
        }
    }
}
