using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterMovement : MonoBehaviour
{
    // 2D movement settings
    [Header("Movement")]
    // If this object is controlled by the player or not
    public bool player_controlled;
    // The amount of movement the player will be given
    // when pressing a-d (x) or s-w (y)
    public Vector2 movement;
    // 

    // The direction in x the player is pointing at
    private float direction = 1;
    private bool player_controlled_init;

    // Jumping settings
    [Header("Jumping")]
    // The amount of force given in y when jumping
    public Vector2 jump_force;
    // The max amount of jumps that the player can do
    // without touching the ground
    public int max_jumps = 1;
    // If the player can move when it's in the air
    public bool in_air_movement = false;
    // Fake lateral and vertical speed to fake when
    // in_air_movement is false
    public float jump_speed;

    // The amount of jumps the player has done without
    // touching the ground
    private int jumps = 0;
    // Bool set if the player is currently touching ground
    private bool isGrounded = true;

    // Player main settings
    [Header("Player")]
    // The amount of hits the player can recieve before
    // the current scene is resetted
    public int lives = 1;
    // Time to die
    public float die_animation_time;
    // Time to respawn
    public float respawn_time;
    // If the player will die and respawn in the last set
    // spawn of it will continue (when lives > 0)
    public bool respawn_on_hit = true;
    // The force the player will be put when touching an enemy
    public Vector2 hit_force;
    // The tag the player will take damage from
    public string hurting_tag = "Enemy";
    // 
    public bool enemy_reapear = true;

    // If the player can be hit again
    private bool invulnerable = false;
    // Save the number of initial lives
    private int lives_init = 0;
    private bool dead = false;

    [Header("Attack")]
    // If the player has wepons to attack with
    public bool can_attack = true;
    //
    public bool kinematic_attack;
    // Where the weapon will be spawned
    public Vector3 attack_spawn_position;
    // The amount of speed (dynamic rigidbody) the object will be
    // shot at
    public Vector2 attack_spawn_force;
    // The amount of recoil the player will recieve when attacking
    public Vector2 recoil;
    // The attack object
    public GameObject attack_object;
    // The amount attacks the player can have stored
    public int max_ammo = 30;
    // The amount of attacks the player has stored
    public int ammo = 30;
    // The cost of ammo per attack
    public int ammo_per_attack = 1;
    // The gap (in seconds) between attacks
    public float seconds_between_attacks;

    // If the player is allowed to attack
    private bool hasnt_attacked = true;

    // Ray casting setup
    [Header("Ray casting")]
    // A list of the positions (relative to the player)
    // the rays will be casted from
    public List<Vector2> rays;
    // The length of the rays
    public float RayLength = 0.1f;
    // If the lines must be shown
    public bool draw_lines = true;

    // The Ground Layer, which allows the player to
    // reset its jumps
    LayerMask Ground;

    // Point where the player will respawn when Start()
    [Header("Respawn point")]
    public Vector2 respawn_point;

    // Other settings I've been unable to keep apart
    [Header("Other")]
    // The rigidbody object the forces will be applied to
    public Rigidbody2D rigidBody;
    // The animator object
    public Animator animator;
    public bool test;

    private GameObject self;

    //   FUNCTIONS

    // Use this for initialization
    void Start()
    {
        lives_init = lives;
        player_controlled_init = player_controlled;

        rigidBody = transform.GetComponent<Rigidbody2D>();

        Ground = 1 << LayerMask.NameToLayer("Ground");

        animator = gameObject.GetComponent<Animator>();

        jumps = 0;

        transform.position = respawn_point;

        self = this.gameObject;
    }


    // Update is called once per frame
    void Update()
    {
        IsGrounded();
    }

    private void FixedUpdate()
    {
        if (player_controlled && Input.GetButton("Jump"))
        {
            Jump();
        }
        else if (player_controlled && can_attack && Input.GetButton("Fire1") && hasnt_attacked)
        {
            Attack();
        }
        else if (isGrounded)
        {
            jumps = 0;
            animator.SetBool("Falling", false);
            animator.SetBool("Attacking", false);
        }
        else if (rigidBody.velocity.y < 0)
        {
            animator.SetBool("Falling", true);
            animator.SetBool("Jumping", false);
        }

        if (player_controlled && (in_air_movement || isGrounded)) Move();
    }

    // Moves the player based on the movement Vector2 info
    void Move()
    {
        float mov_x = Input.GetAxisRaw("Horizontal") * movement[0];
        float mov_y = Input.GetAxisRaw("Vertical") * movement[1];

        transform.Translate(new Vector3(mov_x, mov_y, 0) * Time.deltaTime);

        if (mov_x > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);

            animator.SetBool("Running", true);

            direction = 1;
        }
        else if(mov_x < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);

            animator.SetBool("Running", true);

            direction = -1;
        }
        else animator.SetBool("Running", false);
    }

    // Jumping function will apply Vector3 jump as a
    // global directional force
    void Jump()
    {
        if (jumps < max_jumps)
        {
            animator.SetBool("Jumping", true);
            rigidBody.AddForce(new Vector2(jump_force.x * direction, jump_force.y)
                                                    , ForceMode2D.Impulse);
            isGrounded = false;

            jumps++;
        }
    }

    // Shots a Raycast which detects wether there´s a
    // near Ground Layered object
    void IsGrounded()
    {
        foreach (Vector2 ray in rays)
        {
            isGrounded = Physics2D.Raycast(new Vector2(transform.position.x + ray.x, transform.position.y + ray.y)
                                                        ,-Vector3.up, RayLength, Ground);
            if(draw_lines) Debug.DrawLine(new Vector2(transform.position.x + ray.x, transform.position.y + ray.y)
                                                        , new Vector3(transform.position.x + ray.x, transform.position.y + ray.y, 0) - Vector3.up * RayLength
                                                        , Color.red);
            if (isGrounded)
            {
                animator.SetBool("Jumping", false);

                return;
            }
        }
    }

    // Attack function will spawn an attack in spawn_attack position
    // with a attack_spawn_speed force
    void Attack()
    {
        Attack(attack_spawn_force);
    }

    void Attack(Vector3 force_vector)
    {
        animator.SetBool("Attacking", true);
        //Vector3 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (ammo > 0)
        {
            GameObject projectil = Instantiate(attack_object, transform.position + new Vector3(attack_spawn_position.x * direction, attack_spawn_position.y, attack_spawn_position.z),
                                                Quaternion.identity) as GameObject;

            Rigidbody2D rigidBody_proj = projectil.GetComponent<Rigidbody2D>();

            if (kinematic_attack)
            {
                rigidBody_proj.gravityScale = 0;
            }

            rigidBody_proj.AddForce(new Vector2(force_vector.x * direction, force_vector.y));

            if (ammo < ammo_per_attack) ammo = 0;
            else ammo -= ammo_per_attack;

            rigidBody.AddForce(recoil * direction, ForceMode2D.Impulse);

            hasnt_attacked = false;
            Invoke("AllowAttack", seconds_between_attacks);
        }
        animator.SetBool("Attacking",false);
    }

    void AllowAttack()
    {
        hasnt_attacked = true;
    }

    void GetAmmo(int num_ammo)
    {
        ammo += num_ammo;
        if (ammo > max_ammo) ammo = max_ammo;
    }

    // Hitbox detector
    private void OnTriggerEnter2D(Collider2D collider)
    {
        test = true;
        if (collider.gameObject.CompareTag(hurting_tag) && !invulnerable)
        {
            if (player_controlled)
            {
                invulnerable = true;
                Invoke("DisableInvulnerability", 1.5f);
            }
            animator.SetBool("Hit", true);
            animator.SetBool("Attacking", false);
            lives--;
            if (lives < 1) Die();
            else if (respawn_on_hit && player_controlled) Invoke("Respawn", die_animation_time);
            else
            {
                rigidBody.AddRelativeForce(hit_force , ForceMode2D.Impulse);

                isGrounded = false;
                jumps = max_jumps;
            }
        }
    }

    void DisableInvulnerability()
    {
        invulnerable = false;
    }

    // Death function
    void Die()
    {
        player_controlled = false;
        dead = true;
        animator.SetBool("Dead", true);
        Invoke("Respawn", die_animation_time);
    }

    // Respawn function
    void Respawn()
    {
        if (dead && player_controlled_init)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else if (player_controlled_init && respawn_on_hit)
        {
            jumps = 0;

            transform.position = respawn_point;

            if (dead) lives = lives_init;

            dead = false;
            animator.SetBool("Dead", false);
        }
        else if (enemy_reapear && !player_controlled_init)
        {
            this.gameObject.SetActive(false);
            Invoke("NewEnemy", respawn_time);
        }
        
    }
    void NewEnemy()
    {
        this.gameObject.SetActive(true);
        this.gameObject.transform.position = respawn_point;
        lives = lives_init;
    }
    public void ApplyCC(float seconds)
    {
        player_controlled = false;
        Invoke("ExitCC", seconds);
    }
    public void ExitCC()
    {
        player_controlled = true;
    }
    public void ApplyDot(float seconds)
    {

    }
}