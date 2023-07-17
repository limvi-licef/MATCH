using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VDS;
using VDS.RDF;
//using VDS.RDF.Parsing;
//using VDS.RDF.Query;
using System.Reflection;


namespace MATCH
{
    public class TestOntology : MonoBehaviour
    {
        
       VDS.RDF.Graph Graph;
        // Start is called before the first frame update
        void Start()
        {
            /*
            //string room = MATCH.Managers.Rooms.Instance.InWhatRoomIsUser();
            string room = "Dining room";
            string messageTest = MATCH.Managers.Rooms.Instance.ContextualizedRoomQuery("dustingTable", room);
            DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, messageTest);
            */
        }

        // Test pour afficher le message de l'assistance alpha, puisqu'il est seul dans l'assistance donc il n'y a pas encore besoin de la gradation
        /*
        VDS.RDF.Parsing.SparqlQueryParser testparser = new VDS.RDF.Parsing.SparqlQueryParser();
        VDS.RDF.Query.SparqlQuery testquery = testparser.ParseFromString("PREFIX mao: <https://ontology.staging.domus.usherbrooke.ca/MAO#> PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> PREFIX terms: <http://purl.org/dc/terms/>" +
            "SELECT ?message WHERE {?texte rdf:type mao:AssistanceAlpha . ?texte terms:title ?message}");

        VDS.RDF.Query.SparqlResultSet testresults = (VDS.RDF.Query.SparqlResultSet)graph.ExecuteQuery(testquery.ToString());

        if (testresults.Count > 0)
        {
            var result = testresults[0];
            string message = result.Value("message").ToString();
            Debug.Log(message);
        }
        else
        {
            Debug.Log("Aucun résultat trouvé.");
        }


    } */

        // Update is called once per frame
        void Update()
        {

        }
    }
}

