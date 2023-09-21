using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
This script should be attached to Patient game object. This script disables and enables
injuries on Patient as necessary depending on whether Patient is wearing clothes (in
other words, injuries can only be treated if no clothes are covering the injury).
Additionaly, this script enables visual indicators (markings) to appear on top of
clothing to indicate to the player where injuries on Patient are located.
*/

public class InjuryMarkings : MonoBehaviour
{
    // attach to patient object

    // private variables
    private List<GameObject> chest = new List<GameObject>();
    private List<GameObject> leftArm = new List<GameObject>();
    private List<GameObject> rightArm = new List<GameObject>();
    private List<GameObject> leftLeg = new List<GameObject>();
    private List<GameObject> rightLeg = new List<GameObject>();
    private List<GameObject> leftHand = new List<GameObject>();
    private List<GameObject> rightHand = new List<GameObject>();
    private List<GameObject> leftFoot = new List<GameObject>();
    private List<GameObject> rightFoot = new List<GameObject>();

    private GameObject topClothingObject;
    private GameObject bottomClothingObject;
    private GameObject leftHandObject;
    private GameObject rightHandObject;
    private GameObject leftFootObject;
    private GameObject rightFootObject;

    private bool chestOpen = false;
    private bool leftArmOpen = false;
    private bool rightArmOpen = false;
    private bool leftLegOpen = false;
    private bool rightLegOpen = false;
    private bool leftHandOpen = false;
    private bool rightHandOpen = false;
    private bool leftFootOpen = false;
    private bool rightFootOpen = false;

