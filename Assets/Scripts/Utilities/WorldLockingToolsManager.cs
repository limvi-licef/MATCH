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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.IO;

namespace MATCH
{
    namespace Utilities
    {
        public class WorldLockingToolsManager : MonoBehaviour
        {
            private class ObjectsInformation
            {
                public Vector3 Position;
                public Vector3 Scale;
                public Vector3 Rotation;

                public ObjectsInformation(float posx, float posy, float posz, float scalex, float scaley, float scalez, float rotationx, float rotationy, float roationz)
                {
                    Position = new Vector3(posx, posy, posz);
                    Scale = new Vector3(scalex, scaley, scalez);
                    Rotation = new Vector3(rotationx, rotationy, roationz);
                }

                public ObjectsInformation(Transform objectTransform)
                {
                    Position = objectTransform.position;
                    Scale = objectTransform.localScale;
                    Rotation = objectTransform.rotation.eulerAngles;
                }

                public ObjectsInformation(Vector3 position, Vector3 rotation, Vector3 scale)
                {
                    Position = position;
                    Scale = scale;
                    Rotation = rotation;
                }
            }


            private static WorldLockingToolsManager InstanceInternal;

            public static WorldLockingToolsManager Instance { get { return InstanceInternal; } }

            Dictionary<string, Transform> Objects;

            Dictionary<string, ObjectsInformation> ObjectsPositions;

            public MATCH.Assistances.Dialogs.Dialog1 DebugPositioner;

            private void Awake()
            {
                if (InstanceInternal != null && InstanceInternal != this)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Objects = new Dictionary<string, Transform>();
                    ObjectsPositions = new Dictionary<string, ObjectsInformation>();

                    // Loading file content
                    string line;
                    char[] splitChar = { '\t' };


                    try
                    {
                        string path = Application.persistentDataPath + "/ObjectsPositions.txt";

                        FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                        StreamReader sr = new StreamReader(file);
                        line = sr.ReadLine();
                        string dataFromFile = line;
                        while (line != null)
                        {
                            string[] res = line.Split(splitChar, 10);
                            string id = res[0];

                            //Vector3 pos = new Vector3(float.Parse(res[1]), float.Parse(res[2]), float.Parse(res[3]));

                            ObjectsInformation objInfo = new ObjectsInformation(float.Parse(res[1]), float.Parse(res[2]), float.Parse(res[3]), float.Parse(res[4]), float.Parse(res[5]), float.Parse(res[6]), float.Parse(res[7]), float.Parse(res[8]), float.Parse(res[9]));

                            ObjectsPositions.Add(res[0], objInfo);

                            line = sr.ReadLine();
                            dataFromFile += line;
                        }

                        //DebugPositioner.SetDescription("File read:\n" + dataFromFile, 0.2f);

                        sr.Close();
                    }
                    catch (Exception e)
                    {
                        //DebugPositioner.SetDescription("File does not exist", 0.2f);
                    }

                    InstanceInternal = this;
                }
            }

            // Start is called before the first frame update
            void Start()
            {

            }

            // Update is called once per frame
            void Update()
            {

            }

