using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float dashForce = 20f;
    public float dashCooldown = 2f;
    private bool canDash = true;
    private Rigidbody rb;
    private Vector3 inputDir;

    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private string verticalAxis = "Vertical";
    [SerializeField] private KeyCode dashKey = KeyCode.Space;

    void Start() => rb = GetComponent<Rigidbody>();

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

    private System.Collections.IEnumerator Dash()
    {
        canDash = false;
        rb.AddForce(inputDir.normalized * dashForce, ForceMode.Impulse);
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
