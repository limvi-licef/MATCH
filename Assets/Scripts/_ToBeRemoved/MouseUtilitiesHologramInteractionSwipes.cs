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
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Linq;

/**
 * Kind of obsolete try for a post-wimp interaction. 
 * Displays a cube with 4 cubes around it. Then the idea is to grab the middle cube and move to one of the surrounding cube to trigger en event.
 * Maybe interesting: manages transparency, i.e. if the user is far away, the cube will have a high alpha, that will reduce linearly as the user comes closer. 
 * */
public class MouseUtilitiesHologramInteractionSwipes : MonoBehaviour
{
    bool HologramsAlreadyDisplayed;
    Vector3 m_ParentOrigin;

    public event EventHandler m_utilitiesInteractionSwipesEventOk;
    public event EventHandler m_utilitiesInteractionSwipesEventNok;
    public event EventHandler m_utilitiesInteractionSwipesEventHelp;
    public event EventHandler m_utilitiesInteractionSwipesEventReminder;

    //MouseUtilitiesAnimation m_animatorParent;

    public enum GradationState
    {
        Default = 0,
        VividColor = 1,
        SpatialAudio = 2
    }

    GradationState m_gradationState = GradationState.Default;

    enum InteractionState
    {
        InteractionStateParentStandBy = 0,      // Nothing happens with the parent
        InteractionStateParentMoveStart = 1,    // The parent is starting to be moved
        InteractionStateParentMoveOngoing = 2,  // The parent is being moved
        InteractionStateParentMoveEnd = 3,      // The parent is released
        InteractionStateParentHitNothing = 4,   // When the parent is released, it does not hit anything
        InteractionStateParentHitOk = 5,        // When the parent is released, it hits the ok button
        InteractionStateParentHitNok = 6,       // When the parent is released, it hits the nok button
        InteractionStateParentHitHelp = 7,       // When the parent is released, it hits the help button
        InteractionStateParentHitReminder = 8,
        InteractionStateParentAnimationOngoing = 9,
        InteractionStateParentAnimationFinished = 10,
        InteractionStateMenuAnimationOngoing = 11,
        InteractionStateMenuAnimationFinished = 12
    }

    public MouseUtilitiesInteractionHologram HologramNok;
    public MouseUtilitiesInteractionHologram HologramOk;
    public MouseUtilitiesInteractionHologram m_hologramHelp;
    public MouseUtilitiesInteractionHologram m_hologramReminder;
    MouseUtilitiesInteractionHologram m_hologramHelpArrow;
    MouseUtilitiesInteractionHologram HologramNokArrow;
    MouseUtilitiesInteractionHologram HologramOkArrow;
    MouseUtilitiesInteractionHologram m_hologramReminderArrow;

    InteractionState ObjectStatus;

    public GameObject m_light;

    public float m_distanceToDisplayInteractionsHolograms = 4.0f;
    float m_transparencyManagementLinearFunctionCoefficientA;
    float m_transparencyManagementLinearFunctionCoefficientB;
    float m_transparencyManagementMin = 0.1f;
    float m_transparencyManagementMax = 1.0f;
    float m_transparencyManagementDistanceMin = 1.5f;


    enum InteractionHologramsId
    {
        Ok = 0,
        Nok = 1,   
        Help = 2,
        OkArrow = 3,
        NokArrow = 4,
        HelpArrow = 5
    }

    Dictionary<String,  MouseUtilitiesInteractionHologram> InteractionHolograms;

    bool OneShotActivated;

