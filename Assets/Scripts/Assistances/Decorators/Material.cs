using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

/**
 * Material will be set when calling the "show" function
 * */
namespace MATCH
{
    namespace Assistances
    {
        namespace Decorators
        {
            public class Material : Assistance, IBasic
            {
                IBasic AssistanceToDecorate;
                String MaterialName;

                public Material(IBasic assistanceToDecorate, string materialName): base()
                {
                    AssistanceToDecorate = assistanceToDecorate;
                    MaterialName = materialName;
                }

                public override Transform GetTransform()
                {
                    return AssistanceToDecorate.GetAssistance().GetTransform();
                }

                public override void Hide(EventHandler callback)
                {
                    AssistanceToDecorate.GetAssistance().Hide(callback);
                }

                public void SetMaterial(string materialName)
                {
                    AssistanceToDecorate.SetMaterial(materialName);
                }

                public override void Show(EventHandler callback)
                {
                    DebugMessagesManager.Instance.displayMessage(MethodBase.GetCurrentMethod().ReflectedType.Name, MethodBase.GetCurrentMethod().Name, DebugMessagesManager.MessageLevel.Info, "Decorated assistance is going to be shown");

                    if (AssistanceToDecorate.GetAssistance().GetTransform().gameObject.activeSelf == false)
                    {
                        AssistanceToDecorate.SetMaterial(MaterialName);
                        AssistanceToDecorate.GetAssistance().Show(callback);
                    }
                    else
                    {
                        AssistanceToDecorate.GetAssistance().Hide(delegate (System.Object o, EventArgs e)
                        {
                            AssistanceToDecorate.SetMaterial(MaterialName);
                            AssistanceToDecorate.GetAssistance().Show(callback);
                        });
                    }
                }

                public override void ShowHelp(bool show)
                {
                    AssistanceToDecorate.GetAssistance().ShowHelp(show);
                }

                public Assistance GetAssistance()
                {
                    return AssistanceToDecorate.GetAssistance();
                }

                public override bool IsDecorator()
                {
                    return true;
                }
            }
        }
    }
}