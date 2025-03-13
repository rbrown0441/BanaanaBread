using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    // THIS IS CODE FOR AN ACTUAL BAR JUST IN CASE WE NEED TO USE THIS
    
    // public float health, maxHealth, width, height;

    // [SerializeField]
    // private RectTransform fillbar;

    // public void SetMaxHealth(float maxhealth){
    //     maxHealth = maxhealth;
    // }

    // public void SetHealth(float Health){
    //     health = Health;
    //     float newWidth = (health / maxHealth) * width;
    //     fillbar.sizeDelta = new Vector2(newWidth, height);
    // }
    public Image[] healthFruits; 
    void Start()
    {
        ResetHealth();
    }

    public void TakeDamage(int health)
    {
        if (health >= 0)
        {
            healthFruits[health].enabled = false; // Hide the last active fruit
        }
    }
    public void ResetHealth()
    {
        // Enable all fruit icons
        for(int i = 0; i < 5; i ++)
        {
            healthFruits[i].enabled = true;
        }
    }
}