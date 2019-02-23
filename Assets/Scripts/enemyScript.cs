using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * */

public class enemyScript : MonoBehaviour
{
    public bool test;
    // All states an enemy can be
    public EnemyState currentState;
    // The object whose children's x position will
    // be used as the points where the enemy will go
    public Transform path;
    // The speed the enemy will walk at
    public float walkSpeed;
    // The speed the enemy will chase at
    public float chaseSpeed;
    // The player the enemy will try to kill
    public GameObject player;
    public Component characterScript;

    // The collider of the gameObjects that will act as detection zone
    public List<GameObject> detection;

    public List<Collider2D> colliders = new List<Collider2D>();

    // If the player is being detected by the enemy
    public bool playerDetected;

    public Vector3 last_seen;
    private bool seen_timeout = false;

    // Time waiting when reaching one path position
    public float seconds_waiting;


    private List<Vector3> pathPositions = new List<Vector3>();
    private int currentPosition = 0;
    private int nextPosition;
    public float margen = 0.5f;
    private bool forwardMovement = true;
    public bool waiting = false;

    //A distancia

    [Header("Comportaments")]
    public bool jumping;
    public bool shooting;
    public GameObject bullet;
    public float spawnTime = 1f;

    [Header("RayCast")]
    // If the lines must be shown
    public bool draw_lines = true;
    // If the npc will look if it can keep walking
    public bool careful_walk = true;
    // The position of the ray that will look if the npc
    // can keep walking
    public float vision_max;
    public Vector2 walking_ray_position;
    // The length of the ray
    public float walking_RayLength = 0.5f;

    public bool can_walk = true;
    public bool can_see = true;

    LayerMask Ground;
    LayerMask Player_layer;


    // Start is called before the first frame update
    void Start()
    {
        currentState = EnemyState.Patrol;
        //rb = transform.GetComponent<Rigidbody2D>();

        for (int path_num = 0; path_num < path.childCount; path_num++)
        {
            pathPositions.Add(path.GetChild(path_num).position);
        }

        if (pathPositions.Count > 1) nextPosition = currentPosition + 1;

        foreach (GameObject detector in detection)
        {
            colliders.Add(detector.GetComponent<Collider2D>());
        }

        Ground = 1 << LayerMask.NameToLayer("Ground");
        Player_layer = 1 << LayerMask.NameToLayer("Player");

        characterScript = this.gameObject.GetComponent("CharacterMovement");

        if (shooting) InvokeRepeating("Shoot", spawnTime, spawnTime);

        
    }
    public enum EnemyState
    {
        Patrol,
        Chase,
        Stay
    };



    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                Chase();
                break;
            case EnemyState.Stay:
                if (!waiting)
                {
                    Invoke("Stay", 3f);
                    waiting = true;
                }
                break;
            default:
                currentState = EnemyState.Patrol;
                break;
        }
        if (!jumping)
        {
            Jump();
            jumping = true;
        }

        if (careful_walk) CanWalk();
        if (can_see && (Physics2D.Raycast(transform.position, player.transform.position, vision_max, Player_layer) &&
                        !Physics2D.Raycast(transform.position, player.transform.position, vision_max, Ground)))
        {
            OnTriggerEnter2D(player.transform.position);
            if (draw_lines) Debug.DrawLine(transform.position, player.transform.position,
                                        Color.red);
        }
    }

    private void Patrol()
    {
        if (Vector3.Distance(transform.position, pathPositions[nextPosition]) > margen && can_walk)
        {
            Vector3 position_diff = new Vector3((transform.position.x < pathPositions[nextPosition].x ? 1 : -1), 0, 0);
            transform.Translate(position_diff * walkSpeed * Time.deltaTime);
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * position_diff.x, transform.localScale.y, transform.localScale.z);
        }
        else if (!waiting)
        {
            waiting = true;
            Invoke("CalculateNextPosition", seconds_waiting);
        }

        if (playerDetected)
        {
            currentState = EnemyState.Chase;
        }
    }

    private void CalculateNextPosition()
    {
        currentPosition = nextPosition;
        if (forwardMovement)
        {
            if (currentPosition == pathPositions.Count - 1)
            {
                forwardMovement = false;
                nextPosition--;
            }
            else nextPosition++;
        }
        else
        {
            if (currentPosition == 0)
            {
                forwardMovement = true;
                nextPosition++;
            }
            else nextPosition--;
        }
        waiting = false;
    }

    private void Chase()
    {
        if (Mathf.Abs(transform.position.x - last_seen.x) > margen && can_walk)
        {
            int side = transform.position.x > last_seen.x ? -1 : 1;
            transform.Translate(new Vector3(chaseSpeed * side * Time.deltaTime, 0, 0));
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * side, transform.localScale.y, transform.localScale.z);
        }
        if (!playerDetected && (seen_timeout || Mathf.Abs(transform.position.x - last_seen.x) <= margen))
        {
            if (can_walk && pathPositions.Count > 0) currentState = EnemyState.Patrol;
            else currentState = EnemyState.Stay;
        }

    }

    private void Stay()
    {
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        waiting = false;
        if (playerDetected)
        {
            currentState = EnemyState.Chase;
        }
    }

    void CanWalk()
    {
        Vector2 ray_start = new Vector2(transform.position.x + walking_ray_position.x * transform.localScale.x,
                                        transform.position.y + walking_ray_position.y);
        can_walk = Physics2D.Raycast(ray_start, -Vector3.up, walking_RayLength, Ground);
        if (draw_lines) Debug.DrawLine(ray_start, ray_start - Vector2.up * walking_RayLength,
                                        Color.cyan);
    }

    private void Jump()
    {
        characterScript.SendMessage("Jump");
        Invoke("JumpTimeout", 1f);
    }
    private void Attack()
    {
        characterScript.SendMessage("Attack");
    }

    public void OnTriggerEnter2D(Vector3 location)
    {
        playerDetected = true;
        seen_timeout = false;
        last_seen = location;
        Invoke("DetectionTimeout",5f);
    }

    private void DetectionTimeout()
    {
        seen_timeout = true;
        playerDetected = false;
    }
    private void JumpingTimeout()
    {
        jumping = false;
    }
}
