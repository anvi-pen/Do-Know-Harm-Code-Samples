using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*
This script should be attached to the Splint game object, which is
a child of the broken arm / leg game object. The UnityEvent complete
should be set to the declareHealed() function in the respective
broken arm / leg script.
*/

public class BandageCheck : MonoBehaviour
{
    [NonSerialized] public int tapeCount = 0;
    [NonSerialized] public bool isValid = false;

    public UnityEvent complete = new UnityEvent();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // If 2 tape pieces have been placed on the splint,
        // then the broken arm / leg injury is declared healed
        if (isValid && tapeCount == 2)
        {
            isValid = false;
            complete.Invoke();
        }
    }

    public void incrementTapeCount()
    {
        if (isValid && (tapeCount < 2))
            tapeCount++;
    }
}
