/*Copyright 2023 Aurélie Le Guidec, Guillaume Merviel.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

using UnityEngine;
using UnityEngine.Localization.Settings;

namespace MATCH
{
    public class Localization : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[1];
            
            AdminMenu.Instance.AddButton("Passer en anglais", delegate () { ChangeLocaleToEnglish(); });
            AdminMenu.Instance.AddButton("Passer en français", delegate () { ChangeLocaleToFrench(); });
        }

        void ChangeLocaleToEnglish()
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
        }

        void ChangeLocaleToFrench()
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[1];
        }
    }
}
