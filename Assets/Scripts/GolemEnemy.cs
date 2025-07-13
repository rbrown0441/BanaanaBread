using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Vector2 = UnityEngine.Vector2;

public class GolemEnemy : MonoBehaviour
{
    [SerializeField] private Vector2 spawnPoint;
    [SerializeField] private float sightRadius;
    [SerializeField] private float attackRange;
    [SerializeField] private float speed;
    [SerializeField] private int damage;
    [SerializeField] private float pushStrength;
    [SerializeField] private int health;
    [SerializeField] private GameObject player;
    [SerializeField] private Animator animator;
    private SpriteRenderer spriteRenderer; // for fade
    private CharacterScript playerScript;
    private bool chasing;
    private bool returning;
    [SerializeField] GameObject attackArea;
    [SerializeField] private bool attacking = false;
    [SerializeField] private bool isDead = false;
    [SerializeField] private GameObject MidCheckRay;
    [SerializeField] private GameObject BottomRay;
    [SerializeField] private GameObject TopCheckRay;
    [SerializeField] public GameObject Crystal;
    [SerializeField] private float rayDist = 0.4f;
    [SerializeField] private LayerMask groundLayer;
    float dirction = 0.4f;
    // [SerializeField] private float randomAttack = 0;
    float originalScale;
    [SerializeField] private float walkingOnStairsTime = 0;
    // private bool attackFrames = false;

    void Start()
    {
        spawnPoint = transform.position;
        chasing = false;
        returning = false;
        playerScript = player.GetComponent<CharacterScript>();
        attackArea.SetActive(false);
        originalScale = transform.localScale.x;
        spriteRenderer = GetComponent<SpriteRenderer>(); // for fade
    }
    
    void Update()
    {

        //   CheckonStairs();
        if ((Vector2.Distance(transform.position, player.transform.position) <= attackRange) && (!attacking))
        {

            attacking = true;
            animator.SetBool("Attacking", true);
            // randomAttack = Random.Range(0, 1);
            // if (randomAttack == 0) animator.SetBool("Attacking", true);
            // else if (randomAttack == 1) animator.SetBool("Attacking1", true);

        }

        if (!attacking)
        {
            
            if (Vector2.Distance(transform.position, player.transform.position) <= sightRadius)
            {

                chasing = true;
                returning = false;

            }
            else if (Vector2.Distance(transform.position, player.transform.position) >= sightRadius * 2 && chasing)
            {
                chasing = false;
                returning = true;

            }
            if (chasing)
            {
                dirction = Mathf.Sign( player.transform.position.x- transform.position.x );
                
                transform.localScale = new UnityEngine.Vector3(dirction * originalScale, transform.localScale.y, transform.localScale.z);
                if (!IsFasingWall(dirction))
                {
                    transform.position = Vector2.MoveTowards(transform.position, new Vector2(player.transform.position.x, transform.position.y), speed * Time.deltaTime);
                    animator.SetBool("Walking", true);
                }
                else
                    animator.SetBool("Walking", false);

                CheckForStairs(dirction);


            }
            else if (returning)
            {
                dirction = Mathf.Sign(spawnPoint.x - transform.position.x);
                transform.localScale = new UnityEngine.Vector3(dirction * originalScale, transform.localScale.y, transform.localScale.z);
                if (!IsFasingWall(dirction))
                {
                    animator.SetBool("Walking", true);
                    transform.position = Vector2.MoveTowards(transform.position, new Vector2(spawnPoint.x, transform.position.y), speed * Time.deltaTime);
                }
                else
                    animator.SetBool("Walking", false);


                CheckForStairs(dirction);

                if (Vector2.Distance(transform.position, spawnPoint) < 0.2f)
                {
                    returning = false;
                    animator.SetBool("Walking", false);
                }

            }
        }
    
    }
    void OnCollisionEnter2D(Collision2D collider)
    {
        Debug.Log($"Getting Attacked: {playerScript.IsAttacking}");
        if (collider.gameObject == player && playerScript.IsAttacking) StartCoroutine(TakeDamage());
        else DamagePlayer(collider.gameObject);
    }
    void OnCollisionStay2D(Collision2D collider)
    {
        if (collider.gameObject == player && !playerScript.IsAttacking) DamagePlayer(collider.gameObject);
    }
    void DamagePlayer(GameObject gameObject)
    {
        if (isDead) return;
        if (gameObject == player)
        {
            Vector2 pushDirection = gameObject.transform.position - transform.position;
            pushDirection.Normalize();
            playerScript.Hurt(damage, pushDirection * pushStrength);
        }
    }