            /**
             * In some situation, we might want the user to interact with a certain objet (objectToInteract) but register its parent (objectToRegister)
             */
            public void RegisterObject(string id, Transform objectToRegister, Transform objectToInteract)
            {
                DebugPositioner.SetDescription("Positioner - RegisterObject - Called", 0.2f);

                string forDebug = "Registering new object:\n" + id + "\n" + objectToRegister.gameObject.transform.position.ToString();


                    ObjectManipulator objectManipulator = objectToInteract.GetComponent<ObjectManipulator>();
                if (objectManipulator == null)
                {
                    objectManipulator = objectToInteract.gameObject.AddComponent<ObjectManipulator>();
                }

                Objects.Add(id, objectToRegister);

                /*objectManipulator.OnManipulationStarted.AddListener(delegate (ManipulationEventData data)
                    {
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Called");
                    });*/


                BoundsControl bounds = objectToInteract.GetComponent<BoundsControl>();

                bounds.RotateStopped.AddListener(delegate
                {
                    MATCH.Utilities.EventHandlerArgs.WorldLockingToolObjectArgs args = new MATCH.Utilities.EventHandlerArgs.WorldLockingToolObjectArgs();
                    args.id = id;

                    CallbackObjectMoved(objectToInteract.gameObject, args);
                });

                bounds.ScaleStopped.AddListener(delegate
                {
                    MATCH.Utilities.EventHandlerArgs.WorldLockingToolObjectArgs args = new MATCH.Utilities.EventHandlerArgs.WorldLockingToolObjectArgs();
                    args.id = id;

                    CallbackObjectMoved(objectToInteract.gameObject, args);
                });

                objectManipulator.OnManipulationEnded.AddListener(delegate (ManipulationEventData data)
                {
                    //CallbackObjectMoved. // data.ManipulationSource

                    MATCH.Utilities.EventHandlerArgs.WorldLockingToolObjectArgs args = new MATCH.Utilities.EventHandlerArgs.WorldLockingToolObjectArgs();
                    args.id = id;

                    CallbackObjectMoved(data.ManipulationSource, args);
                });


                // If the object is already present in the backup file, then set it back to its original position
                if (ObjectsPositions.ContainsKey(id))
                {
                    Objects[id].position = ObjectsPositions[id].Position;
                    Objects[id].rotation = Quaternion.Euler(ObjectsPositions[id].Rotation);
                    //Objects[id].localScale = ObjectsPositions[id].Scale;


                    objectToInteract.GetComponent<BoundsControl>().enabled = false;
                    objectToInteract.localScale = ObjectsPositions[id].Scale;
                    objectToInteract.GetComponent<BoundsControl>().enabled = true;
                    
                    forDebug += "\nObject already present in file: moving it to its previous position";
                }

                DebugPositioner.SetDescription(forDebug, 0.2f);
            }

            // Callback to be called when an object has been moved
            void CallbackObjectMoved(System.Object o, EventArgs e)
            {
                DebugPositioner.SetDescription("Object moved ... processing", 0.2f);

                GameObject gameObjectToInteract = ((GameObject)o);

                // Save new position to the dictionary ...
                MATCH.Utilities.EventHandlerArgs.WorldLockingToolObjectArgs args = (MATCH.Utilities.EventHandlerArgs.WorldLockingToolObjectArgs)e;
                ObjectsPositions[args.id] = /*((GameObject)o).transform.position*/ new ObjectsInformation(Objects[args.id].position, Objects[args.id].rotation.eulerAngles, gameObjectToInteract.transform.localScale); //Objects[args.id].position; // Last argument is the scale of the interaction surface itself, because we do not want it to affect the parent (othwesie all children assistance will be scaled as well. That's why for scaling, we store the scaling from the surface itself and not the parent.

                string forDebug = "Object " + args.id + " moved to position: " + ((GameObject)o).transform.position.ToString();

                // ... And to the file
                string path = Application.persistentDataPath + "/ObjectsPositions.txt";

                try
                {
                    //StreamWriter sw = new StreamWriter(path);
                    FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write);
                    StreamWriter sw = new StreamWriter(file);

                    foreach (KeyValuePair<string, ObjectsInformation> objInfo in ObjectsPositions)
                    {
                        string posx = objInfo.Value.Position.x.ToString();
                        string posy = objInfo.Value.Position.y.ToString();
                        string posz = objInfo.Value.Position.z.ToString();

                        string rotationx = objInfo.Value.Rotation.x.ToString();
                        string rotationy = objInfo.Value.Rotation.y.ToString();
                        string rotationz = objInfo.Value.Rotation.z.ToString();

                        string scalex = objInfo.Value.Scale.x.ToString();
                        string scaley = objInfo.Value.Scale.y.ToString();
                        string scalez = objInfo.Value.Scale.z.ToString();

                        string toWrite = objInfo.Key + "\t" + posx + "\t" + posy + "\t" + posz + "\t" + scalex + "\t" + scaley + "\t" + scalez + "\t"  + rotationx + "\t" + rotationy + "\t" + rotationz;

                        sw.WriteLine(toWrite);
                    }

                    sw.Close();
                    forDebug += "\nFile updated";
                }
                catch (Exception ee)
                {
                    forDebug += "\nFile NOT updated";
                }

                DebugPositioner.SetDescription(forDebug, 0.2f);
            }
        }
    }
}


