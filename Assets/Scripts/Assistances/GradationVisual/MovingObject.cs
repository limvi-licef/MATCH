using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MATCH
{
    namespace Assistances
    {
        namespace GradationVisual
        {
            public class MovingObject : MonoBehaviour
            {
                bool Move;

                private void Awake()
                {
                    Move = false;
                }

                // Start is called before the first frame update
                void Start()
                {
                    
                }

                // Update is called once per frame
                void Update()
                {
                    if (Move)
                    {
                        transform.Rotate(Vector3.forward * Time.deltaTime * 15);
                    }
                }

                public void StartMove(bool move)
                {
                    Move = move;
                }
            }
        }
    }
}

