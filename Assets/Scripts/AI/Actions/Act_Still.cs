using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Netherkin/AI Actions/Still")]


//Adjustment fields

// 


public class Still : AIAction
{
    [Header("Phase weighting (multiplies baseWeight)")]
    public float dayMul = 0.5f;
    public float twilightMul = 1.0f;
    public float nightMul = 2.0f;


    [Header("Idle variance travel distance")]
    public float idlevariance = 0f;     // speed variance in idle animation
    public float pacedistance = 0.3f;     // how fast the sway target moves
   

    [Header("Animation (optional)")]
    public string SunflowerwalkBool = "Idle";











}
