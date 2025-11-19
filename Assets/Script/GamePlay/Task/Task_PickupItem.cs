using System.Collections;
using System.Collections.Generic;
using UltEvents;
using UnityEngine;

public class Task_PickupItem : Task
{
    public GameObject itemToPickup;

    public UltEvent onItemPickedUp = new UltEvent();

    public void OnDestroy()
    {
        onItemPickedUp.Invoke();
        onItemPickedUp.Clear();
    }
}
