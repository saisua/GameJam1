using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_script : MonoBehaviour {
    //Bon dia
    //Movement settings
    [Header("Movement axis")]
    public Vector3 movement;
    public List<string> directions = new List<string>(3){"Vertical", "Vertical", "Horizontal"};

    //Jumping settings
    [Header("Jumping axis")]
    public Vector3 jump;
    public int max_jumps = 1;
    public bool in_air_movement = false;
    public float jump_speed;

    private int jumps = 0;
    private bool isGrounded = true;

    //public Animator animator;
    
    [Header("Ray casting")]
    //Raycasts
    public List<Vector3> rays;
    LayerMask Ground;
    public float RayLength = 0.1f;

    [Header("Respawn point")]
    public Vector3 respawn_point;

    [Header("Other")]
    public Rigidbody rigidBody;
    bool IsDead;
    public bool test;

    // Use this for initialization
    void Start()
    {
        rigidBody = transform.GetComponent<Rigidbody>();
        Ground = 1 << LayerMask.NameToLayer("Ground");
        IsDead = false;
        jumps = 0;
        transform.position = respawn_point;
    }


    // Update is called once per frame
    void Update()
    {
        IsGrounded();
    }

    private void FixedUpdate()
    {
        
        if (Input.GetButton("Jump")) 
        {
            Jump();
            
        }
        else if (isGrounded)
        {
            jumps = 0;
        }
        if (!IsDead && (in_air_movement || isGrounded)) Move();
    }

    // Moves the player based on the movement and directions Vector3 info
    void Move()
    {
        transform.Translate(new Vector3(Input.GetAxisRaw(directions[0])*movement[0],
                                        Input.GetAxisRaw(directions[1]) * movement[1],
                                        Input.GetAxisRaw(directions[2]) * movement[2]));
        rigidBody.velocity = new Vector3(rigidBody.velocity.x + Input.GetAxisRaw(directions[0]) * movement[0] * jump_speed,
                                        rigidBody.velocity.y + Input.GetAxisRaw(directions[1]) * movement[1] * jump_speed,
                                        rigidBody.velocity.z + Input.GetAxisRaw(directions[2]) * movement[2] * jump_speed);
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
    }


    // Jumping function will apply Vector3 jump as a
    // local directional force
    void Jump()
    {
        if (jumps < max_jumps)
        {
            //animator.SetBool("IsJumping", true);
            rigidBody.AddRelativeForce(new Vector3(jump[0] + rigidBody.velocity.x, jump[1] + rigidBody.velocity.y, jump[2] + rigidBody.velocity.z), ForceMode.Impulse);
            isGrounded = false;
            test = true;
            jumps++;
        }
    }

    // Shots a Raycast which detects wether there´s a
    // near Ground Layered object
    void IsGrounded()
    {
        foreach (Vector3 ray in rays)
        {
            isGrounded = Physics.Raycast(transform.position + ray, -Vector3.up, RayLength, Ground);
            Debug.DrawLine(transform.position + ray, transform.position + ray + -Vector3.up * RayLength, Color.red);
            if (isGrounded)
            {
                return;
            }
        }
    }

    // Respawn 
    void Respawn(){
        //UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        Start();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Auch!!");
            //animator.SetBool("IsHurt",true);
            IsDead = true;
            Invoke("Respawn", 1f);
        }
    }
}
