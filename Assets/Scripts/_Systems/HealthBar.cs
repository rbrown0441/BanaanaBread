using UnityEngine;

public class HealthBarUI : MonoBehaviour
{
    private int currentHealth;

    [SerializeField] private GameObject[] healthIcons;
    void Start()
    {
        currentHealth = FindFirstObjectByType<CharacterScript>().health;
    }
    

    public void ResetHealth()
    {
        foreach (var icon in healthIcons)
        {
            Animator animator = icon.GetComponent<Animator>();
            animator.SetBool("Hurt", false);
        }
        currentHealth = FindFirstObjectByType<CharacterScript>().health;
        
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