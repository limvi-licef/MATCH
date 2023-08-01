/*Copyright 2022 Emma Foulon, Guillaume Spalla

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
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Reflection;
using System;
using System.Timers;

using VDS;
using VDS.RDF;

/**
 * Static class containing various utilities functions
 * */

namespace MATCH
{
    namespace Utilities
    {
        namespace Materials
        {
            public class Ontology
            {
                private VDS.RDF.Graph Graph;
                private static Ontology instance;

                //static string Path = "./Assets/Materials/Resources/MATCH/Ontology/";

                /* private string OntologyPath = "./Assets/Materials/Resources/MATCH/Ontology/MIRAO.rdf"; */
                //string MIRAO = "MATCH/Ontology/MIRAO";

                private Ontology()
                {
                    string MIRAO = "MATCH/Ontology/MIRAO";

                    // Load the ontology
                    Graph = new VDS.RDF.Graph();
                    //VDS.RDF.Parsing.FileLoader.Load(Graph, MIRAO);
                    var text = Resources.Load<TextAsset>(MIRAO);
                    string data = text.ToString();
                    VDS.RDF.Parsing.StringParser.Parse(Graph, data);

                }

                /*public VDS.RDF.Graph GetGraph()
                {
                    return Graph;
                }*/

                public VDS.RDF.Query.SparqlResultSet ExecuteQuery(string query)
                {
                    VDS.RDF.Parsing.SparqlQueryParser parser = new VDS.RDF.Parsing.SparqlQueryParser();
                    VDS.RDF.Query.SparqlQuery queryProcessed = parser.ParseFromString(query);

                    return (VDS.RDF.Query.SparqlResultSet)Graph.ExecuteQuery(query);

                }

                public static Ontology Instance
                {
                    get
                    {
                        if (instance == null)
                            instance = new Ontology();
                        return instance;
                    }
                }


                // Allows to display the message only, without the URI which always starts with ^
                public string ShortenMessage(string longMessage)
                {
                    char symbol = '^';
                    int endIndex = longMessage.IndexOf(symbol);
                    longMessage = longMessage.Substring(0, endIndex);
                    return longMessage;
                }

                public string ShortenMessage2(string longMessage)
                {
                    char symbol = '#';
                    int beginIndex = longMessage.IndexOf(symbol);
                    longMessage = longMessage.Substring(beginIndex + 1);
                    return longMessage;
                }

                /*
                // Make a simple query to get the assistance text message
                public string AssistanceQuery(string assistanceName, string illocutionaryAct, string impairment, string assistanceType)
                {
                    VDS.RDF.Parsing.SparqlQueryParser parser = new VDS.RDF.Parsing.SparqlQueryParser();

                    string sparqlQuery = $"PREFIX mirao: <https://ontology.staging.domus.usherbrooke.ca/MIRAO#> PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>" +
                                        $"SELECT ?message WHERE {{ ?texte rdf:type mirao:Text . ?texte mirao:isLinkedToAssistance mirao:{assistanceName} . ?texte mirao:hasIllocutionaryAct mirao:{illocutionaryAct}. ?texte mirao:isLinkedToImpairment mirao:{impairment}. ?texte mirao:hasAssistanceType mirao:{assistanceType}. ?texte mirao:hasContent ?message}}";

                    VDS.RDF.Query.SparqlQuery query = parser.ParseFromString(sparqlQuery);

                    VDS.RDF.Query.SparqlResultSet testresults = (VDS.RDF.Query.SparqlResultSet)Graph.ExecuteQuery(query.ToString());
                    string message = "";

                    if (testresults.Count > 0)
                    {
                        var result = testresults[0];
                        string text = result.Value("message").ToString();
                        message = ShortenMessage(text);
                    }
                    else
                    {
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Aucun résultat trouvé.");
                    }

                    return message;
                }
                */

                public string AssistanceQuery(string assistanceName, string illocutionaryAct, string impairment, string assistanceType)
                {
                    VDS.RDF.Parsing.SparqlQueryParser parser = new VDS.RDF.Parsing.SparqlQueryParser();

                    // Query to find the assistance text according to the user's profile and the assistance gradation
                    string sparqlQuery = $"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#> PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> PREFIX mirao: <https://ontology.staging.domus.usherbrooke.ca/MIRAO#>" +
                        $"SELECT ?message WHERE {{mirao:{MATCH.Managers.Users.Instance.UserProfile} mirao:preferredCommChannel ?modeComm . ?assistance rdf:type mirao:{assistanceName} . ?assistance mirao:isLinkedToImpairment mirao:{impairment} . ?assistance mirao:hasIllocutionaryAct mirao:{illocutionaryAct} . ?assistance mirao:hasAssistanceType mirao:{assistanceType} . ?assistance ?a ?texte . ?texte rdf:type ?modeComm . ?texte mirao:hasContent ?message}}";
                    
                    VDS.RDF.Query.SparqlQuery query = parser.ParseFromString(sparqlQuery);
                    VDS.RDF.Query.SparqlResultSet results = (VDS.RDF.Query.SparqlResultSet)Graph.ExecuteQuery(query.ToString());
                    string message = "";

                    if (results.Count > 0)
                    {
                        var result = results[0];
                        string text = result.Value("message").ToString();
                        message = ShortenMessage(text);
                    }
                    else
                    {
                        sparqlQuery = $"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#> PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> PREFIX mirao: <https://ontology.staging.domus.usherbrooke.ca/MIRAO#>" +
                        $"SELECT ?message WHERE {{?assistance rdf:type mirao:{assistanceName} . ?assistance mirao:isLinkedToImpairment mirao:{impairment} . ?assistance mirao:hasIllocutionaryAct mirao:{illocutionaryAct} . ?assistance mirao:hasAssistanceType mirao:{assistanceType} . ?assistance ?a ?texte . ?texte rdf:type mirao:Text . ?texte mirao:hasContent ?message}}";

                        query = parser.ParseFromString(sparqlQuery);
                        results = (VDS.RDF.Query.SparqlResultSet)Graph.ExecuteQuery(query.ToString());

                        if (results.Count > 0)
                        {
                            var result = results[0];
                            string text = result.Value("message").ToString();
                            message = ShortenMessage(text);
                        }
                        else
                        {
                            DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Aucun résultat trouvé.");
                        }
                    }

                    return message;
                }
            }
        }
    }
}

