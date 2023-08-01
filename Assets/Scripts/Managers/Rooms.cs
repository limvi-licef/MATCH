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
                // List the already existing rooms
                List<string> RoomList = MATCH.Utilities.WorldLockingToolsManager.Instance.GetPositioningStorage().GetObjetsRegisteredNames();

                // Query to get all the rooms from the ontology
                //VDS.RDF.Parsing.SparqlQueryParser parser = new VDS.RDF.Parsing.SparqlQueryParser();
                //VDS.RDF.Query.SparqlQuery query = parser.ParseFromString("PREFIX mirao: <https://ontology.staging.domus.usherbrooke.ca/MIRAO#> PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#> SELECT ?roomnames WHERE {?rooms rdfs:subClassOf mirao:Rooms . ?rooms rdfs:label ?roomnames}");
                string query = "PREFIX mirao: <https://ontology.staging.domus.usherbrooke.ca/MIRAO#> PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#> SELECT ?roomnames WHERE {?rooms rdfs:subClassOf mirao:Rooms . ?rooms rdfs:label ?roomnames}";
                VDS.RDF.Query.SparqlResultSet results = MATCH.Utilities.Materials.Ontology.Instance.ExecuteQuery(query);//(VDS.RDF.Query.SparqlResultSet)MATCH.Utilities.Materials.Ontology.Instance.GetGraph().ExecuteQuery(query.ToString());

                if (results != null)
                {
                    foreach (VDS.RDF.Query.SparqlResult result in results)
                    {
                        string room = result.Value("roomnames").ToString();
                        room = MATCH.Utilities.Materials.Ontology.Instance.ShortenMessage(room); // Get the room name only, and not the whole URI
                        bool roomExists = false;

                        foreach (string existingRoom in RoomList)
                        {

                            if (existingRoom == room)
                            {
                                // If the room already exists, display an interaction surface
                                roomExists = true;
                                MATCH.Assistances.InteractionSurface test = MATCH.Assistances.Factory.Instance.CreateInteractionSurface(room, MATCH.AdminMenu.Panels.Right, new Vector3(1f, 0.01f, 0.8f), new Vector3(-0.4f, -1.6f, -4f), MATCH.Utilities.Materials.Colors.Cyan, true, true, MATCH.Utilities.Utility.GetEventHandlerEmpty(), true, transform);
                                test.SetPreventResizeY(true);
                            }
                        }

                        if (roomExists == false)
                        {
                            // If the room doesn't exist already, put a button to create it
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
                // List the existing rooms
                List<string> RoomList = MATCH.Utilities.WorldLockingToolsManager.Instance.GetPositioningStorage().GetObjetsRegisteredNames();

                foreach (string roomName in RoomList)
                {
                    GameObject roomObject = GameObject.Find(roomName);
                    if (roomObject != null)
                    {
                        Assistances.InteractionSurface temp = roomObject.GetComponent<Assistances.InteractionSurface>();

                        Vector3 roomPosition = roomObject.transform.position;
                        Vector3 roomScale = temp.GetInteractionSurface().localScale; //roomObject.transform.localScale;

                        // Find the 4 corners of the room
                        Vector2 pointA = new Vector2(roomPosition.x - roomScale.x / 2f, roomPosition.z + roomScale.z / 2f);
                        Vector2 pointB = new Vector2(roomPosition.x + roomScale.x / 2f, roomPosition.z + roomScale.z / 2f);
                        Vector2 pointC = new Vector2(roomPosition.x + roomScale.x / 2f, roomPosition.z - roomScale.z / 2f);
                        Vector2 pointD = new Vector2(roomPosition.x - roomScale.x / 2f, roomPosition.z - roomScale.z / 2f);

                        Vector3 userPosition = Camera.main.transform.position;
                        Vector2 userPoint = new Vector2(userPosition.x, userPosition.z); // "U" for user, i.e. user's position

                        if (MATCH.Utilities.Utility.IsPointInRectangle(pointA, pointB, pointC, pointD, userPoint))
                        {
                            return roomName; // Name of the room the user is in
                        }
                    }
                }

                return "Aucune pièce ne correspond à l'emplacement de l'utilisateur.";
            }

            /*
            public bool IsUserInWrongRoom(string scenario)
            {
                VDS.RDF.Query.SparqlResultSet results = RoomQueryResults(scenario); // Find the room the user has to be in to do the task
                string room = InWhatRoomIsUser(); // Find what room the user is in

                if (results.Count > 0)
                {
                    var result = results[0];
                    string expectedRoom = result.Value("roomname").ToString();
                    expectedRoom = MATCH.Utilities.Materials.Ontology.Instance.ShortenMessage(expectedRoom);
                    if (expectedRoom.ToLower() == room.ToLower())
                    {
                        return false; // If the user is in the right room to perform the task
                    }
                    else
                    {
                        return true; // If the user is not in the room where the task should be performed
                    }
                }
                else
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Aucun résultat trouvé.");
                }

                return true;
            }
            */

            public VDS.RDF.Query.SparqlResultSet RoomQueryResults(string scenario)
            {
                //VDS.RDF.Parsing.SparqlQueryParser parser = new VDS.RDF.Parsing.SparqlQueryParser();

                // Query to find the room the task has to be performed in
                string sparqlQuery = $"PREFIX mirao: <https://ontology.staging.domus.usherbrooke.ca/MIRAO#> PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>" +
                                    $"SELECT ?roomname ?place WHERE {{ mirao:{scenario} mirao:isAssociatedToRoom ?room . ?room rdfs:label ?roomname . ?room mirao:hasLocation ?place}}";

                //VDS.RDF.Query.SparqlQuery query = parser.ParseFromString(sparqlQuery);
                VDS.RDF.Query.SparqlResultSet results = MATCH.Utilities.Materials.Ontology.Instance.ExecuteQuery(sparqlQuery/*query.ToString()*/); /*(VDS.RDF.Query.SparqlResultSet)MATCH.Utilities.Materials.Ontology.Instance.GetGraph().ExecuteQuery(query.ToString());*/
                return results;
            }


            public string ContextualizedRoomQuery(string scenario, string room)
            {
                VDS.RDF.Query.SparqlResultSet results = RoomQueryResults(scenario);
                
                if (results.Count > 0)
                {
                    var result = results[0];
                    string expectedRoom = result.Value("roomname").ToString(); // The room where the activity needs to be done, for example "kitchen"
                    expectedRoom = MATCH.Utilities.Materials.Ontology.Instance.ShortenMessage(expectedRoom);
                    string location = result.Value("place").ToString(); // The location of the activity, for example "in the kitchen" (allows to avoid the gender of the rooms' nouns)
                    location = MATCH.Utilities.Materials.Ontology.Instance.ShortenMessage(location);

                    if (expectedRoom.ToLower() == room.ToLower())
                    {
                        return "Vous êtes dans la bonne pièce pour réaliser la tâche."; // (not displayed, just for testing)
                    }
                    else
                    {
                        return $"Vous devriez être {location}."; // Tells the user in which room he should be to perform his activity
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