    // Start is called before the first frame update
    void Start()
    {
        OneShotActivated = false;

        InteractionHolograms = new Dictionary<string, MouseUtilitiesInteractionHologram>();

        ObjectStatus = InteractionState.InteractionStateParentStandBy; // To start, nothing is happening with the object, so stand by status.

        HologramsAlreadyDisplayed = false;

        // Instanciate Nok hologram
        MouseUtilitiesInteractionHologram.HologramPositioning tempRelativePositioning = MouseUtilitiesInteractionHologram.HologramPositioning.Bottom;
        HologramNok = new MouseUtilitiesInteractionHologram(gameObject, MATCH.Utilities.Materials.Textures.Refuse, tempRelativePositioning, convertRelativeToConcretePosition(tempRelativePositioning),
            convertPositionRelativeToScaling(tempRelativePositioning),
            convertPositionRelativeToRotation(tempRelativePositioning), false);

        Vector3 pos = convertRelativeToConcretePosition(tempRelativePositioning);
        pos.y = (pos.y + 1.0f) / 2.0f - 0.75f; // Locating the arrow at mid-distance between the edge (hence the "1.0f") parent's hologram and the action hologram 
        Vector3 rotation = new Vector3(0.0f, 0.0f, 90.0f);
        HologramNokArrow = new MouseUtilitiesInteractionHologram(gameObject, MATCH.Utilities.Materials.Textures.ArrowProgressive, tempRelativePositioning, pos,
            convertPositionRelativeToScaling(tempRelativePositioning),
            rotation, false);

        // Instanciate Ok hologram
        tempRelativePositioning = MouseUtilitiesInteractionHologram.HologramPositioning.Right;
        HologramOk = new MouseUtilitiesInteractionHologram(gameObject, MATCH.Utilities.Materials.Textures.Agree, tempRelativePositioning, convertRelativeToConcretePosition(tempRelativePositioning),
            convertPositionRelativeToScaling(tempRelativePositioning),
            convertPositionRelativeToRotation(tempRelativePositioning), false);

        pos = convertRelativeToConcretePosition(tempRelativePositioning);
        pos.x = (pos.x - 1.0f) / 2.0f + 0.75f; // Locating the arrow at mid-distance between the edge (hence the "1.0f") parent's hologram and the action hologram 
        rotation = new Vector3(0.0f, 0.0f, 180.0f);
        HologramOkArrow = new MouseUtilitiesInteractionHologram(gameObject, MATCH.Utilities.Materials.Textures.ArrowProgressive, tempRelativePositioning, pos,
            convertPositionRelativeToScaling(tempRelativePositioning),
            rotation, false);

        // Instanciate help hologram
        tempRelativePositioning = MouseUtilitiesInteractionHologram.HologramPositioning.Left;
        m_hologramHelp = new MouseUtilitiesInteractionHologram(gameObject, MATCH.Utilities.Materials.Textures.Help, tempRelativePositioning, convertRelativeToConcretePosition(tempRelativePositioning),
            convertPositionRelativeToScaling(tempRelativePositioning),
            convertPositionRelativeToRotation(tempRelativePositioning), false);

        pos = convertRelativeToConcretePosition(tempRelativePositioning);
        pos.x = (pos.x + 1.0f) / 2.0f - 0.75f; // Locating the arrow at mid-distance between the edge (hence the "1.0f") parent's hologram and the action hologram 
        rotation = new Vector3(0.0f, 0.0f, 0.0f);
        m_hologramHelpArrow = new MouseUtilitiesInteractionHologram(gameObject, MATCH.Utilities.Materials.Textures.ArrowProgressive, tempRelativePositioning, pos,
            convertPositionRelativeToScaling(tempRelativePositioning),
            rotation, false);

        // Instanciate reminder hologram and the corresponding arrow
        tempRelativePositioning = MouseUtilitiesInteractionHologram.HologramPositioning.Top;
        m_hologramReminder = new MouseUtilitiesInteractionHologram(gameObject, MATCH.Utilities.Materials.Textures.Reminder, tempRelativePositioning, convertRelativeToConcretePosition(tempRelativePositioning),
            convertPositionRelativeToScaling(tempRelativePositioning),
            convertPositionRelativeToRotation(tempRelativePositioning), false);

        pos = convertRelativeToConcretePosition(tempRelativePositioning);
        pos.y = (pos.y - 1.0f) / 2.0f + 0.75f; // Locating the arrow at mid-distance between the edge (hence the "1.0f") parent's hologram and the action hologram 
        rotation = new Vector3(0.0f, 0.0f, 270.0f);
        m_hologramReminderArrow = new MouseUtilitiesInteractionHologram(gameObject, MATCH.Utilities.Materials.Textures.ArrowProgressive, tempRelativePositioning, pos,
            convertPositionRelativeToScaling(tempRelativePositioning),
            rotation, false);

        // Get parent's origin
        m_ParentOrigin = gameObject.transform.position;

        // Set billboard to parent
        setBillboardToGameObject(true);

        // Prepare parent's animator, i.e. connect the callback. For the initialization of the different parameters, will be done when required during the process
        MATCH.Utilities.Animation animatorParent = gameObject.AddComponent<MATCH.Utilities.Animation>();
        animatorParent.AnimationSpeed = 1.0f;
        animatorParent.TriggerStopAnimation = MATCH.Utilities.Animation.ConditionStopAnimation.OnPositioning;

        // Computation of the coefficients for the transparency management
        m_transparencyManagementLinearFunctionCoefficientA = (m_transparencyManagementMin - m_transparencyManagementMax) / (m_distanceToDisplayInteractionsHolograms - m_transparencyManagementDistanceMin);
        m_transparencyManagementLinearFunctionCoefficientB = m_transparencyManagementMax - m_transparencyManagementLinearFunctionCoefficientA * m_transparencyManagementDistanceMin;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeSelf && OneShotActivated == false)
        {
            OneShotActivated = true;

            MATCH.DebugMessagesManager.Instance.displayMessage("MouseUtilitiesHologramInteractionSwipes", "Update", MATCH.DebugMessagesManager.MessageLevel.Info, "Just been displayed");
        }


