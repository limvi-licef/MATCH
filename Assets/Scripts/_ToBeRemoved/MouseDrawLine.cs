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
using Microsoft.MixedReality.Toolkit.Input;
using System.Linq;

public class MouseDrawLine : MonoBehaviour, IMixedRealityTouchHandler
{
    public GameObject m_linePrefab;

    List<GameObject> m_lines;

    float m_lineYOffset;

    enum States
    {
        CreateNewLine,
        AddPointToCurrentLine,
        NotInitialized
    }

    States m_status;

    // Start is called before the first frame update
    void Start()
    {
        m_status = States.NotInitialized;
        m_lines = new List<GameObject>();
        m_lineYOffset = 0.07f;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_status == States.CreateNewLine)
        {
            m_status = States.NotInitialized;
        }

        
    }

    void createLine(float posx, float posz)
    {
        Vector3 posLine = new Vector3(posx, gameObject.transform.position.y + m_lineYOffset, posz);

        m_lines.Add(Instantiate(m_linePrefab, gameObject.transform, true ));

        LineRenderer lineRenderer = m_lines.Last().GetComponent<LineRenderer>();      
        lineRenderer.SetPosition(0, posLine);
        lineRenderer.SetPosition(1, posLine);

        MATCH.DebugMessagesManager.Instance.displayMessage("MouseDrawLine", "createLine", MATCH.DebugMessagesManager.MessageLevel.Info, "Number of lines in the list: " + m_lines.Count.ToString() + " Index position: " + lineRenderer.positionCount.ToString());

    }

    void addPointToCurrentLine(float posx, float posz)
    {
        LineRenderer lineRenderer = m_lines.Last().GetComponent<LineRenderer>();
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, new Vector3(posx, gameObject.transform.position.y + m_lineYOffset, posz));

        MATCH.DebugMessagesManager.Instance.displayMessage("MouseDrawLine", "addPointToCurrentLine", MATCH.DebugMessagesManager.MessageLevel.Info, "Number of lines in the list: " + m_lines.Count.ToString() + " Index position: " + lineRenderer.positionCount.ToString());
    }

    void IMixedRealityTouchHandler.OnTouchStarted(HandTrackingInputEventData eventData)
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();
        float xorigin = gameObject.transform.position.x - renderer.bounds.size.x / (float)2.0;
        float zorigin = gameObject.transform.position.z - renderer.bounds.size.z / (float)2.0;

        float xinplane = eventData.InputData.x - xorigin;
        float zinplane = eventData.InputData.z - zorigin;

        createLine(eventData.InputData.x, eventData.InputData.z);
    }

    void IMixedRealityTouchHandler.OnTouchCompleted(HandTrackingInputEventData eventData)
    {
        MATCH.DebugMessagesManager.Instance.displayMessage("MouseDrawLine", "IMixedRealityTouchHandler.OnTouchCompleted", MATCH.DebugMessagesManager.MessageLevel.Info, "");
    }

    void IMixedRealityTouchHandler.OnTouchUpdated(HandTrackingInputEventData eventData)
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();
        float xorigin = gameObject.transform.position.x - renderer.bounds.size.x / (float)2.0;
        float zorigin = gameObject.transform.position.z - renderer.bounds.size.z / (float)2.0;

        float xinplane = eventData.InputData.x - xorigin;
        float zinplane = eventData.InputData.z - zorigin;

        addPointToCurrentLine(eventData.InputData.x, eventData.InputData.z);
    }
}
