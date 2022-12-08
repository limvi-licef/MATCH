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
using System.Timers;

/**
 * Static class containing various utilities functions
 * */
namespace MATCH
{
    namespace Utilities
    {
        namespace Materials
        {
            public static class Textures
            {
                static string PathUnified = "MATCH/Textures/Unified/";
                static string PathSplit = "MATCH/Textures/Split/";

                public static string Exclamation = PathUnified + "Exclamation";
                public static string ExclamationRed = PathUnified + "ExclamationRed";
                public static string Congratulation = PathUnified + "Congratulation";
                public static string HolographicBackPlate = PathUnified + "HolographicBackPlate";
                public static string HelpBottomVivid = PathSplit + "HelpBottomVivid";
                public static string HelpTopLeftVivid = PathSplit + "HelpTopLeftVivid";
                public static string HelpTopRightVivid = PathSplit + "HelpTopRightVivid";
                public static string Refuse = PathUnified + "Refuse";
                public static string ArrowProgressive = PathUnified + "ArrowProgressive";
                public static string Agree = PathUnified + "Agree";
                public static string Help = PathUnified + "Help";
                public static string Reminder = PathUnified + "Reminder";
                public static string Clean = PathUnified + "Clean";
                public static string CleanVivid = PathUnified + "CleanVivid";
                public static string GarbageLevel1 = PathUnified + "GarbageLevel1";
                public static string GarbageLevel2 = PathUnified + "GarbageLevel2";
                public static string GarbageLevel2Pressed = PathUnified + "GarbageLevel2Pressed";
                public static string Flower = PathUnified + "Flower";
                public static string FlowerPressed = PathUnified + "FlowerPressed";
                public static string CleanTable = PathUnified + "CleanTable";
                public static string CleanTablePressed = PathUnified + "CleanTablePressed";

            }
        }
    }
}