using UnityEngine;

public class PuckController : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 startPos;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
    }

    public void ResetPuck()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startPos;
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
        // Check if the puck collided with a wall
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Calculate the reflection vector to get the bounce direction
            Vector3 reflection = Vector3.Reflect(transform.forward, collision.contacts[0].normal);
            
            // Apply a small force in the direction of the bounce to prevent sticking
            rb.AddForce(reflection * 0.1f, ForceMode.Impulse);
        }
    }
}
