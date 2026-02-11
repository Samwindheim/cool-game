using UnityEngine;

public class AirHockeyAI : MonoBehaviour
{
    [Header("Targeting")]
    public Transform puck;
    public Transform defensiveLine; // A transform or position the AI retreats to

    [Header("Movement Settings")]
    public float speed = 5f;
    public float maxSpeed = 10f;
    public float attackPower = 1.5f; // Multiplier for when striking forward
    public float strikeCooldown = 0.5f; // Time to wait between strikes
    
    [Header("Boundary Settings")]
    public float tableCenterZ = 0f; // The middle of the table
    public float sideBoundaryX = 0.45f; // How far left/right it can go
    public float backBoundaryZ = 0.9f; // The back wall
    public float goalWidth = 0.2f; // The width of the goal area to protect

    private Rigidbody rb;
    private Vector3 targetPosition;
    private Vector3 startingPosition;
    private float lastStrikeTime;
    private bool isStriking;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startingPosition = transform.position;
        
        // Find the puck automatically if not assigned
        if (puck == null)
            puck = GameObject.FindGameObjectWithTag("Puck")?.transform;
    }

    void FixedUpdate()
    {
        if (puck == null) return;

        // 1. Decide where to move
        if (puck.position.z > tableCenterZ)
        {
            // Puck is on AI's side
            
            // Check if we are currently in a strike or if we can start a new one
            if (isStriking)
            {
                // If we've reached the target or enough time has passed, end the strike
                if (Vector3.Distance(transform.position, targetPosition) < 0.1f || Time.time > lastStrikeTime + 0.5f)
                {
                    isStriking = false;
                }
            }
            else if (Time.time > lastStrikeTime + strikeCooldown)
            {
                // Start a new strike!
                isStriking = true;
                lastStrikeTime = Time.time;

                if (puck.position.z > transform.position.z)
                {
                    // Recovery maneouver: Move to the side and slightly behind the puck
                    // CRITICAL: If the puck is directly in front of the goal, be extra careful
                    float sideOffset = 0.25f;
                    
                    // If the puck is in the "danger zone" (in front of goal), move to the side that pushes it AWAY from center
                    if (Mathf.Abs(puck.position.x) < goalWidth)
                    {
                        sideOffset = puck.position.x > 0 ? -0.3f : 0.3f;
                    }
                    else
                    {
                        // Otherwise just move to the side closest to the puck
                        sideOffset = puck.position.x > transform.position.x ? -0.25f : 0.25f;
                    }
                    
                    targetPosition = new Vector3(puck.position.x + sideOffset, transform.position.y, puck.position.z + 0.15f);
                }
                else
                {
                    // Strike through! 
                    // If the puck is very close to the goal, aim to hit it at an angle away from the center
                    float targetX = puck.position.x;
                    if (transform.position.z > backBoundaryZ - 0.2f && Mathf.Abs(puck.position.x) < goalWidth)
                    {
                        // Aim for the corners of the table instead of straight ahead
                        targetX = puck.position.x > 0 ? sideBoundaryX : -sideBoundaryX;
                    }

                    float strikeDepth = 0.3f;
                    targetPosition = new Vector3(targetX, transform.position.y, puck.position.z - strikeDepth);
                }
            }
            else
            {
                // In cooldown: Just shadow the puck's X position from a safe distance
                targetPosition = new Vector3(puck.position.x, transform.position.y, startingPosition.z);
            }
        }
        else
        {
            // Puck is on Player's side - RETREAT to starting position
            isStriking = false;
            targetPosition = startingPosition;
        }

        // 2. Constrain the target to the AI's half of the table
        targetPosition.x = Mathf.Clamp(targetPosition.x, -sideBoundaryX, sideBoundaryX);
        // Allow the AI to move slightly past the center line during a strike for follow-through
        targetPosition.z = Mathf.Clamp(targetPosition.z, tableCenterZ - 0.05f, backBoundaryZ);

        // 3. Move the Rigidbody towards the target
        Vector3 direction = (targetPosition - transform.position);
        
        // Apply extra power if moving forward (attacking)
        float currentSpeed = speed;
        if (isStriking && direction.z < 0) 
        {
            currentSpeed *= attackPower;
        }

        rb.linearVelocity = direction * currentSpeed;

        // 4. Cap the speed
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }
}