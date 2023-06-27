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
                public enum MovingType
                {
                    Circle = 0,
                    HalfCircle = 1
                }

                bool Move;
                MovingType MoveType;

                private void Awake()
                {
                    Move = false;
                }

                // Start is called before the first frame update
                void Start()
                {
                    
                }

                int MovingAmplitudeCounter = 0;
                int MovingAmplitude = 130;
                Vector3 MovingDirection = Vector3.forward;
                // Update is called once per frame
                void Update()
                {
                    if (Move)
                    {
                        if (MoveType == MovingType.Circle)
                        {
                            transform.Rotate(Vector3.forward * Time.deltaTime * 15);
                        }
                        else if (MoveType == MovingType.HalfCircle)
                        {
                            MovingAmplitudeCounter++;
                            if (MovingAmplitudeCounter > MovingAmplitude)
                            {
                                MovingDirection = (MovingDirection == Vector3.forward) ? Vector3.back : Vector3.forward ;
                                MovingAmplitudeCounter = 0;
                            }
                            /*else if (MovingAmplitudeCounter < MovingAmplitude)
                            {
                                direction = Vector3.back;
                            }
                            else
                            {
                                MovingAmplitudeCounter = 0;
                            }*/

                            transform.Rotate(MovingDirection * Time.deltaTime * 80);
                        }
                    }
                }

                public void SetMoveShape (MovingType type)
                {
                    MoveType = type;
                }

                public void StartMove(bool move)
                {
                    Move = move;
                }
            }
        }
    }
}

