using UnityEngine;
using UnityEngine.InputSystem;

public class VRResetHandler : MonoBehaviour
{
    [Header("Input Actions (New Input System)")]
    [Tooltip("The 'Reset' action. Map this to 'XRI Left Interaction/Reset'.")]
    public InputActionReference resetAction;

    [Header("References")]
    public PlayerController player1;
    public PlayerController player2;
    public PuckController puck;

    private void OnEnable()
    {
        if (resetAction != null && resetAction.action != null)
        {
            resetAction.action.Enable();
            resetAction.action.performed += OnResetTriggered;
        }
    }

    private void OnDisable()
    {
        if (resetAction != null && resetAction.action != null)
        {
            resetAction.action.performed -= OnResetTriggered;
            resetAction.action.Disable();
        }
    }

    private void OnResetTriggered(InputAction.CallbackContext context)
    {
        ResetPositions();
    }

    public void ResetPositions()
    {
        Debug.Log("VR Reset Triggered - Resetting Player and Puck positions");

        // 1. Reset Puck
        if (puck != null)
        {
            puck.ResetPuck();
        }

        // 2. Reset Players
        if (player1 != null)
        {
            player1.ForceRelease();
            player1.ResetPosition();
        }

        if (player2 != null)
        {
            player2.ForceRelease();
            player2.ResetPosition();
        }

        // 3. Audio/Haptic feedback
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayUIClick();
        }

        if (player1 != null) player1.TriggerHaptic(0.3f, 0.1f);
        if (player2 != null) player2.TriggerHaptic(0.3f, 0.1f);
    }
}
