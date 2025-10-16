using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHurtbox : MonoBehaviour
{
    [SerializeField] GameObject attackArea; // child with trigger + HurtOnTrigger2D

    void Awake()
    {
        if (attackArea) attackArea.SetActive(false);
    }

    // Call from Animation Events
    public void Open() { if (attackArea) attackArea.SetActive(true); }
    public void Close() { if (attackArea) attackArea.SetActive(false); }
}
