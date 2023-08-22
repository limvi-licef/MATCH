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

        //readonly Dialogs.Dialog1 TextController;
        readonly LineToObject LineController;

        Assistances.Dialogs.Dialog2 Dialog;
        Decorators.BackgroundColorMessage2 DialogBackground;
        Decorators.BackgroundColorIcon2 DialogIcon;

        Assistances.LightedPath Path;

        private void Start()
        {
            
        }

        public void InitializeAssistance(string title, string description, Transform lineDestination, Transform parent)
        {
            this.transform.parent = parent;

            //Dialog = Assistances.Factory.Instance.CreateAssistanceGradationAttention(assistanceName);
            //toReturn.name = assistanceName;

            //TextController.SetDescription(text, fontSize);

            // Dialog
            Assistances.Dialogs.Dialog2 dialog = Assistances.Factory.Instance.CreateDialog2NoButton(title, description, transform);
            DialogBackground = (Decorators.BackgroundColorMessage2)Assistances.Decorators.Factory.Instance.CreateBackgroundMessage(dialog, Utilities.Materials.Colors.Cyan);
            DialogIcon = (Decorators.BackgroundColorIcon2)Assistances.Decorators.Factory.Instance.CreateBackgroundIcon(DialogBackground, Utilities.Materials.Colors.Cyan, true);

            //Transform destination = GameObject.Find("MATCH").transform.Find("Cube");
            //Transform destination = MATCH.Assistances.InteractionSurfaceFollower.Instance.GetInteractionSurface().transform;

            // Path
            //Path = Assistances.Factory.Instance.CreatePathFinding(name + "_Path", destination, lineDestination, transform);
            Path = Assistances.Factory.Instance.CreatePathFindingWithFollowerBegin(name + "_Path", lineDestination, transform);
        }

        /*public void SetPathStartAndEndPoint(Transform origin, Transform target)
        {
            LineController.PointOrigin = origin.position;
            LineController.PointEnd = target.position;
        }*/

        public override Transform GetTransform()
        {
            return transform;
        }

        public override bool IsDecorator()
        {
            return false;
        }

        public override void Hide(EventHandler eventHandler, bool withAnimation)
        {
            Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();

            if (IsDisplayed)
            {
                DialogIcon.Hide(delegate (System.Object o, EventArgs e)
                {
                    Path.Hide(Utilities.Utility.GetEventHandlerEmpty(), false);
                    IsDisplayed = false;
                    args.Success = true;
                    eventHandler?.Invoke(this, args);
                    OnIsHidden(this, args);
                }, false);

                /*TextController.Hide(delegate (System.Object o, EventArgs e)
                {
                    // Hiding line
                    if (LineView.gameObject.activeSelf)
                    {
                        LineView.GetComponent<LineToObject>().hide(eventHandler); // The eventhandler being already called above, we do not want it to be called twice, as this could create strange behaviors.
                        IsDisplayed = false;
                    }
                    else
                    {
                        DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Line already hidden: nothing to do");
                        eventHandler?.Invoke(this, EventArgs.Empty);
                        IsDisplayed = false;
                    }
                }, withAnimation);*/
            }
            else
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Mutex locked - request ignored");
                args.Success = false;
                eventHandler?.Invoke(this, args);
                OnIsHidden(this, args);
            }
        }

        //bool m_mutexShow = false;

        public override void Show(EventHandler eventHandler, bool withAnimation = true)
        {
            Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();

            if (IsDisplayed == false)
            {
                // Disable the box collider: not needed here:
                //transform.GetComponent<BoxCollider>().gameObject.SetActive(false);

                //IsDisplayed = true;

                DialogIcon.Show(delegate (System.Object o, EventArgs e)
                {
                    Path.Show(Utilities.Utility.GetEventHandlerEmpty(), false);
                    IsDisplayed = true;
                    args.Success = true;
                    eventHandler?.Invoke(this, args);
                    OnIsHidden(this, args);
                }, false);

                /*TextView.position = new Vector3(LineController.PointOrigin.x, Camera.main.transform.position.y, LineController.PointOrigin.z);
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
                });*/
            }
            else
            {
                DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Mutex locked - request ignored");
                args.Success = false;
                eventHandler?.Invoke(this, args);
                OnIsHidden(this, args);
            }
        }

        public override void ShowHelp(bool show, EventHandler callback, bool withAnimation)
        {
            /*if (show)
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

            }*/

        }
    }
}