    // Start is called before the first frame update
    void Start()
    {
        int count = gameObject.transform.childCount;
        // For each injury of Patient game object
        for (int i = 0; i < count; i++)
        {
            Transform cTran = gameObject.transform.GetChild(i);
            GameObject cGame = cTran.gameObject;

            if (cGame.GetComponent<Injury>() == null)
                continue;

            // Indicate that the injury is covered by clothing
            cGame.GetComponent<Injury>().isClothed = true;

            // Deduce what area of the patient's body the injury is located
            switch (cGame.GetComponent<Injury>().injuryType)
            {
                case Injury.InTypes.CHEST:
                    chest.Add(cGame);
                    break;
                case Injury.InTypes.LEFT_ARM:
                    leftArm.Add(cGame);
                    break;
                case Injury.InTypes.RIGHT_ARM:
                    rightArm.Add(cGame);
                    break;
                case Injury.InTypes.LEFT_LEG:
                    leftLeg.Add(cGame);
                    break;
                case Injury.InTypes.RIGHT_LEG:
                    rightLeg.Add(cGame);
                    break;
                case Injury.InTypes.LEFT_HAND:
                    leftHand.Add(cGame);
                    break;
                case Injury.InTypes.RIGHT_HAND:
                    rightHand.Add(cGame);
                    break;
                case Injury.InTypes.LEFT_FOOT:
                    leftFoot.Add(cGame);
                    break;
                case Injury.InTypes.RIGHT_FOOT:
                    rightFoot.Add(cGame);
                    break;
            }
        }

        // Save Clothing game object, which stores all of the clothing game objects as children
        GameObject clothing = gameObject.transform.Find("Clothing").gameObject;
        count = clothing.transform.childCount;
        // Save the type of each clothing object
        for (int i = 0; i < count; i++)
        {
            if (clothing.transform.GetChild(i).gameObject.GetComponent<TopClothingObject>() != null)
            {
                topClothingObject = clothing.transform.GetChild(i).gameObject;
            }
            else if (clothing.transform.GetChild(i).gameObject.GetComponent<BottomClothingObject>() != null)
            {
                bottomClothingObject = clothing.transform.GetChild(i).gameObject;
            }
            else
            {
                GameObject currentChild = clothing.transform.GetChild(i).gameObject;
                int c = currentChild.transform.childCount;
                for (int j = 0; j < c; j++)
                {
                    if (currentChild.transform.GetChild(j).gameObject.GetComponent<AccessoryClothingObject>() != null)
                    {
                        string firstName = currentChild.name;
                        string secondName = currentChild.transform.GetChild(j).gameObject.name;
                        string fullName = firstName + secondName;

                        switch (fullName)
                        {
                            case "ShoesLeft":
                                leftFootObject = currentChild.transform.GetChild(j).gameObject;
                                break;
                            case "ShoesRight":
                                rightFootObject = currentChild.transform.GetChild(j).gameObject;
                                break;
                            case "GlovesLeft":
                                leftHandObject = currentChild.transform.GetChild(j).gameObject;
                                break;
                            case "GlovesRight":
                                rightHandObject = currentChild.transform.GetChild(j).gameObject;
                                break;
                        }
                    }
                }
            }
        }

        // If Patient's shirt has been removed, disable all injury markings and enable all injuries
        // located on Patient's chest and arms to be treated
        if (!topClothingObject || !topClothingObject.activeSelf)
        {
            Debug.Log("No shirt covering injuries");

            foreach (GameObject g in chest)
            {
                g.transform.Find("Marking")?.gameObject.SetActive(false);
                g.GetComponent<Injury>().isClothed = false;
            }
            chestOpen = true;

            foreach (GameObject g in leftArm)
            {
                g.transform.Find("Marking")?.gameObject.SetActive(false);
                g.GetComponent<Injury>().isClothed = false;
            }
            leftArmOpen = true;

            foreach (GameObject g in rightArm)
            {
                g.transform.Find("Marking")?.gameObject.SetActive(false);
                g.GetComponent<Injury>().isClothed = false;
            }
            rightArmOpen = true;
        }
        // If Patient's pants has been removed, disable all injury markings and enable all injuries
        // located on Patient's legs to be treated
        if (!bottomClothingObject || !bottomClothingObject.activeSelf)
        {
            Debug.Log("No pants covering injuries");

            foreach (GameObject g in leftLeg)
            {
                g.transform.Find("Marking")?.gameObject.SetActive(false);
                g.GetComponent<Injury>().isClothed = false;
            }
            leftLegOpen = true;

            foreach (GameObject g in rightLeg)
            {
                g.transform.Find("Marking")?.gameObject.SetActive(false);
                g.GetComponent<Injury>().isClothed = false;
            }
            rightLegOpen = true;
        }
        // If Patient's left glove has been removed, disable all injury markings and enable all injuries
        // located on Patient's left hand to be treated
        if (!leftHandObject || !leftHandObject.activeSelf)
        {
            foreach (GameObject g in leftHand)
            {
                g.transform.Find("Marking")?.gameObject.SetActive(false);
                g.GetComponent<Injury>().isClothed = false;
            }
            leftHandOpen = true;
        }
        // If Patient's right glove has been removed, disable all injury markings and enable all injuries
        // located on Patient's right hand to be treated
        if (!rightHandObject || !rightHandObject.activeSelf)
        {
            foreach (GameObject g in rightHand)
            {
                g.transform.Find("Marking")?.gameObject.SetActive(false);
                g.GetComponent<Injury>().isClothed = false;
            }
            rightHandOpen = true;
        }
        // If Patient's left shoe has been removed, disable all injury markings and enable all injuries
        // located on Patient's left foot to be treated
        if (!leftFootObject || !leftFootObject.activeSelf)
        {
            foreach (GameObject g in leftFoot)
            {
                g.transform.Find("Marking")?.gameObject.SetActive(false);
                g.GetComponent<Injury>().isClothed = false;
            }
            leftFootOpen = true;
        }
        // If Patient's right shoe has been removed, disable all injury markings and enable all injuries
        // located on Patient's right foot to be treated
        if (!rightFootObject || !rightFootObject.activeSelf)
        {
            foreach (GameObject g in rightFoot)
            {
                g.transform.Find("Marking")?.gameObject.SetActive(false);
                g.GetComponent<Injury>().isClothed = false;
            }
            rightFootOpen = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // If Patient's shirt has been cut or unbuttoned, disable all injury markings and enable all injuries
        // located on Patient's chest and arms as necessary to be treated
        if (!chestOpen && topClothingObject.GetComponent<TopClothingObject>().ChestAccessible)
        {
            foreach (GameObject g in chest)
            {
                g.transform.Find("Marking")?.gameObject.SetActive(false);
                g.GetComponent<Injury>().isClothed = false;
            }

            chestOpen = true;
        }
        else if (!leftArmOpen && topClothingObject.GetComponent<TopClothingObject>().LeftArmAccessible)
        {
            foreach (GameObject g in leftArm)
            {
                g.transform.Find("Marking")?.gameObject.SetActive(false);
                g.GetComponent<Injury>().isClothed = false;
            }

            leftArmOpen = true;
        }
        else if (!rightArmOpen && topClothingObject.GetComponent<TopClothingObject>().RightArmAccessible)
        {
            foreach (GameObject g in rightArm)
            {
                g.transform.Find("Marking")?.gameObject.SetActive(false);
                g.GetComponent<Injury>().isClothed = false;
            }

            rightArmOpen = true;
        }
        // If Patient's pants has been cut, disable all injury markings and enable all injuries
        // located on Patient's legs as necessary to be treated
        else if (!leftLegOpen && bottomClothingObject.GetComponent<BottomClothingObject>().LeftLegAccessible)
        {
            foreach (GameObject g in leftLeg)
            {
                g.transform.Find("Marking")?.gameObject.SetActive(false);
                g.GetComponent<Injury>().isClothed = false;
            }

            leftLegOpen = true;
        }
        else if (!rightLegOpen && bottomClothingObject.GetComponent<BottomClothingObject>().RightLegAccessible)
        {
            foreach (GameObject g in rightLeg)
            {
                g.transform.Find("Marking")?.gameObject.SetActive(false);
                g.GetComponent<Injury>().isClothed = false;
            }

            rightLegOpen = true;
        }
        // If Patient's gloves has been removed, disable all injury markings and enable all injuries
        // located on Patient's hands as necessary to be treated
        else if (!leftHandOpen && !leftHandObject.activeSelf)
        {
            foreach (GameObject g in leftHand)
            {
                g.transform.Find("Marking")?.gameObject.SetActive(false);
                g.GetComponent<Injury>().isClothed = false;
            }

            leftHandOpen = true;
        }
        else if (!rightHandOpen && !rightHandObject.activeSelf)
        {
            foreach (GameObject g in rightHand)
            {
                g.transform.Find("Marking")?.gameObject.SetActive(false);
                g.GetComponent<Injury>().isClothed = false;
            }

            rightHandOpen = true;
        }
        // If Patient's shoes has been removed, disable all injury markings and enable all injuries
        // located on Patient's feet as necessary to be treated
        else if (!leftFootOpen && !leftFootObject.activeSelf)
        {
            foreach (GameObject g in leftFoot)
            {
                g.transform.Find("Marking")?.gameObject.SetActive(false);
                g.GetComponent<Injury>().isClothed = false;
            }

            leftFootOpen = true;
        }
        else if (!rightFootOpen && !rightFootObject.activeSelf)
        {
            foreach (GameObject g in rightFoot)
            {
                g.transform.Find("Marking")?.gameObject.SetActive(false);
                g.GetComponent<Injury>().isClothed = false;
            }

            rightFootOpen = true;
        }
    }
}
