using UnityEngine;

public class PuckController : MonoBehaviour
{
    private Rigidbody rb;
    public Vector3 StartPosition { get; private set; }
    private Vector3 lastVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartPosition = transform.position;
    }

    void FixedUpdate()
    {
        lastVelocity = rb.linearVelocity;
    }

    public void ResetPuck()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = StartPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GoalLeft"))
            GameManager.Instance.AddScore(2);
        else if (other.CompareTag("GoalRight"))
            GameManager.Instance.AddScore(1);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (rb.isKinematic) return; // Ignore collisions during reset phase

        AudioManager.Instance.PlayHit();

        // Check if the puck collided with a wall
        if (collision.gameObject.CompareTag("Wall"))
        {
            // More robust bounce logic
            var speed = lastVelocity.magnitude;
            var direction = Vector3.Reflect(lastVelocity.normalized, collision.contacts[0].normal);
            rb.linearVelocity = direction * speed;
        }
    }
}
