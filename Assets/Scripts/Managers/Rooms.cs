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
using System;
using System.Reflection;
using VDS;
using VDS.RDF;


namespace MATCH
{
    namespace Managers
    {
        public class Rooms : MonoBehaviour
        {
            private static Rooms instance;

            private Rooms()
            {
            }

             public static Rooms Instance
             {
                get
                {
                    if (instance == null)
                        instance = FindObjectOfType<Rooms>();
                    return instance;
                }
             }

            // Start is called before the first frame update
            void Start()
            {
                List<string> RoomList = MATCH.Utilities.WorldLockingToolsManager.Instance.GetPositioningStorage().GetObjetsRegisteredNames();
                    
                VDS.RDF.Parsing.SparqlQueryParser parser = new VDS.RDF.Parsing.SparqlQueryParser();
                VDS.RDF.Query.SparqlQuery query = parser.ParseFromString("PREFIX mirao: <https://ontology.staging.domus.usherbrooke.ca/MIRAO#> PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#> SELECT ?roomnames WHERE {?rooms rdfs:subClassOf mirao:Rooms . ?rooms rdfs:label ?roomnames}");
                VDS.RDF.Query.SparqlResultSet results = (VDS.RDF.Query.SparqlResultSet)MATCH.Utilities.Materials.Ontology.Instance.Graph.ExecuteQuery(query.ToString());

                if (results != null)
                {
                    foreach (VDS.RDF.Query.SparqlResult result in results)
                    {
                        string room = result.Value("roomnames").ToString();
                        room = MATCH.Utilities.Materials.Ontology.Instance.ShortenMessage(room);
                        bool roomExists = false;

                        foreach (string existingRoom in RoomList)
                        {

                            if (existingRoom == room)
                            {
                                roomExists = true;
                                MATCH.Assistances.InteractionSurface test = MATCH.Assistances.Factory.Instance.CreateInteractionSurface(room, MATCH.AdminMenu.Panels.Right, new Vector3(1f, 0.01f, 0.8f), new Vector3(-0.4f, -1.6f, -4f), MATCH.Utilities.Materials.Colors.Cyan, true, true, MATCH.Utilities.Utility.GetEventHandlerEmpty(), true, transform);
                                test.SetPreventResizeY(true);
                            }
                        }

                        if (roomExists == false)
                        {
                            string message = "Créer la pièce : " + room;

                            MATCH.DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, MATCH.DebugMessagesManager.MessageLevel.Info, room);
                            MATCH.AdminMenu.Instance.AddButton(message, delegate ()
                            {
                                MATCH.Assistances.InteractionSurface test = MATCH.Assistances.Factory.Instance.CreateInteractionSurface(room, MATCH.AdminMenu.Panels.Right, new Vector3(1f, 0.01f, 0.8f), new Vector3(-0.4f, -1.6f, -4f), MATCH.Utilities.Materials.Colors.Cyan, true, true, MATCH.Utilities.Utility.GetEventHandlerEmpty(), true, transform);
                                test.SetPreventResizeY(true);
                            }, MATCH.AdminMenu.Panels.Right);
                        }
                    }
                }
            }

            
            public string InWhatRoomIsUser()
            {
                List<string> RoomList = MATCH.Utilities.WorldLockingToolsManager.Instance.GetPositioningStorage().GetObjetsRegisteredNames();

                foreach (string roomName in RoomList)
                {
                    GameObject roomObject = GameObject.Find(roomName);
                    if (roomObject != null)
                    {
                        Assistances.InteractionSurface temp = roomObject.GetComponent<Assistances.InteractionSurface>();

                        Vector3 roomPosition = roomObject.transform.position;
                        Vector3 roomScale = temp.GetInteractionSurface().localScale; //roomObject.transform.localScale;



                        Vector2 pointA = new Vector2(roomPosition.x - roomScale.x / 2f, roomPosition.z + roomScale.z / 2f);
                        Vector2 pointB = new Vector2(roomPosition.x + roomScale.x / 2f, roomPosition.z + roomScale.z / 2f);
                        Vector2 pointC = new Vector2(roomPosition.x + roomScale.x / 2f, roomPosition.z - roomScale.z / 2f);
                        Vector2 pointD = new Vector2(roomPosition.x - roomScale.x / 2f, roomPosition.z - roomScale.z / 2f);

                        Vector3 userPosition = Camera.main.transform.position;
                        Vector2 userPoint = new Vector2(userPosition.x, userPosition.z); // "U" for user, i.e. user's position

                        if (MATCH.Utilities.Utility.IsPointInRectangle(pointA, pointB, pointC, pointD, userPoint))
                        {
                            return roomName;
                        }
                    }
                }

                return "Aucune pièce ne correspond à l'emplacement de l'utilisateur.";
            }


            public bool IsUserInWrongRoom(string scenario)
            {
                VDS.RDF.Query.SparqlResultSet results = RoomQueryResults(scenario);
                string room = InWhatRoomIsUser();

                if (results.Count > 0)
                {
                    var result = results[0];
                    string expectedRoom = result.Value("roomname").ToString();
                    expectedRoom = MATCH.Utilities.Materials.Ontology.Instance.ShortenMessage(expectedRoom);
                    if (expectedRoom.ToLower() == room.ToLower())
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Aucun résultat trouvé.");
                }

                return true;
            }


            public VDS.RDF.Query.SparqlResultSet RoomQueryResults(string scenario)
            {
                VDS.RDF.Parsing.SparqlQueryParser parser = new VDS.RDF.Parsing.SparqlQueryParser();

                string sparqlQuery = $"PREFIX mirao: <https://ontology.staging.domus.usherbrooke.ca/MIRAO#> PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>" +
                                    $"SELECT ?roomname ?place WHERE {{ mirao:{scenario} mirao:isAssociatedToRoom ?room . ?room rdfs:label ?roomname . ?room mirao:hasLocation ?place}}";

                VDS.RDF.Query.SparqlQuery query = parser.ParseFromString(sparqlQuery);
                VDS.RDF.Query.SparqlResultSet results = (VDS.RDF.Query.SparqlResultSet)MATCH.Utilities.Materials.Ontology.Instance.Graph.ExecuteQuery(query.ToString());
                return results;
            }


            public string ContextualizedRoomQuery(string scenario, string room)
            {
                VDS.RDF.Query.SparqlResultSet results = RoomQueryResults(scenario);
                string expectedRoom = "";
                string location = "";
                
                if (results.Count > 0)
                {
                    var result = results[0];
                    expectedRoom = result.Value("roomname").ToString();
                    expectedRoom = MATCH.Utilities.Materials.Ontology.Instance.ShortenMessage(expectedRoom);
                    location = result.Value("place").ToString();
                    location = MATCH.Utilities.Materials.Ontology.Instance.ShortenMessage(location);

                    if (expectedRoom.ToLower() == room.ToLower())
                    {
                        return "Vous êtes dans la bonne pièce pour réaliser la tâche.";
                    }
                    else
                    {
                        return $"Vous devriez être {location}.";
                    }
                }
                else
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Aucun résultat trouvé.");
                }

                return "Rien";
                
            }

        }
    }
}
