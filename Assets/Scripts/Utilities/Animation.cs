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
using System;
using System.Reflection;

/**
 * Generic component to handle animations. For now show and hide are supported, in place, to a given position and at a certain scaling.
 * */
namespace MATCH
{
    namespace Utilities
    {
        public class Animation : MonoBehaviour
        {

            public Vector3 PositionEnd;
            public Vector3 ScalingEnd;
            public float AnimationSpeed = 4.0f;
            public Vector3 Scalingstep;

            event EventHandler EventAnimationFinished;

            bool StartAnimationStatus = false;

            bool ScalingGrow;

            public enum ConditionStopAnimation
            {
                // Gérer les cas où l'animation doit être arrêtée par le scaling ou par la position
                OnPositioning = 0,
                OnScaling = 1
            }

            public ConditionStopAnimation TriggerStopAnimation = ConditionStopAnimation.OnPositioning;

            BitArray ScalingFinished;

            private void Awake()
            {
                // Initialization of the variables
                ScalingFinished = new BitArray(3);

                Scalingstep.x = 0.05f;
                Scalingstep.y = 0.05f;
                Scalingstep.z = 0.05f;
            }

            // Start is called before the first frame update
            void Start()
            {
                // To decide if the animation should scale up or down. By default, scaling down.
                if (gameObject.transform.localScale.x < ScalingEnd.x)
                {
                    ScalingGrow = true;
                }
                else
                {
                    ScalingGrow = false;
                }
            }

            // Update is called once per frame
            void Update()
            {
                if (StartAnimationStatus)
                {
                    float step = AnimationSpeed * Time.deltaTime;

                    if (Vector3.Distance(gameObject.transform.position, PositionEnd) > 0.001f)
                    {
                        gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, PositionEnd, step);
                    }

                    if (ScalingGrow && Utility.ConvertBitArrayToInt(ScalingFinished) < 7) // < 7 because if the value is 7, that means the three bits (corresponding to x, y, z respectively) are set to 1, i.e. scaling is finished in the 3 directions.
                    {
                        Vector3 newScaling = gameObject.transform.localScale; // current value by default, and changed below if necessary

                        for (int i = 0; i < 3; i++)
                        {
                            if (gameObject.transform.localScale[i] < ScalingEnd[i])
                            {
                                newScaling[i] = gameObject.transform.localScale[i] + Scalingstep[i];
                            }
                            else if (ScalingFinished[i] == false)
                            {
                                ScalingFinished[i] = true;
                            }
                        }

                        gameObject.transform.localScale = newScaling;
                    }
                    else if (ScalingGrow == false && Utility.ConvertBitArrayToInt(ScalingFinished) < 7)
                    {
                        Vector3 newScaling = gameObject.transform.localScale; // current value by default, and changed below if necessary

                        for (int i = 0; i < 3; i++)
                        {
                            if (gameObject.transform.localScale[i] < ScalingEnd[i])
                            {
                                newScaling[i] = gameObject.transform.localScale[i] - Scalingstep[i];
                            }
                            else if (ScalingFinished[i] == false)
                            {
                                ScalingFinished[i] = true;
                            }
                        }

                        gameObject.transform.localScale = newScaling;

                    }

                    if (TriggerStopAnimation == ConditionStopAnimation.OnPositioning)
                    {
                        if (Vector3.Distance(gameObject.transform.position, PositionEnd) < 0.001f)
                        {
                            // Animation is finished: trigger event
                            EventAnimationFinished?.Invoke(this, EventArgs.Empty);

                            StartAnimationStatus = false;
                        }
                    }
                    else if (TriggerStopAnimation == ConditionStopAnimation.OnScaling)
                    {
                        if (Utility.ConvertBitArrayToInt(ScalingFinished) == 7)
                        {
                            // Animation is finished: trigger event
                            EventAnimationFinished?.Invoke(this, EventArgs.Empty);

                            StartAnimationStatus = false;
                        }
                    }
                }
            }

