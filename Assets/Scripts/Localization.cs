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
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using System;

namespace MATCH
{
    //Localizad String : UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetLocalizedString("Key");
    //Localized Texture2D : UnityEngine.Localization.Settings.LocalizationSettings.AssetDatabase.GetLocalizedAsset<Texture2D>("Key");
    //Localized Material : UnityEngine.Localization.Settings.LocalizationSettings.AssetDatase.GetLocalizedAsset<Material>("Key");
    //Localized Audio : AudioClip audioClip = UnityEngine.Localization.Settings.LocalizationSettings.AssetDatabase.GetLocalizedAsset<AudioClip>("Key");

    public class Localization : MonoBehaviour
    {
        //Buttons to change the language
        Assistances.Buttons.Basic ButtonFrench;
        Assistances.Buttons.Basic ButtonEnglish;
        //Button to show the current language and hear it on play
        Assistances.Buttons.Basic ButtonLanguage;

        void Start()
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[1];

            ButtonEnglish = AdminMenu.Instance.AddButton("Passer en anglais", delegate () { ChangeLocale("english"); });
            ButtonFrench = AdminMenu.Instance.AddButton("Passer en français", delegate () { ChangeLocale("french"); });

            InitializeLocale();

            //The button showing the current locale and the associated flag
            var material = new Material(Shader.Find("UI/Default"));
            material.mainTexture = LocalizationSettings.AssetDatabase.GetLocalizedAsset<Texture2D>("flag");
            ButtonLanguage = AdminMenu.Instance.AddButtonIcon(LocalizationSettings.StringDatabase.GetLocalizedString("CurrentLanguage"), material, delegate () { AudioCurrentLanguage(); });
        }

        //Get the language from the configParam dictionnary, set the color of the current language button to green
        void InitializeLocale()
        {
            String locale = GlobalInitializer.Instance.GetConfigParam("locale");
            if (locale == "english")
            {
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
                ButtonEnglish.CallbackSetButtonBackgroundGreen(this, EventArgs.Empty);
            }
            if (locale == "french")
            {
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[1];
                ButtonFrench.CallbackSetButtonBackgroundGreen(this, EventArgs.Empty);
            }
        }

        //Change the locale in the dictionnary and reload Match
        void ChangeLocale(string value)
        {
            GlobalInitializer.Instance.SetConfigParam("locale", value);
            GlobalInitializer.Instance.RestartMatch();
        }

        void AudioCurrentLanguage()
        {
            AudioSource audioSource;
            audioSource = ButtonLanguage.gameObject.AddComponent<AudioSource>();
            AudioClip audioClip = UnityEngine.Localization.Settings.LocalizationSettings.AssetDatabase.GetLocalizedAsset<AudioClip>("Language_Sound");
            audioSource.PlayOneShot(audioClip);
        }

    }
}
