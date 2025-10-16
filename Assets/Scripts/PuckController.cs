using UnityEngine;

// This class controls the puck's behavior, including physics, collisions, and visual effects.
public class PuckController : MonoBehaviour
{
    public GameObject hitEffectPrefab;
    
    // The starting position of the puck, set on Awake. Can be read by other scripts.
    public Vector3 StartPosition { get; private set; }

    private Rigidbody rb;
    private Vector3 lastVelocity; // Stores the velocity from the previous physics frame.
    private bool canPlaySound = false; // Flag to prevent sound from playing on startup.

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
    }

    // Resets the puck's position and stops all movement.
    public void ResetPuck()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = StartPosition;
    }

    // Called by the physics engine when the puck enters a trigger collider (the goals).
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
        if (collision.gameObject.CompareTag("Wall"))
        {
            var speed = lastVelocity.magnitude;
            var reflectionDirection = Vector3.Reflect(lastVelocity.normalized, contact.normal);
            
            // We add a small amount of the wall's normal vector to the reflection.
            // This forces the puck to bounce outwards, even on very shallow glancing hits.
            var robustDirection = (reflectionDirection + contact.normal * 0.2f).normalized;

            rb.linearVelocity = robustDirection * speed;
        }
    }
}
