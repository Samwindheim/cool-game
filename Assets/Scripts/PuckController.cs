using UnityEngine;

// This class controls the puck's behavior, including physics, collisions, and visual effects.
public class PuckController : MonoBehaviour
{
    public static PuckController Instance;

    public GameObject hitEffectPrefab;
    public float maxSpeed = 20f; // Limit how fast the puck can go
    
    // The starting position of the puck, set on Awake. Can be read by other scripts.
    public Vector3 StartPosition { get; private set; }

    private Rigidbody rb;
    private Vector3 lastVelocity; // Stores the velocity from the previous physics frame.
    private bool canPlaySound = false; // Flag to prevent sound from playing on startup.

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartPosition = transform.position;

        // Create a short "grace period" on startup to prevent the initial collision sound.
        Invoke(nameof(EnableSound), 0.1f);
    }

    // This method is called by Invoke in Start() after a short delay.
    void EnableSound()
    {
        canPlaySound = true;
    }

    // We store the velocity in FixedUpdate to ensure it's always accurate for physics calculations.
    void FixedUpdate()
    {
        lastVelocity = rb.linearVelocity;

        // Enforce the speed limit to prevent the puck from "breaking" through walls or moving too fast for VR.
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    // Resets the puck's position and stops all movement.
    public void ResetPuck()
    {
        // If we’re currently in a "frozen" / kinematic state (e.g. during reset),
        // just move the puck back without touching velocity.
        if (rb.isKinematic)
        {
            transform.position = StartPosition;
            return;
        }

        // Safe to clear velocities on a dynamic rigidbody.
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = StartPosition;
    }

    // Called by the physics engine when the puck enters a trigger collider (the goals).
    // When a goal is scored
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GoalLeft"))
        {
            GameManager.Instance.AddScore(2); // Player 2 scored.
        }
        else if (other.CompareTag("GoalRight"))
        {
            GameManager.Instance.AddScore(1); // Player 1 scored.
        }
    }

    // Called by the physics engine when the puck physically collides with another object.
    private void OnCollisionEnter(Collision collision)
    {
        // During the reset sequence, the puck is kinematic. We should ignore any collisions that happen then.
        if (rb.isKinematic) return;

        // Only play the hit sound if the initial grace period is over.
        if (canPlaySound)
        {
            AudioManager.Instance.PlayHit();
        }

        // --- Visual Effects ---
        // Spawn the hit particle effect at the exact point of contact.
        ContactPoint contact = collision.contacts[0];
        Vector3 pos = contact.point + contact.normal * 0.1f; // Offset slightly from the surface to ensure it's visible.
        Quaternion rot = Quaternion.LookRotation(contact.normal);
        GameObject effect = Instantiate(hitEffectPrefab, pos, rot);
        Destroy(effect, 2f); // Clean up the effect after 2 seconds.

        // --- Physics Response ---
        // We use custom bounce logic to prevent the puck from "sticking" to surfaces.
        // Vector3.reflect is a method that returns a vector that is the reflection of the input vector off of a surface.
        if (collision.gameObject.CompareTag("Wall"))
        {
            Vector3 incoming = lastVelocity;
            float speed = incoming.magnitude;
            if (speed <= 0.01f) return;

            Vector3 normal = contact.normal.normalized;

            // Decompose velocity into normal (into wall) and tangent (along wall)
            float vDotN = Vector3.Dot(incoming, normal);
            Vector3 vNormal = vDotN * normal;
            Vector3 vTangent = incoming - vNormal;

            // Reflect the normal component (bounce)
            Vector3 reflectedNormal = -vNormal;

            // Dampen the tangential component to reduce "wall sliding"
            float tangentDamping = 0.5f; // 0 = no slide, 1 = full slide; tweak as needed
            Vector3 dampedTangent = vTangent * tangentDamping;

            // Combine and enforce original speed
            Vector3 newVelocity = reflectedNormal + dampedTangent;

            // Make sure we’re not accidentally pushing into the wall
            if (Vector3.Dot(newVelocity, normal) < 0f)
            {
                // Add a small outward bias
                newVelocity += normal * (speed * 0.2f);
            }

            rb.linearVelocity = newVelocity.normalized * speed;
        }
    }
}
