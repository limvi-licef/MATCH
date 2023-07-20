/*Copyright 2022 Emma Foulon

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
using VDS;
using VDS.RDF;


namespace MATCH
{
    namespace Managers
    {
        public class Users : MonoBehaviour
        {
            private static Users instance;
            public string UserProfile;
            public string FavoriteColor;
            public string AttractiveColor;
            public string EmergencyColor;
            EventHandler EventUserButtonClicked;
            //MATCH.Assistances.Buttons.Basic SelectedUserButton;

            private Users()
            {
            }

            public static Users Instance
            {
                get
                {
                    if (instance == null)
                        instance = FindObjectOfType<Users>();
                    return instance;
                }
            }

            // Start is called before the first frame update
            void Start()
            {
                //Buttons = new List<GameObject>();
                // To load a profile by default in case no profile is chosen manually on the cockpit
                UserProfile = "emmaFoulon";
                FavoriteColor = "Cyan";
                AttractiveColor = "Orange";
                EmergencyColor = "Red";
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Profil par défaut : " + UserProfile);

                

                VDS.RDF.Parsing.SparqlQueryParser parser = new VDS.RDF.Parsing.SparqlQueryParser();
                VDS.RDF.Query.SparqlQuery query = parser.ParseFromString("PREFIX mirao: <https://ontology.staging.domus.usherbrooke.ca/MIRAO#> PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>" +
                    "SELECT ?firstNameLabel ?familyNameLabel ?a WHERE {?a mirao:hasFirstName ?firstName . ?firstName rdfs:label ?firstNameLabel . ?a mirao:hasFamilyName ?familyName . ?familyName rdfs:label ?familyNameLabel}");
                VDS.RDF.Query.SparqlResultSet results = (VDS.RDF.Query.SparqlResultSet)MATCH.Utilities.Materials.Ontology.Instance.Graph.ExecuteQuery(query.ToString());

                if (results != null)
                {
                    foreach (VDS.RDF.Query.SparqlResult result in results)
                    {
                        string firstName = result.Value("firstNameLabel").ToString();
                        firstName = MATCH.Utilities.Materials.Ontology.Instance.ShortenMessage(firstName);
                        string familyName = result.Value("familyNameLabel").ToString();
                        familyName = MATCH.Utilities.Materials.Ontology.Instance.ShortenMessage(familyName);
                        string name = $"Choisir le profil de {firstName} {familyName}";
                        MATCH.Assistances.Buttons.Basic newButton = MATCH.AdminMenu.Instance.AddButton(name, delegate() { }, /*delegate ()
                        {
                            UserProfile = result.Value("a").ToString();
                            char symbol = '#';
                            int beginIndex = UserProfile.IndexOf(symbol);
                            UserProfile = UserProfile.Substring(beginIndex + 1);
                            DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Profil choisi : " + UserProfile);

                            ColorPreferencesQuery(UserProfile);
                            //EventUserButtonClicked?.Invoke(this, EventArgs.Empty);
                            //newButton.CallbackSetButtonBackgroundGreen;
                            //transform.Find("BackPlate").Find("Quad").GetComponent<Renderer>().material = Resources.Load(Utilities.Materials.Colors.GreenGlowing, typeof(Material)) as Material;
                            newButton.CallbackSetButtonBackgroundGreen(this, EventArgs.Empty);
                        },*/ MATCH.AdminMenu.Panels.Left);

                        newButton.EventButtonClicked += delegate (System.Object o, EventArgs e)
                        {
                            UserProfile = result.Value("a").ToString();
                            char symbol = '#';
                            int beginIndex = UserProfile.IndexOf(symbol);
                            UserProfile = UserProfile.Substring(beginIndex + 1);
                            DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Profil choisi : " + UserProfile);
                            ColorPreferencesQuery(UserProfile);
                            //EventUserButtonClicked?.Invoke(this, EventArgs.Empty);
                            //newButton.CallbackSetButtonBackgroundGreen;
                            //transform.Find("BackPlate").Find("Quad").GetComponent<Renderer>().material = Resources.Load(Utilities.Materials.Colors.GreenGlowing, typeof(Material)) as Material;
                            newButton.CallbackSetButtonBackgroundGreen(this, EventArgs.Empty);
                        };

                        //EventUserButtonClicked += newButton.CallbackSetButtonBackgroundGreen;
                    }
                }
            }



            void ColorPreferencesQuery(string user)
            {
                VDS.RDF.Parsing.SparqlQueryParser parser = new VDS.RDF.Parsing.SparqlQueryParser();
                VDS.RDF.Query.SparqlQuery query = parser.ParseFromString("PREFIX mirao: <https://ontology.staging.domus.usherbrooke.ca/MIRAO#> " +
                    $"SELECT ?favoriteColorRef ?attractiveColorRef ?emergencyColorRef WHERE {{mirao:{UserProfile} mirao:hasFavoriteColor ?favoriteColor . ?favoriteColor mirao:hasColorReference ?favoriteColorRef . mirao:{UserProfile} mirao:findsColorAttractive ?attractiveColor . ?attractiveColor mirao:hasColorReference ?attractiveColorRef . mirao:{UserProfile} mirao:associatesColorWithUrgency ?emergencyColor . ?emergencyColor mirao:hasColorReference ?emergencyColorRef .}}");
                VDS.RDF.Query.SparqlResultSet results = (VDS.RDF.Query.SparqlResultSet)MATCH.Utilities.Materials.Ontology.Instance.Graph.ExecuteQuery(query.ToString());

                if (results != null)
                {
                    foreach (VDS.RDF.Query.SparqlResult result in results)
                    {
                        FavoriteColor = result.Value("favoriteColorRef").ToString();
                        FavoriteColor = MATCH.Utilities.Materials.Ontology.Instance.ShortenMessage(FavoriteColor);
                        AttractiveColor = result.Value("attractiveColorRef").ToString();
                        AttractiveColor = MATCH.Utilities.Materials.Ontology.Instance.ShortenMessage(AttractiveColor);
                        EmergencyColor = result.Value("emergencyColorRef").ToString();
                        EmergencyColor = MATCH.Utilities.Materials.Ontology.Instance.ShortenMessage(EmergencyColor);
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Couleur préférée : " + FavoriteColor);
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Couleur attractive : " + AttractiveColor);
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Couleur associée à l'urgence : " + EmergencyColor);
                    }
                }
            }
        }
    }
}