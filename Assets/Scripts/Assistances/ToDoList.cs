/*Copyright 2022 Louis Marquet, Léri Lamour, Aurélie Le Guidec, Guillaume Merviel.

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
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;
using System.Reflection;
using System.Linq;
using TMPro;

namespace MATCH
{
    namespace Assistances
    {
        public class ToDoList : MATCH.Assistances.Dialogs.Dialog1
        {
            
            // Start is called before the first frame update
            void Start()
            {

            }
            // Update is called once per frame
            void Update()
            {
                ToDoListConfig(); //config of the todo list for update the time
            }

            void ToDoListConfig()
            {
                string date = System.DateTime.Now.ToString("D", new System.Globalization.CultureInfo("fr-FR"));
                string hour = System.DateTime.Now.ToString("HH:mm");
                string textToDisplay = "Date : " + date + "                              Heure : " + hour + "\nSaison : " + MATCH.ToDoListController.GetSeason(System.DateTime.Now) + "\n\nTâches à réaliser : ";
                //if (textToDisplay != TodoList.GetDescription())
                //{ // To avoid updating the text at each frame
                this.SetDescription(textToDisplay, 0.1f);
                //}
            }

        }

    }
}

