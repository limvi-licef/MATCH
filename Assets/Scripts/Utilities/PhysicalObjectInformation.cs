/*Copyright 2022 Louis Marquet

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
        /**
         * Container to host the details about a detected object
         * */
        public class PhysicalObjectInformation
        {
            string Name; //object name
            Vector3 Coord; //coordinates
            Vector3 FirstCorner; //boundingboxcorner
            Vector3 SecondCorner; //boundingboxcorner

            public string GetObjectName()
            {
                return Name;
            }

            public void SetObjectParams(string name, Vector3 center, Vector3 firstBoundary, Vector3 secondBoundary)
            {
                Name = name;
                Coord = center;
                FirstCorner = firstBoundary;
                SecondCorner = secondBoundary;
            }

            public Vector3 GetCenter()
            {
                return Coord;
            }

            public Vector3 GetFirstBoundary()
            {
                return FirstCorner;
            }

            public Vector3 GetSecondBoundary()
            {
                return SecondCorner;
            }
        }
    }
}
