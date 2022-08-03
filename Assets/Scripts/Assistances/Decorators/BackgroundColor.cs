using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MATCH
{
    namespace Assistances
    {
        namespace Decorators
        {
            public class BackgroundColor : IPanel
            {
                IPanel PanelToDecorate;
                string BackgroundColorDecorated;

                public BackgroundColor(IPanel panelToDecorate, string backgroundColor)
                {
                    PanelToDecorate = panelToDecorate;
                    BackgroundColorDecorated = backgroundColor;
                }

                public void Hide(EventHandler callback)
                {
                    PanelToDecorate.GetAssistance().Hide(callback);
                }

                public void Show(EventHandler callback)
                {
                    PanelToDecorate.SetBackgroundColor(BackgroundColorDecorated);
                    PanelToDecorate.GetAssistance().Show(callback);
                }

                public void ShowHelp(bool show)
                {
                    PanelToDecorate.GetAssistance().ShowHelp(show);
                }

                public void SetBackgroundColor(string colorName)
                {
                    PanelToDecorate.SetBackgroundColor(colorName);
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

                public Transform GetTransform()
                {
                    return PanelToDecorate.GetAssistance().GetTransform();
                }

                public Assistance GetAssistance()
                {
                    return PanelToDecorate.GetAssistance();
                }
            }
        }
    }
}


