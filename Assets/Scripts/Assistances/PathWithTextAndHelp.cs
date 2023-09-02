using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace MATCH.Assistances
{
    public class PathWithTextAndHelp : Assistance, IAssistance
    {
        readonly Transform HelpController;
        readonly Transform LineView;
        readonly Transform TextView;

        //readonly Dialogs.Dialog1 TextController;
        //readonly LineToObject LineController;

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
            Dialog = Assistances.Factory.Instance.CreateDialog2NoButton(title, description, transform);
            DialogBackground = (Decorators.BackgroundColorMessage2)Assistances.Decorators.Factory.Instance.CreateBackgroundMessage(Dialog, Utilities.Materials.Colors.Cyan);
            DialogIcon = (Decorators.BackgroundColorIcon2)Assistances.Decorators.Factory.Instance.CreateBackgroundIcon(DialogBackground, Utilities.Materials.Colors.Cyan, true);

            //Transform destination = GameObject.Find("MATCH").transform.Find("Cube");
            //Transform destination = MATCH.Assistances.InteractionSurfaceFollower.Instance.GetInteractionSurface().transform;

            // Path
            //Path = Assistances.Factory.Instance.CreatePathFinding(name + "_Path", destination, lineDestination, transform);
            Path = Assistances.Factory.Instance.CreatePathFindingWithFollowerBegin(name + "_Path", lineDestination, transform);
            Path.SetHeightToFollowInteractionSurface(true);
        }

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
            }
            else
            {
                DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Mutex locked - request ignored");
                args.Success = false;
                eventHandler?.Invoke(this, args);
                OnIsHidden(this, args);
            }
        }

        public override void Show(EventHandler eventHandler, bool withAnimation = true)
        {
            Utilities.EventHandlerArgs.Animation args = new Utilities.EventHandlerArgs.Animation();

            if (IsDisplayed == false)
            {
                DialogIcon.Show(delegate (System.Object o, EventArgs e)
                {
                    Path.Show(Utilities.Utility.GetEventHandlerEmpty(), false);
                    IsDisplayed = true;
                    args.Success = true;
                    eventHandler?.Invoke(this, args);
                    OnIsHidden(this, args);
                }, false);
            }
            else
            {
                DebugMessagesManager.Instance.DisplayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Warning, "Mutex locked - request ignored");
                args.Success = false;
                eventHandler?.Invoke(this, args);
                OnIsHidden(this, args);
            }
        }

        public override void ShowHelp(bool show, EventHandler callback, bool withAnimation)
        {

        }

            public override void Emphasize(bool enable)
        {
            DialogBackground.Emphasize(enable);
            DialogIcon.Emphasize(enable);
        }

        public Dialogs.Dialog2 GetDialog2()
        {
            return Dialog;
        }

        public Assistance GetRootDecoratedAssistance()
        {
            return this;
        }

        public Assistance GetAssistance()
        {
            return this;
        }

        public Assistance GetDecoratedAssistance()
        {
            return this;
        }

        public Transform GetSound()
        {
            return Dialog.GetSound();
        }

        public Transform GetArch()
        {
            return Dialog.GetArch();
        }

        public Icon GetIcon()
        {
            return Dialog.GetIcon() ;
        }

        public Transform GetLinePath()
        {
            return Path.GetTransform();
        }
    }
}