        if (ObjectStatus == InteractionState.InteractionStateParentStandBy)
        { // No interactions are happening with the parent. The person might be moving around, and so the "reference" position of the surrounding cubes (i.e. being used above when the parent is being moved by hand) is updated

            if (Vector3.Distance(gameObject.transform.position, Camera.main.transform.position) < m_distanceToDisplayInteractionsHolograms && HologramsAlreadyDisplayed == false)
            {
                HologramsAlreadyDisplayed = true;

                interactionHologramsDisplay(HologramsAlreadyDisplayed);
            }
            else if (Vector3.Distance(gameObject.transform.position, Camera.main.transform.position) < m_distanceToDisplayInteractionsHolograms && HologramsAlreadyDisplayed) // Update transparency of the holograms according to the distance of the user, in order to have a fading effect
            {
                // Equation of type y = ax+b so that when the distance is 2.5, transparency is equal to 0.1, and gradually goes to 1 at a distance of 1.
                float t = m_transparencyManagementLinearFunctionCoefficientA * Vector3.Distance(gameObject.transform.position, Camera.main.transform.position) + m_transparencyManagementLinearFunctionCoefficientB;

                interactionHologramsTransparency(t);
            }
            else if (Vector3.Distance(gameObject.transform.position, Camera.main.transform.position) > m_distanceToDisplayInteractionsHolograms && HologramsAlreadyDisplayed)
            {
                // Time to hide the holograms and set the status boolean to false
                HologramsAlreadyDisplayed = false;
                interactionHologramsDisplay(HologramsAlreadyDisplayed);
            }

            m_ParentOrigin = gameObject.transform.position;

            setBillboardToGameObject(true);

            interactionHologramBackupOriginWorld();
            interactionHologramBackupRotation();
        }
        else if (ObjectStatus == InteractionState.InteractionStateParentMoveStart)
        {
            ObjectStatus = InteractionState.InteractionStateParentMoveOngoing;
            setBillboardToGameObject(false);
        }
        else if (ObjectStatus == InteractionState.InteractionStateParentMoveOngoing)
        { // If the parent is being moved by the hand, two processes to do here ...
          // First : the parent's position has to be updated in case it reaches the "boundaries" it can be moved to
            Vector3 parentPosition = gameObject.transform.position;

            for (int p = 0; p < 3; p ++)
            {
                if (parentPosition[p] <= m_ParentOrigin[p] -0.5f)
                {
                    parentPosition[p] = m_ParentOrigin[p] - 0.5f;
                }

                if (parentPosition[p] >= m_ParentOrigin[p] + 0.5f)
                {
                    parentPosition[p] = m_ParentOrigin[p] + 0.5f;
                }
            }

            gameObject.transform.position = parentPosition;

            // Second: the surrounding cubes are kept in their original position and rotation.
            interactionHologramsUpdatePositionOriginWorld();
            interactionHologramUpdateRotation();
        }
        else if (ObjectStatus == InteractionState.InteractionStateParentMoveEnd) 
        {
            // If one of the following three conditions is true, no animation: the whole hologram disapears. If the else condition is reached, that means no interaction occured, and thus the animation can run
            if (Vector3.Distance(gameObject.transform.position, HologramNok.getPositionWorld()) < 0.15f)
            {
                ObjectStatus = InteractionState.InteractionStateParentHitNok;
            }
            else if (Vector3.Distance(gameObject.transform.position, HologramOk.getPositionWorld()) < 0.15f)
            {
                ObjectStatus = InteractionState.InteractionStateParentHitOk;
            }
            else if (Vector3.Distance(gameObject.transform.position, m_hologramHelp.getPositionWorld()) < 0.15f)
            {
                ObjectStatus = InteractionState.InteractionStateParentHitHelp;
            }
            else if (Vector3.Distance(gameObject.transform.position, m_hologramReminder.getPositionWorld()) < 0.15f)
            {
                ObjectStatus = InteractionState.InteractionStateParentHitReminder;
            }
            else
            { // If we reach this point, that means nothing has been hit when releasing the parent's object
                ObjectStatus = InteractionState.InteractionStateParentHitNothing;
            }
        }
        else if (ObjectStatus == InteractionState.InteractionStateParentHitNothing)
        {
            MATCH.DebugMessagesManager.Instance.displayMessage("MouseUtilitiesHologramInteractionSwipes", "Update", MATCH.DebugMessagesManager.MessageLevel.Info, "Nothing was hit: bring the cube back to the center, i.e. from " + gameObject.transform.position.ToString() +  " to " + m_ParentOrigin.ToString());

            // Start the animation: be careful, the animation run on another object, so some processes will run on the callback called by the dedicated object when the animation is finished
            MATCH.Utilities.Animation animatorParent = gameObject.GetComponent<MATCH.Utilities.Animation>();
            animatorParent.PositionEnd = m_ParentOrigin;
            animatorParent.TriggerStopAnimation = MATCH.Utilities.Animation.ConditionStopAnimation.OnPositioning;
            animatorParent.ScalingEnd = gameObject.transform.localScale;

            ObjectStatus = InteractionState.InteractionStateParentAnimationOngoing;

            animatorParent.StartAnimation();
        }
        else if (ObjectStatus == InteractionState.InteractionStateParentAnimationOngoing)
        {
            interactionHologramsUpdatePositionOriginWorld();
            interactionHologramUpdateRotation();
        }
        else if (ObjectStatus == InteractionState.InteractionStateParentAnimationFinished)
        {
            ObjectStatus = InteractionState.InteractionStateParentStandBy;

            gameObject.transform.position = m_ParentOrigin;

            interactionHologramLocalPosition();

            setBillboardToGameObject(true);
        }
        else if (ObjectStatus == InteractionState.InteractionStateParentHitNok ||
            ObjectStatus == InteractionState.InteractionStateParentHitOk ||
            ObjectStatus == InteractionState.InteractionStateParentHitHelp ||
            ObjectStatus == InteractionState.InteractionStateParentHitReminder)
        {
            // Animate to make the parent's cube disapearing "in" the touched hologram.i.e. adding one animation per hologram
            interactionHologramsAnimations(3.0f, new Vector3(0.001f, 0.001f, 0.001f), 0.01f);

            MATCH.Utilities.Animation animatorParent = gameObject.GetComponent<MATCH.Utilities.Animation>();
            animatorParent.TriggerStopAnimation = MATCH.Utilities.Animation.ConditionStopAnimation.OnScaling;
            animatorParent.ScalingEnd = new Vector3(0.001f, 0.001f, 0.001f);
            animatorParent.AnimationSpeed = 3.0f;
            animatorParent.Scalingstep.x = 0.01f;
            animatorParent.Scalingstep.y = 0.01f;
            animatorParent.Scalingstep.z = 0.01f;

            // Start the animations
            interactionHologramStartAnimation();
            animatorParent.StartAnimation();

            if (ObjectStatus == InteractionState.InteractionStateParentHitNok)
            {
                animatorParent.PositionEnd = HologramNok.getPositionWorld();
                HologramNok.setTouched(true);
            }
            else if (ObjectStatus == InteractionState.InteractionStateParentHitOk)
            {
                animatorParent.PositionEnd = HologramOk.getPositionWorld();
                HologramOk.setTouched(true);
            }
            else if (ObjectStatus == InteractionState.InteractionStateParentHitHelp)
            {
                animatorParent.PositionEnd = m_hologramHelp.getPositionWorld();
                m_hologramHelp.setTouched(true);
            }
            else if (ObjectStatus == InteractionState.InteractionStateParentHitReminder)
            {
                animatorParent.PositionEnd = m_hologramReminder.getPositionWorld();
                m_hologramReminder.setTouched(true);
            }

            ObjectStatus = InteractionState.InteractionStateMenuAnimationOngoing;
        }
        else if (ObjectStatus == InteractionState.InteractionStateMenuAnimationOngoing)
        {
            interactionHologramsUpdatePositionOriginWorld();
            interactionHologramUpdateRotation();
        }
        else if ( ObjectStatus == InteractionState.InteractionStateMenuAnimationFinished)
        {
            // Animation is finished: inform the rest of the world that the button has been hit and back to stand by mode
            if(HologramNok.isTouched())
            {
                m_utilitiesInteractionSwipesEventNok?.Invoke(this, EventArgs.Empty);
            }
            else if (HologramOk.isTouched())
            {
                m_utilitiesInteractionSwipesEventOk?.Invoke(this, EventArgs.Empty);
            }
            else if (m_hologramHelp.isTouched())
            {
                m_utilitiesInteractionSwipesEventHelp?.Invoke(this, EventArgs.Empty);
            }
            else if (m_hologramReminder.isTouched())
            {
                m_utilitiesInteractionSwipesEventReminder?.Invoke(this, EventArgs.Empty);
            }
            else {
                MATCH.DebugMessagesManager.Instance.displayMessage("MouseUtilitiesHologramInteractionSwipes", "Update", MATCH.DebugMessagesManager.MessageLevel.Error, "The state InteractionStateMenuAnimationFinished should not be invoked if no hologram's menus touched bool are set to true");
            }

            gameObject.SetActive(false);

            resetHolograms();
            ObjectStatus = InteractionState.InteractionStateParentStandBy;
        }
        else
        {
            MATCH.DebugMessagesManager.Instance.displayMessage("MouseUtilitiesHologramInteractionSwipes", "Update", MATCH.DebugMessagesManager.MessageLevel.Error, "At least one state is not managed. This will likely cause this part of the software to fail.");
        }
    }

    public void callbackParentIsMoved()
    {
        ObjectStatus = InteractionState.InteractionStateParentMoveStart;
    }

    public void callbackParentReleased()
    {
        MATCH.DebugMessagesManager.Instance.displayMessage("MouseUtilitiesHologramInteractionSwipes", "callbackParentReleased", MATCH.DebugMessagesManager.MessageLevel.Info, "Called");

        ObjectStatus = InteractionState.InteractionStateParentMoveEnd;
    }

    void setBillboardToGameObject(bool add)
    {
        if (add)
        {
            // Restore billboard
            if (gameObject.GetComponent<Billboard>() == null)
            {
                gameObject.AddComponent(typeof(Billboard));
                if (gameObject.GetComponent<Billboard>() != null)
                {
                    gameObject.GetComponent<Billboard>().PivotAxis = PivotAxis.Y;
                    MATCH.DebugMessagesManager.Instance.displayMessage("MouseUtilitiesHologramInteractionSwipes", "setBillboardToParent", MATCH.DebugMessagesManager.MessageLevel.Info, "Billboard component has been successfully added");
                }
            }
        }
        else
        {
            // Disable billboard
            if (gameObject.GetComponent<Billboard>() != null)
            {
                Destroy(gameObject.GetComponent<Billboard>());
                if (gameObject.GetComponent<Billboard>() == null)
                {
                    MATCH.DebugMessagesManager.Instance.displayMessage("MouseUtilitiesHologramInteractionSwipes", "setBillboardToParent", MATCH.DebugMessagesManager.MessageLevel.Info, "Billboard component has been successfully destroyed");
                }
            }
        }
    }

    Vector3 convertRelativeToConcretePosition(MouseUtilitiesInteractionHologram.HologramPositioning positionRelative)
    {
        Vector3 toReturn;

        if (positionRelative == MouseUtilitiesInteractionHologram.HologramPositioning.Bottom)
        {
            toReturn = new Vector3(0, -1.5f, 0);
        }
        else if (positionRelative == MouseUtilitiesInteractionHologram.HologramPositioning.Left)
        {
            toReturn = new Vector3(-1.5f, 0, 0);
        }
        else if (positionRelative == MouseUtilitiesInteractionHologram.HologramPositioning.Right)
        {
            toReturn = new Vector3(1.5f, 0, 0);
        }
        else if (positionRelative == MouseUtilitiesInteractionHologram.HologramPositioning.Top)
        {
            toReturn = new Vector3(0, 1.5f, 0);
        }
        else
        {
            toReturn = new Vector3(-1, -1, -1);
        }

        return toReturn;
    }

    Vector3 convertPositionRelativeToScaling(MouseUtilitiesInteractionHologram.HologramPositioning positionRelative)
    {
        Vector3 toReturn;

        if (positionRelative == MouseUtilitiesInteractionHologram.HologramPositioning.Bottom || positionRelative == MouseUtilitiesInteractionHologram.HologramPositioning.Left || positionRelative == MouseUtilitiesInteractionHologram.HologramPositioning.Right || positionRelative == MouseUtilitiesInteractionHologram.HologramPositioning.Top)
        {
            toReturn = new Vector3(0.3f, 0.3f, 0.01f);
        }
        else
        {
            toReturn = new Vector3(-1, -1, -1);
        }

        return toReturn;
    }

    Vector3 convertPositionRelativeToRotation(MouseUtilitiesInteractionHologram.HologramPositioning positionRelative)
    {
        Vector3 toReturn;

        if (positionRelative == MouseUtilitiesInteractionHologram.HologramPositioning.Bottom)
        {
            toReturn = new Vector3(0, 0, 180);
        }
        else if (positionRelative == MouseUtilitiesInteractionHologram.HologramPositioning.Left)
        {
            toReturn = new Vector3(0, 0, 180);
        }
        else if(positionRelative == MouseUtilitiesInteractionHologram.HologramPositioning.Right)
        {
            toReturn = new Vector3(0, 0, 180);
        }
        else if (positionRelative == MouseUtilitiesInteractionHologram.HologramPositioning.Top)
        {
            toReturn = new Vector3(0, 0, 180);
        }
        else
        {
            toReturn = new Vector3(-1, -1, -1);
        }

        return toReturn;
    }

    void resetHolograms()
    {
        interactionHologramRemoveAnimationComponent();

        gameObject.transform.position = m_ParentOrigin;
        gameObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

        setGradationState(GradationState.Default);

        interactionHologramLocalPosition();

        m_hologramHelp.setScale(convertPositionRelativeToScaling(m_hologramHelp.getPositionRelativeToParent()));
        HologramNok.setScale(convertPositionRelativeToScaling(HologramNok.getPositionRelativeToParent()));
        HologramOk.setScale(convertPositionRelativeToScaling(HologramOk.getPositionRelativeToParent()));
        m_hologramReminder.setScale(convertPositionRelativeToScaling(m_hologramReminder.getPositionRelativeToParent()));        
        m_hologramHelpArrow.setScale(convertPositionRelativeToScaling(m_hologramHelpArrow.getPositionRelativeToParent()));

        HologramNokArrow.setScale(convertPositionRelativeToScaling(HologramNokArrow.getPositionRelativeToParent()));
        HologramOkArrow.setScale(convertPositionRelativeToScaling(HologramOkArrow.getPositionRelativeToParent()));
        m_hologramReminderArrow.setScale(convertPositionRelativeToScaling(m_hologramReminderArrow.getPositionRelativeToParent()));

        m_hologramHelp.setTouched(false);
        HologramNok.setTouched(false);
        HologramOk.setTouched(false);
        m_hologramReminder.setTouched(false);
    }

    void callbackAnimationParentFinished(object sender, EventArgs e)
    {
        ObjectStatus = InteractionState.InteractionStateParentAnimationFinished;
    }

    void callbackAnimationMenuFinished(object sender, EventArgs e)
    {
        ObjectStatus = InteractionState.InteractionStateMenuAnimationFinished;
    }

    void interactionHologramsDisplay(bool display)
    {
        MATCH.DebugMessagesManager.Instance.displayMessage("MouseUtilitiesHologramInteractionSwipes", "interactionHologramsDisplay", MATCH.DebugMessagesManager.MessageLevel.Info, "Called");

        HologramNok.setLocalRotation(convertPositionRelativeToRotation(HologramNok.getPositionRelativeToParent()));
        HologramOk.setLocalRotation(convertPositionRelativeToRotation(HologramOk.getPositionRelativeToParent()));
        m_hologramHelp.setLocalRotation(convertPositionRelativeToRotation(m_hologramHelp.getPositionRelativeToParent()));
        m_hologramReminder.setLocalRotation(convertPositionRelativeToRotation(m_hologramReminder.getPositionRelativeToParent()));

        m_hologramHelpArrow.setLocalRotation(new Vector3(0.0f, 0.0f, 0.0f));
        HologramNokArrow.setLocalRotation(new Vector3(0.0f, 0.0f, 90.0f));
        HologramOkArrow.setLocalRotation(new Vector3(0.0f, 0.0f, 180.0f));
        m_hologramReminderArrow.setLocalRotation(new Vector3(0.0f, 0.0f, 270.0f));

        HologramNok.displayHologram(display);
        HologramOk.displayHologram(display);
        m_hologramHelp.displayHologram(display);
        m_hologramReminder.displayHologram(display);

        m_hologramHelpArrow.displayHologram(display);
        HologramNokArrow.displayHologram(display);
        HologramOkArrow.displayHologram(display);
        m_hologramReminderArrow.displayHologram(display);

        interactionHologramsTransparency(0.1f);
    }

    void interactionHologramsTransparency(float transparency)
    {
        m_hologramHelp.setTransparency(transparency);
        HologramNok.setTransparency(transparency);
        HologramOk.setTransparency(transparency);
        m_hologramReminder.setTransparency(transparency);

        m_hologramHelpArrow.setTransparency(transparency);
        HologramNokArrow.setTransparency(transparency);
        HologramOkArrow.setTransparency(transparency);
        m_hologramReminderArrow.setTransparency(transparency);
    }

    void interactionHologramsUpdatePositionOriginWorld()
    {
        HologramOk.updatePositionOriginWorld();
        HologramNok.updatePositionOriginWorld();
        m_hologramHelp.updatePositionOriginWorld();
        m_hologramReminder.updatePositionOriginWorld();

        m_hologramHelpArrow.updatePositionOriginWorld();
        HologramNokArrow.updatePositionOriginWorld();
        HologramOkArrow.updatePositionOriginWorld();
        m_hologramReminderArrow.updatePositionOriginWorld();
    }

    void interactionHologramsAnimations(float animationSpeed, Vector3 scalingEnd, float scalingStep)
    {
        HologramNok.addAnimationComponent(animationSpeed, scalingEnd, scalingStep);
        HologramOk.addAnimationComponent(animationSpeed, scalingEnd, scalingStep);
        m_hologramHelp.addAnimationComponent(animationSpeed, scalingEnd, scalingStep);
        m_hologramReminder.addAnimationComponent(animationSpeed, scalingEnd, scalingStep);

        m_hologramHelpArrow.addAnimationComponent(animationSpeed, scalingEnd, scalingStep);
        HologramNokArrow.addAnimationComponent(animationSpeed, scalingEnd, scalingStep);
        HologramOkArrow.addAnimationComponent(animationSpeed, scalingEnd, scalingStep);
        m_hologramReminderArrow.addAnimationComponent(animationSpeed, scalingEnd, scalingStep);
    }

    void interactionHologramStartAnimation()
    {
        HologramNok.startAnimation();
        m_hologramHelp.startAnimation();
        HologramOk.startAnimation();
        m_hologramReminder.startAnimation();

        m_hologramHelpArrow.startAnimation();
        HologramNokArrow.startAnimation();
        HologramOkArrow.startAnimation();
        m_hologramReminderArrow.startAnimation();
    }

    void interactionHologramLocalPosition()
    {
        HologramNok.setLocalPosition(convertRelativeToConcretePosition(HologramNok.getPositionRelativeToParent()));
        HologramOk.setLocalPosition(convertRelativeToConcretePosition(HologramOk.getPositionRelativeToParent()));
        m_hologramHelp.setLocalPosition(convertRelativeToConcretePosition(m_hologramHelp.getPositionRelativeToParent()));
        m_hologramReminder.setLocalPosition(convertRelativeToConcretePosition(m_hologramReminder.getPositionRelativeToParent()));

        Vector3 pos;
        pos = convertRelativeToConcretePosition(m_hologramHelpArrow.getPositionRelativeToParent());
        pos.x = (pos.x + 1.0f) / 2.0f - 0.75f;
        m_hologramHelpArrow.setLocalPosition(pos);
        pos = convertRelativeToConcretePosition(HologramNokArrow.getPositionRelativeToParent());
        pos.y = (pos.y + 1.0f) / 2.0f - 0.75f;
        HologramNokArrow.setLocalPosition(pos);
        pos = convertRelativeToConcretePosition(HologramOkArrow.getPositionRelativeToParent());
        pos.x = (pos.x - 1.0f) / 2.0f + 0.75f;
        HologramOkArrow.setLocalPosition(pos);
        pos = convertRelativeToConcretePosition(m_hologramReminderArrow.getPositionRelativeToParent());
        pos.y = (pos.y - 1.0f) / 2.0f + 0.75f;
        m_hologramReminderArrow.setLocalPosition(pos);
    }

    void interactionHologramBackupOriginWorld()
    {
        HologramOk.backupPositionOriginWorld();
        HologramNok.backupPositionOriginWorld();
        m_hologramHelp.backupPositionOriginWorld();
        m_hologramReminder.backupPositionOriginWorld();

        m_hologramHelpArrow.backupPositionOriginWorld();
        HologramNokArrow.backupPositionOriginWorld();
        HologramOkArrow.backupPositionOriginWorld();
        m_hologramReminderArrow.backupPositionOriginWorld();
    }

    void interactionHologramUpdateRotation()
    {
        HologramOk.updateRotation();
        HologramNok.updateRotation();
        m_hologramHelp.updateRotation();
        m_hologramReminder.updateRotation();

        m_hologramHelpArrow.updateRotation();
        HologramNokArrow.updateRotation();
        HologramOkArrow.updateRotation();
        m_hologramReminderArrow.updateRotation();
    }

    void interactionHologramBackupRotation()
    {
        HologramOk.backupRotation();
        HologramNok.backupRotation();
        m_hologramHelp.backupRotation();
        m_hologramReminder.backupRotation();

        m_hologramHelpArrow.backupRotation();
        HologramNokArrow.backupRotation();
        HologramOkArrow.backupRotation();
        m_hologramReminderArrow.backupRotation();
    }

    void interactionHologramRemoveAnimationComponent()
    {
        HologramNok.removeAnimationComponent();
        HologramOk.removeAnimationComponent();
        m_hologramHelp.removeAnimationComponent();
        m_hologramReminder.removeAnimationComponent();

        m_hologramHelpArrow.removeAnimationComponent();
        HologramNokArrow.removeAnimationComponent();
        HologramOkArrow.removeAnimationComponent();
        m_hologramReminderArrow.removeAnimationComponent();
    }

    /*
     * Returns True if a new gradation level has been enabled, false otherwise
     */
        public bool increaseAssistanceGradation()
    {
        MATCH.DebugMessagesManager.Instance.displayMessage("MouseUtilitiesHologramInteractionSwipes", "setColorToVivid", MATCH.DebugMessagesManager.MessageLevel.Info, "Called");

        bool toReturn = false;
        int maxGradation = Enum.GetValues(typeof(GradationState)).Cast<int>().Max();

        if ((int)m_gradationState == maxGradation)
        {
            toReturn = false;
        }
        else
        {
            m_gradationState++;
            toReturn = true;

            setGradationState(m_gradationState);
        }

        return toReturn;
    }

    public void setGradationState(GradationState newState)
    {
        m_gradationState = newState;

        switch (newState)
        {
            case GradationState.Default:
                gameObject.GetComponent<Renderer>().material = Resources.Load(MATCH.Utilities.Materials.Textures.Clean, typeof(Material)) as Material;
                m_light.SetActive(false);
                break;
            case GradationState.VividColor:
                gameObject.GetComponent<Renderer>().material = Resources.Load(MATCH.Utilities.Materials.Textures.CleanVivid, typeof(Material)) as Material;
                m_light.SetActive(true);
                break;
            case GradationState.SpatialAudio:
                break;
            default:
                MATCH.DebugMessagesManager.Instance.displayMessage("MouseUtilitiesHologramInteractionSwipes", "increaseAssistanceGradation", MATCH.DebugMessagesManager.MessageLevel.Warning, "This place should not be reached");
                break;
        }
    }
}

