/*Copyright 2022 Louis Marquet, Léri Lamour

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
using NPBehave;
using System;
using System.Reflection;
using System.Linq;

namespace MATCH
{
    namespace Assistances
    {
        namespace Decorators
        {
            namespace Adapters
            {
                public class Factory : MonoBehaviour
                {
                    private static Factory InstanceInternal;
                    public static Factory Instance { get { return InstanceInternal; } }

                    private void Awake()
                    {
                        if (InstanceInternal != null && InstanceInternal != this)
                        {
                            Destroy(this.gameObject);
                        }
                        else
                        {
                            InstanceInternal = this;
                        }
                    }

                    public IAssistanceToIPanel2 CreateAdapterForIPanel2(IAssistance assistanceToAdapt, IPanel2 assistanceDecorated)
                    {
                        IAssistanceToIPanel2 controller = assistanceToAdapt.GetAssistance().gameObject.AddComponent<IAssistanceToIPanel2>();
                        controller.InitializeAssistanceToAdapt(assistanceToAdapt, assistanceDecorated);

                        return controller;
                    }

                }
            }
        }
    }
}