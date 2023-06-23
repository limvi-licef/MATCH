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
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;

namespace MATCH
{
    namespace Utilities
    {
        public class WorldLockingToolsManager : MonoBehaviour
        {
            private static WorldLockingToolsManager InstanceInternal;

            public static WorldLockingToolsManager Instance { get { return InstanceInternal; } }

            public MATCH.Assistances.Dialogs.Dialog1 DebugPositioner;

            private ObjectPositioningStorage PositioningStorage;

            private void Awake()
            {
                if (InstanceInternal != null && InstanceInternal != this)
                {
                    Destroy(gameObject);
                }
                else
                {
                    // Loading file content
                    PositioningStorage = new ObjectPositioningStorage("ObjectsPositions.txt");

                    InstanceInternal = this;
                }
            }

            /**
             * In some situation, we might want the user to interact with a certain objet (objectToInteract) but register its parent (objectToRegister)
             */
            public void RegisterObject(string id, Transform objectToRegister, Transform objectToInteract)
            {
                DebugPositioner.SetDescription("Positioner - RegisterObject - Called", 0.2f);

                string forDebug = "Registering new object:\n" + id + "\n" + objectToRegister.gameObject.transform.position.ToString();

                PositioningStorage.RegisterObject(id, objectToRegister, objectToInteract);
              
                DebugPositioner.SetDescription(forDebug, 0.2f);
            }
        }
    }
}


