using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public Animator doorAnimation;
    // Set collision function that will be called when the player enters the trigger.
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            doorAnimation.Play("door_open");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        doorAnimation.Play("door_close");
    }
}
