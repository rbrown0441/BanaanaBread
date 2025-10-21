// Player.cs

// Unity Components
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using Newtonsoft.Json.Schema;
using UnityEngine.VFX;
using UnityEditor.Rendering;

// Main Loop
public class Player : MonoBehaviour
{
    // ----------------------------------------------------------------
    // MARK: INITIALIZATION
    // ----------------------------------------------------------------

    // Layers
    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask enemyLayer;

    // Components
    [Header("Components")]
    [SerializeField] public Rigidbody2D body;
    [SerializeField] private BoxCollider2D GroundCheck;
    [SerializeField] private BoxCollider2D PlayerCollider;
    [SerializeField] private GameObject TopCheckRay;
    [SerializeField] private GameObject MidCheckRay;
    [SerializeField] private GameObject BottomRay;
    [SerializeField] public SpriteRenderer sprite;
    [SerializeField] public Animator anim;
    [SerializeField] public PlayerVisualSquish visual_squish;

    // Additional References
    AudioSource walkingFX;
    GameObject ground;
    GameObject GridObject;
    Grid grid;

    // PARAMETERS - Movement
    [Header("Movement")]
    [SerializeField] public float run_speed_init = 2.0f;
    [SerializeField] public float run_speed_max = 3.0f;
    [SerializeField] public float run_accel = 100.0f;
    [SerializeField] public float ground_fric = 100.0f;
    [SerializeField] public float jump_strength = 3.5f;
    [SerializeField] private int  amountofJumps = 0;
    [SerializeField] private float rayDist = 0.4f;

    [SerializeField] private float WallJumpPowerHeight = 0.5f;
    [SerializeField] private float WalljumpCd = 0.3f;

    // PARAMETERS - Stats
    [Header("Stats")]
    [SerializeField] public int maxHealth;
    [SerializeField] public int health;
    [SerializeField] public int maxLives;
    [SerializeField] public int lives;

    // PARAMETERS - Level Design
    [Header("Level Design")]
    [SerializeField] public Vector2 spawnPoint;
    [SerializeField] private float floorBoxHeight;
    [SerializeField] TileTypes tileTypes;

    // PARAMETERS - Sound FX
    [Header("Sound FX")]
    [SerializeField] AudioClip JumpFX;
    [SerializeField] AudioClip DoubleJumpFX;
    [SerializeField] AudioClip WalkFX;
    [SerializeField] AudioClip WalkGrassFX;
    [SerializeField] AudioClip LandingFX;
    [SerializeField] AudioClip LandingGrassFX;

    // Event Functions
    [Header("Event Functions")]
    [SerializeField] private UnityIntEvent OnHurt;
    [SerializeField] private UnityEvent OnDeath;
    [SerializeField] private UnityEvent OnGameOver;

    // Input Actions
    [Header("Input Actions")]
    public InputAction actionMoveRight;
    public InputAction actionMoveLeft;
    public InputAction actionMoveUp;
    public InputAction actionMoveDown;
    public InputAction actionMoveJump;
    public InputAction actionAttack;
    public InputAction actionInteract;

    // State Machine
    [Header("State Machine")]
    public PlayerStateMachine state_machine;

    // Conditional Variables
    [Header("Conditionals")]
    public int facing = 1; // 1 Right, -1 Left
    public bool isJumping = false;
    private bool inInvincibleFrames = false;
    private bool pushed = false;
    private bool wallJumpReady = false;
    public bool isGrounded;
    public float currentSpeed;
    public bool IsAttacking { get; set; } = false;
    public bool EnemyHit { get; set; } = false;
    private float walkingOnStairsTime = 0;
    float CurrentWalljumpCd = 0;

    // Initialize State Machine
    private void Awake()
    {
        state_machine = GetComponent<PlayerStateMachine>();
    }

    // On Start
    private void Start()
    {
        walkingFX = GetComponent<AudioSource>();
        walkingFX.clip = WalkFX;
        ground = GameObject.FindWithTag("Ground");
        GridObject = GameObject.FindWithTag("Grid");
        grid = GridObject.GetComponent<Grid>();
        // Input References
        actionMoveRight = InputSystem.actions.FindAction("Move_Right");
        actionMoveLeft = InputSystem.actions.FindAction("Move_Left");
        actionMoveUp = InputSystem.actions.FindAction("Move_Up");
        actionMoveDown = InputSystem.actions.FindAction("Move_Down");
        actionMoveJump = InputSystem.actions.FindAction("Move_Jump");
        actionAttack = InputSystem.actions.FindAction("Attack");
        actionInteract = InputSystem.actions.FindAction("Interact");
    }