public class MouseUtilitiesInteractionHologram
{
    public enum HologramPositioning
    {
        Left = 0,
        Right = 1,
        Bottom = 2,
        Top = 3
    }

    GameObject m_hologram;
    HologramPositioning m_positionRelativeToParent;
    Vector3 m_positionOriginWorld;
    bool m_touched; // Should be set to true if touched by the main cube, false otherwise
    Quaternion m_backupRotation;

    public MouseUtilitiesInteractionHologram (GameObject parent, string materialName, HologramPositioning positionRelativeToParent, Vector3 positionLocal, Vector3 scaling, Vector3 rotation,  bool touched)
    {
        m_hologram = GameObject.CreatePrimitive(PrimitiveType.Cube);
        m_hologram.transform.SetParent(parent.transform);
        m_hologram.GetComponent<Renderer>().material = Resources.Load(materialName, typeof(Material)) as Material;
        m_hologram.transform.localPosition = positionLocal;
        m_hologram.transform.localScale = scaling;
        m_hologram.transform.localRotation = Quaternion.Euler(rotation);
        m_hologram.SetActive(false);

        m_positionRelativeToParent = positionRelativeToParent;
        m_positionOriginWorld = m_hologram.transform.position;
        m_touched = touched;
    }

    public void setScale(Vector3 scale)
    {
        m_hologram.transform.localScale = scale;
    }

