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
    namespace Assistances
    {
        namespace Decorators
        {
            public class Sound : Assistance, IAssistance
            {
                IAssistance PanelToDecorate;
                AudioSource audioSource;
                AudioClip audioClip;
                float timeBetweenShots = 0.75f;
                float timer;

                //Transform SoundTransform;


                private void Awake()
                {
                    //SoundTransform = transform;
                }

                public void Start()
                {
                    
                }

                public void Update()
                {
                    timer += Time.deltaTime;
                    if (timer > timeBetweenShots && audioClip != null)
                    {
                        audioSource.PlayOneShot(audioClip);
                        timer = 0;
                    }
                }
             

                public void SetAssistanceToDecorate(IAssistance toDecorate, string soundPath, float timeBetweenSoundShots)
                {
                    PanelToDecorate = toDecorate;
                    
                    timeBetweenShots = timeBetweenSoundShots;

                    //this.soundToPlay = soundPath;

                    name = PanelToDecorate.GetAssistance().name + "_decoratorSound";
                    
                    transform.parent = PanelToDecorate.GetDecoratedAssistance().GetTransform();
                    //transform.parent = PanelToDecorate.GetSound();
                    transform.localPosition = PanelToDecorate.GetDecoratedAssistance().GetTransform().localPosition;

                    //initialiaze the sound
                    audioSource = GetComponent<AudioSource>();
                    audioClip = MATCH.Utilities.Materials.Sounds.Load(soundPath);


                    Assistance temp = PanelToDecorate.GetDecoratedAssistance();
                    temp.EventHelpButtonClicked += delegate (System.Object o, EventArgs e)
                    {
                        MATCH.Utilities.EventHandlerArgs.Button args = (MATCH.Utilities.EventHandlerArgs.Button)e;
                        OnHelpButtonClicked(args.ButtonType);
                    };
                }

                public override void Hide(EventHandler callback, bool withAnimation)
                {
                    if (IsDisplayed)
                    {
                        PanelToDecorate.GetAssistance().Hide(delegate (System.Object o, EventArgs e)
                        {
                            //PanelToDecorate.GetDecoratedAssistance().Show(Utilities.Utility.GetEventHandlerEmpty(), false);
                            //SoundTransform.gameObject.SetActive(false);
                            //audioSource.Stop();
                            DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Sound stopped ");
                            GetSound().gameObject.SetActive(false);

                            IsDisplayed = false;

                            callback?.Invoke(o, e);
                        }, withAnimation);

                    }
                    else
                    {
                        Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();
                        args.Success = false;
                        callback?.Invoke(this, args);
                    }
                }

                public override void Show(EventHandler callback, bool withAnimation)
                {
                    //SoundTransform.position = PanelToDecorate.GetSound().position;
                    //SoundTransform.localScale = PanelToDecorate.GetSound().localScale;

                    //SoundTransform.gameObject.SetActive(true);

                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "TEST100");
                    if (IsDisplayed == false)
                    {
                        IsDisplayed = true;
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "TEST101");

                        PanelToDecorate.GetAssistance().Show(delegate (System.Object o, EventArgs e)
                        {
                            //audioSource = GetComponent<AudioSource>();
                            //audioClip = MATCH.Utilities.Materials.Sounds.Load(MATCH.Utilities.Materials.Sounds.Debug);
                            PanelToDecorate.GetDecoratedAssistance().Show(Utilities.Utility.GetEventHandlerEmpty(), false);
                            PanelToDecorate.GetSound().gameObject.SetActive(false); //le transform des panels décorés n'est plus visible

                            DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Sound played ");


                            

                            callback?.Invoke(this, e);
                        }, withAnimation);
                    }
                    else
                    {
                        // Check first if the decorated assistance needs to be displayed
                        PanelToDecorate.GetDecoratedAssistance().Show(delegate (System.Object o, EventArgs e)
                        {
                            Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();

                            PanelToDecorate.GetSound().gameObject.SetActive(false); //le transform des panels décorés n'est plus visible
                            DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Sound ??? ");

                            //PanelToDecorate.GetBackground().gameObject.SetActive(false);
                            ///////PanelToDecorate.GetBackgroundMessage().gameObject.SetActive(false);
                            //PanelToDecorate.GetBackgroundIcon().gameObject.SetActive(false);
                            args.Success = false;
                            callback?.Invoke(this, args);
                        }, withAnimation);
                    }
                }
                
                
                public override void ShowHelp(bool show, EventHandler callback, bool withAnimation)
                {
                    PanelToDecorate.GetDecoratedAssistance().ShowHelp(show, callback, withAnimation);
                }
                /*
                public void EnableWeavingHand(bool enable)
                {
                    PanelToDecorate.EnableWeavingHand(enable);
                }*/

                public override Transform GetTransform()
                {
                    return PanelToDecorate.GetDecoratedAssistance().GetTransform();
                }

                public Assistance GetDecoratedAssistance()
                {
                    return PanelToDecorate.GetAssistance();//.GetDecoratedAssistance();//.GetDecoratedAssistance();
                }

                public Assistance GetAssistance()
                {
                    return this;//PanelToDecorate.GetAssistance();
                }
                
                public override bool IsDecorator()
                {
                    return true;
                }
                    
                public Transform GetSound()
                {
                    return transform;// SoundTransform;
                }

                /*
                public Transform GetBackground()
                {
                    return PanelToDecorate.GetBackground();
                }

                public Transform GetBackgroundIcon()
                {
                    return PanelToDecorate.GetBackgroundIcon();
                }

                public Transform GetBackgroundMessage()
                {
                    return PanelToDecorate.GetBackgroundMessage();
                }*/
            }
        }
    }
}