    // ----------------------------------------------------------------
    // MARK: MAIN LOOP
    // ----------------------------------------------------------------

    // Main Update Tick 
    void Update()
    {
        // Check if on Stairs
        // check_stairs();
        // Jump or Walljump
        if (wallJumpReady)
            check_walljump();
        else
            check_jump();
    }

    // Physics Update Tick
    void FixedUpdate()
    {
        // Check if on Ground
        check_grounded();
        //Kills player if they fall in hole
        if (transform.position.y < floorBoxHeight)
            event_die();
        // Print X Vel
        Debug.Log($"vx={body.velocity.x}");
    }



    // ----------------------------------------------------------------
    // MARK: INPUTS
    // ----------------------------------------------------------------

    // CHECK: Get Horizontal Input
    public float get_input_horizontal()
    {
        float value = 0f;
        if (actionMoveLeft.IsPressed()) value -= 1f;
        if (actionMoveRight.IsPressed()) value += 1f;
        return value;
    }



    // ----------------------------------------------------------------
    // MARK: EVENT FUNCTIONS
    // ----------------------------------------------------------------

    // Execute Jump
    public void event_jump()
    {
        // Squish
        sprite_squish(0.9f, 1.2f);
        // Apply Movement
        body.velocity = new Vector2(body.velocity.x, jump_strength);
        // SFX
        if (amountofJumps > 1) {
            SoundFXManager.Instance.playSFXClip(DoubleJumpFX, transform, 1);
        } else {
            SoundFXManager.Instance.playSFXClip(JumpFX, transform, 1);
        }
        // Variables
        amountofJumps--;
        isJumping = true;
        // State
        state_switch<pState_Aerial>();
    }

    // Execute Walljump
    public void event_walljump()
    {
        // Get Direction, Flip
        float dir;
        if (transform.localScale.x < 0)
            dir = -1;
        else
            dir = 1;
        // Apply Movement
        body.velocity = new Vector2(-1f * dir * WallJumpPowerHeight, WallJumpPowerHeight);
        // Variables
        isJumping = true;
        CurrentWalljumpCd = WalljumpCd;
        wallJumpReady = false;
    }

    // On Footstep
    public void event_footstep()
    {
        // Get SFX
        sfx_footstep(check_floor_material());
        // Play SFX
        if (!walkingFX.isPlaying)
            walkingFX.Play();
    }

    // On Landed
    public void event_landed()
    {
        // Squish
        sprite_squish(1.2f, 0.9f);
        // Get SFX
        sfx_landed(check_floor_material());
        // Play SFX
        if (!walkingFX.isPlaying)
            walkingFX.Play();
    }

    // On Death
    public void event_die()
    {
        health = maxHealth;
        transform.position = spawnPoint;
        lives -= 1;
        if (lives <= 0)
        {
            OnGameOver.Invoke();
        }
        OnDeath.Invoke();
    }



    // ----------------------------------------------------------------
    // MARK: HELPER FUNCTIONS
    // ----------------------------------------------------------------

    // Changed Faced Direction
    public void change_facing(int dir)
    {
        // Don't continue if not valid
        if (facing != -1 && facing != 1) return;
        // Change Facing
        facing = dir;
        // Edit Transforms
        transform.localScale = new Vector3(dir, 1, 1);
    }

    // Switch State
    public void state_switch<T>() where T : PlayerState
    {
        state_machine.state_switch<T>();
    }

    // Reset State
    public void state_reset()
    {
        state_machine.state_reset();
    }

    // Apply Damage
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
                // Reduce Push if Jumping (can get out of hand)
                if (isJumping)
                    pushForce = new Vector2(pushForce.x, pushForce.y / 2);

