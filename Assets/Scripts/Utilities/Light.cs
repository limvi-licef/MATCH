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

/**
 * Used to have a lighting object to point to a provided gameobject, from the same direction than the user's gaze
 * */
namespace MATCH
{
    namespace Utilities
    {
        public class Light : MonoBehaviour
        {
            public bool m_followUser = false;
            public GameObject m_hologramToLookAt;

            // Start is called before the first frame update
            void Start()
            {

            }

            // Update is called once per frame
            void Update()
            {
                if (m_followUser)
                {
                    Vector3 cameraPosition = Camera.main.transform.position;
                    Vector3 cubePosition = m_hologramToLookAt.transform.parent.position;

                    Vector3 direction = (cubePosition - cameraPosition).normalized;

                    Vector3 positionFinal = cubePosition - direction * 1.5f;

                    gameObject.transform.position = positionFinal;
                    gameObject.transform.LookAt(m_hologramToLookAt.transform.parent.position);
                }
            }
        }

    }
}
