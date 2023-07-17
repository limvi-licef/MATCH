/*Copyright 2022 Guillaume Spalla, Louis Marquet, Léri Lamour

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

using System.Collections.Generic;
using UnityEngine;
using System.Timers;
using System.Reflection;
using TMPro;
using System.Linq;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System;

namespace MATCH
{
    namespace Assistances
    {
        namespace Dialogs
        {
            public class Dialog2Contextualized : Dialog2, IPanel2
            {
                /*public enum Context
                {
                    LocationRelativeToTheUser = 0
                }*/

                Transform ContextObject;

                protected override void Awake()
                {
                    base.Awake();
                }

                public void SetDescription(string text, Transform contextObject, float fontSize = -1.0f)
                {
                    base.SetDescription(text, fontSize);

                    ContextObject = contextObject;
                }

                public override void Show(EventHandler eventHandler, bool withAnimation)
                {
                    /*Vector3 userPos = Camera.main.transform.position;
                    Vector2 pointA = new Vector2(userPos.x - 10, userPos.z + 10);
                    Vector2 pointB = new Vector2(userPos.x + 10, userPos.z + 10);
                    Vector2 pointC = new Vector2(userPos.x - 10, userPos.z - 10);
                    Vector2 pointD = new Vector2(userPos.x + 10, userPos.z - 10);
                    Vector2 pointU = new Vector2(userPos.x, userPos.z); // "U" for user, i.e. user's position*/

                    Vector3 userPos = Camera.main.transform.position;
                    Vector3 userDirection = Camera.main.transform.forward;
                    Vector3 directionDefault = new Vector3(0, 0, 1);
                    Vector3 dir = userDirection - directionDefault;
                    Quaternion rotation = Quaternion.Euler(dir.x, 0, dir.z);

                    float angle = Vector3.SignedAngle(directionDefault, userDirection, new Vector3(0, 0, 1));

                    Vector2 pointA =  new Vector2(userPos.x - 10, userPos.z + 10);
                    Vector2 pointB = new Vector2(userPos.x + 10, userPos.z + 10);
                    Vector2 pointC = new Vector2(userPos.x - 10, userPos.z - 10);
                    Vector2 pointD = new Vector2(userPos.x + 10, userPos.z - 10);
                    Vector2 pointU = new Vector2(userPos.x, userPos.z); // "U" for user, i.e. user's position

                    Vector2 pointAR = Quaternion.AngleAxis(angle, new Vector3(0, 0, 1)) * pointA;
                    Vector2 pointBR = Quaternion.AngleAxis(angle, new Vector3(0, 0, 1)) * pointB;
                    Vector2 pointCR = Quaternion.AngleAxis(angle, new Vector3(0, 0, 1)) * pointC;
                    Vector2 pointDR = Quaternion.AngleAxis(angle, new Vector3(0, 0, 1)) * pointD;

                    Vector2 pointToFind = new Vector2(ContextObject.transform.position.x, ContextObject.transform.position.z);

                    string toAdd = "<Not initialized>";

                    if (Utilities.Utility.IsPointInTriangle(pointAR, pointBR, pointU, pointToFind))
                    {
                        toAdd = "devant vous";
                    }
                    else if (Utilities.Utility.IsPointInTriangle(pointAR, pointCR, pointU, pointToFind))
                    {
                        toAdd = "sur votre gauche";
                    }
                    else if (Utilities.Utility.IsPointInTriangle(pointBR, pointDR, pointU, pointToFind))
                    {
                        toAdd = "sur votre droite";
                    }
                    else if (Utilities.Utility.IsPointInTriangle(pointDR, pointCR, pointU, pointToFind))
                    {
                        toAdd = "derrière vous";
                    }

                    string originalDescription = GetDescription();
                    SetDescription(originalDescription.Replace("<Location>", toAdd));

                    base.Show(eventHandler, withAnimation);
                }
            }
        }
    }
}