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
//using Newtonsoft.Json.Linq;

/**
 * Used to have a lighting object to point to a provided gameobject, from the same direction than the user's gaze
 * */
namespace MATCH
{
    namespace Utilities
    {
        namespace EventHandlerArgs
        {
           public class PhysicalObject : EventArgs
            {
                public PhysicalObjectInformation ObjectDetected;

                public PhysicalObject(PhysicalObjectInformation obj)
                {
                    ObjectDetected = obj;
                }
            }
        }
    }
}
