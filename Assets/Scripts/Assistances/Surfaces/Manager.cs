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
                Dictionary<GameObject, EventHandler> Objects;

                private void Awake()
                {
                    // Initialize variables
                    Surfaces = new List<InteractionSurface>();
                    Objects = new Dictionary<GameObject, EventHandler>();
                }

                void Start()
                {
                    // Add admin button to create new surface
                    AdminMenu.Instance.AddButton("Add surface", CallbackAddSurface, AdminMenu.Panels.Obstacles);
                }


                public void CallbackAddSurface()
                {
                    InteractionSurface surface = Assistances.Factory.Instance.CreateInteractionSurface("Surface " + Surfaces.Count, AdminMenu.Panels.Obstacles, new Vector3(0.4f, 0.2f, 0.4f), Utilities.Materials.Colors.PurpleGlowing, true, true, Utilities.Utility.GetEventHandlerEmpty(), transform);
                    Surfaces.Add(surface);

                    // Increasing the boxcollider in y to have a margin to detect if the object is close to the surface
                    BoxCollider collider = surface.GetInteractionSurface().GetComponent<BoxCollider>();
                    collider.center = new Vector3(0, 1, 0);
                    collider.size = new Vector3(1, 3, 1);
                }

                /**
                 * The object must contain an ObjectManipulator component. If not present, the object is not registered and false is returned.
                 * The callback is called when the object has finished moving and crosses one of the interaction surface.
                 */
                public bool RegisterObject(GameObject gameObject, EventHandler callback)
                {
                    bool toReturn = false;

                    ObjectManipulator objectManipulator = gameObject.GetComponent<ObjectManipulator>();

                    if (objectManipulator != null)
                    {
                        toReturn = true;

                        objectManipulator.OnManipulationEnded.AddListener(delegate
                        {
                            IsObjectInteractingWithSurface(gameObject);
                        });

                        Objects.Add(gameObject, callback);
                    }

                    return toReturn;
                }

                void IsObjectInteractingWithSurface(GameObject gameObject)
                {
                    //bool toReturn = false;

                    foreach(InteractionSurface surface in Surfaces)
                    {
                        //Vector3.Distance(positionDetected, )
                        //surface.GetD
                        BoxCollider collider = surface.GetInteractionSurface().gameObject.GetComponent<BoxCollider>();

                        // If the point if contained by the interaction surface, then returns true
                        if (collider.bounds.Contains(gameObject.transform.position))
                        {
                            //toReturn = true;
                            Objects[gameObject]?.Invoke(gameObject, EventArgs.Empty);
                        }
                    }
                    

                    //return toReturn;
                }

                public bool IsObjectInteractingWithSurface(Vector3 positionToEvaluate)
                {
                    bool toReturn = false;

                    foreach (InteractionSurface surface in Surfaces)
                    {
                        BoxCollider collider = surface.GetInteractionSurface().gameObject.GetComponent<BoxCollider>();

                        // If the point if contained by the interaction surface, then returns true
                        if (collider.bounds.Contains(positionToEvaluate))
                        {
                            toReturn = true;
                        }
                    }

                    return toReturn;
                }
            }
        }
    }
}
