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

namespace MATCH
{
    namespace Assistances
    {
        public class InteractionSurfaceFollower : MonoBehaviour
        {
            private static InteractionSurfaceFollower InstanceInternal;
            public static InteractionSurfaceFollower Instance { get { return InstanceInternal; } }

            private void Awake()
            {
                if (InstanceInternal != null && InstanceInternal != this)
                {
                    Destroy(this.gameObject);
                }
                else
                {
                    InstanceInternal = this;
                }
            }


            // Start is called before the first frame update
            void Start()
            {
                InteractionSurface controller = gameObject.GetComponent<InteractionSurface>();

                //controller.SetAdminButtons(id, panel);
                controller.SetScaling(new Vector3(0.2f, 0.05f, 0.1f));
                controller.SetColor(Utilities.Materials.Colors.GreenGlowing);
                controller.SetObjectResizable(false);
                //controller.EventConfigMoved += onMove;

                controller.ShowInteractionSurfaceTable(true);
                controller.ShowInteractionSurfaceTable(false);
            }

            // Update is called once per frame
            void Update()
            {

            }

            public InteractionSurface GetInteractionSurface()
            {
                return transform.GetComponent<InteractionSurface>();
            }
        }

    }
}