                body.AddForce(pushForce, ForceMode2D.Impulse);
            }
            StartCoroutine(ShowDamage());

            OnHurt.Invoke(damage);
        }
    }

    // Damage Coroutine
    IEnumerator ShowDamage()
    {
        // Color red, start iFrames
        sprite.color = Color.red;
        inInvincibleFrames = true;
        // Wait
        yield return new WaitForSeconds(0.15f);
        pushed = false;
        yield return new WaitForSeconds(0.6f);
        // Color white, end iFrames
        sprite.color = Color.white;
        inInvincibleFrames = false;
        // Kill player if Health at 0
        if (health <= 0)
            event_die();
    }

    // Approach (Float)
    public float approach(float current, float target, float rate, float dt)
    {
        return Mathf.MoveTowards(current, target, rate * dt);
    }

    // Approach (Vector2)
    public Vector2 approach(Vector2 current, Vector2 target, float rate, float dt)
    {
        return Vector2.MoveTowards(current, target, rate * dt);
    }

    // Set X Velocity Target
    public void set_vel_x(float targetX, float rate, float dt)
    {
        Vector2 v = body.velocity;
        v.x = approach(v.x, targetX, rate, dt);
        body.velocity = v;
    }

    // Multiply Current X Velocity
    public void mod_vel_x(float factor)
    {
        float vx = body.velocity.x;
        float vy = body.velocity.y;
        body.velocity = new Vector2(vx * factor, vy);

    }

    // Apply X Friction
    public void apply_friction(float rate, float dt)
    {
        Vector2 v = body.velocity;
        v.x = approach(v.x, 0f, rate, dt);
        body.velocity = v;
    }



    // ----------------------------------------------------------------
    // MARK: SFX FUNCTIONS
    // ----------------------------------------------------------------


    // Footstep SFX
    public void sfx_footstep(string Type)
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

    // Landing SFX
    public void sfx_landed(string Type)
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



    // ----------------------------------------------------------------
    // MARK: SPRITE FUNCTIONS
    // ----------------------------------------------------------------

    // Request Sprite Change
    public void sprite_change(string animName, float speed = 1f)
    {
        // Reference Anim Info
        AnimatorStateInfo anim_info = anim.GetCurrentAnimatorStateInfo(0);
        // Check Current Anim
        if (!anim_info.IsName(animName))
        {
            // Play
            anim.Play(animName);
            // Set Speed
            anim.speed = speed;
        }
    }

    // Squish Sprite
    public void sprite_squish(float xScale, float yScale, float duration = -1f, float elasticity = -1f)
        => visual_squish.squish(xScale, yScale, duration, elasticity);

    // Reset Squish Scale
    public void sprite_squish_reset() => visual_squish.reset_scale();

    // Check if Sprite Anim Just Finished
    public bool sprite_anim_just_finished()
    {
        AnimatorStateInfo anim_info = anim.GetCurrentAnimatorStateInfo(0);
        return anim_info.normalizedTime >= 0.999f;
    }



    // ----------------------------------------------------------------
    // MARK: CONDITIONALS
    // ----------------------------------------------------------------

    // CHECK: Jump
    public void check_jump()
    {
        if (isJumping && isGrounded && Mathf.Abs(body.velocity.y) < 0.01)
        {
            isJumping = false;
        }
        else if (actionMoveJump.IsPressed() && (amountofJumps > 0))
        {
            event_jump();
        }
    }

    // CHECK: Walljump
    public void check_walljump()
    {
        if (isJumping && isGrounded && Mathf.Abs(body.velocity.y) < 0.01)
        {
            isJumping = false;
        }
        if (actionMoveJump.IsPressed() && wallJumpReady)
        {
            event_walljump();
        }
    }

    // CHECK: Stairs
    public void check_stairs(float direction)
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

    // CHECK: Walljump
    public void check_if_can_walljump(float direction)
    {
        CurrentWalljumpCd -= Time.deltaTime;
        if (CurrentWalljumpCd < 0)
            CurrentWalljumpCd = 0;
        RaycastHit2D botCheckRay = Physics2D.Raycast(BottomRay.transform.position, new Vector3(direction, 0, 0), rayDist, groundLayer); //check if wall at the feet level
        RaycastHit2D topCheckRay = Physics2D.Raycast(MidCheckRay.transform.position, new Vector3(direction, 0, 0), rayDist, groundLayer); //check if wall at the head level
        if ((!isGrounded) && botCheckRay && topCheckRay && (topCheckRay.distance - botCheckRay.distance < 0.05) && (CurrentWalljumpCd == 0))
        {
            wallJumpReady = true;
        }
        else
        {
            wallJumpReady = false;
        }
    }

    // CHECK: Grounded
    public void check_grounded()
    {
        isGrounded = Physics2D.OverlapAreaAll(GroundCheck.bounds.min, GroundCheck.bounds.max, groundLayer).Length > 0;
        if (isGrounded)
            amountofJumps = 1;
    }

    // CHECK: Stairs?
    void check_stairs()
    {
        if (walkingOnStairsTime > 0)
            walkingOnStairsTime -= Time.deltaTime;
        else
            walkingOnStairsTime = 0;
    }

    // CHECK: Floor Type
    string check_floor_material()
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

    // CHECK: Enemy Collision
    void OnCollisionEnter2D(Collision2D collider)
    {
        if (collider.gameObject.tag.ToLower() == "enemy" && IsAttacking) EnemyHit = true;
    }
}


[System.Serializable]
public class UnityIntEvent : UnityEvent<int> { }