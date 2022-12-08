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
    namespace Inferences
    {
        public class Factory : MonoBehaviour
        {
            private static Factory InstanceInternal;
            public static Factory Instance { get { return InstanceInternal; } }

            private void Awake()
            {
                if (InstanceInternal != null && InstanceInternal != this)
                {
                    Destroy(this);
                }
                else
                {
                    InstanceInternal = this;
                }
            }

            public MATCH.Inferences.Manager CreateManager(Transform parent)
            {
                Transform view = MATCH.Utilities.Materials.Prefabs.Load(MATCH.Utilities.Materials.Prefabs.InferenceManager);
                view.parent = parent;

                MATCH.Inferences.Manager controller = view.GetComponent<MATCH.Inferences.Manager>();

                return controller;
            }

            public void CreateDistanceLeavingInferenceOneShot(MATCH.Inferences.Manager inferenceManager, string inferenceId, EventHandler toTrigger, GameObject refObject, float trigerringDistance = 2.0f)
            {
                MATCH.Inferences.DistanceLeaving inference = new MATCH.Inferences.DistanceLeaving(inferenceId, delegate (System.Object o, EventArgs e)
                {
                    inferenceManager.UnregisterInference(inferenceId);
                    toTrigger?.Invoke(o, e);
                }, refObject, trigerringDistance);
                inferenceManager.RegisterInference(inference);
            }

            public void CreateDistanceComingInferenceOneShot(MATCH.Inferences.Manager inferenceManager, string inferenceId, EventHandler toTrigger, GameObject refObject, float trigerringDistance = 1.5f)
            {
                MATCH.Inferences.DistanceComing inference = new MATCH.Inferences.DistanceComing(inferenceId, delegate (System.Object o, EventArgs e)
                {
                    inferenceManager.UnregisterInference(inferenceId);
                    toTrigger?.Invoke(o, e);
                }, refObject, trigerringDistance);
                inferenceManager.RegisterInference(inference);
            }

            /**
             * This inference creates 2 nested inferences: the first is trigerred when the user comes close to the object. It DOES NOT trigger the provided EventHandler yet. Instead, it creates a new inference trigerred if the user leave the place where the object is displayed. And here the EventHandler is triggered.
             * The reason to implement those inferences this way is that in case we have several assistances in a row that can trigger if the user is at a certain distance, then they will all trigger at once. With this way of doing, the next inference will be triggered only if the user first come closer and then leaves again
             * */
            public void CreateDistanceComingAndLeavingInferenceOneShot(MATCH.Inferences.Manager inferenceManager, string inferenceId, EventHandler toTrigger, GameObject refObject, float trigerringDistanceComing = 1.5f, float trigerringDistanceLeaving = 2.0f)
            {
                CreateDistanceComingInferenceOneShot(inferenceManager, inferenceId, delegate (System.Object o, EventArgs e)
                {
                    CreateDistanceLeavingInferenceOneShot(inferenceManager, inferenceId + "Internal", toTrigger, refObject, trigerringDistanceLeaving);
                }, refObject, trigerringDistanceComing);
            }
            public void CreateDistanceLeavingAndComingInferenceOneShot(MATCH.Inferences.Manager inferenceManager, string inferenceId, EventHandler toTrigger, GameObject refObject, float trigerringDistanceComing = 1.5f, float trigerringDistanceLeaving = 2.0f)
            {
                CreateDistanceLeavingInferenceOneShot(inferenceManager, inferenceId, delegate (System.Object o, EventArgs e)
                {
                    CreateDistanceComingInferenceOneShot(inferenceManager, inferenceId + "Internal", toTrigger, refObject, trigerringDistanceComing);
                }, refObject, trigerringDistanceLeaving);
            }

            public void CreateTemporalInferenceOneShot(MATCH.Inferences.Manager inferenceManager, string inferenceId, EventHandler toTrigger, int hour)
            {
                DateTime tempTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, 0, 0);
                MATCH.Inferences.Time inference = new MATCH.Inferences.Time(inferenceId, tempTime, delegate (System.Object o, EventArgs e)
                {
                    inferenceManager.UnregisterInference(inferenceId);
                    toTrigger?.Invoke(o, e);
                });
                inferenceManager.RegisterInference(inference);
            }
        }

    }
}
