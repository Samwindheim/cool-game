using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;


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
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction dashAction;
    
    // --- Private State ---
    private bool canDash = true;
    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector3 inputDir;
    private Vector3 startPosition;
    private Vector3 originalScale;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInput = GetComponent<PlayerInput>();
        
        // Cache the actions from the PlayerInput component
        moveAction = playerInput.actions["Move"];
        dashAction = playerInput.actions["Dash"];
        
        startPosition = transform.position;
        originalScale = transform.localScale;
    }

    void Start()
    {
        // Initialization moved to Awake for PlayerInput consistency
    }

    // Input should be read in Update for maximum responsiveness.
    void Update()
    {
        // Read movement input as a Vector2 and convert to Vector3
        moveInput = moveAction.ReadValue<Vector2>();
        inputDir = new Vector3(moveInput.x, 0, moveInput.y);

        // Check for the dash trigger on this specific player's input
        if (dashAction.triggered && canDash)
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
        // Lerp is a linear interpolation between two values over a given time.
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

    // Forcefully drops the paddle from the VR hand.
    public void ForceRelease()
    {
        var interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (interactable != null && interactable.isSelected)
        {
            // Tells the XR system to release the object.
            // interactables.firstInteractorSelecting was deprecated in newer XRI versions, 
            // so we use the interactionManager to cancel all selections.
            interactable.interactionManager.CancelInteractableSelection((UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable)interactable);
        }
    }

    // Triggers a haptic impulse on the controller currently holding this paddle.
    public void TriggerHaptic(float intensity, float duration)
    {
        var interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (interactable != null && interactable.isSelected)
        {
            // Get the interactor (the hand) currently holding the paddle
            var interactor = interactable.interactorsSelecting[0];
            if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor inputInteractor)
            {
                inputInteractor.SendHapticImpulse(intensity, duration);
            }
        }
    }
}
