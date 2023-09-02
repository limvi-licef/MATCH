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
        public class Logger : MonoBehaviour
        {
            private static Logger InstanceInternal;

            public static Logger Instance { get { return InstanceInternal; } }

            MATCH.Assistances.Dialogs.Dialog1 DebugLogger;

            //private StreamWriter StreamToFile;

            string PathToFile;

            private void Awake()
            {
                if (InstanceInternal != null && InstanceInternal != this)
                {
                    Destroy(gameObject);
                }
                else
                {
                    DebugLogger = Assistances.Factory.Instance.CreateDialogNoButton("Debug Logger", "", transform);


                    try
                    {
                        DateTime now = DateTime.Now;
                        PathToFile = Application.persistentDataPath + "/Log_"+ now.Year + "-" + now.Month + "-" + now.Day + "_" + now.Hour + "-" + now.Minute + ".txt";

                        //FileStream file = new FileStream(PathToFile, FileMode.Create, FileAccess.Write, FileShare.Write);
                        //StreamToFile = new StreamWriter(file);
                    }
                    catch (Exception)
                    {
                        //DebugPositioner.SetDescription("File does not exist", 0.2f);
                    }

                    InstanceInternal = this;
                }
            }

            // Callback to be called when an object has been moved
            public void Log(string id, string className,  string functionName, string message)
            {
                DateTime now = DateTime.Now;
                string toLog = now.Year + "-" + now.Month + "-" + now.Day + "_" + now.Hour + "-" + now.Minute + "\t" + id + " " + className + " " + functionName + " " + message;

                

                try
                {
                    FileStream file = new FileStream(PathToFile, FileMode.Append, FileAccess.Write, FileShare.Write);
                    StreamWriter sw = new StreamWriter(file);

                    sw.WriteLine(toLog);
                    sw.Close();
                    DebugLogger.SetDescription("Last message logged: " + toLog);
                }
                catch (Exception)
                {
                    DebugLogger.SetDescription("Error - message not logged");
                }
            }
        }
    }
}


