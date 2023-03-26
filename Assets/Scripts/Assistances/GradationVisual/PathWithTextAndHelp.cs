using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace MATCH.Assistances.GradationVisual
{
    public class PathWithTextAndHelp : Assistance
    {
        readonly Transform HelpController;
        readonly Transform LineView;
        readonly Transform TextView;

        readonly Dialogs.Dialog1 TextController;
        readonly LineToObject LineController;

        public void SetDescription(string text, float fontSize = -1.0f)
        {
            TextController.SetDescription(text, fontSize);
        }

        public void SetPathStartAndEndPoint(Transform origin, Transform target)
        {
            LineController.PointOrigin = origin.position;
            LineController.PointEnd = target.position;
        }

        public override Transform GetTransform()
        {
            return transform;
        }

        public override bool IsDecorator()
        {
            return false;
        }

        bool m_mutexHide = false;

        public override void Hide(EventHandler eventHandler, bool withAnimation)
        {
            if (m_mutexHide == false)
            {
                m_mutexHide = true;

                TextController.Hide(/*eventHandler*/ delegate (System.Object o, EventArgs e)
                {
                    // Hiding line
                    if (LineView.gameObject.activeSelf)
                    {
                        LineView.GetComponent<LineToObject>().hide(eventHandler); // The eventhandler being already called above, we do not want it to be called twice, as this could create strange behaviors.
                        m_mutexHide = false;
                    }
                    else
                    {
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Line already hidden: nothing to do");
                        eventHandler?.Invoke(this, EventArgs.Empty);
                        m_mutexHide = false;
                    }
                }, withAnimation);
            }
            else
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Mutex locked - request ignored");
            }
        }

        bool m_mutexShow = false;

        public override void Show(EventHandler eventHandler, bool withAnimation = true)
        {
            if (m_mutexShow == false)
            {
                m_mutexShow = true;

                TextView.position = new Vector3(LineController.PointOrigin.x, Camera.main.transform.position.y, LineController.PointOrigin.z);
                TextView.transform.LookAt(Camera.main.transform);
                TextView.transform.Rotate(new Vector3(0, 1, 0), 180);

                // Trick to start the line to the text position, i.e. to start at user's head's position
                LineController.PointOrigin = TextView.position;
                TextView.position = Vector3.MoveTowards(TextView.position, Camera.main.transform.position, 0.01f);

                TextController.Show(delegate
                {
                    m_mutexShow = false;
                }, withAnimation);

                // Showing line
                LineView.GetComponent<LineToObject>().show(delegate
                {
                    eventHandler?.Invoke(this, EventArgs.Empty);
                    m_mutexShow = false;
                });
            }
            else
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Mutex locked - request ignored");
            }
        }

        public override void ShowHelp(bool show, EventHandler callback, bool withAnimation)
        {
            if (show)
            {
                Utilities.Utility.AdjustObjectHeightToHeadHeight(HelpController);

                if (withAnimation)
                {
                    HelpController.gameObject.AddComponent<Utilities.Animation>().AnimateAppearInPlaceToScaling(new Vector3(0.1f, 0.1f, 0.1f), delegate
                    {
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Called");

                        Destroy(HelpController.gameObject.GetComponent<Animation>());
                        callback?.Invoke(this, EventArgs.Empty);
                    }
                    );
                }
                else
                {
                    HelpController.gameObject.SetActive(true);
                    callback?.Invoke(this, EventArgs.Empty);
                }

            }
            else
            {
                if (withAnimation)
                {
                    Utilities.Utility.AnimateDisappearInPlace(HelpController.gameObject, new Vector3(0.1f, 0.1f, 0.1f), callback);
                }
                else
                {
                    HelpController.gameObject.SetActive(false);
                    HelpController.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    callback?.Invoke(this, EventArgs.Empty);
                }

            }

        }
    }
}