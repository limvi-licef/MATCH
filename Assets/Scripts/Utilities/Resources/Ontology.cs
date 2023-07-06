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
            public static class Ontology
            {
                //static string Path = "MATCH/Ontology/";
                static string Path = "./Assets/Materials/Resources/MATCH/Ontology/";
                public static string Test = Path + "Test.rdf";

                /*public static VDS.RDF.IGraph Load(string prefabPath)
                {


                    //return Resources.Load<VDS.RDF.IGraph>(prefabPath);
                }*/
            } 
        }
    }
}
