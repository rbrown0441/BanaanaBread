using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class CharacterScript : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private BoxCollider2D GroundCheck;
    [SerializeField] private BoxCollider2D PlayerCollider;
    [SerializeField] private Animator PlayerAnimator;
    [SerializeField] private GameObject TopCheckRay;
    [SerializeField] private GameObject MidCheckRay;
    [SerializeField] private GameObject BottomRay;
    
    //[SerializeField] private GameObject GroundCheckPoint;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private float rayDist = 0.4f;

    // These values need to be played with to have desired movement feel

    [SerializeField] private float baseSpeed = 3f;
    [SerializeField] private float currentSpeed;
    [SerializeField] private float sprint = 1f;


    [SerializeField] private float jumpPower = 3.5f;//4.0f
    [SerializeField] private float groundPoundPower = -8.0f;
    [SerializeField] private float groundDecay = 0.4f;//0.6f 
    [SerializeField] private float WallJumpPowerHeight = 0.5f;
    private float WallJumpPowerLength = 0.5f;

    //not longer relevant

    //  [SerializeField] private float acceleration = 0.20f;//0.25f

    // level tools
    [SerializeField] public Vector2 spawnPoint;
    [SerializeField] private float floorBoxHeight;
    [SerializeField] public int maxHealth;
    [SerializeField] public int health;
    [SerializeField] public int maxLives;
    [SerializeField] public int lives;
    [SerializeField] TileTypes tileTypes;
    GameObject ground;
    GameObject GridObject;
    Grid grid;
    //Sound Effects
    [SerializeField] AudioClip JumpFX;
    [SerializeField] AudioClip DoubleJumpFX;
    [SerializeField] AudioClip WalkFX;
    [SerializeField] AudioClip WalkGrassFX;
    [SerializeField] AudioClip LandingFX;
    [SerializeField] AudioClip LandingGrassFX;
    AudioSource walkingFX;
    // Player Status Variables
    private float horizontalInput;
    private float verticalInput;
    private bool isJumping = false;
    private bool inInvincibleFrames = false;
    private bool pushed = false;
    private bool wallJumpReady = false;
    private bool isGrounded;

    public bool IsAttacking { get; set; } = false;
    public bool EnemyHit { get; set; } = false;

    [SerializeField] private int amountofJumps = 0;
    private float walkingOnStairsTime = 0;
    float CurrentWalljumpCd = 0;
    float WalljumpCd = 0.3f;

    [SerializeField] private UnityIntEvent OnHurt;
    [SerializeField] private UnityEvent OnDeath;
    [SerializeField] private UnityEvent OnGameOver;
    
    private void Start()
    {
        walkingFX = GetComponent<AudioSource>();
        walkingFX.clip = WalkFX;
        ground = GameObject.FindWithTag("Ground");
        GridObject = GameObject.FindWithTag("Grid");
        grid = GridObject.GetComponent<Grid>();
    }
    
    // Runs every frame
    void Update()
    {
        CheckInput();
        CheckonStairs();
        if (wallJumpReady)
            WallJump();
        else
            Jump();
    }

    // Runs every frame (physics) 
    void FixedUpdate()
    {
        CheckGrounded();
        Movement();
        ApplyFriction();
        
        //Kills player if they fall in hole
        if (transform.position.y < floorBoxHeight)
            Die();
    }

    // Reset player on death
    void Die()
    {
        health = maxHealth;
        transform.position = spawnPoint;
        lives -= 1;
        if (lives <= 0){
            OnGameOver.Invoke();
        }
        OnDeath.Invoke();
    }

    // Handles player taking damage
    public void Hurt(int damage, Vector2 pushForce)
    {
        if (!inInvincibleFrames)
        {
            pushed = true;
            health -= damage;
            if (health <= 0)
                health = 0;
            else
            {
                //reduce vertical push force when character is jumping
                //this prevents character from being launched even further vertically
                if (isJumping)
                    pushForce = new Vector2(pushForce.x, pushForce.y / 2);

                body.AddForce(pushForce, ForceMode2D.Impulse);
            }
            StartCoroutine(ShowDamage());
            
            OnHurt.Invoke(damage);
        }
    }

    // Makes player red for a moment and gives inviniciblity frames
    IEnumerator ShowDamage()
    {
        sprite.color = Color.red;
        inInvincibleFrames = true;
        yield return new WaitForSeconds(0.15f);
        pushed = false;
        yield return new WaitForSeconds(0.6f);
        sprite.color = Color.white;
        inInvincibleFrames = false;

        // Kill player if they lose all their health
        if (health == 0)
            Die();
    }

    // Handle horizontal movement (A and D or arrow keys)
    //Handles attack as well - Khitty
    void CheckInput()
    {
        /* horizontalInput = Input.GetAxis("Horizontal");
         verticalInput = Input.GetAxis("Vertical");
       
      */

        if ((Input.GetKey(KeyCode.A)) || (Input.GetKey(KeyCode.LeftArrow)))
        {
            horizontalInput = -1f;

        }
        else if ((Input.GetKey(KeyCode.D)) || (Input.GetKey(KeyCode.RightArrow)))
        {
            horizontalInput = 1f;


        }
        else
        {
            horizontalInput = 0;
        }

        if(Input.GetKey(KeyCode.DownArrow))
        {
            if((!isGrounded || EnemyHit) && !IsAttacking) StartCoroutine(GroundPound());
        }

        if(Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = baseSpeed + sprint;
        }
        else
        {
            currentSpeed = baseSpeed;
        }

        
    }

    private IEnumerator GroundPound()
    {
        IsAttacking = true;

        body.velocity = new Vector2(body.velocity.x, groundPoundPower);

        yield return new WaitUntil(() => isGrounded || EnemyHit);

        if (EnemyHit)
        {
            body.velocity = new Vector2(body.velocity.x, -groundPoundPower/2);
            yield return new WaitForSeconds(0.25f);
        }

        EnemyHit = false;
        IsAttacking = false;
    }

    void OnCollisionEnter2D(Collision2D collider)
    {
        if (collider.gameObject.tag.ToLower() == "enemy" && IsAttacking) EnemyHit = true;  
    }

    // Handle Jump and double jump
    void Jump()
    {
        if (isJumping && isGrounded && Mathf.Abs(body.velocity.y) < 0.01)
        {
            PlayerAnimator.SetBool("isJumping", false);
            isJumping = false;
            makeLandingSound(whatAmISteppingOn());
        }
        else if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && (amountofJumps > 0))
        {
            body.velocity = new Vector2(body.velocity.x, jumpPower);
            PlayerAnimator.SetTrigger("StartJump");
            PlayerAnimator.SetBool("isJumping", true);

            if (isJumping)
                SoundFXManager.Instance.playSFXClip(DoubleJumpFX, transform, 1);
            else
                SoundFXManager.Instance.playSFXClip(JumpFX, transform, 1);

            isJumping = true;

            amountofJumps--;


        }
    }

    // Handle wall jump
    void WallJump()
    {
        if (isJumping && isGrounded && Mathf.Abs(body.velocity.y) < 0.01)
        {
            PlayerAnimator.SetBool("isJumping", false);
            isJumping = false;
        }
        if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) && wallJumpReady)
        {
            float dir;
            if (transform.localScale.x < 0)
                dir = -1;
            else
                dir = 1;
            body.velocity = new Vector2(-1f * dir * WallJumpPowerLength, WallJumpPowerHeight);
            PlayerAnimator.SetTrigger("StartJump");
            PlayerAnimator.SetBool("isJumping", true);
            isJumping = true;
            CurrentWalljumpCd = WalljumpCd;
            wallJumpReady = false;
        }
    }

    // Move the player with horizontal movement and animate
    void Movement()
    {
        // old version for rolling back
        if (Mathf.Abs(horizontalInput) > 0)
        {
            //   float inc = horizontalInput * acceleration;
            //   float newSpeed = Mathf.Clamp(body.velocity.x + inc, -speed, speed);
            PlayerAnimator.SetFloat("speed", Mathf.Abs(horizontalInput));
            PlayerAnimator.SetFloat("inputMagnitude", Mathf.Abs(currentSpeed));
            // body.velocity = new Vector2(newSpeed, body.velocity.y);
            float direction = Mathf.Sign(horizontalInput);

            body.velocity = new Vector2(currentSpeed * direction, body.velocity.y);
            if ((isGrounded) || (walkingOnStairsTime > 0))
            {
                makeSteppingSound(whatAmISteppingOn());
                if (!walkingFX.isPlaying)
                    walkingFX.Play();
                CheckForStairs(direction);
            }

            CheckForWalljump(direction);
            transform.localScale = new Vector3(direction, 1, 1);
            if ((walkingFX.isPlaying) && (isJumping))
                walkingFX.Stop();
        }
        else
        {
            if (isJumping)
            {
                //reduce horizontal velocity when no horizontal input
                float bodyVelocityX = body.velocity.x - groundDecay;
                if (bodyVelocityX < 0) bodyVelocityX = 0;
                body.velocity = new Vector2(bodyVelocityX, body.velocity.y);
            }
            PlayerAnimator.SetFloat("inputMagnitude", 0.0f);
            PlayerAnimator.SetFloat("speed", 0.0f);
            walkingFX.Stop();
        }



    }

    // Stairs
    void CheckForStairs(float direction)
    {
        RaycastHit2D botCheckRay = Physics2D.Raycast(BottomRay.transform.position, new Vector3(direction, 0, 0), rayDist, groundLayer); //check if there a stair at the characters feet
        if (botCheckRay)
        {
            RaycastHit2D midCheckRay = Physics2D.Raycast(MidCheckRay.transform.position, new Vector3(direction, 0, 0), rayDist, groundLayer); //check if it's not the wall option
            if ((!midCheckRay) || (midCheckRay.distance - botCheckRay.distance > 0.05))
            {

                walkingOnStairsTime = 0.5f;
                Vector3 heightSeekerPos = MidCheckRay.transform.position + direction * new Vector3(botCheckRay.distance + 0.01f, 0, 0);
                RaycastHit2D heightSeekerRay = Physics2D.Raycast(heightSeekerPos, Vector3.down, 5, groundLayer);

                float upDist = MidCheckRay.transform.position.y - BottomRay.transform.position.y - heightSeekerRay.distance;
                transform.position += new Vector3(direction * 0.03f, upDist * 1.1f, 0);
            }
        }
    }

    // Walljump
    void CheckForWalljump(float direction)
    {
        CurrentWalljumpCd -= Time.deltaTime;
        if (CurrentWalljumpCd < 0)
            CurrentWalljumpCd = 0;
        RaycastHit2D botCheckRay = Physics2D.Raycast(BottomRay.transform.position, new Vector3(direction, 0, 0), rayDist, groundLayer); //check if wall at the feet level
        RaycastHit2D topCheckRay = Physics2D.Raycast(MidCheckRay.transform.position, new Vector3(direction, 0, 0), rayDist, groundLayer); //check if wall at the head level
        if ((!isGrounded) && botCheckRay && topCheckRay && (topCheckRay.distance - botCheckRay.distance < 0.05) && (CurrentWalljumpCd == 0))
        {
            wallJumpReady = true;
            PlayerAnimator.SetBool("readyToWallJump", true);
            body.drag = 3;
        }
        else
        {
            wallJumpReady = false;
            PlayerAnimator.SetBool("readyToWallJump", false);
            body.drag = 0;
        }
    }

    // Checks if player is grounded
    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapAreaAll(GroundCheck.bounds.min, GroundCheck.bounds.max, groundLayer).Length > 0;
        if (isGrounded)
            amountofJumps = 1;
    }

    // Checks if player is on stairs
    void CheckonStairs()
    {
        if (walkingOnStairsTime > 0)
            walkingOnStairsTime -= Time.deltaTime;
        else
            walkingOnStairsTime = 0;
    }

    // When player stops moving, slow down at a rate to mimic deacceleration more smoothly
    void ApplyFriction()
    {
        if (isGrounded && horizontalInput == 0 && verticalInput == 0 && !isJumping && !pushed)
            body.velocity *= groundDecay;
    }

    string whatAmISteppingOn()
    {
        Tilemap groundTiles = ground.GetComponent<Tilemap>();
        Vector3Int tilePos = grid.WorldToCell(BottomRay.transform.position);
        // int SZ = groundTiles.cellSize; 

        TileBase theTile = groundTiles.GetTile(tilePos);

        /*  while (theTile == null)
          {
              tilePos = Vector3Int.RoundToInt(tilePos - new Vector3 (0,groundTiles.cellSize.y,0));
              theTile = groundTiles.GetTile(tilePos);
          }*/

        if (tileTypes.Grasstiles.Contains(theTile))
            return "grass";
        else
            return "concrete";
    }

    void makeSteppingSound(string Type)
    {
        switch (Type)

        {
            case "grass":
                walkingFX.clip = WalkGrassFX;
                break;

            default:
                walkingFX.clip = WalkFX;
                break;
        }



    }

    void makeLandingSound(string Type)
    {
        switch (Type)

        {
            case "grass":
                SoundFXManager.Instance.playSFXClip(LandingGrassFX, transform, 1);
                break;

            default:
                SoundFXManager.Instance.playSFXClip(LandingFX, transform, 1);
                break;
        }

    }
}


[System.Serializable]
public class UnityIntEvent : UnityEvent<int> { }