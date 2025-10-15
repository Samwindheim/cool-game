using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float dashSpeedMultiplier = 2f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 2f;

    [Header("VFX")]
    public GameObject dashEffectPrefab;

    private bool canDash = true;
    private Rigidbody rb;
    private Vector3 inputDir;
    private Vector3 startPosition;
    private Vector3 originalScale;

    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private string verticalAxis = "Vertical";
    [SerializeField] private KeyCode dashKey = KeyCode.Space;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        originalScale = transform.localScale;
    }

    void Update()
    {
        inputDir = new Vector3(Input.GetAxisRaw(horizontalAxis), 0, Input.GetAxisRaw(verticalAxis));

        if (Input.GetKeyDown(dashKey) && canDash)
            StartCoroutine(Dash());
    }

    void FixedUpdate()
    {
        Vector3 move = inputDir.normalized * moveSpeed;
        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
    }

    // **Dash effect**
    private System.Collections.IEnumerator Dash()
    {
        canDash = false;
        
        AudioManager.Instance.PlayDash();

        float originalSpeed = moveSpeed;
        moveSpeed *= dashSpeedMultiplier;

        // Start the stretch animation
        StartCoroutine(StretchEffect(dashDuration));

        // Trigger the dash effect
        if (dashEffectPrefab != null && inputDir != Vector3.zero)
        {
            // We want the effect to point away from the direction of movement
            Quaternion effectRotation = Quaternion.LookRotation(-inputDir);
            GameObject effect = Instantiate(dashEffectPrefab, transform.position, effectRotation);
            Destroy(effect, 2f);
        }

        yield return new WaitForSeconds(dashDuration);
        moveSpeed = originalSpeed;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // **Stretch effect**
    private IEnumerator StretchEffect(float duration)
    {
        float timer = 0f;
        Vector3 stretchedScale = new Vector3(originalScale.x * 0.7f, originalScale.y, originalScale.z * 1.3f); // Example stretch

        // Stretch out
        while (timer < duration / 2)
        {
            transform.localScale = Vector3.Lerp(originalScale, stretchedScale, timer / (duration / 2));
            timer += Time.deltaTime;
            yield return null;
        }

        // Return to normal
        timer = 0f;
        while (timer < duration / 2)
        {
            transform.localScale = Vector3.Lerp(stretchedScale, originalScale, timer / (duration / 2));
            timer += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
    }

    public void ResetPosition()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startPosition;
    }
}
