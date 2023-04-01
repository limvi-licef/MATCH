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
using UnityEngine.Localization;

/**
 * Static class containing various utilities functions
 * */
namespace MATCH
{
    namespace Utilities
    {
        namespace Materials
        {
            public static class Localization
            {
                public static string TutorialAlpha = "Tutorial_Alpha";

                public static String GetLocalizedString(string id)
                {
                    return UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetLocalizedString(id);
                }

                public static Material GetLocalizedMaterial(string id)
                {
return                     UnityEngine.Localization.Settings.LocalizationSettings.AssetDatabase.GetLocalizedAsset<Material>(id);
                }
            } 
        }
    }
}
