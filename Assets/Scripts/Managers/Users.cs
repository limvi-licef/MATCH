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
                // To load a profile by default in case no profile is chosen manually on the cockpit
                UserProfile = "https://ontology.staging.domus.usherbrooke.ca/MIRAO#emmaFoulon";

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
                        MATCH.AdminMenu.Instance.AddButton(name, delegate ()
                        {
                            MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, "Ok profil.");
                            UserProfile = result.Value("a").ToString();
                        }, MATCH.AdminMenu.Panels.Left);
                    }
                }
            }
        }
    }
}