    public void setTransparency(float newTransparencyFactor)
    {
        Renderer r = m_hologram.GetComponent<Renderer>();
        Color c = r.material.color;
        c.a = newTransparencyFactor;
        r.material.color = c;
    }

    public HologramPositioning getPositionRelativeToParent()
    {
        return m_positionRelativeToParent;
    }

    public void setPositionRelativeToParent(HologramPositioning hologramPositioning)
    {
        m_positionRelativeToParent = hologramPositioning;
    }

    public void setPositionOriginWorld(Vector3 positionOriginWorld)
    {
        m_positionOriginWorld = positionOriginWorld;
    }

    public void backupPositionOriginWorld()
    {
        m_positionOriginWorld = m_hologram.transform.position;
    }

    public void updatePositionOriginWorld()
    {
        m_hologram.transform.position = m_positionOriginWorld;
    }

    public void backupRotation()
    {
        m_backupRotation = m_hologram.transform.rotation;
    }

    public void updateRotation()
    {
        m_hologram.transform.rotation = m_backupRotation;
    }

    public Vector3 getPositionWorld()
    {
        return m_hologram.transform.position;
    }

    public void setLocalPosition(Vector3 localPosition)
    {
        m_hologram.transform.localPosition = localPosition;
    }

    public void setTouched(bool touched)
    {
        m_touched = touched;
    }

