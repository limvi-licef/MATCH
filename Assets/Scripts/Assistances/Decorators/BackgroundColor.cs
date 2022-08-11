using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace MATCH
{
    namespace Assistances
    {
        namespace Decorators
        {
            public class BackgroundColor : Assistance, IPanel
            {
                IPanel PanelToDecorate;
                string BackgroundColorDecorated;

                /*public BackgroundColor(IPanel panelToDecorate, string backgroundColor)
                {
                    PanelToDecorate = panelToDecorate;
                    BackgroundColorDecorated = backgroundColor;
                }*/

                public void SetAssistanceToDecorate(IPanel toDecorate)
                {
                    PanelToDecorate = toDecorate;

                    // Relaying the eventhandler
                    Assistance temp = (Assistance)PanelToDecorate;
                    temp.EventHelpButtonClicked += delegate (System.Object o, EventArgs e)
                    {
                        MATCH.Utilities.EventHandlerArgs.Button args = (MATCH.Utilities.EventHandlerArgs.Button)e;

                        //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "EventHelpButtonClicked caught by decorated object. Relaying the event ...");

                        OnHelpButtonClicked(args.ButtonType);
                    };
                }

                public override void Hide(EventHandler callback)
                {
                    PanelToDecorate.GetAssistance().Hide(callback);
                }

                public override void Show(EventHandler callback)
                {
                    //DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Display assistance " + name);

                    PanelToDecorate.SetBackgroundColor(BackgroundColorDecorated);
                    PanelToDecorate.GetAssistance().Show(callback);

                    /*if (PanelToDecorate.GetAssistance().GetTransform().gameObject.activeSelf == false)
                    {
                        PanelToDecorate.SetBackgroundColor(BackgroundColorDecorated);
                        PanelToDecorate.GetAssistance().Show(callback);
                    }
                    else
                    {
                        PanelToDecorate.GetAssistance().Hide(delegate (System.Object o, EventArgs e)
                        {
                            PanelToDecorate.SetBackgroundColor(BackgroundColorDecorated);
                            PanelToDecorate.GetAssistance().Show(callback);
                        });
                    }*/ 
                }

                public override void ShowHelp(bool show, EventHandler callback)
                {
                    PanelToDecorate.GetAssistance().ShowHelp(show, callback);
                }

                public void SetBackgroundColor(string colorName)
                {
                    BackgroundColorDecorated = colorName;
                    //PanelToDecorate.SetBackgroundColor(colorName);
                }

                public void SetEdgeColor(string colorName)
                {
                    PanelToDecorate.SetEdgeColor(colorName);
                }

                public void SetEdgeThickness(float thickness)
                {
                    PanelToDecorate.SetEdgeThickness(thickness);
                }

                public void EnableWeavingHand(bool enable)
                {
                    PanelToDecorate.EnableWeavingHand(enable);
                }

                public override Transform GetTransform()
                {
                    return PanelToDecorate.GetAssistance().GetTransform();
                }

                public Assistance GetAssistance()
                {
                    return PanelToDecorate.GetAssistance();
                }

                public override bool IsDecorator()
                {
                    return true;
                }
            }
        }
    }
}


