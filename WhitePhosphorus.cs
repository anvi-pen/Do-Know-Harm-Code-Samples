using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/*
This script should be attached to White Phosphorus game object and manages all
the steps to be performed to treat a white phosphorus injury. A white
phosphorus injury involves a burning piece of white phosphorus on a 2nd degree 
burn, which can turn into a 3rd degree burn if the white phosphorus is left
untreated on the 2nd degree burn for too long. The white phosphorus must first
be extinguished and removed in order to treat the burn injury underneath.
*/

public class WhitePhosphorous : Injury
{
    private enum States
    {
        UNTREATED_WP,
        EXTINGUISHED,
        REMOVAL,
        UNTREATED,
        OINTMENT_APPLIED,
        HEALED
    }

    //Internal Variables
    private AudioSource audio;
    private States curState = States.UNTREATED_WP;
    private bool applyingOintment = false;
    private float amountOintment = 0f;
    private Vector3 mousePosition;
    private FODisposal disposal;
    private GameObject activeBurn;
    private float secondsUntilReignite = 5f;
    private bool reignitePause = false;
    private float secondsUntilThiDegBurn = 10f;
    private bool burnPause = false;
    private float injurySeverityBurn = 0;

    //Required Components
    [SerializeField] private GameObject WP;
    [SerializeField] private Animator WPAnimation;
    [SerializeField] private GameObject secDegBurn;
    [SerializeField] private GameObject thiDegBurn;
    [SerializeField] private float WPSeverity;
    [SerializeField] private float secDegBurnSeverity;
    [SerializeField] private float thiDegBurnSeverity;
    [SerializeField] private DressingObject dressingObject;
    [SerializeField] private AudioClip stepComplete;
    [SerializeField] private AudioClip completelyHealed;

    //Adjustable Settings
    [Space]
    [SerializeField] private float ointmentTimeRequired = 2.5f;

    private void Start()
    {
        // Set 2nd degree burn to be the current injury underneath the white phosphorus
        audio = GetComponent<AudioSource>();

        secDegBurn.GetComponent<Collider2D>().enabled = false;
        thiDegBurn.GetComponent<Collider2D>().enabled = false;
        thiDegBurn.SetActive(false);

        activeBurn = secDegBurn;
        injurySeverityBurn = secDegBurnSeverity;

        StartCoroutine(BurnSeverity());
    }

    private void Update()
    {
        if (curState == States.REMOVAL && WP != null)
        {
            // Move White Phosphorus based on location of mouse
            mousePosition = Mouse.current.position.ReadValue();
            mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
            WP.transform.position = Vector2.Lerp(transform.position, mousePosition, 1);

            disposal = GameObject.FindGameObjectWithTag("Foreign Object Disposal").GetComponent<FODisposal>();

            // Enable burn injury once White Phosphorus has been disposed
            if (disposal.dropObjectIn)
            {
                Destroy(WP);

                activeBurn.GetComponent<Collider2D>().enabled = true;
                curState = States.UNTREATED;
                CSharpTools.PlaySound(audio, stepComplete);
            }

            return;
        }

        if (curState == States.UNTREATED)
        {
            if (applyingOintment)
            {
                amountOintment += Time.deltaTime;
            }

            // If ointment has been applied on the burn injury for a set time,
            // proceed to the next stage of the injury
            // (bandaging injury is controlled by Dressing Object script)
            if (amountOintment >= ointmentTimeRequired)
            {
                curState = States.OINTMENT_APPLIED;
                activeBurn.GetComponent<Animator>().Play("Ointmented");
                // Tell dressing object that it is usable now
                dressingObject.SetDressable();

                CSharpTools.PlaySound(audio, stepComplete);
            }
        }
    }

    //Events
    #region Events

    // Function is called when player left clicks on burning White Phosphorus
    public void ExtinguishWP()
    {
        // Check if no clothing is covering injury, player is using the correct tool, and injury is at the desired stage
        if (isClothed) return;
        if (GameManager.toolWheel.curTool != ToolWheel.Tools.THERMAL_OINTMENT) return;
        if (curState != States.UNTREATED_WP) return;

        // Proceed to the next stage of the injury
        WPAnimation.Play("Smoke");
        curState = States.EXTINGUISHED;
        gameObject.GetComponent<Injury>().SetInjurySeverity(injurySeverityBurn);
        CSharpTools.PlaySound(audio, stepComplete);
        burnPause = true;
        reignitePause = false;
        StartCoroutine(RestartIgnition());
    }

    // Function is called when player left clicks on White Phosphorus
    public void PickUpForeignObject()
    {
        // Check if player is using the correct tool and injury is at the desired stage
        if (GameManager.toolWheel.curTool != ToolWheel.Tools.FORCEPS) return;
        if (curState != States.EXTINGUISHED) return;

        // Proceed to the next stage of the injury
        WPAnimation.Play("Still");
        disposal = GameObject.FindGameObjectWithTag("Foreign Object Disposal").GetComponent<FODisposal>();
        WP.GetComponent<SpriteRenderer>().sortingLayerName = "Draggable Object";
        reignitePause = true;
        curState = States.REMOVAL;
    }

    // Function is called when player is no longer clicking on White Phosphorus
    public void StopPickUpForeignObject()
    {
        // Check if injury is at the desired stage
        if (curState != States.REMOVAL) return;

        // Revert to previous stage of the injury
        WPAnimation.Play("Smoke");
        disposal = null;
        reignitePause = false;
        curState = States.EXTINGUISHED;
    }

    // Function is called when player left clicks on Burn
    public void StartApplyingOintment()
    {
        // Check if player is using the correct tool and injury is at the desired stage
        if (GameManager.toolWheel.curTool != ToolWheel.Tools.THERMAL_OINTMENT) return;
        if (curState != States.UNTREATED) return;

        applyingOintment = true;
    }

    // Function is called when player is no longer clicking on Burn
    public void StopApplyingOintment()
    {
        // Check if injury is at the desired stage
        if (curState != States.UNTREATED) return;

        applyingOintment = false;
    }

    // Function is called when White Phosphorus injury has been completely fixed
    public void DeclareHealed()
    {
        curState = States.HEALED;
        healed = true;

        CSharpTools.PlaySound(audio, completelyHealed);
    }

    // After a set amount of time, if White Phosphorus has been extinguished but not removed,
    // White Phosphorus will reignite
    IEnumerator RestartIgnition()
    {
        float sec = 0;
        while (sec < secondsUntilReignite)
        {
            if (!reignitePause)
                sec += Time.deltaTime;
            yield return null;
        }

        Debug.Log("reignite wp");

        if (curState == States.EXTINGUISHED)
        {
            curState = States.UNTREATED_WP;
            gameObject.GetComponent<Injury>().SetInjurySeverity(WPSeverity);
            WPAnimation.Play("Ignite");
            burnPause = false;
        }
    }

    // After a set amount of time, if White Phosphorus is still burning (not extinguished),
    // the 2nd degree burn underneath the White Phosphorus will turn into a 3rd degree burn
    IEnumerator BurnSeverity()
    {
        float sec = 0;
        while (sec < secondsUntilThiDegBurn)
        {
            if (!burnPause)
                sec += Time.deltaTime;
            yield return null;
        }

        Debug.Log("third degree burn");

        if (curState == States.UNTREATED_WP && (Vector2.Distance(WP.transform.localPosition, Vector2.zero) < 0.2))
        {
            secDegBurn.SetActive(false);
            thiDegBurn.SetActive(true);
            activeBurn = thiDegBurn;
            injurySeverityBurn = thiDegBurnSeverity;
        }
    }

    #endregion
}