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
                String audioPath;
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

                    this.audioPath = soundPath;

                    name = PanelToDecorate.GetAssistance().name + "_decoratorSound";
                    
                    //transform.parent = PanelToDecorate.GetRootDecoratedAssistance().GetTransform();
                    transform.localPosition = PanelToDecorate.GetRootDecoratedAssistance().GetTransform().localPosition;

                    Assistance temp = PanelToDecorate.GetRootDecoratedAssistance();
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
                    if (IsDisplayed == false)
                    {
                        IsDisplayed = true;
                        
                        PanelToDecorate.GetAssistance().Show(delegate (System.Object o, EventArgs e)
                        {
                            PanelToDecorate.GetRootDecoratedAssistance().Show(Utilities.Utility.GetEventHandlerEmpty(), false);

                            //initialiaze the sound
                            audioSource = GetComponent<AudioSource>();
                            audioClip = MATCH.Utilities.Materials.Sounds.Load(audioPath);

                            PanelToDecorate.GetSound().gameObject.SetActive(false); //The decorated panels transform become invisible

                            callback?.Invoke(this, e);
                        }, withAnimation);
                    }
                    else
                    {
                        // Check first if the decorated assistance needs to be displayed
                        PanelToDecorate.GetAssistance().Show(delegate (System.Object o, EventArgs e)
                        {
                            Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();

                            PanelToDecorate.GetSound().gameObject.SetActive(false); //The decorated panels transform become invisible

                            args.Success = false;
                            callback?.Invoke(this, args);
                        }, withAnimation);
                    }
                }
                
                
                public override void ShowHelp(bool show, EventHandler callback, bool withAnimation)
                {
                    PanelToDecorate.GetRootDecoratedAssistance().ShowHelp(show, callback, withAnimation);
                }

                public override Transform GetTransform()
                {
                    return PanelToDecorate.GetRootDecoratedAssistance().GetTransform();
                }

                public Assistance GetRootDecoratedAssistance()
                {
                    return PanelToDecorate.GetRootDecoratedAssistance();
                }

                public Assistance GetAssistance()
                {
                    return this;
                }
                
                public Assistance GetDecoratedAssistance()
                {
                    return PanelToDecorate.GetAssistance();
                }

                public override bool IsDecorator()
                {
                    return true;
                }
                    
                public Transform GetSound()
                {
                    return transform;// SoundTransform;
                }

                public Transform GetArch()
                {
                    return PanelToDecorate.GetArch();
                }
            }
        }
    }
}



