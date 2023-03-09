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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

/**
 * Material will be set when calling the "show" function
 * */
namespace MATCH
{
    namespace Assistances
    {
        namespace Decorators
        {
            public class Sound : Assistance, IAssistance
            {

                IAssistance AssistanceToDecorate;


                // Start is called before the first frame update
                void Start()
                {

                }

                // Update is called once per frame
                void Update()
                {

                }

                public void SetAssistanceToDecorate(IAssistance toDecorate)
                {
                    AssistanceToDecorate = toDecorate;

                    name = AssistanceToDecorate.GetAssistance().name + "_decoratorSound";

                }

                public override void Hide(EventHandler callback, bool withAnimation)// : base(callback,withAnimation)
                {
                   /* if (IsDisplayed)
                    {
                        AssistanceToDecorate.GetAssistance().Hide(delegate (System.Object o, EventArgs e)
                        {
                            IsDisplayed = false;

                            callback?.Invoke(o, e);
                        }, withAnimation);

                    }
                    else
                    {
                        Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();
                        args.Success = false;
                        callback?.Invoke(this, args);
                    }*/
                }

                public override void Show(EventHandler callback, bool withAnimation)//: base(callback,withAnimation)
                {
                    /*if (IsDisplayed == false)
                    {
                        IsDisplayed = true;

                        AssistanceToDecorate.GetAssistance().Show(delegate (System.Object o, EventArgs e)
                        {
                            /*AssistanceToDecorate.GetBackground().gameObject.SetActive(false);
                            BackgroundView.position = AssistanceToDecorate.GetBackground().position;
                            BackgroundView.gameObject.SetActive(true);*//*

                            callback?.Invoke(this, e);
                        }, withAnimation);
                        //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called with color "  + BackgroundColorDecorated); 
                    }
                    else
                    {
                        // Check first if the decorated assistance needs to be displayed
                        AssistanceToDecorate.GetAssistance().Show(delegate (System.Object o, EventArgs e)
                        {
                            Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();
                            //AssistanceToDecorate.GetBackground().gameObject.SetActive(false);
                            args.Success = false;
                            callback?.Invoke(this, args);
                        }, withAnimation);
                    }*/
                }

                public override void ShowHelp(bool show, EventHandler callback, bool withAnimation)
                {
                    AssistanceToDecorate.GetAssistance().ShowHelp(show, callback, withAnimation);
                }

                /*
                public void EnableWeavingHand(bool enable)
                {
                    AssistanceToDecorate.EnableWeavingHand(enable);
                }
                */

                public override Transform GetTransform()
                {
                    return AssistanceToDecorate.GetAssistance().GetTransform();
                }

                public Assistance GetAssistance()
                {
                    return this;
                }

                public Assistance GetDecoratedAssistance()
                {
                    return AssistanceToDecorate.GetDecoratedAssistance();
                }

                public override bool IsDecorator()
                {
                    return true;
                }
                /*
                public Transform GetBackground()
                {
                    return BackgroundView;
                }
                */
            }
        }
    }
}
