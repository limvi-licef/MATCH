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
            public static class Prefabs
            {
                static string Path = "MATCH/";

                public static string DecoratorMaterial = Path + "Assistances/Decorators/Material";
                public static string DecoratorBackgroundColor = Path + "Assistances/Decorators/BackgroundColor";
                public static string DecoratorBackgroundColor2 = Path + "Assistances/Decorators/BackgroundColor2";
                public static string DecoratorEdge = Path + "Assistances/Decorators/Edge";

                public static string AssistanceGradationAttention = Path + "Assistances/Gradation/Attention";
                public static string AssistanceGradationExplicit = Path + "Assistances/Gradation/Explicit";
                public static string AssistanceInteractionSurface = Path + "Assistances/InteractionSurface";
                public static string AssistanceSurfaceToProcess = Path + "Assistances/SurfaceToProcess";
                public static string AssistanceArch = Path + "Assistances/Arch";
                public static string AssistanceCube = Path + "Assistances/Cube";
                public static string AssistanceLightPath = Path + "Assistances/LightedPath";
                public static string AssistanceDialog = Path + "Assistances/Dialogs/Dialog";
                public static string AssistanceDialogButtons = Path + "Assistances/Dialogs/Buttons";
                public static string AssistanceDialogToDoList = Path + "Assistances/Dialogs/ToDoList";
                public static string AssistanceDialogCheckList = Path + "Assistances/Dialogs/CheckList";
                public static string AssistanceExclamationMark = Path + "Assistances/ExclamationMark";
                public static string AssistanceDialog2 = Path + "Assistances/Dialogs/Dialog2";

                public static string InferenceManager = Path + "Inferences/Manager";

                public static string Button = Path + "Assistances/Buttons/Button";
                public static string ButtonSwitch = Path + "Assistances/Buttons/ButtonSwitch";
                public static string ButtonIcon = Path + "Assistances/Buttons/ButtonIcon";

                public static Transform Load(string prefabPath, Transform parent = null)
                {
                    Transform toReturn;

                    if (parent != null)
                    {
                        toReturn = UnityEngine.Object.Instantiate<Transform>(Resources.Load<Transform>(prefabPath), parent);
                    }
                    else
                    {
                        toReturn = UnityEngine.Object.Instantiate<Transform>(Resources.Load<Transform>(prefabPath));
                    }

                    return toReturn;
                }
            } 
        }
    }
}
