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

using System;
using UnityEngine;
using static UnityEngine.XR.OpenXR.Features.Interactions.HTCViveControllerProfile;

namespace MATCH.Assistances.GradationVisual
{
    public class PathWithTextAndHelp : Assistance
    {

        MATCH.Assistances.Dialogs.Dialog1 TextController;

        LineToObject LineController;


        public override Transform GetTransform()
        {
            throw new NotImplementedException();
        }

        public override void Hide(EventHandler callback, bool withAnimation = true)
        {
            throw new NotImplementedException();
        }

        public override bool IsDecorator()
        {
            throw new NotImplementedException();
        }

        public override void Show(EventHandler callback, bool withAnimation = true)
        {
            throw new NotImplementedException();
        }

        public override void ShowHelp(bool show, EventHandler callback, bool withAnimation = true)
        {
            throw new NotImplementedException();
        }

        public void SetDescription(string text, float fontSize = -1.0f)
        {
            TextController.SetDescription(text, fontSize);
        }

        public void SetPathStartAndEndPoint(Transform origin, Transform target)
        {
            /*LineController.m_hologramOrigin = origin.gameObject;
            LineController.m_hologramTarget = target.gameObject;*/

            LineController.PointOrigin = origin.position;
            LineController.PointEnd = target.position;
        }
    }
}