    public bool isTouched()
    {
        return m_touched;
    }

    public void displayHologram(bool display)
    {
        m_hologram.SetActive(display);
    }

    public void setLocalRotation(Vector3 rotation)
    {
        m_hologram.transform.localRotation = Quaternion.Euler(rotation);
    }

    public void addAnimationComponent(float animationSpeed, Vector3 scalingEnd, float scalingStep)
    {
        MATCH.Utilities.Animation animator = m_hologram.AddComponent<MATCH.Utilities.Animation>();
        animator.TriggerStopAnimation = MATCH.Utilities.Animation.ConditionStopAnimation.OnScaling;
        animator.PositionEnd = m_hologram.transform.position;
        animator.ScalingEnd = scalingEnd;
        animator.AnimationSpeed = animationSpeed;
        animator.Scalingstep.x = scalingStep;
        animator.Scalingstep.y = scalingStep;
        animator.Scalingstep.z = scalingStep;
    }

    /*
     * Returns false if the animation component is not present and that consequently the animation has not been started. Returns true otherwise
     */
    public bool startAnimation()
    {
        bool toReturn = false;

        MATCH.Utilities.Animation animator = m_hologram.GetComponent<MATCH.Utilities.Animation>();

        if (animator != null)
        {
            animator.StartAnimation();
            toReturn = true;
        }
        else
        {
            toReturn = false;
        }

        return toReturn;
    }

    public void removeAnimationComponent()
    {
        UnityEngine.Object.Destroy(m_hologram.GetComponent<MATCH.Utilities.Animation>());
    }
}