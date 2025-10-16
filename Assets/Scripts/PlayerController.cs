using UnityEngine;
using System.Collections;

// This class handles player input, paddle movement, dashing, and related visual effects.
// It can be configured with different input axes to allow for multiple players.
public class PlayerController : MonoBehaviour
{
    // --- Public Fields ---
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float dashSpeedMultiplier = 2f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 2f;

    [Header("VFX")]
    public GameObject dashEffectPrefab;

    [Header("Input")]
    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private string verticalAxis = "Vertical";
    [SerializeField] private KeyCode dashKey = KeyCode.Space;
    
    // --- Private State ---
    private bool canDash = true;
    private Rigidbody rb;
    private Vector3 inputDir;
    private Vector3 startPosition;
    private Vector3 originalScale;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        originalScale = transform.localScale;
    }

    // Input should be read in Update for maximum responsiveness.
    void Update()
    {
        // Read movement input and store it.
        inputDir = new Vector3(Input.GetAxisRaw(horizontalAxis), 0, Input.GetAxisRaw(verticalAxis));

        // Check for the dash key press and if the dash is not on cooldown.
        if (Input.GetKeyDown(dashKey) && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    // Physics calculations should be done in FixedUpdate for consistency.
    void FixedUpdate()
    {
        Vector3 move = inputDir.normalized * moveSpeed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }

    // DASH EFFECT
    // This coroutine handles the entire dash sequence.
    private IEnumerator Dash()
    {
        canDash = false;
        
        AudioManager.Instance.PlayDash();

        // --- Speed Boost ---
        // Temporarily increase move speed for the duration of the dash.
        float originalSpeed = moveSpeed;
        moveSpeed *= dashSpeedMultiplier;

        // --- Visual Effects ---
        StartCoroutine(StretchEffect(dashDuration));

        if (dashEffectPrefab != null && inputDir != Vector3.zero)
        {
            // The effect should trail behind the player, so we rotate it to face away from the movement direction.
            Quaternion effectRotation = Quaternion.LookRotation(-inputDir);
            GameObject effect = Instantiate(dashEffectPrefab, transform.position, effectRotation);
            Destroy(effect, 2f);
        }

        // Wait for the dash to end.
        yield return new WaitForSeconds(dashDuration);
        moveSpeed = originalSpeed; // Reset speed.

        // Wait for the cooldown period before allowing another dash.
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // STRETCH EFFECT
    // This coroutine animates the paddle's scale to create a "stretch" effect during the dash.
    private IEnumerator StretchEffect(float duration)
    {
        float timer = 0f;
        // Stretches along the Z-axis (forward) and squashes on the X-axis (sideways).
        Vector3 stretchedScale = new Vector3(originalScale.x * 0.7f, originalScale.y, originalScale.z * 1.3f);

        // Animate from original to stretched scale over the first half of the dash.
        while (timer < duration / 2)
        {
            transform.localScale = Vector3.Lerp(originalScale, stretchedScale, timer / (duration / 2));
            timer += Time.deltaTime;
            yield return null;
        }

        // Animate from stretched back to original scale over the second half.
        timer = 0f;
        while (timer < duration / 2)
        {
            transform.localScale = Vector3.Lerp(stretchedScale, originalScale, timer / (duration / 2));
            timer += Time.deltaTime;
            yield return null;
        }

        // Ensure the scale is perfectly reset at the end.
        transform.localScale = originalScale;
    }

    // Resets the player to their starting position. Called by the GameManager after a goal.
    public void ResetPosition()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startPosition;
    }
}
