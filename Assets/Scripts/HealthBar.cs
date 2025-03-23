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

    [SerializeField] private GameObject[] healthIcons;
    void Start()
    {
        currentHealth = FindFirstObjectByType<CharacterScript>().health;
        //currentHealth = healthSprites.Length - 1;
        //ResetHealth();
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

    public void ResetHealth()
    {
        foreach (var icon in healthIcons)
        {
            Animator animator = icon.GetComponent<Animator>();
            animator.SetBool("Hurt", false);
        }
        currentHealth = FindFirstObjectByType<CharacterScript>().health;
        
        // healthImage.sprite = healthSprites[0];
        // currentHealth = healthSprites.Length - 1;
    }
    
    public void UpdateHealthIcons(int damageTaken)
    {
        // Start from the last "healthy" icon and work backward for the amount of damage taken
        for (int i = currentHealth - 1; i >= currentHealth - damageTaken; i--)
        {
            if (i >= 0 && i < healthIcons.Length) // Ensure valid index range
            {
                Animator animator = healthIcons[i].GetComponent<Animator>();
                
                animator.SetBool("Hurt", true);
            }
        }

        // Update current health
        currentHealth = Mathf.Max(currentHealth - damageTaken, 0);
    }
}