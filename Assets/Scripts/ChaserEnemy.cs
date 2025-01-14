using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Vector2 = UnityEngine.Vector2;

public class ChaserEnemy : MonoBehaviour
{
    [SerializeField] private Vector2 spawnPoint;
    [SerializeField] private float sightRadius;
    [SerializeField] private float attackRange;
    [SerializeField] private float speed;
    [SerializeField] private int damage;
    [SerializeField] private float pushStrength;
    [SerializeField] private GameObject player;
    [SerializeField] private Animator animator;
    private CharacterScript playerScript;
    private bool chasing;
    private bool returning;
    [SerializeField] GameObject attackArea;
    [SerializeField] private bool attacking = false;
    [SerializeField] private GameObject MidCheckRay;
    [SerializeField] private GameObject BottomRay;
    [SerializeField] private GameObject TopCheckRay;
    [SerializeField] private float rayDist = 0.4f;
    [SerializeField] private LayerMask groundLayer;
    float dirction = 0.4f;
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
    }
    
    void Update()
    {
     //   CheckonStairs();
        if ((Vector2.Distance(transform.position, player.transform.position) <= attackRange) && (!attacking))
        {
            attacking = true;
            animator.SetTrigger("Attack");
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
                dirction = Mathf.Sign(spawnPoint.x - transform.position.x  );
                transform.localScale = new UnityEngine.Vector3(dirction * originalScale, transform.localScale.y, transform.localScale.z);
                if (!IsFasingWall(dirction))
                {
                    animator.SetBool("Walking", true);
                    transform.position = Vector2.MoveTowards(transform.position, new Vector2(spawnPoint.x, transform.position.y), speed * Time.deltaTime);
                }
                else
                    animator.SetBool("Walking", false);


                CheckForStairs(dirction);

                if (Vector2.Distance(transform.position, spawnPoint) < 0.1f)
                {
                    returning = false;
                    animator.SetBool("Walking", false);
                }

            }
        }
    
    }
    void OnCollisionEnter2D(Collision2D collider)
    {
        DamagePlayer(collider.gameObject);
    }
    void OnCollisionStay2D(Collision2D collider)
    {
        DamagePlayer(collider.gameObject);
    }
    void DamagePlayer(GameObject gameObject)
    {
        if (gameObject == player)
        {
            Vector2 pushDirection = gameObject.transform.position - transform.position;
            pushDirection.Normalize();
            playerScript.Hurt(damage, pushDirection * pushStrength);
        }
    }

    public void EndAttack()
    {
        attacking = false;
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
