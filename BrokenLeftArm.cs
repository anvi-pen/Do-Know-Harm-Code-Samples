using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

/*
This script creates a broken left arm injury on Patient game object and manages 
all the steps to be performed to treat a broken left arm. This script is to be
attached to Broken Left Arm game object.
*/

public class BrokenLeftArm : Injury
{
    private enum States
    {
        BROKEN,
        FIXING_UPPER,
        UPPER_DONE,
        FIXING_LOWER,
        LOWER_DONE,
        SPLINT_1,
        ROTATE_SPLINT_1,
        ROTATE_SPLINT_1_IN_PROGRESS,
        SPLINT_1_DONE,
        HEALED
    }

    // Internal variables
    private AudioSource audio;
    private States curState = States.BROKEN;
    private Vector3 startMousePosition = Vector3.zero;
    private Transform upperArm;
    private Transform lowerArm;
    private float rotation = 0;
    private float rotationLeft = 0;
    private bool upperArmStarted = false;
    private bool lowerArmStarted = false;
    private bool rotation1Started = false;
    private UnityEvent onClickUpUpper = null;
    private UnityEvent onMouseOverUpper = null;
    private UnityEvent onMouseExitUpper = null;
    private UnityEvent onClickUpLower = null;
    private UnityEvent onMouseOverLower = null;
    private UnityEvent onMouseExitLower = null;

    // Required components
    [SerializeField] private AudioClip stepComplete;
    [SerializeField] private AudioClip completelyHealed;

    // Start is called before the first frame update
    void Start()
    {
        audio = GetComponent<AudioSource>();

        // Store bones used to rig Patient game object
        upperArm = gameObject.transform.parent.GetChild(0).GetChild(1);
        lowerArm = upperArm.GetChild(0);

        // Attach necessary components to make upper arm of Patient interactable
        upperArm.gameObject.AddComponent<Clickable>();
        onClickUpUpper = upperArm.gameObject.GetComponent<Clickable>().onClickUp;
        onClickUpUpper.AddListener(StopAlignmentUpper);
        onMouseOverUpper = upperArm.gameObject.GetComponent<Clickable>().onLeftClick;
        onMouseOverUpper.AddListener(StartAlignmentUpper);
        onMouseExitUpper = upperArm.gameObject.GetComponent<Clickable>().onMouseExit;
        onMouseExitUpper.AddListener(StopAlignmentUpper);

        upperArm.gameObject.AddComponent<BoxCollider2D>();
        upperArm.gameObject.GetComponent<BoxCollider2D>().size = new Vector2(2.13654f, 1.418453f);
        upperArm.gameObject.GetComponent<BoxCollider2D>().offset = new Vector2(0.5682697f, -0.02496967f);

        // Attach necessary components to make lower arm of Patient interactable
        lowerArm.gameObject.AddComponent<Clickable>();
        onClickUpLower = lowerArm.gameObject.GetComponent<Clickable>().onClickUp;
        onClickUpLower.AddListener(StopAlignmentLower);
        onMouseOverLower = lowerArm.gameObject.GetComponent<Clickable>().onLeftClick;
        onMouseOverLower.AddListener(StartAlignmentLower);
        onMouseExitLower = lowerArm.gameObject.GetComponent<Clickable>().onMouseExit;
        onMouseExitLower.AddListener(StopAlignmentLower);

        lowerArm.gameObject.AddComponent<BoxCollider2D>();
        lowerArm.gameObject.GetComponent<BoxCollider2D>().size = new Vector2(2.104039f, 1.34821f);
        lowerArm.gameObject.GetComponent<BoxCollider2D>().offset = new Vector2(0.8049988f, 0.02302724f);

        // Rotate arm of Patient to create appearance that left arm is broken
        lowerArm.Rotate(0, 0, 50);
        upperArm.Rotate(0, 0, 22.72f);

        // Disable splint hit box and splint game objects
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        gameObject.transform.GetChild(1).gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        onClickUpUpper.RemoveListener(StopAlignmentUpper);
        onMouseOverUpper.RemoveListener(StartAlignmentUpper);
        onMouseExitUpper.RemoveListener(StopAlignmentUpper);

        onClickUpLower.RemoveListener(StopAlignmentLower);
        onMouseOverUpper.RemoveListener(StartAlignmentLower);
        onMouseExitUpper.RemoveListener(StopAlignmentLower);
    }