    private IEnumerator TakeDamage()
    {
        var renderer = GetComponent<SpriteRenderer>();
        Color originalColor = renderer.color;

        renderer.color = Color.red;

        yield return new WaitForSeconds(0.15f);

        renderer.color = originalColor;

        //yield return new WaitForSeconds(0.1f);

        health -= 1;
        
        if (health < 1) StartCoroutine(Die());
    }

    IEnumerator Die()
    {
        isDead = true;
        animator.SetTrigger("Death");
        // BoxCollider2D collider = GetComponent<BoxCollider2D>();
        // collider.size = new Vector2(1.4f, 0.4f);     // Width, Height
        // collider.offset = new Vector2(0f, -0.8f);
        // Rigidbody2D rb = GetComponent<Rigidbody2D>();
        // rb.bodyType = RigidbodyType2D.Kinematic;
        // rb.velocity = Vector2.zero;
        // rb.angularVelocity = 0f;
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length + 0.2f); //no fade

        yield return StartCoroutine(FadeOut(1.5f)); // fade out over 1.5 seconds 
        
        Instantiate(Crystal, transform.position, Crystal.transform.rotation);
        Destroy(gameObject);
        
    }
     IEnumerator FadeOut(float duration)
    {
        float startAlpha = spriteRenderer.color.a;
        float rate = 1.0f / duration;
        float progress = 0f;

        while (progress < 1.0f)
        {
            Color tmpColor = spriteRenderer.color;
            spriteRenderer.color = new Color(tmpColor.r, tmpColor.g, tmpColor.b, Mathf.Lerp(startAlpha, 0, progress));
            progress += rate * Time.deltaTime;
            yield return null;
        }

        // Final alpha set to 0
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
    }

    public void EndAttack()
    {
        attacking = false;
        animator.SetBool("Attacking", false);
        // if (randomAttack == 0) animator.SetBool("Attacking", false);
        // else if (randomAttack == 1) animator.SetBool("Attacking1", false);
    }

    public void AttackWindowOpen()
    {
        attackArea.SetActive(true);
    }

    public void AttackWindowClose()
    {
        attackArea.SetActive(false);
    }
    void CheckForStairs(float direction)
    {
        RaycastHit2D botCheckRay = Physics2D.Raycast(BottomRay.transform.position, new UnityEngine.Vector3(direction, 0, 0), rayDist, groundLayer); //check if there a stair at the characters feet
        if (botCheckRay)
        {
            RaycastHit2D midCheckRay = Physics2D.Raycast(MidCheckRay.transform.position, new UnityEngine.Vector3(direction, 0, 0), rayDist, groundLayer); //check if it's not the wall option
            if ((!midCheckRay) || (midCheckRay.distance - botCheckRay.distance > 0.05))
            {

                walkingOnStairsTime = 0.5f;
                UnityEngine.Vector3 heightSeekerPos = MidCheckRay.transform.position + direction * new UnityEngine.Vector3(botCheckRay.distance + 0.01f, 0, 0);
                RaycastHit2D heightSeekerRay = Physics2D.Raycast(heightSeekerPos, UnityEngine.Vector3.down, 5, groundLayer);

                float upDist = MidCheckRay.transform.position.y - BottomRay.transform.position.y - heightSeekerRay.distance;
                transform.position += new UnityEngine.Vector3(direction * 0.03f, upDist * 1.1f, 0);
            }
        }
    }

    bool IsFasingWall(float direction)
    {
        RaycastHit2D botCheckRay = Physics2D.Raycast(BottomRay.transform.position, new UnityEngine.Vector3(direction, 0, 0), rayDist, groundLayer);
        RaycastHit2D midCheckRay = Physics2D.Raycast(MidCheckRay.transform.position, new UnityEngine.Vector3(direction, 0, 0), rayDist, groundLayer);//check if there a stair at the characters feet
        return ((botCheckRay && midCheckRay) && (midCheckRay.distance - botCheckRay.distance <= 0.05));
       
    }
}
