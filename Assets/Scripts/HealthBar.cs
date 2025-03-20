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
    public Image healthImage; 
    public Sprite[] healthSprites;
     private int currentHealth;
    void Start()
    {
        currentHealth = healthSprites.Length - 1;
        ResetHealth();
    }

    public void TakeDamage()
    {
        if (currentHealth > 0)
        {
            currentHealth--; 
            UpdateHealth();
        }
    }
    public void UpdateHealth()
    {
        if (currentHealth >= 0 && currentHealth < healthSprites.Length)
        {
            healthImage.sprite = healthSprites[currentHealth]; 
        }
    }

    public void ResetHealth(){
        healthImage.sprite = healthSprites[0];
        currentHealth = healthSprites.Length - 1;
    }
}