    // Update is called once per frame
    void Update()
    {
        if (curState == States.FIXING_UPPER)
        {
            // Rotate upper arm to the correct position based on distance arm is dragged to the left
            rotation = Input.GetAxis("Mouse X");

            if (rotation > 0)
                return;

            rotationLeft += (rotation * 60);

            if (rotationLeft > 0)
            {
                upperArm.Rotate(0, 0, rotation * 60);
            }
            else
            {
                // Upper arm has been dragged to the correct position
                upperArm.localEulerAngles = new Vector3(0, 0, -167.7f);
                upperArm.gameObject.GetComponent<BoxCollider2D>().enabled = false;
                upperArm.gameObject.GetComponent<Clickable>().enabled = false;
                curState = States.UPPER_DONE;
                Debug.Log("curState = UPPER_DONE");
                CSharpTools.PlaySound(audio, stepComplete);
            }

            return;
        }

        if (curState == States.FIXING_LOWER)
        {
            // Rotate lower arm to the correct position based on distance arm is dragged to the left
            rotation = Input.GetAxis("Mouse X");

            if (rotation > 0)
                return;

            rotationLeft += (rotation * 60);

            if (rotationLeft > 0)
            {
                lowerArm.Rotate(0, 0, rotation * 60);
            }
            else
            {
                // Lower arm has been dragged to the correct position
                lowerArm.localEulerAngles = new Vector3(0, 0, 0);
                lowerArm.gameObject.GetComponent<BoxCollider2D>().enabled = false;
                lowerArm.gameObject.GetComponent<Clickable>().enabled = false;
                curState = States.LOWER_DONE;
                Debug.Log("curState = LOWER_DONE");

                gameObject.transform.GetChild(0).gameObject.SetActive(true);
                CSharpTools.PlaySound(audio, stepComplete);
            }

            return;
        }

        if (curState == States.ROTATE_SPLINT_1_IN_PROGRESS)
        {
            // Rotate splint to the correct position over the left arm based on distance splint is dragged to the left
            rotation = Input.GetAxis("Mouse X");

            if (rotation > 0)
                return;

            rotationLeft += (rotation * 30);

            if (rotationLeft > 0)
            {
                gameObject.transform.GetChild(1).Rotate(0, 0, -(rotation * 30));
            }
            else
            {
                // Splint has been dragged to the correct position
                gameObject.transform.GetChild(1).localEulerAngles = new Vector3(0, 0, 17);
                // Activate BandageCheck script, which checks if the splint has been secured with tape
                gameObject.transform.GetChild(1).GetComponent<BandageCheck>().isValid = true;
                curState = States.SPLINT_1_DONE;
                Debug.Log("curState = SPLINT_1_DONE");
                CSharpTools.PlaySound(audio, stepComplete);
            }

            return;
        }
    }

    #region Events

    // Function is called when player left clicks on the upper arm of Patient
    public void StartAlignmentUpper()
    {
        // Check if player is using the correct tool and the injury is at the desired stage
        if (GameManager.toolWheel.curTool != ToolWheel.Tools.HANDTOOL) return;
        if (curState != States.BROKEN) return;

        if (!upperArmStarted)
        {
            rotation = 0;
            rotationLeft = 22.72f;

            upperArmStarted = true;
        }

        // Proceed to the next stage of the injury
        curState = States.FIXING_UPPER;
        Debug.Log("curState = FIXING_UPPER");
    }

    // Function is called when player is no longer clicking on the upper arm of Patient
    public void StopAlignmentUpper()
    {
        if (curState != States.FIXING_UPPER) return;

        // Revert to previous stage of the injury
        curState = States.BROKEN;
        Debug.Log("curState = BROKEN");
    }

    // Function is called when player left clicks on the lower arm of Patient
    public void StartAlignmentLower()
    {
        // Check if player is using the correct tool and the injury is at the desired stage
        if (GameManager.toolWheel.curTool != ToolWheel.Tools.HANDTOOL) return;
        if (curState != States.UPPER_DONE) return;

        if (!lowerArmStarted)
        {
            rotation = 0;
            rotationLeft = 50;

            lowerArmStarted = true;
        }

        // Proceed to the next stage of the injury
        curState = States.FIXING_LOWER;
        Debug.Log("curState = FIXING_LOWER");
    }

    // Function is called when player is no longer clicking on the lower arm of Patient
    public void StopAlignmentLower()
    {
        if (curState != States.FIXING_LOWER) return;

        // Revert to previous stage of the injury
        curState = States.UPPER_DONE;
        Debug.Log("curState = UPPER_DONE");
    }

    // Function is called when player left clicks on the splint hit box
    public void StartSplinting1()
    {
        // Check if player is using the correct tool and the injury is at the desired stage
        if (curState != States.LOWER_DONE) return;
        if (GameManager.toolWheel.curTool != ToolWheel.Tools.SPLINT) return;

        // Proceed to the next stage of the injury
        curState = States.SPLINT_1;
        Debug.Log("curState = SPLINT_1");
        
        // Enable the rotatable splint and disable the splint hit box
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        gameObject.transform.GetChild(1).gameObject.SetActive(true);

        // Proceed to the next stage of the injury
        curState = States.ROTATE_SPLINT_1;
    }

    // Function is called when player is no longer clicking on the splint hit box
    public void StopSplinting1()
    {
        if (curState != States.SPLINT_1) return;

        // Revert to previous stage of the injury
        curState = States.LOWER_DONE;
        Debug.Log("curState = LOWER_DONE");
    }

    // Function is called when player left clicks on the splint
    public void StartRotatingSplint1()
    {
        // Check if player is using the correct tool and the injury is at the desired stage
        if (curState != States.ROTATE_SPLINT_1) return;
        if (GameManager.toolWheel.curTool != ToolWheel.Tools.HANDTOOL) return;
        
        if (!rotation1Started)
        {
            rotation = 0;
            rotationLeft = 17;

            rotation1Started = true;
        }

        // Proceed to the next stage of the injury
        curState = States.ROTATE_SPLINT_1_IN_PROGRESS;
        Debug.Log("curState = ROTATE_SPLINT_1_IN_PROGRESS");
    }

    // Function is called when player is no longer clicking on the splint
    public void StopRotatingSplint1()
    {
        if (curState != States.ROTATE_SPLINT_1_IN_PROGRESS) return;

        // Revert to previous stage of the injury
        curState = States.ROTATE_SPLINT_1;
        Debug.Log("curState = ROTATE_SPLINT_1");
    }

    // Function is called when the broken left arm has been completely fixed
    public void declareHealed()
    {
        curState = States.HEALED;
        healed = true;

        Debug.Log("curState = HEALED");

        CSharpTools.PlaySound(audio, completelyHealed);
    }

    #endregion
}
