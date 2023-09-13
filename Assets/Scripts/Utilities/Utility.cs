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
        static class Utility
        {
            public enum GreekAlphabet
            {
                Alpha = 0,
                Beta = 1,
                Gamma = 2,
                Delta = 3,
                Epsilon = 4,
                Zeta = 5,
                Eta = 6,
                Theta = 7,
                Iota = 8,
                Kappa = 9,
                Lambda = 10,
                Mu = 11,
                Nu = 12,
                Xi = 13,
                Omicron = 14,
                Pi = 15,
                Rho = 16,
                Sigma = 17,
                Tau = 18,
                Upsilon = 19,
                Phi = 20,
                Chi = 21,
                Psi = 22,
                Omega = 23
            }


            public struct Linear
            {
                public float a;
                public float b;
            }

            // Utilities functions: to be moved to a dedicated namespace later?
            public static void AddTouchCallback(Transform transform, UnityEngine.Events.UnityAction callback)
            {
                GameObject gameObject = transform.gameObject;

                Interactable interactable = gameObject.GetComponent<Interactable>();

                if (interactable == null)
                {
                    DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "No interactable component to the gameobject: adding one");

                    interactable = gameObject.AddComponent<Interactable>();
                }

                InteractableOnTouchReceiver receiver = interactable.GetReceiver<InteractableOnTouchReceiver>();

                if (receiver == null)
                {
                    DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "No touch receiver to the interactable gameobject: adding one");

                    receiver = interactable.AddReceiver<InteractableOnTouchReceiver>();
                }

                receiver.OnTouchStart.AddListener(callback);
            }

            public static Transform FindChild(GameObject parent, string childId)
            {
                return parent.transform.Find(childId);
            }

            public static EventHandler GetEventHandlerEmpty()
            {
                return delegate
                {

                };
            }

            public static EventHandler GetEventHandlerWithDebugMessage(string debugMessage)
            {
                return new EventHandler(delegate (System.Object o, EventArgs e)
                {
                    DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, debugMessage);
                });
            }

            public static void AdjustObjectHeightToHeadHeight(Transform t, float originalLocalHeightPos = 0.0f)
            {
                t.position = new Vector3(t.position.x, Camera.main.transform.position.y + originalLocalHeightPos - 0.2f, t.position.z); // The -0.2 is to be more aligned with the hololens.
            }

            // This function keeps its local position relative to the new parent. I.e. if the current object local position is (0,0,0), it will remain (0,0,0) for the new parent
            public static void setParentToObject(Transform o, Transform p)
            {
                Vector3 localPos = o.localPosition;

                //o.parent = p;
                o.SetParent(p);
                o.localPosition = localPos;
            }

            /*
             * Add the animate component to the object, animate the object, and then destroy the component.
             * */
            public static void AnimateDisappearInPlace(GameObject gameObject, Vector3 scalingOriginal, EventHandler eventHandler)
            {
                Utilities.EventHandlerArgs.Animation args = new EventHandlerArgs.Animation();

                if (gameObject.activeSelf)
                {
                    gameObject.AddComponent<Animation>().AnimateDiseappearInPlace(new EventHandler(delegate (System.Object o, EventArgs e)
                    {
                        UnityEngine.Object.Destroy(gameObject.GetComponent<Animation>());

                        gameObject.SetActive(false);
                        gameObject.transform.localScale = scalingOriginal;

                        args.Success = true;
                        eventHandler?.Invoke(gameObject, args);
                    }));
                }
                else
                {
                    args.Success = false;
                    eventHandler?.Invoke(gameObject, args);
                }
            }
            public static void AnimateDisappearInPlace(GameObject gameObject, Vector3 scalingOriginal)
            {
                AnimateDisappearInPlace(gameObject, scalingOriginal, GetEventHandlerEmpty());
            }

            public static void AnimateAppearInPlace(GameObject gameObject, EventHandler eventHandler)
            {
                gameObject.SetActive(true);
                gameObject.AddComponent<MATCH.Utilities.Animation>().AnimateAppearInPlace(new EventHandler(delegate (System.Object o, EventArgs e)
                {
                    UnityEngine.Object.Destroy(gameObject.GetComponent<Animation>());

                    eventHandler?.Invoke(gameObject, EventArgs.Empty);
                }));
            }
            public static void AnimateAppearInPlace(GameObject gameObject)
            {
                AnimateAppearInPlace(gameObject, GetEventHandlerEmpty());
            }

            public static void AnimateAppearInPlace(GameObject gameObject, Vector3 scaling, EventHandler eventHandler)
            {
                gameObject.SetActive(true);

                Animation animator = gameObject.AddComponent<Animation>();

                animator.Scalingstep.x = scaling.x / 50.0f;
                animator.Scalingstep.y = scaling.y / 50.0f;
                animator.Scalingstep.z = scaling.z / 50.0f;

                animator.AnimateAppearInPlaceToScaling(scaling, delegate
                {
                    UnityEngine.Object.Destroy(gameObject.GetComponent<Animation>());

                    eventHandler?.Invoke(gameObject, EventArgs.Empty);
                });
            }
            public static void AnimateAppearInPlace(GameObject gameObject, Vector3 scaling)
            {
                AnimateAppearInPlace(gameObject, scaling, GetEventHandlerEmpty());
            }

            public static void BringObject(Transform t)
            {
                t.position = new Vector3(Camera.main.transform.position.x + 0.5f, Camera.main.transform.position.y, Camera.main.transform.position.z);
                t.LookAt(Camera.main.transform);
                t.Rotate(new Vector3(0, 1, 0), 180);
            }

            public static void ShowInteractionSurface(Transform gameobject, bool show)
            {
                gameobject.GetComponent<Renderer>().enabled = show; // To hide the surface while keeping it interactable, then the renderer is disabled if show==false;
                gameobject.GetComponent<BoundsControl>().enabled = show;
                gameobject.GetComponent<ObjectManipulator>().enabled = show;
            }

            public static void SetColor(Transform gameobject, string colorName)
            {
                Renderer r = gameobject.GetComponent<Renderer>();
                r.material = Resources.Load(colorName, typeof(Material)) as Material;
            }

            public static Material LoadMaterial(string name)
            {
                return Resources.Load(name, typeof(Material)) as Material;
            }

            /**
             * Convert a BitArray to int.
             * Be careful: the least significant bit is the first element of the array
             * */
            public static int ConvertBitArrayToInt(BitArray array)
            {
                int[] arrayInt = new int[1];
                array.CopyTo(arrayInt, 0);

                return arrayInt[0];
            }

            // From https://forum.unity.com/threads/how-to-know-if-a-script-is-running-inside-unity-editor-when-using-device-simulator.921827/
            public static bool IsEditorSimulator()
            {
#if UNITY_EDITOR
                return !Application.isEditor;
#else
                return false;
#endif

            }
            public static bool IsEditorGameView()
            {
#if UNITY_EDITOR
                return Application.isEditor;
#else
                return false;
#endif

            }

            public static List<Vector3> CalculateBezierCurve(Vector3 startPoint, Vector3 endPoint, Vector3 cornerPoint)
            {
                List<Vector3> points = new List<Vector3>();
                
                float t = 0.0f;
                int nbPoints = 1000;

                for (int i = 0; i < nbPoints; i++)
                {
                    t = (float)i / (float)nbPoints;

                    points.Add((1.0f - t) * (1.0f - t) * startPoint + 2 * (1 - t) * t * cornerPoint + t * t * endPoint);
                }

                return points;

            }

            public static Linear CalculateLinearCoefficients (float x1, float y1, float x2, float y2)
            {
                Linear toReturn;

                toReturn.a = (y2 - y1) / (x2 - x1);
                toReturn.b = y1 - toReturn.a * x1;

                return toReturn;
            }

            public static float CalculateMinDistanceOfALine(LineRenderer line)
            {
                float dMin = 10000;
                float d = -1;
                Vector3 corner;

                Vector3 cameraPos = Camera.main.transform.position;

                for (int i = 0; i < line.positionCount; i++)
                {
                    corner = line.GetPosition(i);

                    d = Utilities.Utility.CalculateDistancePoints(cameraPos, corner);

                    if (d < dMin)
                    {
                        dMin = d;
                    }
                }

                return dMin;
            }

            public static float CalculateDistancePoints(Vector3 start, Vector3 end)
            {
                float toReturn = Mathf.Sqrt(Mathf.Pow(end.x - start.x, 2) + Mathf.Pow(end.y - start.y, 2) + Mathf.Pow(end.z - start.z, 2));

                return toReturn;
            }

            /**
             * Returns in degree
             */
            public static float CalculateAngleInTriangle(Vector2 trianglePointA, Vector2 trianglePointB, Vector2 pointToFindAngle)
            {
                return Mathf.Rad2Deg * Mathf.Acos((Mathf.Pow(Vector2.Distance(trianglePointA, trianglePointB), 2) - Mathf.Pow(Vector2.Distance(pointToFindAngle, trianglePointA), 2) - Mathf.Pow(Vector2.Distance(pointToFindAngle, trianglePointB), 2)) / (-2 * Vector2.Distance(pointToFindAngle, trianglePointA) * Vector2.Distance(pointToFindAngle, trianglePointB)));
            }

            public static bool IsPointInTriangle(Vector2 trianglePointA, Vector2 trianglePointB, Vector2 trianglePointC, Vector2 pointToFindInTriangle)
            {
                bool toReturn = false;

                float angleAB = CalculateAngleInTriangle(trianglePointA, trianglePointB, pointToFindInTriangle);
                float angleAC = CalculateAngleInTriangle(trianglePointA, trianglePointC, pointToFindInTriangle);
                float angleBC = CalculateAngleInTriangle(trianglePointB, trianglePointC, pointToFindInTriangle);

                float totalAngles = angleAB + angleAC + angleBC;

                if ( Mathf.RoundToInt(totalAngles) == 360)
                {
                    toReturn = true;
                }

                return toReturn;
            }
        }
    }
}