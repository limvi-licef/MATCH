using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VDS;
/*using VDS.RDF;
using VDS.RDF.Writing;*/
/*using VDS.RDF;
using VDS.RDF.Writing;*/

public class TestOntology : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        VDS.RDF.IGraph g = new VDS.RDF.Graph();
        //IGraph g = new Graph();
        VDS.RDF.Parsing.FileLoader.Load(g, "<Insert link here>");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
