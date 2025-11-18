using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

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
    [SerializeField] private PlayerInput playerInput;          
    [SerializeField] private InputActionAsset fallbackActions;
    [SerializeField] private LayerMask groundPoundHitMask;         // must include the layer your Sunflowers are on
    [SerializeField] private Vector2 poundHitBoxSize = new Vector2(0.8f, 0.35f);
    [SerializeField] private float poundHitBoxOffsetY = 0.20f;  
    [SerializeField] private int groundPoundDamage = 1;
    [SerializeField] private float poundKnockback = 4f;






    //[SerializeField] private GameObject GroundCheckPoint;
    [SerializeField] bool useBoxcastGrounding = false; // enable per scene (scene 4 only)
    [SerializeField] bool useColliderCastGrounding = true; //(turn on for scene 4
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
    [Header("Respawn")]
    public Transform respawnPoint;                 // set per-scene or via checkpoints
    [SerializeField] Vector2 fallbackSpawn = new Vector2(-1f, 0.5f); // legacy safety
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
    [SerializeField] private UnityIntEvent OnHeal;
    [SerializeField] private UnityEvent OnDeath;
    [SerializeField] private UnityEvent OnGameOver;

    //Player Action Variables
    public InputAction moveAction;
    public InputAction jumpAction;
    public InputAction sprintAction;
    public InputAction groundPoundAction;

    private void Start()
    {
        walkingFX = GetComponent<AudioSource>();
        if (walkingFX) walkingFX.clip = WalkFX;
        walkingFX.clip = WalkFX;
        ground = GameObject.FindWithTag("Ground");
        if (!ground)
        {
            var anyTilemap = FindFirstObjectByType<UnityEngine.Tilemaps.Tilemap>();
            if (anyTilemap) ground = anyTilemap.gameObject;
        }

        // Grid – try tag first, then any Grid in scene
        var taggedGridObj = GameObject.FindWithTag("Grid");
        var gridComp = taggedGridObj ? taggedGridObj.GetComponent<Grid>() : null;
        if (!gridComp)
        {
            gridComp = FindFirstObjectByType<Grid>();
        }
        grid = gridComp; // may be null; whatAmISteppingOn() guards it
        GridObject = GameObject.FindWithTag("Grid");
        grid = GridObject.GetComponent<Grid>();

        if (!playerInput) playerInput = GetComponent<PlayerInput>();
        var actionsAsset = playerInput ? playerInput.actions : fallbackActions;

        if (actionsAsset == null)
        {
            Debug.LogError("No InputActionAsset found. Add a PlayerInput to the Player or assign fallbackActions.");
            enabled = false;
            return;
        }

        // --- INPUT WIRING: get a valid actionsAsset, bind actions, enable them ---
        if (playerInput == null) playerInput = GetComponent<PlayerInput>();

        // Prefer the asset on PlayerInput; if none, keep whatever you already assign to actionsAsset
        var resolvedActions = (playerInput != null && playerInput.actions != null)
            ? playerInput.actions
            : actionsAsset; // <-- if you already serialize/assign this in the inspector

        if (resolvedActions == null)
        {
            Debug.LogError("[CharacterScript] No InputActionAsset available. " +
                           "Assign PlayerInput.actions on the Player or drag your .inputactions into 'actionsAsset'.");
        }
        else
        {
            // Try map-qualified names first (e.g., 'Gameplay/Move'), then plain names
            moveAction = resolvedActions.FindAction("Gameplay/Move", throwIfNotFound: false)
                             ?? resolvedActions.FindAction("Move", throwIfNotFound: true);

            jumpAction = resolvedActions.FindAction("Gameplay/Jump", throwIfNotFound: false)
                             ?? resolvedActions.FindAction("Jump", throwIfNotFound: true);

            sprintAction = resolvedActions.FindAction("Gameplay/Sprint", throwIfNotFound: false)
                             ?? resolvedActions.FindAction("Sprint", throwIfNotFound: false);

            groundPoundAction = resolvedActions.FindAction("Gameplay/GroundPound", throwIfNotFound: false)
                             ?? resolvedActions.FindAction("GroundPound", throwIfNotFound: false);

            // Enable what we found (must be enabled to read values)
            if (!moveAction.enabled) moveAction.Enable();
            if (!jumpAction.enabled) jumpAction.Enable();
            if (sprintAction != null && !sprintAction.enabled) sprintAction.Enable();
            if (groundPoundAction != null && !groundPoundAction.enabled) groundPoundAction.Enable();

            string sprintName = (sprintAction != null) ? sprintAction.name : "—";
            string poundName = (groundPoundAction != null) ? groundPoundAction.name : "—";
            Debug.Log($"[CharacterScript] Actions bound → Move:{moveAction?.name}  Jump:{jumpAction?.name}  " +
            $"Sprint:{sprintName}  Pound:{poundName}");

        }

    }


    // Runs every frame
    void Update()
    {
        //if (Time.frameCount % 30 == 0)
            //Debug.Log($"isGrounded={isGrounded}  jumps={amountofJumps}  vel={body.velocity}");   debug for jump test

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
        isGrounded = useColliderCastGrounding ? IsGrounded_ColliderCast()
                     : (useBoxcastGrounding ? IsGrounded_Boxcast()
                                            : isGrounded); // (or call your old CheckGrounded())

        if (isGrounded) amountofJumps = 2;    // refill here

        Movement();
        ApplyFriction();

        if (transform.position.y < floorBoxHeight) Die();
    }


    bool IsGrounded_Boxcast()
    {
        // Seam-proof “feet box” directly under the collider
        var b = PlayerCollider.bounds;
        const float skin = 0.02f;                 // just below feet
        var center = new Vector2(b.center.x, b.min.y - skin);
        var size = new Vector2(b.size.x * 0.8f, 0.06f);  // wide, very thin

        return Physics2D.OverlapBox(center, size, 0f, groundLayer) != null;
    }

    bool IsGrounded_ColliderCast()
    {
        if (!PlayerCollider) return false;

        // Cast the player's collider straight down a tiny distance
        var filter = new ContactFilter2D { useLayerMask = true, layerMask = groundLayer, useTriggers = false };
        var hits = new RaycastHit2D[4];
        const float dist = 0.06f; // small skin

        int count = PlayerCollider.Cast(Vector2.down, filter, hits, dist);
        for (int i = 0; i < count; i++)
        {
            if (hits[i].collider && hits[i].normal.y > 0.2f) return true; // upward-ish surface
        }
        return false;
    }

    // helper to see it
    void OnDrawGizmosSelected()
    {
        if (!PlayerCollider) return;
        var b = PlayerCollider.bounds;
        const float skin = 0.02f;
        var center = new Vector2(b.center.x, b.min.y - skin);
        var size = new Vector2(b.size.x * 0.8f, 0.06f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center, size);
    }



    // set checkpoint to player location
    public void SetCheckpoint(Transform t)
    {        // <-- used by Checkpoint; does NOT teleport now
        respawnPoint = t;
    }

    // Reset player on death
    void Die()
    {
        health = maxHealth;

        // choose respawn location
        Vector3 pos = respawnPoint
            ? respawnPoint.position
            : new Vector3(fallbackSpawn.x, fallbackSpawn.y, transform.position.z);

        if (body) body.velocity = Vector2.zero; // clear fall momentum
        transform.position = pos;

        lives -= 1;
        if (lives <= 0) OnGameOver.Invoke();
        OnDeath.Invoke();                      // UI etc.
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

    public void RespawnAtAnchor()
    {
        Vector3 pos = (respawnPoint != null)
            ? respawnPoint.position
            : new Vector3(fallbackSpawn.x, fallbackSpawn.y, transform.position.z);

        if (body) body.velocity = Vector2.zero;
        transform.position = pos;
    }


    // lets you pass any Transform (e.g., a tunnel's child SpawnPoint)
    public void RespawnAt(Transform t)
    {
        respawnPoint = t;
        RespawnAtAnchor();
    }
    void DoGroundPoundHit()
    {
        if (PlayerCollider == null) return;

        Bounds b = PlayerCollider.bounds;

        // centered just under the feet; tweak in the Inspector
        Vector2 center = new Vector2(
            b.center.x,
            b.min.y - poundHitBoxOffsetY
        );

        var hits = Physics2D.OverlapBoxAll(center, poundHitBoxSize, 0f);

        float maxBounce = 0f;

        foreach (var h in hits)
        {
            // ALWAYS try to get Health2D
            Health2D hp = h.GetComponentInParent<Health2D>();
            if (!hp) continue; // no health = no damage

            // Try to get per-collider stomp settings (optional)
            var surf = h.GetComponent<EnemyJumpStompSurface2D>();
            if (!surf) surf = h.GetComponentInParent<EnemyJumpStompSurface2D>();

            // pick damage: stomp override if set, otherwise default groundPoundDamage
            int dmg = groundPoundDamage;
            if (surf && surf.stompDamage > 0)
                dmg = surf.stompDamage;

            Vector2 dir = (h.bounds.center - b.center).normalized;
            hp.TakeHit(dmg, dir * poundKnockback);

            // remember strongest bounce if surface defines it
            if (surf && surf.stompBounceVelocity > maxBounce)
                maxBounce = surf.stompBounceVelocity;
        }

        // Apply bounce if we stomped something
        if (maxBounce > 0f && body != null)
        {
            body.velocity = new Vector2(body.velocity.x, maxBounce);
        }

        // Debug box so you can see it
        Vector3 p0 = new Vector3(center.x - poundHitBoxSize.x * 0.5f, center.y - poundHitBoxSize.y * 0.5f);
        Vector3 p1 = new Vector3(center.x + poundHitBoxSize.x * 0.5f, center.y - poundHitBoxSize.y * 0.5f);
        Vector3 p2 = new Vector3(center.x + poundHitBoxSize.x * 0.5f, center.y + poundHitBoxSize.y * 0.5f);
        Vector3 p3 = new Vector3(center.x - poundHitBoxSize.x * 0.5f, center.y + poundHitBoxSize.y * 0.5f);
        Debug.DrawLine(p0, p1, Color.yellow, 0.15f);
        Debug.DrawLine(p1, p2, Color.yellow, 0.15f);
        Debug.DrawLine(p2, p3, Color.yellow, 0.15f);
        Debug.DrawLine(p3, p0, Color.yellow, 0.15f);
    }



    public void Heal(int amount)
    {
        if (health != maxHealth)
        {
            health += amount;
            OnHeal.Invoke(amount);
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

        Vector2 _moveDirection = moveAction.ReadValue<Vector2>();

        if (_moveDirection.x < -0.2f) //The -0.2f is mostly for controller, can be adjusted to make it feel better.
        {
            horizontalInput = -1f;
        }
        else if (_moveDirection.x > 0.2f) //The 0.2f is mostly for controller, can be adjusted to make it feel better.
        {
            horizontalInput = 1f;
        }
        else
        {
            horizontalInput = 0;
        }

        if (groundPoundAction.IsPressed())
        {
            if ((!isGrounded || EnemyHit) && !IsAttacking) StartCoroutine(GroundPound());
        }

        if (sprintAction.IsPressed())
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

        // start falling fast
        body.velocity = new Vector2(body.velocity.x, groundPoundPower);

        // wait until we hit ground OR we collided with an enemy
        yield return new WaitUntil(() => isGrounded || EnemyHit);

        // deal damage 
        DoGroundPoundHit();

        // little bounce if connected on an enemy
        if (EnemyHit)
        {
            body.velocity = new Vector2(body.velocity.x, -groundPoundPower / 2f);
            yield return new WaitForSeconds(0.25f);
        }

        EnemyHit = false;
        IsAttacking = false;
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        // If we're in a ground-pound (IsAttacking), treat "anything with Health2D"
        // as an enemy hit so the coroutine can finish and DoGroundPoundHit() can run.
        if (IsAttacking)
        {
            var hpOnHit = collision.collider.GetComponentInParent<Health2D>();
            if (hpOnHit != null)
            {
                EnemyHit = true;
            }

            // IMPORTANT: skip stomp/bounce logic while ground-pounding.
            // The pound coroutine will handle damage + bounce.
            return;
        }

        // --- Jump-off-enemy logic (ONLY when NOT ground-pounding) ---
        foreach (var contact in collision.contacts)
        {
            // contact normal pointing up => we landed on top of something
            if (contact.normal.y > 0.5f)
            {
                var surf = contact.collider.GetComponent<EnemyJumpStompSurface2D>()
                           ?? contact.collider.GetComponentInParent<EnemyJumpStompSurface2D>();

                if (surf == null) continue;

                // optional damage on head-jump (currently 0 for you, so no damage)
                var hp = surf.GetHealth();
                if (hp && surf.headJumpDamage > 0)
                {
                    hp.TakeHit(surf.headJumpDamage, Vector2.zero);
                }

                // bounce up
                float bounce = Mathf.Max(surf.headJumpBounceVelocity, body.velocity.y);
                body.velocity = new Vector2(body.velocity.x, bounce);

                isJumping = true;
                amountofJumps = 1;

                break;
            }
        }
    }



    // Handle Jump and double jump
    void Jump()
    {
        //Debug.Log("Jumping!");
        if (isJumping && isGrounded && Mathf.Abs(body.velocity.y) < 0.01)
        {
            PlayerAnimator.SetBool("isJumping", false);
            isJumping = false;
            makeLandingSound(whatAmISteppingOn());
        }
        else if (jumpAction.triggered && amountofJumps > 0)
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
        //Debug.Log("Wall Jumping!");
        if (isJumping && isGrounded && Mathf.Abs(body.velocity.y) < 0.01)
        {
            PlayerAnimator.SetBool("isJumping", false);
            isJumping = false;
        }
        if (jumpAction.triggered && wallJumpReady)
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
            amountofJumps = 2;
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