            public void StartAnimation()
            {
                StartAnimationStatus = true;
            }

            public void AnimateDiseappearInPlace(EventHandler eventHandler)
            {
                EventHandler[] temp = new EventHandler[1];

                temp[0] = eventHandler;

                AnimateDiseappearInPlace(temp);
            }

            public void AnimateDiseappearInPlace(EventHandler[] eventHandlers)
            {
                PositionEnd = gameObject.transform.position;
                ScalingEnd = new Vector3(0f, 0f, 0f);
                TriggerStopAnimation = Animation.ConditionStopAnimation.OnScaling;

                foreach (EventHandler e in eventHandlers)
                {
                    EventAnimationFinished += e;
                }

                StartAnimation();
            }

            public void AnimateAppearInPlace(EventHandler e)
            {
                EventHandler[] eventHandlers = new EventHandler[] { e };

                AnimateAppearInPlace(eventHandlers);
            }

            public void AnimateAppearInPlace(EventHandler[] e)
            {
                AnimateAppearInPlaceToScaling(new Vector3(1.0f, 1.0f, 1.0f), e);
            }

            public void AnimateAppearInPlaceToScaling(Vector3 targetScaling, EventHandler eventHandler)
            {
                AnimateAppearInPlaceToScaling(targetScaling, new EventHandler[] { eventHandler });
            }

            public void AnimateAppearInPlaceToScaling(Vector3 targetScaling, EventHandler[] eventHandlers)
            {
                //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called for object " + gameObject.name);

                gameObject.transform.localScale = new Vector3(0, 0, 0);
                PositionEnd = gameObject.transform.position;
                ScalingEnd = targetScaling;
                TriggerStopAnimation = Animation.ConditionStopAnimation.OnScaling;
                foreach (EventHandler e in eventHandlers)
                {
                    EventAnimationFinished += e;
                }

                gameObject.SetActive(true);

                StartAnimation();
            }

            /**
             * In this function, the animation will start from a given position, and will bring it to its current position
             **/
            public void AnimateAppearFromPosition(Vector3 pos, EventHandler e)
            {
                PositionEnd = gameObject.transform.position;
                ScalingEnd = new Vector3(1.0f, 1.0f, 1.0f);
                TriggerStopAnimation = Animation.ConditionStopAnimation.OnScaling;
                EventAnimationFinished += e;

                gameObject.transform.position = pos; // Moving the object to the starting position
                gameObject.transform.localScale = new Vector3(0.0f, 0.0f, 0.0f); // Set scaling to 0 before starting

                gameObject.SetActive(true);

                StartAnimation();
            }

            /**
         * In this function, the animation will start from the object's current position, and will diseppear gradually to the target position given as input parameter
         **/
            public void AnimateDiseappearToPosition(Vector3 pos, EventHandler e)
            {
                EventHandler[] ee = new EventHandler[] { e };
                AnimateDiseappearToPosition(pos, ee);
            }

            public void AnimateDiseappearToPosition(Vector3 pos, EventHandler[] eventHandlers)
            {
                PositionEnd = pos;
                ScalingEnd = new Vector3(0f, 0f, 0f);
                TriggerStopAnimation = Animation.ConditionStopAnimation.OnScaling;

                foreach (EventHandler e in eventHandlers)
                {
                    EventAnimationFinished += e;
                }

                StartAnimation();
            }

            /**
            * In this function, the animation will start from the current position, and will bring it to the given position
            **/
            public void AnimateMoveToPosition(Vector3 posDest, EventHandler e)
            {
                PositionEnd = posDest;
                ScalingEnd = gameObject.transform.localScale;
                TriggerStopAnimation = Animation.ConditionStopAnimation.OnPositioning;
                EventAnimationFinished += e;

                gameObject.SetActive(true);

                StartAnimation();
            }
        }

    }
}
