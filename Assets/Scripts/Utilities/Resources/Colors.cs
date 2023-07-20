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
            public static class Colors
            {
                public static string Path = "MATCH/Colors/";

                public static string Cyan = Path + "Cyan";
                public static string CyanGlowing = Path+"CyanGlowing";
                public static string RedGlowing = Path + "RedGlowing";
                public static string PurpleGlowing = Path + "PurpleGlowing";
                public static string OrangeGlowing = Path + "OrangeGlowing";
                public static string Orange = Path + "Orange";
                public static string GreenGlowing = Path + "GreenGlowing";
                public static string YellowGlowing = Path + "YellowGlowing";
                public static string WhiteTransparent = Path + "WhiteTransparent";
                public static string WhiteMetallic = Path + "WhiteMetallic";
                public static string Red = Path + "Red";
                public static string Red2 = Path + "Red2";
                public static string HolographicBackPlate = Path + "HolographicBackPlate";
                public static string Green = Path + "Green";
                public static string Yellow = Path + "Yellow";
                public static string Pink = Path + "Pink";
                public static string Purple = Path + "Purple";
                public static string White = Path + "White";
                public static string Blue = Path + "Blue";
            }
        }
    }
}
