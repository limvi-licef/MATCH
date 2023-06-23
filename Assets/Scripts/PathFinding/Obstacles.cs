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
    namespace PathFinding
    {
        public class Obstacles : MonoBehaviour
        {
            List<GameObject> Cubes;

            public EventHandler EventResized;
            public EventHandler EventMoved;

            Utilities.ObjectPositioningStorage ObstaclePositioningStorage;

            private void Awake()
            {
                Cubes = new List<GameObject>();

                ObstaclePositioningStorage = new Utilities.ObjectPositioningStorage("ObstaclesStorage.txt");
                
            }

            public void Start()
            {
                List<String> registeredObjectsIds = ObstaclePositioningStorage.GetObjetsRegisteredNames();
                foreach (string id in registeredObjectsIds)
                {
                    Utilities.ObjectPositioningStorage.ObjectsInformation objectsInformation = ObstaclePositioningStorage.GetRegisteredObjectInformation(id);

                    AddObstacle(id, objectsInformation.Scale, objectsInformation.Position,  Utilities.Materials.Colors.WhiteTransparent, true, false, true, transform);
                }
            }

            public void AddObstacle(string name, Vector3 scaling, Vector3 position, string color, bool navMeshTag, bool callbackOnTouch, bool registerObject, Transform parent)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

                // Set parent
                cube.transform.parent = parent;

                // Add buttons to interface
                AdminMenu.Instance.AddButton("Obstacle " + name + " - Bring", delegate ()
                {
                    MATCH.Utilities.Utility.BringObject(cube.transform);
                }, AdminMenu.Panels.Left);
                AdminMenu.Instance.AddSwitchButton("Obstacle " + name + " - Hide", delegate ()
                {
                    MATCH.Utilities.Utility.ShowInteractionSurface(cube.transform, !cube.GetComponent<Renderer>().enabled);
                }, AdminMenu.Panels.Left, AdminMenu.ButtonType.Hide);

                // Set color
                MATCH.Utilities.Utility.SetColor(cube.transform.transform, color);

                // Set scaling and position
                cube.transform.position = position;
                cube.transform.localScale = scaling;

                // Set the manipulation features
                ObjectManipulator objectManipulator = cube.AddComponent<ObjectManipulator>();
                cube.AddComponent<RotationAxisConstraint>().ConstraintOnRotation = Microsoft.MixedReality.Toolkit.Utilities.AxisFlags.XAxis | Microsoft.MixedReality.Toolkit.Utilities.AxisFlags.ZAxis;
                BoundsControl boundsControl = cube.AddComponent<BoundsControl>();
                boundsControl.ScaleHandlesConfig.ScaleBehavior = Microsoft.MixedReality.Toolkit.UI.BoundsControlTypes.HandleScaleMode.NonUniform;
                boundsControl.TranslationHandlesConfig.ShowHandleForX = true;
                boundsControl.TranslationHandlesConfig.ShowHandleForY = true;
                boundsControl.TranslationHandlesConfig.ShowHandleForZ = true;

                // Set optional features
                if (navMeshTag)
                {
                    cube.AddComponent<NavMeshSourceTag>();
                }

                if (callbackOnTouch)
                { // As we will be adding the MouseAssistanceBasic, it requires to encapsulate the cube in an empty gameobject, and to rename the cube "Child"
                    GameObject child = cube;
                    child.name = "Child";
                    cube = new GameObject(name);
                    child.transform.parent = cube.transform;

                    cube.AddComponent<MATCH.Assistances.Basic>();
                }

                // Add the callbacks
                boundsControl.ScaleStopped.AddListener(delegate
                {
                    EventResized?.Invoke(cube, EventArgs.Empty);
                });

                objectManipulator.OnManipulationEnded.AddListener(delegate (ManipulationEventData data)
                {
                    EventMoved?.Invoke(cube, EventArgs.Empty);
                });

                Cubes.Add(cube);

                if (registerObject)
                {
                    ObstaclePositioningStorage.RegisterObject(name, cube.transform, cube.transform);
                }
            }

            public List<GameObject> GetObstacles()
            {
                return Cubes;
            }
        }

    }
}

